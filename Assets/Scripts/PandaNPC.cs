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
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && canStartDialogue && !isDialogueActive)
        {
            StartDialogue();
        }
    }

    private void StartDialogue()
    {
        isDialogueActive = true;
        canStartDialogue = false;  // Запрещаем повторный запуск
        dialogueManager.StartDialogue("dialogue");  // Запускаем диалог
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
            if (isDialogueActive)
            {
                EndDialogue();
            }
        }
    }

    public void EndDialogue()
    {
        StartCoroutine(ResetDialogue());
    }

    private IEnumerator ResetDialogue()
    {
        yield return new WaitForSeconds(2f);  // Подождите немного, прежде чем можно будет начать новый диалог
        isDialogueActive = false;
        canStartDialogue = true;
        dialogueManager.EndDialogue();
    }
}