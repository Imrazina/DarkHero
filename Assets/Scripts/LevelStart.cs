using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelStart : MonoBehaviour
{
    private ScreenFade screenFade;

    private void Awake()
    {
        Debug.Log("LevelStart активен в сцене!");
    }
    public void StartIntro()
    {
        screenFade = FindObjectOfType<ScreenFade>();
        if (screenFade == null) Debug.LogWarning("ScreenFade not found!");

        // Проверка: если уже играли интро — выходим
        if (GameStateManager.Instance.CurrentState.hasPlayedIntro)
            return;

        // Ставим флаг, что интро сыграно
        GameStateManager.Instance.CurrentState.hasPlayedIntro = true;
        GameStateManager.Instance.SaveGame();

        StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        
        Debug.Log("Starting intro...");

        var subtitleManager = FindObjectOfType<SubtitleManager>();
        if (subtitleManager == null) Debug.LogWarning("SubtitleManager not found!");
        else Debug.Log("SubtitleManager found!");

        var canvas = subtitleManager.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Debug.Log("Subtitle Canvas isActive: " + canvas.gameObject.activeSelf);
        }
        
        FindObjectOfType<SubtitleManager>().ShowSubtitle("Where am I?..", 3f);
        yield return new WaitForSeconds(1.5f);

        FindObjectOfType<SubtitleManager>().ShowSubtitle("This city... Why does it feel so familiar?", 3f);
        yield return new WaitForSeconds(3.5f);

        FindObjectOfType<SubtitleManager>().ShowSubtitle("Move forward. The truth is hidden behind the veil.", 2f);
        yield return new WaitForSeconds(3.5f);

        screenFade.FadeIn(2f);
        yield return new WaitForSeconds(2.5f);

        FindObjectOfType<SubtitleManager>().ShowSubtitle("I still remember how to move...", 2f);
        yield return new WaitForSeconds(2.5f);

        FindObjectOfType<SubtitleManager>().ShowSubtitle("This symbol... It's calling me.", 2f);
        yield return new WaitForSeconds(2.5f);
    }
}
