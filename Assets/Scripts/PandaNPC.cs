using System.Collections;
using UnityEngine;

public class PandaNPC : MonoBehaviour
{
    public DialogueManager dialogueManager;
    private bool isPlayerNear = false;
    private bool isDialogueActive = false;
    private bool canStartDialogue = true;

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && canStartDialogue)
        {
            isDialogueActive = true;
            canStartDialogue = false;
            dialogueManager.StartDialogue("dialogue");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            dialogueManager.EndDialogue(); 
            isDialogueActive = false;
            canStartDialogue = true;
        }
    }
    
    public void OnDialogueEnd()
    {
        
        isDialogueActive = false;
        StartCoroutine(CooldownStartDialogue());
    }
    
    private IEnumerator CooldownStartDialogue()
    {
        yield return new WaitForSeconds(1f); 
        canStartDialogue = true;
    }
}