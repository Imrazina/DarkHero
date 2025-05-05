using System.Collections;
using UnityEngine;

public class ShopPandaNPC : MonoBehaviour, IDialogueCallback
{
    public DialogueManager dialogueManager;
    public ShopManager shopManager;
    private bool isPlayerNear = false;
    private bool canStartDialogue = true;
    
    [SerializeField] private string dialogueFileName = "shop_dialogue";

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && canStartDialogue && !shopManager.isShopOpen)
        {
            StartDialogue();
        }
    }

    public void StartDialogue()
    {
        canStartDialogue = false;
        dialogueManager.StartDialogue(dialogueFileName, "1", this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerNear = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }

    public void OnDialogueEnd()
    {
        if (!shopManager.isShopOpen)
        {
            StartCoroutine(CooldownStartDialogue());
        }
    }

    private IEnumerator CooldownStartDialogue()
    {
        yield return new WaitForSeconds(1f);
        canStartDialogue = true;
    }
}