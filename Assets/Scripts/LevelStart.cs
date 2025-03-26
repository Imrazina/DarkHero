using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelStart : MonoBehaviour
{
    private ScreenFade screenFade;

    private void Start()
    {
        screenFade = FindObjectOfType<ScreenFade>(); 
        StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {

        FindObjectOfType<SubtitleManager>().ShowSubtitle("Where am I?..", 3f);
        yield return new WaitForSeconds(1.5f);

        FindObjectOfType<SubtitleManager>().ShowSubtitle("This city... Why does it feel so familiar?", 3f);
        yield return new WaitForSeconds(3.5f);

        FindObjectOfType<SubtitleManager>().ShowSubtitle("Move forward. The truth is hidden behind the veil.", 2f);
        yield return new WaitForSeconds(3.5f);

        FindObjectOfType<ScreenFade>().FadeIn(2f);
        yield return new WaitForSeconds(2.5f);
        
        FindObjectOfType<SubtitleManager>().ShowSubtitle("I still remember how to move...", 2f);
        yield return new WaitForSeconds(2.5f);
        
        FindObjectOfType<SubtitleManager>().ShowSubtitle("This symbol... It's calling me.", 2f);
        yield return new WaitForSeconds(2.5f);
        
    }
}
