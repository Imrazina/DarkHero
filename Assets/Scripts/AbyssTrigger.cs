using System.Collections;
using UnityEngine;

public class AbyssTrigger : MonoBehaviour
{
   public Transform newCameraPosition; // Точка, куда переместится камера
    public Transform startCameraPosition; // Начальная точка камеры
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
            // Если персонаж уже был в канаве, то возвращаем его назад
            bool hasFallen = PlayerPrefs.GetInt("HasFallen", 0) == 1;
            
            if (hasFallen)
            {
                StartCoroutine(ReturnToStartPosition());
            }
            else
            {
                StartCoroutine(FallIntoAbyss());
            }
        }
    }

    private IEnumerator FallIntoAbyss()
    {
        // Затемнение экрана
        if (screenFade != null)
        {
            screenFade.FadeOut(fadeDuration);
        }

        yield return new WaitForSeconds(1f);
        subtitleManager?.ShowSubtitle("Where am I falling?..", 2.5f);

        yield return new WaitForSeconds(2f);
        subtitleManager?.ShowSubtitle("Wait... I see something down there.", 3f);

        yield return new WaitForSeconds(delayBeforeCameraMove);

        // Перемещение камеры в точку падения
        Camera.main.transform.position = new Vector3(newCameraPosition.position.x, newCameraPosition.position.y, Camera.main.transform.position.z);

        yield return new WaitForSeconds(1.5f);

        if (screenFade != null)
        {
            screenFade.FadeIn(fadeDuration);
        }

        // Помечаем, что персонаж был в канаве
        PlayerPrefs.SetInt("HasFallen", 1);
    }

    private IEnumerator ReturnToStartPosition()
    {
        // Затемнение экрана
        if (screenFade != null)
        {
            screenFade.FadeOut(fadeDuration);
        }

        yield return new WaitForSeconds(1f);

        // Перемещение камеры обратно на начальную точку
        Camera.main.transform.position = new Vector3(startCameraPosition.position.x, startCameraPosition.position.y, Camera.main.transform.position.z);

        yield return new WaitForSeconds(1.5f);

        if (screenFade != null)
        {
            screenFade.FadeIn(fadeDuration);
        }

        // Помечаем, что персонаж вернулся обратно
        PlayerPrefs.SetInt("HasFallen", 0);
    }
}