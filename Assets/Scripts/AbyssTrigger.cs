using System.Collections;
using UnityEngine;

public class AbyssTrigger : MonoBehaviour
{
    public Transform newCameraPosition; // Куда камера переместится при падении
    public Transform startCameraPosition; // Куда вернётся при возврате
    private ScreenFade screenFade;
    private SubtitleManager subtitleManager;
    public float delayBeforeCameraMove = 2f;
    public float fadeDuration = 1.5f;

    private void Start()
    {
        screenFade = FindObjectOfType<ScreenFade>();
        subtitleManager = FindObjectOfType<SubtitleManager>();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteKey("HasFallen");
            Debug.Log("HasFallen reset!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        int fallStatus = PlayerPrefs.GetInt("HasFallen", 0);

        if (fallStatus == 0)
        {
            // Первый раз падает → с эффектами
            StartCoroutine(FallIntoAbyss());
        }
        else if (fallStatus == 1)
        {
            // Возврат наверх
            ReturnToStartInstant();
            PlayerPrefs.SetInt("HasFallen", 2); 
        }
        else if (fallStatus == 2)
        {
            ReturnToAbyssInstant();
            PlayerPrefs.SetInt("HasFallen", 1); 
        }
    }
    
    private void ReturnToStartInstant()
    {
        Camera.main.transform.position = new Vector3(
            startCameraPosition.position.x,
            startCameraPosition.position.y,
            Camera.main.transform.position.z
        );
    }

    private IEnumerator FallIntoAbyss()
    {
        if (screenFade != null)
            screenFade.FadeOut(fadeDuration);

        yield return new WaitForSeconds(1f);
        subtitleManager?.ShowSubtitle("Where am I falling?..", 2.5f);

        yield return new WaitForSeconds(2f);
        subtitleManager?.ShowSubtitle("Wait... I see something down there.", 3f);

        yield return new WaitForSeconds(delayBeforeCameraMove);

        Camera.main.transform.position = new Vector3(
            newCameraPosition.position.x,
            newCameraPosition.position.y,
            Camera.main.transform.position.z
        );

        yield return new WaitForSeconds(1.5f);

        if (screenFade != null)
            screenFade.FadeIn(fadeDuration);

        PlayerPrefs.SetInt("HasFallen", 1); // теперь можно вернуться
    }

    private void ReturnToAbyssInstant()
    {
        Camera.main.transform.position = new Vector3(
            newCameraPosition.position.x,
            newCameraPosition.position.y,
            Camera.main.transform.position.z
        );
    }
}