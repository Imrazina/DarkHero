using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour
{
    public Image fadeImage;
    public GameObject fadeCanvas;

    private bool isFading = false; 

    private void Awake()
    {
        fadeCanvas.SetActive(true);
        fadeImage.color = new Color(0f, 0f, 0f, 1f); 
    }

    public void ForceFadeOut()
    {
        fadeCanvas.SetActive(true); 
        fadeImage.color = new Color(0f, 0f, 0f, 1f); 
    }

    public void FadeOut(float duration)
    {
        if (!isFading)
        {
            StartCoroutine(FadeToBlack(duration));
        }
    }

    public void FadeIn(float duration)
    {
        if (!isFading)
        {
            StartCoroutine(FadeToClear(duration));
        }
    }

    private IEnumerator FadeToBlack(float duration)
    {
        isFading = true;
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
        isFading = false; 
    }

    private IEnumerator FadeToClear(float duration)
    {
        isFading = true;
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
        isFading = false; 
    }
}