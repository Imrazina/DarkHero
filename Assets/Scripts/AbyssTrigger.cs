using System.Collections;
using UnityEngine;

public class AbyssTrigger : MonoBehaviour
{
    public Transform newCameraPosition;
    public Transform startCameraPosition;
    private ScreenFade screenFade;
    private SubtitleManager subtitleManager;
    public float delayBeforeCameraMove = 2f;
    public float fadeDuration = 1.5f;

    private void Start()
    {
        screenFade = FindObjectOfType<ScreenFade>();
        subtitleManager = FindObjectOfType<SubtitleManager>();

        if (screenFade == null)
        {
            Debug.LogWarning("[AbyssTrigger] ScreenFade not found on Start!");
        }
        else
        {
            Debug.Log("[AbyssTrigger] ScreenFade found successfully.");
        }

        if (subtitleManager == null)
        {
            Debug.LogWarning("[AbyssTrigger] SubtitleManager not found on Start!");
        }
        else
        {
            Debug.Log("[AbyssTrigger] SubtitleManager found successfully.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteKey("HasFallen");
            Debug.Log("[AbyssTrigger] HasFallen reset manually!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        int fallStatus = GameStateManager.Instance.CurrentState.fallStatus;
        Debug.Log($"[AbyssTrigger] Player entered trigger. Current fall status: {fallStatus}");

        if (fallStatus == 0)
        {
            Debug.Log("[AbyssTrigger] Starting fall into abyss...");
            StartCoroutine(FallIntoAbyss());
        }
        else if (fallStatus == 1)
        {
            Debug.Log("[AbyssTrigger] Returning player to start position instantly.");
            ReturnToStartInstant();
            GameStateManager.Instance.CurrentState.fallStatus = 2;
            GameStateManager.Instance.SaveGame();
        }
        else if (fallStatus == 2)
        {
            Debug.Log("[AbyssTrigger] Returning player to abyss instantly.");
            ReturnToAbyssInstant();
            GameStateManager.Instance.CurrentState.fallStatus = 1;
            GameStateManager.Instance.SaveGame();
        }
    }

    private void ReturnToStartInstant()
    {
        Debug.Log("[AbyssTrigger] Moving camera to start position.");
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

        GameStateManager.Instance.CurrentState.fallStatus = 1;
        GameStateManager.Instance.SaveGame();
    }

    private void ReturnToAbyssInstant()
    {
        Debug.Log("[AbyssTrigger] Moving camera to abyss position instantly.");
        Camera.main.transform.position = new Vector3(
            newCameraPosition.position.x,
            newCameraPosition.position.y,
            Camera.main.transform.position.z
        );
    }
}
