using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MuteSoundsToggle : MonoBehaviour
{
    [SerializeField] private Sprite mutedIcon;
    [SerializeField] private Sprite unmutedIcon;
    [SerializeField] private Image iconImage;
    [SerializeField] private List<AudioSource> audioSources = new List<AudioSource>();

    private bool mutedState = false;

    private void Awake()
    {
        mutedState = PlayerPrefs.GetInt("MutedSound", 0) == 1;

        iconImage.sprite = mutedState ? unmutedIcon : mutedIcon;

        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.mute = mutedState;
        }
    }

    public void OnToggleMuteState()
    {
        mutedState = !mutedState;

        iconImage.sprite = mutedState ? unmutedIcon : mutedIcon;

        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.mute = mutedState;
        }

        int mutedStateAsInt = mutedState ? 1 : 0;
        PlayerPrefs.SetInt("MutedSound", mutedStateAsInt);
        PlayerPrefs.Save();
    }

    public void AddAudioSource(AudioSource audioSource)
    {
        audioSources.Add(audioSource);
        audioSource.mute = mutedState;
    }

    public void RemoveAudioSource(AudioSource audioSource)
    {
        audioSources.Remove(audioSource);
    }
}
