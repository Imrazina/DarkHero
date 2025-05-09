using System.Collections;
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
        screenFade.FadeOut(0f);
        StartCoroutine(PlayFullIntro());

        GameStateManager.Instance.CurrentState.hasPlayedIntro = true;
        GameStateManager.Instance.SaveGame();
        
        if (!GameStateManager.Instance.CurrentState.hasPlayedIntro)
        {
            screenFade.FadeIn(1f); 
        }
    }

    private IEnumerator PlayFullIntro()
    {
        Debug.Log("Starting FULL intro...");
    
        var subtitleManager = FindObjectOfType<SubtitleManager>();
        if (subtitleManager == null) 
        {
            Debug.LogWarning("SubtitleManager not found!");
            yield break;
        }
    
        subtitleManager.ShowSubtitle("Where am I?..", 3f, 0.05f, true); // true - воспроизводить звук
        yield return new WaitForSeconds(1.5f);

        subtitleManager.ShowSubtitle("This city... Why does it feel so familiar?", 3f, 0.05f, true);
        yield return new WaitForSeconds(3.5f);

        subtitleManager.ShowSubtitle("Move forward. The truth is hidden behind the veil.", 2f, 0.05f, true);
        yield return new WaitForSeconds(3.5f);
        screenFade.FadeIn(2f);
    
        yield return new WaitForSeconds(2.5f);

        subtitleManager.ShowSubtitle("I still remember how to move...", 2f, 0.05f, true);
        yield return new WaitForSeconds(2.5f);

        subtitleManager.ShowSubtitle("This symbol... It's calling me.", 2f, 0.05f, true);
        yield return new WaitForSeconds(2.5f);
    
        GameStateManager.Instance.SaveGame();
    }
}