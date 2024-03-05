using System;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Custom PlayableBehaviour for blending multiple TransformTween clips in a TransformTweenTrack.
/// </summary>
public class TransformTweenMixerBehaviour : PlayableBehaviour
{
    // Flag to track the first frame.
    bool m_FirstFrameHappened;

    /// <summary>
    /// Process each frame of the playable, blending the positions and rotations of input clips.
    /// </summary>
    /// <param name="playable">The playable being processed.</param>
    /// <param name="info">FrameData containing information about the frame.</param>
    /// <param name="playerData">The Transform associated with the track.</param>
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Transform trackBinding = playerData as Transform;

        // Check if the track binding is valid.
        if (trackBinding == null)
            return;

        // Retrieve default position and rotation.
        Vector3 defaultPosition = trackBinding.position;
        Quaternion defaultRotation = trackBinding.rotation;

        int inputCount = playable.GetInputCount();

        // Initialize variables for blending.
        float positionTotalWeight = 0f;
        float rotationTotalWeight = 0f;

        Vector3 blendedPosition = Vector3.zero;
        Quaternion blendedRotation = new Quaternion(0f, 0f, 0f, 0f);

        // Iterate over input clips.
        for (int i = 0; i < inputCount; i++)
        {
            ScriptPlayable<TransformTweenBehaviour> playableInput = (ScriptPlayable<TransformTweenBehaviour>)playable.GetInput(i);
            TransformTweenBehaviour input = playableInput.GetBehaviour();

            // Skip invalid clips.
            if (input.endLocation == null)
                continue;

            // Get the weight of the current input.
            float inputWeight = playable.GetInputWeight(i);

            // Set starting position and rotation if it's the first frame.
            if (!m_FirstFrameHappened && !input.startLocation)
            {
                input.startingPosition = defaultPosition;
                input.startingRotation = defaultRotation;
            }

            // Calculate normalized time and tween progress.
            float normalisedTime = (float)(playableInput.GetTime() / playableInput.GetDuration());
            float tweenProgress = input.EvaluateCurrentCurve(normalisedTime);

            // Blend position if specified.
            if (input.tweenPosition)
            {
                positionTotalWeight += inputWeight;
                blendedPosition += Vector3.Lerp(input.startingPosition, input.endLocation.position, tweenProgress) * inputWeight;
            }

            // Blend rotation if specified.
            if (input.tweenRotation)
            {
                rotationTotalWeight += inputWeight;

                Quaternion desiredRotation = Quaternion.Lerp(input.startingRotation, input.endLocation.rotation, tweenProgress);
                desiredRotation = NormalizeQuaternion(desiredRotation);

                if (Quaternion.Dot(blendedRotation, desiredRotation) < 0f)
                {
                    desiredRotation = ScaleQuaternion(desiredRotation, -1f);
                }

                desiredRotation = ScaleQuaternion(desiredRotation, inputWeight);

                blendedRotation = AddQuaternions(blendedRotation, desiredRotation);
            }
        }

        // Apply default position and rotation if not fully weighted.
        blendedPosition += defaultPosition * (1f - positionTotalWeight);
        Quaternion weightedDefaultRotation = ScaleQuaternion(defaultRotation, 1f - rotationTotalWeight);
        blendedRotation = AddQuaternions(blendedRotation, weightedDefaultRotation);

        // Update the track binding's position and rotation.
        trackBinding.position = blendedPosition;
        trackBinding.rotation = blendedRotation;

        // Set the first frame flag.
        m_FirstFrameHappened = true;
    }

    /// <summary>
    /// Called when the playable is destroyed, resetting the first frame flag.
    /// </summary>
    /// <param name="playable">The playable being destroyed.</param>
    /// <param name="playable"></param>
    public override void OnPlayableDestroy(Playable playable)
    {
        m_FirstFrameHappened = false;
    }

    /// <summary>
    /// Helper method to add two quaternions.
    /// </summary>
    /// <param name="first">First quaternion.</param>
    /// <param name="second">Second quaternion.</param>
    /// <returns>The result of adding two quaternions.</returns>
    static Quaternion AddQuaternions(Quaternion first, Quaternion second)
    {
        first.w += second.w;
        first.x += second.x;
        first.y += second.y;
        first.z += second.z;
        return first;
    }

    /// <summary>
    /// Helper method to scale a quaternion by a multiplier.
    /// </summary>
    /// <param name="rotation">The quaternion to scale.</param>
    /// <param name="multiplier">The scaling multiplier.</param>
    /// <returns>The scaled quaternion.</returns>
    static Quaternion ScaleQuaternion(Quaternion rotation, float multiplier)
    {
        rotation.w *= multiplier;
        rotation.x *= multiplier;
        rotation.y *= multiplier;
        rotation.z *= multiplier;
        return rotation;
    }

    /// <summary>
    /// Helper method to calculate the magnitude of a quaternion.
    /// </summary>
    /// <param name="rotation">The quaternion to calculate the magnitude for.</param>
    /// <returns>The magnitude of the quaternion.</returns>
    static float QuaternionMagnitude(Quaternion rotation)
    {
        return Mathf.Sqrt(Quaternion.Dot(rotation, rotation));
    }

    /// <summary>
    /// Helper method to normalize a quaternion.
    /// </summary>
    /// <param name="rotation">The quaternion to normalize.</param>
    /// <returns>The normalized quaternion.</returns>
    static Quaternion NormalizeQuaternion(Quaternion rotation)
    {
        float magnitude = QuaternionMagnitude(rotation);

        if (magnitude > 0f)
            return ScaleQuaternion(rotation, 1f / magnitude);

        // Warn if normalizing a quaternion with zero magnitude.
        Debug.LogWarning("Cannot normalize a quaternion with zero magnitude.");
        return Quaternion.identity;
    }
}
