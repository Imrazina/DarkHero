using System.Collections;
using UnityEngine;

public class PandaNPC : MonoBehaviour
{
   public DialogueManager dialogueManager;
    private bool isPlayerNear = false;
    private bool canStartDialogue = true;

    [SerializeField] private string dialogueFileName = "dialogue";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PandaDialogueState loadedState = GameStateManager.Instance.CurrentState.pandaState;
        SetDialogueState(loadedState);
    }

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && canStartDialogue)
        {
            canStartDialogue = false;

            dialogueManager.StartDialogue(dialogueFileName, GetDialogueId());

            if (GameStateManager.Instance.CurrentState.pandaState == PandaDialogueState.AfterEnemyDefeated)
            {
                GameStateManager.Instance.CurrentState.pandaState = PandaDialogueState.SmallTalk;
            }
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

    public void OnDialogueEnd()
    {
        StartCoroutine(CooldownStartDialogue());
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
}