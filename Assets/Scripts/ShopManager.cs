using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public GameObject shopUI;
    [HideInInspector] public bool isShopOpen;
    public DialogueManager dialogueManager;
    public string afterShopDialogueId = "after_shop";

    private void Start()
    {
        if (shopUI != null)
        {
            shopUI.SetActive(false);
            Debug.Log("Магазин принудительно выключен при старте");
        }
        else
        {
            Debug.LogError("ShopUI не назначен в инспекторе!");
        }
    }
    public void OpenShop()
    {
        isShopOpen = true;
        shopUI.SetActive(true);
        dialogueManager.HideDialogueUI();
    }

    public void CloseShop()
    {
        isShopOpen = false;
        shopUI.SetActive(false);
        if (dialogueManager != null)
        {
            dialogueManager.ContinueDialogue(afterShopDialogueId);
        }
    }
}