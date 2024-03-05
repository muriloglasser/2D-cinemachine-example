using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// Custom track for handling TransformTweenClips.
/// </summary>
[TrackColor(0.855f, 0.8623f, 0.870f)]
[TrackClipType(typeof(TransformTweenClip))]
[TrackBindingType(typeof(Transform))]
public class TransformTweenTrack : TrackAsset
{
    /// <summary>
    /// Called when a new clip is created adding transform references to Cutscene scene.
    /// </summary>
    /// <param name="clip"></param>
    protected override void OnCreateClip(TimelineClip clip)
    {
        Debug.Log("HERE");
        base.OnCreateClip(clip);
        CreateTrackWorldPoints(clip);
        EnumerateAndRenameWorldPoints();
    }

    /// <summary>
    /// Creates a playable mixer for the TransformTweenTrack, enabling blending of multiple clips.
    /// </summary>
    /// <param name="graph">The PlayableGraph in which the mixer will be created.</param>
    /// <param name="go">The GameObject associated with the track.</param>
    /// <param name="inputCount">The number of input clips for the track.</param>
    /// <returns>A ScriptPlayable of the TransformTweenMixerBehaviour type.</returns>
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<TransformTweenMixerBehaviour>.Create(graph, inputCount);
    }

    /// <summary>
    /// Gathers properties for the editor display, specifically handling the serialization of Transform properties.
    /// </summary>
    /// <param name="director">The PlayableDirector associated with the track.</param>
    /// <param name="driver">The IPropertyCollector used for collecting serialized properties.</param>
    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
#if UNITY_EDITOR
        var comp = director.GetGenericBinding(this) as Transform;

        if (comp == null)
            return;

        var so = new UnityEditor.SerializedObject(comp);
        var iter = so.GetIterator();

        // Iterates over serialized properties and adds them to the driver.
        while (iter.NextVisible(true))
        {
            if (iter.hasVisibleChildren)
                continue;

            driver.AddFromName<Transform>(comp.gameObject, iter.propertyPath);
        }
#endif
        base.GatherProperties(director, driver);
    }

    /// <summary>
    /// Creates start and end points for the track.
    /// </summary>
    /// <param name="clip">New created clip.</param>
    private void CreateTrackWorldPoints(TimelineClip clip)
    {
        // Load the RelativePosition prefab from Resources.
        // GameObject startLocationPrefab = Resources.Load<GameObject>("CinemachineComponent/RelativePositionObject/RelativePosition");

        //Create a new game object to spawn in world.
        GameObject relativePoint = new GameObject();

        // Find the parent object for relative positions.
        GameObject relativePositionsParent = GameObject.Find("RelativePositions");

        // Instantiate start and end location objects.
        GameObject startLocationInstance = Instantiate(relativePoint, relativePositionsParent.transform.position, Quaternion.identity);
        GameObject endLocationInstance = Instantiate(relativePoint, relativePositionsParent.transform.position, Quaternion.identity);

        // Destroy relative point reference.
        DestroyImmediate(relativePoint);

        // Set parents for proper organization.
        startLocationInstance.transform.parent = relativePositionsParent.transform;
        endLocationInstance.transform.parent = relativePositionsParent.transform;

        // Discover the TransformTweenClip and set start and end locations.
        var discoveredClip = clip.asset as TransformTweenClip;

        if (discoveredClip != null)
        {
            discoveredClip.startLocation.defaultValue = startLocationInstance.transform;
            discoveredClip.endLocation.defaultValue = endLocationInstance.transform;
        }

        // Mark the scene as dirty for changes to take effect.
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetSceneByName("Cutscene"));
    }

    /// <summary>
    /// Method to enumerate RelativePosition objects
    /// </summary>
    public void EnumerateAndRenameWorldPoints()
    {
        GameObject relativePositionsParent = GameObject.Find("RelativePositions");

        // Check if the parent GameObject is assigned
        if (relativePositionsParent == null)
        {
            Debug.LogError("The parent does not exists!");
            return;
        }

        // Get all RelativePosition objects in the parent GameObject
        Transform[] relativePositions = relativePositionsParent.GetComponentsInChildren<Transform>();

        // Enumerate and rename RelativePosition objects
        for (int i = 1; i < relativePositions.Length; i++)
        {
            // Rename the object using the "RelativePosition_X" pattern
            relativePositions[i].gameObject.name = "RelativePosition_" + (i);
        }
    }   
}
