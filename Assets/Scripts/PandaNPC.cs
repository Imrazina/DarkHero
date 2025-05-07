using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class PandaNPC : MonoBehaviour, IDialogueCallback{
    public DialogueManager dialogueManager;
    private bool isPlayerNear = false;
    private bool canStartDialogue = true;
    private bool rewardGiven;
    
    [SerializeField] private string dialogueFileName = "dialogue";
    
    private void Start()
    {
        rewardGiven = GameStateManager.Instance.CurrentState.pandaRewardGiven;
        PandaDialogueState loadedState = GameStateManager.Instance.CurrentState.pandaState;
        SetDialogueState(loadedState);
    }

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && canStartDialogue)
        {
            canStartDialogue = false;

            dialogueManager.StartDialogue(dialogueFileName, GetDialogueId());
        }
    }

    private string GetDialogueId()
    {
        switch (GameStateManager.Instance.CurrentState.pandaState)
        {
            case PandaDialogueState.FirstMeeting: return "1";
            case PandaDialogueState.AfterEnemyDefeated: return "100";
            case PandaDialogueState.SmallTalk: return "200";
            default: return "200";
        }
    }

    private void SetDialogueState(PandaDialogueState state)
    {
        GameStateManager.Instance.CurrentState.pandaState = state;
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
            canStartDialogue = true;
        }
    }
    

    private IEnumerator CooldownStartDialogue()
    {
        yield return new WaitForSeconds(1f);
        canStartDialogue = true;
    }

    public void OnEnemyDefeated()
    {
        GameStateManager.Instance.CurrentState.enemyDefeated = true;
        GameStateManager.Instance.CurrentState.pandaState = PandaDialogueState.AfterEnemyDefeated;
        
        GameStateManager.Instance.SaveGame();
    }

    public void OnDialogueEnd()
    {
        Debug.Log($"[PANDA_DEBUG] === PandaNPC.OnDialogueEnd ===");
        if (GameStateManager.Instance.CurrentState.pandaState == PandaDialogueState.AfterEnemyDefeated && !rewardGiven)
        {
            var inventory = FindObjectOfType<PlayerInventory>();
            if (inventory != null)
            {
                inventory.AddPotion(1);
            }

            rewardGiven = true;
            GameStateManager.Instance.CurrentState.pandaRewardGiven = true;
            GameStateManager.Instance.CurrentState.pandaState = PandaDialogueState.SmallTalk; // <-- Перенесено сюда
            GameStateManager.Instance.SaveGame();
        }
        StartCoroutine(CooldownStartDialogue());
    }
}