using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SubtitleManager : MonoBehaviour
{
    public TextMeshProUGUI subtitleText;
    private Coroutine currentCoroutine;

    public void ShowSubtitle(string text, float duration = 3f, float typingSpeed = 0.05f)
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(TypeSubtitle(text, duration, typingSpeed));
    }

    private IEnumerator TypeSubtitle(string text, float duration, float typingSpeed)
    {
        subtitleText.gameObject.SetActive(true);
        subtitleText.text = ""; // Очищаем текст перед началом печати

        foreach (char letter in text.ToCharArray())
        {
            subtitleText.text += letter;
            yield return new WaitForSeconds(typingSpeed); // Задержка между буквами
        }

        yield return new WaitForSeconds(duration);
        subtitleText.gameObject.SetActive(false);
    }
}
