using System.Collections;
using System.Collections.Generic;
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
    public GameObject imageBoss;

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
    private bool skipTyping = false;
    
    [Header("Sound Effects")]
    public AudioClip typingSound;
    public float typingSoundDelay = 0.08f;
    private AudioSource audioSource;
    private float lastTypingSoundTime;
    [Range(0, 2)] public float TypingSoundVolume = 0.9f;
    
    private MonoBehaviour currentNPC;

    private void Awake()
    {
        Debug.Log("<color=yellow>[Awake] DialogueManager</color>");
    
        dialogueQueue = new Queue<DialogueLine>();
        dialogueMap = new Dictionary<string, DialogueLine>();

        InitializeButtonsByPosition();
        FindTextComponents();
        audioSource = gameObject.AddComponent<AudioSource>();
        // Проверяем ссылки после инициализации
        CheckReferences();
    }


    private void InitializeButtonsByPosition()
    {
        button1 = GameObject.Find("Button1").GetComponent<Button>();
        button2 = GameObject.Find("Button2").GetComponent<Button>();
        button3 = GameObject.Find("Button3").GetComponent<Button>();
    }

    private void FindTextComponents()
    {
        text1 = button1?.GetComponentInChildren<TextMeshProUGUI>(true);
        text2 = button2?.GetComponentInChildren<TextMeshProUGUI>(true);
        text3 = button3?.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private void Start()
    {
        Debug.Log("<color=yellow>[Start] DialogueManager</color>");
    
        dialoguePanel.SetActive(false);
        bgImage.SetActive(false);
        choicePanel.SetActive(false);
        imagePanda.SetActive(false);
        imageSamurai.SetActive(false);
        imageBoss.SetActive(false);
    }
    
    private void CheckReferences()
    {
        if (dialoguePanel == null) Debug.LogError("[DialogueManager] dialoguePanel is NULL!");
        if (bgImage == null) Debug.LogError("[DialogueManager] bgImage is NULL!");
        if (dialogueText == null) Debug.LogError("[DialogueManager] dialogueText is NULL!");
        if (titleName == null) Debug.LogError("[DialogueManager] titleName is NULL!");
        if (imagePanda == null) Debug.LogError("[DialogueManager] imagePanda is NULL!");
        if (imageSamurai == null) Debug.LogError("[DialogueManager] imageSamurai is NULL!");
        if (choicePanel == null) Debug.LogError("[DialogueManager] choicePanel is NULL!");
    
        if (button1 == null) Debug.LogError("[DialogueManager] button1 is NULL!");
        if (button2 == null) Debug.LogError("[DialogueManager] button2 is NULL!");
        if (button3 == null) Debug.LogError("[DialogueManager] button3 is NULL!");

        if (text1 == null) Debug.LogError("[DialogueManager] text1 is NULL!");
        if (text2 == null) Debug.LogError("[DialogueManager] text2 is NULL!");
        if (text3 == null) Debug.LogError("[DialogueManager] text3 is NULL!");
    }
    
    
    public void StartDialogue(string fileName, string startId, MonoBehaviour npc = null)
    {
        currentNPC = npc;
        
        dialogueQueue.Clear();
        dialogueMap.Clear();

        DialogueData data = DialogueLoader.LoadDialogue(fileName);
        if (data == null)
        {
            Debug.LogError($"Dialogue file not found: {fileName}");
            return;
        }

        foreach (var line in data.dialogues)
        {
            if (!string.IsNullOrEmpty(line.id))
            {
                dialogueMap[line.id] = line;
            }
        }
        
        Debug.Log($"Starting dialogue, NPC: {npc != null}, file: {fileName}, startID: {startId}");

        if (dialogueMap.TryGetValue(startId, out DialogueLine firstLine))
        {
            dialogueQueue.Enqueue(firstLine);
            dialoguePanel.SetActive(true);
            bgImage.SetActive(true);
            DisplayNextSentence();
        }
        else
        {
            Debug.LogError($"Dialogue ID {startId} not found in file {fileName}!");
        }
        
        Debug.Log($"Loaded dialogues: {string.Join(", ", dialogueMap.Keys)}");
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

    private IEnumerator TypeSentence(DialogueLine line)
    {
        isTyping = true;
        skipTyping = false;
    
        dialogueText.text = "";
        titleName.text = line.name ?? ""; 

        imagePanda.SetActive(line.avatar == "ImagePanda");
        imageSamurai.SetActive(line.avatar == "ImageSamurai");
        imageBoss.SetActive(line.avatar == "ImageBoss");
        

        float soundCooldown = 0.1f; 
        float nextSoundTime = 0f;
    
        foreach (char letter in line.text.ToCharArray())
        {
            if (skipTyping)
            {
                dialogueText.text = line.text;
                break;
            }
            dialogueText.text += letter;
            if (typingSound != null && Time.time > nextSoundTime)
            {
                audioSource.PlayOneShot(typingSound,TypingSoundVolume);
                nextSoundTime = Time.time + soundCooldown;
            }
            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;
    
        if (line.choices != null && line.choices.Count > 0)
        {
            isWaitingForChoice = true;
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E)); 
            ShowChoices(line.choices);
        }
        else
        {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E));
            
            if (string.IsNullOrEmpty(line.nextId) && dialogueQueue.Count == 0)
            {
                EndDialogue();
            }
        }
    
        Debug.Log($"Showing line: {line.text}, avatar: {line.avatar}, name: {line.name}");
    }

    private void ShowChoices(List<DialogueChoice> choices)
    {
        if (button1 == null || button2 == null || button3 == null)
        {
            Debug.LogError("Кнопки не найдены! Инициализируй кнопки снова!");
            InitializeButtonsByPosition();
            FindTextComponents();
        }
        
        if (choices == null || choices.Count == 0) return;
        
        dialogueText.text = ""; 
        titleName.text = "Samurai"; 
        imagePanda.SetActive(false);
        imageBoss.SetActive(false);
        imageSamurai.SetActive(true);

        choicePanel.SetActive(true);
        isWaitingForChoice = true;

        text1.text = choices.Count > 0 ? choices[0].text : "";
        text2.text = choices.Count > 1 ? choices[1].text : "";
        text3.text = choices.Count > 2 ? choices[2].text : "";

        button1.gameObject.SetActive(choices.Count > 0);
        button2.gameObject.SetActive(choices.Count > 1);
        button3.gameObject.SetActive(choices.Count > 2);

        button1.onClick.RemoveAllListeners();
        button2.onClick.RemoveAllListeners();
        button3.onClick.RemoveAllListeners();

        if (choices.Count > 0)
        {
            string nextId = choices[0].nextId;
            button1.onClick.AddListener(() => HandleChoice(1, nextId));
        }
        
        if (choices.Count > 1)
        {
            string nextId = choices[1].nextId;
            button2.onClick.AddListener(() => HandleChoice(2, nextId));
        }
        
        if (choices.Count > 2)
        {
            string nextId = choices[2].nextId;
            button3.onClick.AddListener(() => HandleChoice(3, nextId));
        }
    }
    
    public void ContinueDialogue(string dialogueId)
    {
        if (string.IsNullOrEmpty(dialogueId))
        {
            Debug.LogError("Dialogue ID is empty!");
            return;
        }
        
        if (dialogueMap == null || dialogueMap.Count == 0)
        {
            Debug.LogError("Dialogue map is not loaded!");
            return;
        }

        if (dialogueMap.TryGetValue(dialogueId, out DialogueLine nextLine))
        {
            dialogueQueue.Clear();
            dialogueQueue.Enqueue(nextLine);
            
            ShowDialogueUI();
            
            isTyping = false;
            isWaitingForChoice = false;
            skipTyping = false;
            DisplayNextSentence();
        
            Debug.Log($"Successfully continued dialogue with ID: {dialogueId}");
        }
        else
        {
            Debug.LogError($"Dialogue ID {dialogueId} not found in dialogue map!");
        }
    }

    private void HandleChoice(int buttonNumber, string nextId)
    {
        var clickedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        Debug.Log($"<color=magenta>Нажата: {clickedButton.name}</color> " +
                  $"({buttonNumber} в коде) → nextId: {nextId}");
        SelectChoice(nextId);
    }

    private void SelectChoice(string nextId)
    {
        if (string.IsNullOrEmpty(nextId))
        {
            Debug.LogError("NextId is empty!");
            EndDialogue();
            return;
        }
        
        if (nextId == "open_shop")
        {
            ShopManager shopManager = FindObjectOfType<ShopManager>();
            if (shopManager != null)
            {
                shopManager.OpenShop();
                HideDialogueUI();
            }
            return;
        }
        
        if (currentNPC is BossAI boss)
        {
            if (nextId == "correct_answer")
            {
                boss.riddleAnsweredCorrectly = true;
            }
            else if (nextId == "wrong_answer")
            {
                boss.riddleAnsweredCorrectly = false;
            }
        }

        if (!dialogueMap.ContainsKey(nextId))
        {
            Debug.LogError($"Диалог с ID {nextId} не найден!");
            EndDialogue();
            return;
        }
        
        Debug.Log($"Перешли к диалогу с ID: {nextId}");
    
        choicePanel.SetActive(false);
        isWaitingForChoice = false;

        dialogueQueue.Enqueue(dialogueMap[nextId]);
        DisplayNextSentence();
    }

    public void EndDialogue()
    {
        Debug.Log($"[DIALOGUE_DEBUG] === Начало EndDialogue ===");
        Debug.Log($"[DIALOGUE_DEBUG] currentNPC: {currentNPC} ({(currentNPC != null ? currentNPC.GetType().Name : "null")})");

        dialogueQueue.Clear();
        dialoguePanel.SetActive(false);
        bgImage.SetActive(false);
        choicePanel.SetActive(false);
        imagePanda.SetActive(false);
        imageSamurai.SetActive(false);
        imageBoss.SetActive(false);
        titleName.text = "";
        dialogueText.text = "";
        
        if (currentNPC != null)
        {
            Debug.Log($"[DIALOGUE_DEBUG] Пытаемся вызвать OnDialogueEnd у currentNPC...");
            if (currentNPC is IDialogueCallback callback)
            {
                Debug.Log($"[DIALOGUE_DEBUG] Вызов callback.OnDialogueEnd()");
                callback.OnDialogueEnd();
            }
            else
            {
                Debug.LogWarning($"[DIALOGUE_DEBUG] currentNPC не реализует IDialogueCallback!");
            }
        }
        else
        {
            Debug.Log($"[DIALOGUE_DEBUG] currentNPC null, ищем PandaNPC...");
            PandaNPC npcReference = FindObjectOfType<PandaNPC>(true); // Ищем даже неактивные
            if (npcReference != null)
            {
                Debug.Log($"[DIALOGUE_DEBUG] Найден PandaNPC: {npcReference.gameObject.name}, isActive: {npcReference.isActiveAndEnabled}");
                if (npcReference.isActiveAndEnabled)
                {
                    Debug.Log($"[DIALOGUE_DEBUG] Вызов OnDialogueEnd у PandaNPC");
                    npcReference.OnDialogueEnd();
                }
            }
            else
            {
                Debug.LogWarning($"[DIALOGUE_DEBUG] PandaNPC не найден на сцене!");
            }
        }

        currentNPC = null;
        Debug.Log($"[DIALOGUE_DEBUG] === Конец EndDialogue ===");
    }
    
    public void HideDialogueUI()
    {
        dialoguePanel.SetActive(false);
        bgImage.SetActive(false);
        choicePanel.SetActive(false);
        imagePanda.SetActive(false);
        imageSamurai.SetActive(false);
        imageBoss.SetActive(false);
        titleName.text = "";
        dialogueText.text = "";
    }

    public void ShowDialogueUI()
    {
        dialoguePanel.SetActive(true);
        bgImage.SetActive(true);
        dialogueText.text = "";
        titleName.text = "";
        imagePanda.SetActive(false);
        imageSamurai.SetActive(false);
        imageBoss.SetActive(false);
        choicePanel.SetActive(false);
    }

    private void Update()
    {
        if (!dialoguePanel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                skipTyping = true; 
            }
            else if (!isWaitingForChoice)
            {
                DisplayNextSentence();
            }
        }
    }
}