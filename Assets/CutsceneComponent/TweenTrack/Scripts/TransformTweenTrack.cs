using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.855f, 0.8623f, 0.870f)]
[TrackClipType(typeof(TransformTweenClip))]
[TrackBindingType(typeof(Transform))]
public class TransformTweenTrack : TrackAsset
{
    protected override void OnCreateClip(TimelineClip clip)
    {
        base.OnCreateClip(clip);
        CreateTrackWorldPoints();
    }
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<TransformTweenMixerBehaviour>.Create(graph, inputCount);
    }
    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {

#if UNITY_EDITOR


        var comp = director.GetGenericBinding(this) as Transform;
        if (comp == null)
            return;
        var so = new UnityEditor.SerializedObject(comp);
        var iter = so.GetIterator();
        while (iter.NextVisible(true))
        {
            if (iter.hasVisibleChildren)
                continue;
            driver.AddFromName<Transform>(comp.gameObject, iter.propertyPath);
        }
#endif
        base.GatherProperties(director, driver);
    }

    private void CreateTrackWorldPoints()
    {
        GameObject startLocationPrefab = Resources.Load<GameObject>("CinemachineComponent/RelativePositionObject/RelativePosition");
        GameObject relativePositionsParent = GameObject.Find("RelativePositions");
        GameObject startLocationInstance = Instantiate(startLocationPrefab, relativePositionsParent.transform.position, Quaternion.identity);
        GameObject endLocationInstance = Instantiate(startLocationPrefab, relativePositionsParent.transform.position, Quaternion.identity);
        startLocationInstance.transform.parent = relativePositionsParent.transform;
        endLocationInstance.transform.parent = relativePositionsParent.transform;

        var discoveredClip = GetClips().Select(clip => clip.asset as TransformTweenClip)
            .FirstOrDefault(transformTweenClip => transformTweenClip != null);

        if (discoveredClip != null)
        {
            discoveredClip.startLocation.defaultValue = startLocationInstance.transform;
            discoveredClip.endLocation.defaultValue = endLocationInstance.transform;
        }

        EditorUtility.SetDirty(this);
    }
}
