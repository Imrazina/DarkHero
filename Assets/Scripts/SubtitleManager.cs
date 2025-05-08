using System.Collections;
using TMPro;
using UnityEngine;

public class SubtitleManager : MonoBehaviour
{
    public TextMeshProUGUI subtitleText;
    private Coroutine currentCoroutine;
    
    [Header("Sound Effects")]
    public AudioClip typingSound;
    [Range(0, 2)] public float typingSoundVolume = 0.9f;
    [Range(0.01f, 0.2f)] public float typingSoundDelay = 0.08f;
    private AudioSource audioSource;
    private float lastTypingSoundTime;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    public void ShowSubtitle(string text, float duration = 3f, float typingSpeed = 0.05f, bool playSound = false)
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(TypeSubtitle(text, duration, typingSpeed, playSound));
    }

    private IEnumerator TypeSubtitle(string text, float duration, float typingSpeed, bool playSound)
    {
        subtitleText.gameObject.SetActive(true);
        subtitleText.text = "";

        foreach (char letter in text.ToCharArray())
        {
            subtitleText.text += letter;
            
            if (playSound && typingSound != null && Time.time > lastTypingSoundTime + typingSoundDelay)
            {
                audioSource.PlayOneShot(typingSound, typingSoundVolume);
                lastTypingSoundTime = Time.time;
            }
            
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(duration);
        subtitleText.gameObject.SetActive(false);
    }
}