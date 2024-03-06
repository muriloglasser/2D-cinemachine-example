using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Manages the binding of objects to a PlayableDirector for a cinematic timeline.
/// </summary>
public class TimeLineBinder : MonoBehaviour
{
    [Header("Cutscene player")]
    public PlayableDirector playableDirector;
    [Header("Instances parent")]
    public Transform instances;
    [Header("Instances to bind")]
    public GameObject playerPrefab;
    public GameObject cinemachinePrefab;
    public GameObject audioSourcePrefab;
    //
    private Animator playerAnimator;
    private Transform cinemachineVirtualCamera;
    private AudioSource musicAudioSource;

    /// <summary>
    /// Called before the first frame update. Initializes components and plays the cutscene.
    /// </summary>
    void Start()
    {
        InstantiateComponents();
        SetBindings();
        PlayCutscene();
    }

    /// <summary>
    /// Instantiates player, cinemachine, and audio source components.
    /// </summary>
    private void InstantiateComponents()
    {
        playerAnimator = Instantiate(playerPrefab, instances.position, Quaternion.identity, instances).GetComponent<Animator>();
        cinemachineVirtualCamera = Instantiate(cinemachinePrefab, instances.position, Quaternion.identity, instances).transform.GetChild(1).transform;
        musicAudioSource = Instantiate(audioSourcePrefab, instances.position, Quaternion.identity, instances).GetComponent<AudioSource>();
    }

    /// <summary>
    /// Sets up bindings for the PlayableDirector, associating objects with specific tracks.
    /// </summary>
    private void SetBindings()
    {
        // Recupera as bindings existentes
        var existingBindings = new List<PlayableBinding>();
        existingBindings.AddRange(playableDirector.playableAsset.outputs);

       /* // Limpa as bindings atuais do PlayableDirector
        playableDirector.playableAsset.outputs.Clear();

        // Adiciona as instâncias às bindings existentes
        playableDirector.SetGenericBinding(playerAnimator, playerAnimator);
        playableDirector.SetGenericBinding(cinemachineVirtualCamera, cinemachineVirtualCamera);
        playableDirector.SetGenericBinding(musicAudioSource, musicAudioSource);

        // Adiciona as bindings existentes de volta ao PlayableDirector
        playableDirector.playableAsset.outputs.AddRange(existingBindings);*/

        // Exemplo de como acessar informações sobre as bindings existentes
        foreach (var binding in existingBindings)
        {
            Debug.Log("Existing Binding: " + binding.streamName + " Type: " + binding.outputTargetType);
            switch (binding.outputTargetType.ToString())
            {
                case "UnityEngine.Animator":
                    playableDirector.SetGenericBinding(binding.sourceObject, playerAnimator);
                    break;
                case "UnityEngine.Transform":
                    playableDirector.SetGenericBinding(binding.sourceObject, cinemachineVirtualCamera);
                    break;
                case "UnityEngine.AudioSource":
                    playableDirector.SetGenericBinding(binding.sourceObject, musicAudioSource);
                    break;
                default:
                    break;
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
