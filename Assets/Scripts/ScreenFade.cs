using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour
{
    public Image fadeImage;
    public GameObject fadeCanvas;

    private void Awake()
    {
        fadeCanvas.SetActive(true);
        fadeImage.color = new Color(0f, 0f, 0f, 1f);
    }
    
    
    public void ForceFadeOut()
    {
        fadeCanvas.SetActive(true);
    }
    
    private IEnumerator DelayedFadeIn()
    {
        
        yield return FadeToClear(2f);
    }

    public void FadeOut(float duration)
    {
        StartCoroutine(FadeToBlack(duration));
    }

    public void FadeIn(float duration)
    {
        StartCoroutine(FadeToClear(duration));
    }

    private IEnumerator FadeToBlack(float duration)
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < duration)
        {
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            fadeImage.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }

    private IEnumerator FadeToClear(float duration)
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < duration)
        {
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            fadeImage.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;

        fadeCanvas.SetActive(false);
    }
}