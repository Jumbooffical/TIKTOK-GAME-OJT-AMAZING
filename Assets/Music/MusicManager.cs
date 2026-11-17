
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;

    [Header("Music Themes")]
    public AudioClip[] musicTracks;

    private int lastPlayedIndex = -1;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        PlayRandomMusic();
    }

    void Update()
    {
        if (!audioSource.isPlaying)
        {
            PlayRandomMusic();
        }
    }

    void PlayRandomMusic()
    {
        if (musicTracks == null || musicTracks.Length == 0)
            return;

        int randomIndex;

        do
        {
            randomIndex = Random.Range(0, musicTracks.Length);
        }
        while (musicTracks.Length > 1 && randomIndex == lastPlayedIndex);

        lastPlayedIndex = randomIndex;

        audioSource.clip = musicTracks[randomIndex];
        audioSource.Play();

        Debug.Log("Now playing: " + musicTracks[randomIndex].name);
    }
}
