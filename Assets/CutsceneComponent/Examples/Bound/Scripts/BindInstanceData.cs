using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

[ExecuteInEditMode]
[CreateAssetMenu(fileName = "NewBindInstanceData", menuName = "CinemachineComponent/BindInstanceData")]
public class BindInstanceData : ScriptableObject
{
    public TimelineAsset timeline;
    public TrackGroup trackGroupType;
    public List<Object> instances;   

   /* private void CheckTimelineAsset()
    {
        if (timeline != null)
        {
            for (int i = 0; i < timeline.rootTrackCount; i++)
            {
                TrackAsset rootTrack = timeline.GetRootTrack(i);

                if (rootTrack.name != trackGroupType.ToString())
                    continue;

                for (int j = 0; j < timeline.outputTrackCount; j++)
                {
                    TrackAsset outputTrack = timeline.GetOutputTrack(j);
                    if (outputTrack.parent.name == rootTrack.name)
                    {
                        instances.Add(null);
                    }
                }

            }

        }
    }*/
}
public enum TrackGroup
{
    Trix,
    Cameras,
    Audio
}

