using System.Collections;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public static MusicController Instance;
    
    [Header("Music Tracks")]
    public AudioClip mainTheme;
    public AudioClip arenaMusic;
    
    [Header("Settings")]
    [Range(0, 1)] public float mainThemeVolume = 0.5f;
    [Range(0, 1)] public float eventMusicVolume = 0.7f;
    
    private AudioSource bgSource;
    private AudioSource eventSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            bgSource = gameObject.AddComponent<AudioSource>();
            eventSource = gameObject.AddComponent<AudioSource>();

            bgSource.clip = mainTheme;
            bgSource.loop = true;
            bgSource.volume = mainThemeVolume;
            bgSource.Play();

            eventSource.loop = false;
            eventSource.volume = eventMusicVolume;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void PlayEventMusic(AudioClip clip, bool loop = false)
    {
        if (eventSource.isPlaying && eventSource.clip == clip) return;
        
        eventSource.Stop();
        eventSource.clip = clip;
        eventSource.loop = loop;
        eventSource.Play();
    }
    
    public void StopEventMusic()
    {
        eventSource.Stop();
    }
    
    public void FadeOutEventMusic(float duration = 1f)
    {
        StartCoroutine(FadeAudioSource(eventSource, 0f, duration));
    }

    public void FadeInMainTheme(float duration = 1f)
    {
        StartCoroutine(FadeAudioSource(bgSource, mainThemeVolume, duration));
    }

    private IEnumerator FadeAudioSource(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;
    
        while (elapsed < duration)
        {
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        source.volume = targetVolume;
    }
    
    public void SwitchToArenaMusic(float fadeDuration = 1f)
    {
        StartCoroutine(SwitchMusicCoroutine(arenaMusic, fadeDuration));
    }

    private IEnumerator SwitchMusicCoroutine(AudioClip newClip, float fadeDuration)
    {
        yield return StartCoroutine(FadeAudioSource(bgSource, 0f, fadeDuration/2));
        bgSource.clip = newClip;
        bgSource.Play();
        yield return StartCoroutine(FadeAudioSource(bgSource, mainThemeVolume, fadeDuration/2));
    }
    
    public void SwitchToMainTheme(float fadeDuration = 1f)
    {
        StartCoroutine(SwitchMusicCoroutine(mainTheme, fadeDuration));
    }
}