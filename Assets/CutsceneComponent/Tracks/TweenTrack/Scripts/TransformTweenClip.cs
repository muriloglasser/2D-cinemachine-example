using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// Represents a playable clip that defines a transformation tween over time.
/// </summary>
[Serializable]
public class TransformTweenClip : PlayableAsset, ITimelineClipAsset
{
    // The template behavior for the TransformTweenClip.
    [SerializeField]
    public TransformTweenBehaviour template = new TransformTweenBehaviour();

    // Exposed reference for the starting location of the transformation.
    [SerializeField]
    public ExposedReference<Transform> startLocation;

    // Exposed reference for the ending location of the transformation.
    [SerializeField]
    public ExposedReference<Transform> endLocation;

    /// <summary>
    /// Called when the scriptable object is validated in the Editor.
    /// </summary>
    private void OnValidate()
    {
        // Uncomment the following line if needed for validation purposes
        // Debug.Log("Changed");
    }

    /// <summary>
    /// Destroy clip world references.
    /// </summary>
    private void OnDestroy()
    {
        GameObject startLocationRef = GameObject.Find(startLocation.defaultValue.name);
        GameObject endLocationRef = GameObject.Find(endLocation.defaultValue.name);

        if (startLocationRef != null)
            DestroyImmediate(startLocationRef);
        if (endLocationRef != null)
            DestroyImmediate(endLocationRef);
    }

    /// <summary>
    /// Gets the supported capabilities of the clip, indicating that it supports blending.
    /// </summary>
    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending; }
    }

    /// <summary>
    /// Creates a playable instance for the TransformTweenClip.
    /// </summary>
    /// <param name="graph">The PlayableGraph in which the playable instance will be created.</param>
    /// <param name="owner">The GameObject owner of the playable.</param>
    /// <returns>A ScriptPlayable of the TransformTweenBehaviour type.</returns>
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        // Create a script playable using the specified template.
        var playable = ScriptPlayable<TransformTweenBehaviour>.Create(graph, template);

        // Retrieve the TransformTweenBehaviour clone from the playable.
        TransformTweenBehaviour clone = playable.GetBehaviour();

        // Resolve and set start and end locations for the clone.
        clone.startLocation = startLocation.Resolve(graph.GetResolver());
        clone.endLocation = endLocation.Resolve(graph.GetResolver());

        return playable;
    }   

}
