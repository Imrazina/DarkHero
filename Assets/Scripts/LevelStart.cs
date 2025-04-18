using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelStart : MonoBehaviour
{
    private ScreenFade screenFade;

    private void Awake()
    {
        Debug.Log("LevelStart активен в сцене!");
        screenFade = FindObjectOfType<ScreenFade>();
    }

    public void StartIntro()
    {
        if (screenFade == null) 
        {
            screenFade = FindObjectOfType<ScreenFade>();
            if (screenFade == null) Debug.LogWarning("ScreenFade not found!");
        }
        
        StartCoroutine(PlayFullIntro());

        GameStateManager.Instance.CurrentState.hasPlayedIntro = true;
        GameStateManager.Instance.SaveGame();
    }

    private IEnumerator PlayFullIntro()
    {
        Debug.Log("Starting FULL intro...");
        
        screenFade.FadeOut(0f); 
        yield return null; 
        
        var subtitleManager = FindObjectOfType<SubtitleManager>();
        if (subtitleManager == null) 
        {
            Debug.LogWarning("SubtitleManager not found!");
            yield break;
        }
        
        subtitleManager.ShowSubtitle("Where am I?..", 3f);
        yield return new WaitForSeconds(1.5f);

        subtitleManager.ShowSubtitle("This city... Why does it feel so familiar?", 3f);
        yield return new WaitForSeconds(3.5f);

        subtitleManager.ShowSubtitle("Move forward. The truth is hidden behind the veil.", 2f);
        yield return new WaitForSeconds(3.5f);
        screenFade.FadeIn(2f);
        
        yield return new WaitForSeconds(2.5f);

        subtitleManager.ShowSubtitle("I still remember how to move...", 2f);
        yield return new WaitForSeconds(2.5f);

        subtitleManager.ShowSubtitle("This symbol... It's calling me.", 2f);
        yield return new WaitForSeconds(2.5f);
        
    }
}
