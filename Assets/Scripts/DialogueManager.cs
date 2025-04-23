using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
   [Header("Main Dialogue UI")]
    public GameObject dialoguePanel;
    public GameObject bgImage;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI titleName;
    public GameObject imagePanda;
    public GameObject imageSamurai;
    public GameObject avatarsRoot; 

    [Header("Choice UI")] 
    public GameObject choicePanel;
    private Button button1;
    private Button button2; 
    private Button button3; 
    private TextMeshProUGUI text1;
    private TextMeshProUGUI text2;
    private TextMeshProUGUI text3;

    private Queue<DialogueLine> dialogueQueue;
    private DialogueLine currentLine;
    private bool isWaitingForChoice = false;
    private bool isTyping = false;
    private Dictionary<string, DialogueLine> dialogueMap;
    private Dictionary<string, DialogueData> loadedDialogueFiles = new Dictionary<string, DialogueData>();
    private PandaNPC currentNPC;
    private string currentFileName;

    private void Awake()
    {
        dialogueQueue = new Queue<DialogueLine>();
        dialogueMap = new Dictionary<string, DialogueLine>();

        InitializeChoicePanel();
    }

    private void InitializeChoicePanel()
    {
        if (choicePanel == null)
        {
            Debug.LogError("ChoicePanel не назначен в инспекторе!");
            return;
        }
        button1 = choicePanel.transform.Find("Button1")?.GetComponent<Button>();
        button2 = choicePanel.transform.Find("Button2")?.GetComponent<Button>();
        button3 = choicePanel.transform.Find("Button3")?.GetComponent<Button>();

        text1 = button1?.GetComponentInChildren<TextMeshProUGUI>(true);
        text2 = button2?.GetComponentInChildren<TextMeshProUGUI>(true);
        text3 = button3?.GetComponentInChildren<TextMeshProUGUI>(true);
        Debug.Log($"Инициализация: Button1={button1 != null}, Text1={text1 != null}");
        Debug.Log($"Инициализация: Button2={button2 != null}, Text2={text2 != null}");
        Debug.Log($"Инициализация: Button3={button3 != null}, Text3={text3 != null}");
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
        bgImage.SetActive(false);
        choicePanel.SetActive(false);
        imagePanda.SetActive(false);
        imageSamurai.SetActive(false);
        avatarsRoot.SetActive(true);
        
        Debug.Log($"Стартовые состояния:");
        Debug.Log($"Panel: {dialoguePanel.activeSelf}");
        Debug.Log($"Samurai: {imageSamurai.activeSelf}");
        Debug.Log($"ChoicePanel: {choicePanel.activeSelf}");
    }

    public void StartDialogue(string fileName, string startId, PandaNPC npc)
    {
        currentFileName = fileName;
        currentNPC = npc;
        
        if (!GameStateManager.Instance.IsNewGame && 
            GameStateManager.Instance.CurrentState.currentDialogueFile == fileName)
        {
            if (dialogueMap.ContainsKey(GameStateManager.Instance.CurrentState.currentDialogueId))
            {
                startId = GameStateManager.Instance.CurrentState.currentDialogueId;
                Debug.Log($"Продолжение диалога с ID: {startId}");
            }
            else
            {
                Debug.LogWarning($"Сохраненный ID {GameStateManager.Instance.CurrentState.currentDialogueId} не найден, начинаем с начала");
            }
        }
        
        if (!loadedDialogueFiles.TryGetValue(fileName, out DialogueData data))
        {
            data = DialogueLoader.LoadDialogue(fileName);
            if (data == null)
            {
                Debug.LogError($"Dialogue file not found: {fileName}");
                return;
            }
            loadedDialogueFiles[fileName] = data; 
        }

        dialogueQueue.Clear();
        dialogueMap.Clear();

        data = loadedDialogueFiles[fileName];

        foreach (var line in data.dialogues)
        {
            if (!string.IsNullOrEmpty(line.id))
            {
                dialogueMap[line.id] = line;
            }
        }

        if (dialogueMap.TryGetValue(startId, out DialogueLine firstLine))
        {
            dialogueQueue.Enqueue(firstLine);
            dialoguePanel.SetActive(true);
            bgImage.SetActive(true);
            DisplayNextSentence();
            Debug.Log($"Starting dialogue with ID: {startId}");  // Логируем начало диалога
        }
        else
        {
            Debug.LogError($"Dialogue ID {startId} not found in file {fileName}!");
        }
    }
    

    public void DisplayNextSentence()
    {
        if (isTyping || isWaitingForChoice) return;

        if (dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentLine = dialogueQueue.Dequeue();
        StartCoroutine(TypeSentence(currentLine));
    }
    
    private void SetActiveSafe(GameObject obj, bool state)
    {
        if (obj == null) 
        {
            Debug.LogError("Попытка изменить активность null объекта!");
            return;
        }
    
        if (obj.activeSelf != state)
        {
            obj.SetActive(state);
            Debug.Log($"{obj.name} активность: {state} (было: {!state})");
        }
    }

    private IEnumerator TypeSentence(DialogueLine line)
    {
        SetActiveSafe(dialoguePanel, true);
        SetActiveSafe(bgImage, true);
        SetActiveSafe(dialogueText.gameObject, true);
        SetActiveSafe(titleName.gameObject, true);
        
        avatarsRoot.SetActive(true);
        imagePanda.SetActive(line.avatar == "ImagePanda");
        imageSamurai.SetActive(line.avatar == "ImageSamurai");
  
        bgImage.SetActive(true);
        
        Debug.Log($"[DialogueManager] Начало TypeSentence для ID: {line.id}");
        
        isTyping = true;
        dialogueText.text = "";
        titleName.text = line.name;

        Debug.Log($"[DialogueManager] Установка аватара: {line.avatar}");
        imagePanda.SetActive(line.avatar == "ImagePanda");
        imageSamurai.SetActive(line.avatar == "ImageSamurai");

        foreach (char letter in line.text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;
        Debug.Log($"[DialogueManager] Текст напечатан, isTyping = false");

        if (line.choices != null && line.choices.Count > 0)
        {
            Debug.Log($"[DialogueManager] Обнаружены choices: {line.choices.Count}");
            isWaitingForChoice = true;
            Debug.Log("[DialogueManager] Ожидаю нажатия E...");
            yield return new WaitUntil(() => 
            {
                bool pressed = Input.GetKeyDown(KeyCode.E);
                if (pressed) Debug.Log("[DialogueManager] Клавиша E нажата!");
                return pressed;
            }); 
            ShowChoices(line.choices);
        }
        else
        {
            Debug.Log("[DialogueManager] Choices не найдены, завершаю диалог");
            EndDialogue();
        }
    }


    private void ShowChoices(List<DialogueChoice> choices)
    {
        SetActiveSafe(dialoguePanel, true);
        avatarsRoot.SetActive(true);
        imagePanda.SetActive(false);
        imageSamurai.SetActive(true);
        
        bgImage.SetActive(true);
        
        SetActiveSafe(dialogueText.gameObject, false);
        SetActiveSafe(imagePanda, false);
        
        SetActiveSafe(imageSamurai, true);
 
        text1.text = choices[0].text;
        text1.ForceMeshUpdate();

        SetActiveSafe(choicePanel, true);
        
        Debug.Log($"[DialogueManager] ShowChoices начал выполнение. Количество choices: {choices?.Count}");
        
        if (choices == null || choices.Count == 0)
        {
            Debug.LogWarning("[DialogueManager] choices пуст или null!");
            return;
        }
        
        Debug.Log($"[DialogueManager] Проверка UI элементов перед отображением:");
        Debug.Log($"choicePanel активен: {choicePanel.activeSelf}");
        Debug.Log($"imagePanda активен: {imagePanda.activeSelf}");
        Debug.Log($"imageSamurai активен: {imageSamurai.activeSelf}");

        dialogueText.text = ""; 
        titleName.text = "Samurai"; 
        imagePanda.SetActive(false);
        imageSamurai.SetActive(true);

        Debug.Log("[DialogueManager] Установил аватар Samurai и очистил текст");

        choicePanel.SetActive(true);
        isWaitingForChoice = true;

        Debug.Log($"[DialogueManager] Устанавливаю текст кнопок:");
        text1.text = choices.Count > 0 ? choices[0].text : "N/A";
        text2.text = choices.Count > 1 ? choices[1].text : "N/A";
        text3.text = choices.Count > 2 ? choices[2].text : "N/A";
        
        Debug.Log($"Кнопка 1 текст: '{text1.text}'");
        Debug.Log($"Кнопка 2 текст: '{text2.text}'");
        Debug.Log($"Кнопка 3 текст: '{text3.text}'");

        button1.gameObject.SetActive(choices.Count > 0);
        button2.gameObject.SetActive(choices.Count > 1);
        button3.gameObject.SetActive(choices.Count > 2);
        
        Debug.Log($"Кнопка 1 активна: {button1.gameObject.activeSelf}");
        Debug.Log($"Кнопка 2 активна: {button2.gameObject.activeSelf}");
        Debug.Log($"Кнопка 3 активна: {button3.gameObject.activeSelf}");

        button1.onClick.RemoveAllListeners();
        button2.onClick.RemoveAllListeners();
        button3.onClick.RemoveAllListeners();

        if (choices.Count > 0)
        {
            string nextId = choices[0].nextId;
            Debug.Log($"[DialogueManager] Настройка кнопки 1 -> nextId: {nextId}");
            button1.onClick.AddListener(() => HandleChoice(1, nextId));
        }
        
        if (choices.Count > 1)
        {
            string nextId = choices[1].nextId;
            Debug.Log($"[DialogueManager] Настройка кнопки 2 -> nextId: {nextId}");
            button2.onClick.AddListener(() => HandleChoice(2, nextId));
        }
        
        if (choices.Count > 2)
        {
            string nextId = choices[2].nextId;
            Debug.Log($"[DialogueManager] Настройка кнопки 3 -> nextId: {nextId}");
            button3.onClick.AddListener(() => HandleChoice(3, nextId));
        }

        Debug.Log("[DialogueManager] ShowChoices завершил выполнение");
    }

    private void HandleChoice(int buttonNumber, string nextId)
    {
        Debug.Log($"[DialogueManager] HandleChoice для кнопки {buttonNumber}");
        
        if (EventSystem.current == null)
        {
            Debug.LogError("[DialogueManager] EventSystem.current равен null!");
            return;
        }

        var clickedButton = EventSystem.current.currentSelectedGameObject?.GetComponent<Button>();
        
        if (clickedButton == null)
        {
            Debug.LogError("[DialogueManager] Не удалось получить Button компонент");
            return;
        }

        Debug.Log($"<color=magenta>Нажата: {clickedButton.name}</color> ({buttonNumber}) → nextId: {nextId}");
        SelectChoice(nextId);
    }

    private void SelectChoice(string nextId)
    {
        SetActiveSafe(choicePanel, false);
        
        SetActiveSafe(dialoguePanel, true);
        SetActiveSafe(dialogueText.gameObject, true);
        
        GameStateManager.Instance.CurrentState.currentDialogueId = nextId;
        GameStateManager.Instance.CurrentState.currentDialogueFile = currentFileName;
        var currentState = GameStateManager.Instance.CurrentState.pandaState;
        var newLine = dialogueMap[nextId];
        var newState = GetStateFromDialogueId(nextId);
        
        if (newState != currentState)
        {
            GameStateManager.Instance.CurrentState.pandaState = newState;
            GameStateManager.Instance.SaveGame();
        }
        
        choicePanel.SetActive(false);
        isWaitingForChoice = false;
        dialogueQueue.Clear();
        dialogueQueue.Enqueue(newLine);
        DisplayNextSentence();
    }
    
    private PandaDialogueState GetStateFromDialogueId(string id)
    {
        if (id.StartsWith("1")) return PandaDialogueState.FirstMeeting;
        if (id.StartsWith("100")) return PandaDialogueState.AfterEnemyDefeated;
        if (id.StartsWith("200")) return PandaDialogueState.SmallTalk;
        return GameStateManager.Instance.CurrentState.pandaState; 
    }

    public void EndDialogue()
    {
        dialogueQueue.Clear();
        dialoguePanel.SetActive(false);
        bgImage.SetActive(false);
        choicePanel.SetActive(false);
        imagePanda.SetActive(false);
        imageSamurai.SetActive(false);
        titleName.text = "";
        dialogueText.text = "";
        
        if (currentNPC != null)
        {
            currentNPC.OnDialogueEnd();
        }
    }

    private void Update()
    {
        if (dialoguePanel.activeSelf && Input.GetKeyDown(KeyCode.E) && !isTyping && !isWaitingForChoice)
        {
            DisplayNextSentence();
        }
    }
}