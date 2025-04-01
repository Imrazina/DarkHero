using System.Collections;
using UnityEngine;

public class AbyssTrigger : MonoBehaviour
{
    public Transform newCameraPosition; // Точка, куда переместится камера
    private ScreenFade screenFade; // Затемнение экрана
    private SubtitleManager subtitleManager; // Система субтитров
    public float delayBeforeCameraMove = 2f; // Задержка перед сменой камеры
    public float fadeDuration = 1.5f; // Длительность затемнения

    private void Start()
    {
        screenFade = FindObjectOfType<ScreenFade>();
        subtitleManager = FindObjectOfType<SubtitleManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(FallIntoAbyss());
        }
    }

    private IEnumerator FallIntoAbyss()
    {
        if (screenFade != null)
        {
            screenFade.FadeOut(fadeDuration);
        }
        
        yield return new WaitForSeconds(1f);
        subtitleManager?.ShowSubtitle("Where am I falling?..", 2.5f);

        yield return new WaitForSeconds(2f);
        subtitleManager?.ShowSubtitle("Wait... I see something down there.", 3f);

        yield return new WaitForSeconds(delayBeforeCameraMove);

        Camera.main.transform.position = new Vector3(newCameraPosition.position.x, newCameraPosition.position.y, Camera.main.transform.position.z);

        yield return new WaitForSeconds(1.5f);
        
        if (screenFade != null)
        {
            screenFade.FadeIn(fadeDuration);
        }
    }
}