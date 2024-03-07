using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [Space(5)]
    [Header("Instances to bind")]
    public List<BindInstanceData> bindInstances;
    [Space(5)]
    [Header("Instances parent")]
    public Transform instances;

    // Dictionary to store track groups and their corresponding tracks
    private Dictionary<string, List<TrackStruct>> tracksGroup = new Dictionary<string, List<TrackStruct>>();

    void Start()
    {
        // Initialize track groups, bind timeline tracks, and play the cutscene
        InitializeTrackGroups();
    }

    /// <summary>
    /// Sets up a dictionary with all track groups and their corresponding tracks in the timeline.
    /// </summary>
    private void InitializeTrackGroups()
    {
        // Check if there are root tracks in the timeline.
        if (timeline.rootTrackCount == 0)
        {
            Debug.LogError("There is no TrackGroup in this timeline");
            return;
        }

        //Check duplicated references on bindInstances data list.
        if (bindInstances.GroupBy(data => data.trackGroupType).Any(group => group.Count() > 1))
        {
            Debug.LogError("There are duplicated TrackGroupType references in TimelineBinder.bindInstances");
            return;
        }

        // Iterate through all track groups.
        for (int rootTrackIndex = 0; rootTrackIndex < timeline.rootTrackCount; rootTrackIndex++)
        {
            TrackAsset rootTrack = timeline.GetRootTrack(rootTrackIndex);
            List<TrackStruct> outputTracks = GetOutputTracksForRootTrack(rootTrack);

            if (outputTracks.Count == 0)
                Debug.Log(rootTrack.name + " TrackGroup is empty!");
            else if (!bindInstances.Any(instance => instance.trackGroupType.ToString() == rootTrack.name))
                Debug.LogWarning(rootTrack.name + " TrackGroup exists but TimelineBinder.bindInstances does not have a reference to it!");
            else
                tracksGroup.Add(rootTrack.name, outputTracks);
        }

        //Now bind all the existent tracks.
        BindTimelineTracks();
    }

    /// <summary>
    /// Retrieves a list of TrackStruct instances for a given root track.
    /// </summary>
    /// <param name="rootTrack">The root track for which to retrieve output tracks.</param>
    /// <returns>A list of TrackStruct instances representing tracks in the same group.</returns>
    private List<TrackStruct> GetOutputTracksForRootTrack(TrackAsset rootTrack)
    {
        List<TrackStruct> outputTracks = new List<TrackStruct>();
        int instanceIdIterator = 0;

        for (int outputTrackIndex = 0; outputTrackIndex < timeline.outputTrackCount; outputTrackIndex++)
        {
            TrackAsset outputTrack = timeline.GetOutputTrack(outputTrackIndex);

            if (IsOutputTrackInRootTrackGroup(outputTrack, rootTrack))
            {
                TrackStruct trackTemp = CreateTrackStruct(instanceIdIterator, outputTrack);
                instanceIdIterator++;
                outputTracks.Add(trackTemp);
            }
            else
                continue;

        }

        return outputTracks;
    }

    /// <summary>
    /// Checks if the given output track belongs to the specified root track group.
    /// </summary>
    /// <param name="outputTrack">The output track to check.</param>
    /// <param name="rootTrack">The root track group to compare against.</param>
    /// <returns>True if the output track belongs to the root track group; otherwise, false.</returns>
    private bool IsOutputTrackInRootTrackGroup(TrackAsset outputTrack, TrackAsset rootTrack)
    {
        return outputTrack.parent.name == rootTrack.name;
    }

    /// <summary>
    /// Creates a new TrackStruct instance.
    /// </summary>
    /// <param name="instanceId">The instance ID associated with the track.</param>
    /// <param name="outputTrack">The output track to bind.</param>
    /// <returns>A new TrackStruct instance.</returns>
    private TrackStruct CreateTrackStruct(int instanceId, TrackAsset outputTrack)
    {
        return new TrackStruct
        {
            instanceID = instanceId,
            track = outputTrack
        };
    }

    /// <summary>
    /// Bind all TrackGroup instances.
    /// </summary>
    private void BindTimelineTracks()
    {
        // Get all trackgroup names.
        List<string> trackAssets = tracksGroup.Keys.Select(key => key.ToString()).ToList();

        foreach (var item in bindInstances)
        {
            string trackGroupname = item.trackGroupType.ToString();

            if (item.instances.Count == 0)
                Debug.LogWarning(item.name + " does not have objects to bind!");
            else if (!trackAssets.Contains(trackGroupname))
                Debug.LogWarning(trackGroupname + " TrackGroup does not exists in dictionary!");
            else
                BindTrackGroup(trackGroupname, item);
        }

        // Play timeline cutscene.
        PlayCutscene();
    }

    /// <summary>
    /// Binds instances for a specific TrackGroup using the provided BindInstanceData.
    /// </summary>
    /// <param name="trackGroupName">Name of the TrackGroup to bind.</param>
    /// <param name="bindInstanceData">BindInstanceData for the instances.</param>
    private void BindTrackGroup(string trackGroupName, BindInstanceData bindInstanceData)
    {
        // Iterate through each track in the specified TrackGroup.
        foreach (var tracks in tracksGroup[trackGroupName])
        {
            // Iterate through each binding in the track.
            foreach (var binding in tracks.track.outputs)
            {
                if (bindInstanceData.instances[tracks.instanceID] == null)
                    Debug.LogError("There is a null reference in " + bindInstanceData.name);
                else
                {
                    // Instantiate the instance from BindInstanceData and set the generic binding.
                    Object instanceObject = Instantiate(bindInstanceData.instances[tracks.instanceID], instances.position, Quaternion.identity, instances);
                    playableDirector.SetGenericBinding(binding.sourceObject, instanceObject);
                }
            }
        }
    }

    /// <summary>
    /// Plays the cutscene using the PlayableDirector.
    /// </summary>
    private void PlayCutscene()
    {
        if (playableDirector == null)
        {
            Debug.LogError("Playable director is Null!");
            return;
        }

        playableDirector.Play();
    }
}

/// <summary>
/// Represents a structure containing information about a track.
/// </summary>
public struct TrackStruct
{
    public int instanceID;
    public TrackAsset track;
}
