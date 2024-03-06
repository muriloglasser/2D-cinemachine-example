using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// Manages the binding of objects to a PlayableDirector for a cinematic timeline.
/// </summary>
public class TimeLineBinder : MonoBehaviour
{
    [Header("Cutscene player and timeline")]
    public PlayableDirector playableDirector;
    public TimelineAsset timeline;   
    [Header("Instances to bind")]
    public BindInstanceData trixBindInstanceData;
    public BindInstanceData camerasBindInstanceData;
    public BindInstanceData audioBindInstanceData;
    [Header("Instances parent")]
    public Transform instances;
    //  
    private Dictionary<string, List<TrackStruct>> tracksGroup = new Dictionary<string, List<TrackStruct>>();
   
    void Start()
    {
        SetBindingDictionary();
        BindAllInstances();
        PlayCutscene();   
    }

    /// <summary>
    /// Sets up bindings for the PlayableDirector, associating objects with specific tracks.
    /// </summary>
    private void SetBindingDictionary()
    {
        for (int i = 0; i < timeline.rootTrackCount; i++)
        {
            TrackAsset rootTrack = timeline.GetRootTrack(i);
            List<TrackStruct> outputTracks = new List<TrackStruct>();
            int instanceIdIterator = 0;

            for (int j = 0; j < timeline.outputTrackCount; j++)
            {
                TrackAsset outputTrack = timeline.GetOutputTrack(j);
                if (outputTrack.parent.name == rootTrack.name)
                {

                    TrackStruct trackTemp = new TrackStruct
                    {
                        instanceID = instanceIdIterator,
                        track = outputTrack
                    };

                    instanceIdIterator++;
                    outputTracks.Add(trackTemp);
                }
            }

            tracksGroup.Add(rootTrack.name, outputTracks);
        } 
    }

    /// <summary>
    /// Bind all TrackGroup instances.
    /// </summary>
    private void BindAllInstances()
    {
        BindInstanceToDirector("Trix", trixBindInstanceData);
        BindInstanceToDirector("Cameras", camerasBindInstanceData);
        BindInstanceToDirector("Audio", audioBindInstanceData);
    }

    /// <summary>
    /// Bind an instance by an TrackGroup name and using a BindInstanceData.
    /// </summary>
    /// <param name="trackGroupName"></param>
    /// <param name="bindInstanceData"></param>
    private void BindInstanceToDirector(string trackGroupName, BindInstanceData bindInstanceData)
    {
        foreach (var tracks in tracksGroup[trackGroupName])
        {
            foreach (var binding in tracks.track.outputs)
            {
                Object instanceObject = Instantiate(bindInstanceData.instances[tracks.instanceID], instances.position, Quaternion.identity, instances);
                playableDirector.SetGenericBinding(binding.sourceObject, instanceObject);
            }
        }
    }

    /// <summary>
    /// Plays the cutscene using the PlayableDirector.
    /// </summary>
    private void PlayCutscene()
    {
        playableDirector.Play();
    }
}


public struct TrackStruct
{
    public int instanceID;
    public TrackAsset track;
}
