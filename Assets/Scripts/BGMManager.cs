using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;

    [Header("Music Settings")]
    public AudioClip musicClip;
    [Range(0f, 1f)] public float volume = 0.5f;
    public bool loop = true;
    public bool playOnAwake = true;

    private AudioSource audioSource;

    void Awake()
    {
        // If an instance already exists, destroy this duplicate
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Set up the AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = musicClip;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.playOnAwake = false; // we control playback manually below

        if (playOnAwake)
        {
            PlayMusic();
        }
    }

    public void PlayMusic()
    {
        if (audioSource != null && audioSource.clip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void StopMusic()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    public void ChangeTrack(AudioClip newClip, bool playImmediately = true)
    {
        musicClip = newClip;
        if (audioSource != null)
        {
            audioSource.clip = newClip;
            if (playImmediately)
            {
                audioSource.Play();
            }
        }
    }
}
