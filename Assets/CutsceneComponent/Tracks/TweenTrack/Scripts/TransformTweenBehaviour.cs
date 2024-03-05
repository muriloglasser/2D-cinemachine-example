using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// Represents the behavior of a TransformTween playable, defining a transformation tween over time.
/// </summary>
[Serializable]
public class TransformTweenBehaviour : PlayableBehaviour
{
    /// <summary>
    /// Enum defining different types of tweens.
    /// </summary>
    public enum TweenType
    {
        Linear,
        Deceleration,
        Harmonic,
        Custom,
    }

    // The starting location for the transformation.
    public Transform startLocation;

    // The ending location for the transformation.
    public Transform endLocation;

    // Determines whether to tween the position during the transformation.
    public bool tweenPosition = true;

    // Determines whether to tween the rotation during the transformation.
    public bool tweenRotation = true;

    // The type of tween to apply.
    public TweenType tweenType;

    // The custom curve to use if the tween type is set to Custom.
    public AnimationCurve customCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    // The starting position for the transformation.
    public Vector3 startingPosition;

    // The starting rotation for the transformation.
    public Quaternion startingRotation = Quaternion.identity;

    // Predefined linear animation curve.
    AnimationCurve m_LinearCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    // Predefined deceleration animation curve.
    AnimationCurve m_DecelerationCurve = new AnimationCurve(
        new Keyframe(0f, 0f, -k_RightAngleInRads, k_RightAngleInRads),
        new Keyframe(1f, 1f, 0f, 0f)
    );

    // Predefined harmonic animation curve.
    AnimationCurve m_HarmonicCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // Constant representing a right angle in radians.
    const float k_RightAngleInRads = Mathf.PI * 0.5f;

    /// <summary>
    /// Prepares the frame by storing the initial position and rotation.
    /// </summary>
    /// <param name="playable">The playable being prepared.</param>
    /// <param name="info">FrameData containing information about the frame.</param>
    public override void PrepareFrame(Playable playable, FrameData info)
    {
        if (startLocation)
        {
            startingPosition = startLocation.position;
            startingRotation = startLocation.rotation;
        }
    }

    /// <summary>
    /// Updates the start and end locations of the transformation.
    /// </summary>
    /// <param name="startLocation">The new starting location.</param>
    /// <param name="endLocation">The new ending location.</param>
    public void UpdateTransforms(Transform startLocation, Transform endLocation)
    {
        if (startLocation)
        {
            this.startLocation = startLocation;
            this.endLocation = endLocation;
        }
    }

    /// <summary>
    /// Evaluates the current tween curve based on the specified tween type.
    /// </summary>
    /// <param name="time">The normalized time value.</param>
    /// <returns>The result of the curve evaluation.</returns>
    public float EvaluateCurrentCurve(float time)
    {
        if (tweenType == TweenType.Custom && !IsCustomCurveNormalised())
        {
            Debug.LogError("Custom Curve is not normalized. Curve must start at 0,0 and end at 1,1.");
            return 0f;
        }

        switch (tweenType)
        {
            case TweenType.Linear:
                return m_LinearCurve.Evaluate(time);
            case TweenType.Deceleration:
                return m_DecelerationCurve.Evaluate(time);
            case TweenType.Harmonic:
                return m_HarmonicCurve.Evaluate(time);
            default:
                return customCurve.Evaluate(time);
        }
    }

    /// <summary>
    /// Checks if the custom curve is normalized.
    /// </summary>
    /// <returns>True if the custom curve is normalized; otherwise, false.</returns>
    bool IsCustomCurveNormalised()
    {
        if (!Mathf.Approximately(customCurve[0].time, 0f))
            return false;

        if (!Mathf.Approximately(customCurve[0].value, 0f))
            return false;

        if (!Mathf.Approximately(customCurve[customCurve.length - 1].time, 1f))
            return false;

        return Mathf.Approximately(customCurve[customCurve.length - 1].value, 1f);
    }
}
