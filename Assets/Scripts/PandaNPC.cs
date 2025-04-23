using System.Collections;
using UnityEngine;

public class PandaNPC : MonoBehaviour
{
    public DialogueManager dialogueManager;
    private bool isPlayerNear = false;
    private bool canStartDialogue = true;

    [SerializeField] private string dialogueFileName = "default_dialogue";
    [SerializeField] private PandaDialogueState initialState = PandaDialogueState.FirstMeeting;

    private void Start()
    {
        Debug.Log("[PandaNPC] Start() начал выполнение");
        
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("[PandaNPC] GameStateManager.Instance равен null!");
            return;
        }

        if (!GameStateManager.Instance.IsNewGame)
        {
            if (GameStateManager.Instance.CurrentState != null)
            {
                Debug.Log($"[PandaNPC] Загружаем состояние панды: {GameStateManager.Instance.CurrentState.pandaState}");
                LogFullGameState(); // Выводим полное состояние игры
            }
            else
            {
                Debug.LogWarning("[PandaNPC] CurrentState равен null при загрузке!");
            }
        }
        else
        {
            Debug.Log($"[PandaNPC] Новая игра, устанавливаем initialState: {initialState}");
            GameStateManager.Instance.CurrentState.pandaState = initialState;
            LogFullGameState();
        }
    }

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && canStartDialogue)
        {
            Debug.Log("[PandaNPC] Игрок нажал E для начала диалога");
            canStartDialogue = false;

            string currentDialogueId = GetDialogueId();
            Debug.Log($"[PandaNPC] Текущий dialogueId: {currentDialogueId} (состояние: {GameStateManager.Instance.CurrentState.pandaState})");

            if (dialogueManager == null)
            {
                Debug.LogError("[PandaNPC] dialogueManager не назначен!");
                return;
            }

            dialogueManager.StartDialogue(dialogueFileName, currentDialogueId, this);
        }
    }

    private string GetDialogueId()
    {
        return GameStateManager.Instance.CurrentState.pandaState switch
        {
            PandaDialogueState.FirstMeeting => "1",
            PandaDialogueState.AfterEnemyDefeated => "100",
            PandaDialogueState.SmallTalk => "200",
            _ => "1" 
        };
    }

    public void SetDialogueState(PandaDialogueState state)
    {
        Debug.Log($"[PandaNPC] SetDialogueState: меняем {GameStateManager.Instance.CurrentState.pandaState} -> {state}");
        GameStateManager.Instance.CurrentState.pandaState = state;
        LogFullGameState();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[PandaNPC] Игрок вошел в триггер");
            isPlayerNear = true;
            
            StartCoroutine(DelayedDialogueStart());
        }
    }
    
    private IEnumerator DelayedDialogueStart()
    {
        yield return null;
    
        if (Input.GetKeyDown(KeyCode.E) && canStartDialogue)
        {
            StartDialogue();
        }
    }
    
    private void StartDialogue()
    {
        canStartDialogue = false;
        string currentDialogueId = GetDialogueId();
        dialogueManager.StartDialogue(dialogueFileName, currentDialogueId, this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[PandaNPC] Игрок вышел из триггера");
            isPlayerNear = false;
            canStartDialogue = true;
        }
    }

    public void OnDialogueEnd()
    {
        if (GameStateManager.Instance.CurrentState.pandaState == PandaDialogueState.AfterEnemyDefeated)
        {
            SetDialogueState(PandaDialogueState.SmallTalk);
            GameStateManager.Instance.SaveGame();
            Debug.Log("Автоматический переход в SmallTalk");
        }
        StartCoroutine(CooldownStartDialogue());
    }

    private IEnumerator CooldownStartDialogue()
    {
        Debug.Log("[PandaNPC] Начинаем кулдаун перед новым диалогом");
        yield return new WaitForSeconds(1f);
        canStartDialogue = true;
        Debug.Log("[PandaNPC] Кулдаун завершен, можно начинать новый диалог");
    }

    public void OnEnemyDefeated()
    {
        Debug.Log($"[PandaNPC] OnEnemyDefeated: меняем состояние на AfterEnemyDefeated");
        GameStateManager.Instance.CurrentState.enemyDefeated = true;
        SetDialogueState(PandaDialogueState.AfterEnemyDefeated);
        GameStateManager.Instance.SaveGame();
    }
    
    public PandaDialogueState GetCurrentState()
    {
        var state = GameStateManager.Instance.CurrentState.pandaState;
        Debug.Log($"[PandaNPC] GetCurrentState: {state}");
        return state;
    }

    private void LogFullGameState()
    {
        var state = GameStateManager.Instance.CurrentState;
        Debug.Log($"[PandaNPC] Полное состояние игры:\n" +
                  $"pandaState: {state.pandaState}\n" +
                  $"enemyDefeated: {state.enemyDefeated}\n" +
                  $"playerPosition: {state.playerPosition}\n" +
                  $"isNewGame: {GameStateManager.Instance.IsNewGame}");
    }
}