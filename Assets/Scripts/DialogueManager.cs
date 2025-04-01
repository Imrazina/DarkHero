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

    [Header("Choice UI")] 
    public GameObject choicePanel;
    private Button button1; // Левый
    private Button button2; // Центральный
    private Button button3; // Правый
    private TextMeshProUGUI text1;
    private TextMeshProUGUI text2;
    private TextMeshProUGUI text3;

    private Queue<DialogueLine> dialogueQueue;
    private DialogueLine currentLine;
    private bool isWaitingForChoice = false;
    private bool isTyping = false;
    private Dictionary<string, DialogueLine> dialogueMap;

    private void Awake()
    {
        dialogueQueue = new Queue<DialogueLine>();
        dialogueMap = new Dictionary<string, DialogueLine>();
        
        InitializeButtonsByPosition();
        FindTextComponents();
    }

    private void InitializeButtonsByPosition()
    {
        // Получаем все кнопки и сортируем по имени
        button1 = GameObject.Find("Button1").GetComponent<Button>();
        button2 = GameObject.Find("Button2").GetComponent<Button>();
        button3 = GameObject.Find("Button3").GetComponent<Button>();

        // Визуальная маркировка (можно убрать после отладки)
        button1.image.color = new Color(1, 0.5f, 0.5f); // Светло-красный
        button2.image.color = new Color(0.5f, 1, 0.5f); // Светло-зеленый
        button3.image.color = new Color(0.5f, 0.5f, 1); // Светло-синий

        Debug.Log($"Фиксированный порядок:\n" +
                  $"1. {button1.name} (цвет: красный)\n" +
                  $"2. {button2.name} (цвет: зеленый)\n" +
                  $"3. {button3.name} (цвет: синий)");
    }

    private void FindTextComponents()
    {
        text1 = button1?.GetComponentInChildren<TextMeshProUGUI>(true);
        text2 = button2?.GetComponentInChildren<TextMeshProUGUI>(true);
        text3 = button3?.GetComponentInChildren<TextMeshProUGUI>(true);
        
        Debug.Log($"<color=cyan>Текстовые компоненты:</color>\n" +
                 $"1. {(text1 != null ? text1.text : "NULL")}\n" +
                 $"2. {(text2 != null ? text2.text : "NULL")}\n" +
                 $"3. {(text3 != null ? text3.text : "NULL")}");
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
        bgImage.SetActive(false);
        choicePanel.SetActive(false);
        imagePanda.SetActive(false);
        imageSamurai.SetActive(false);
    }

    public void StartDialogue(string fileName)
    {
        dialogueQueue.Clear();
        dialogueMap.Clear();

        DialogueData data = DialogueLoader.LoadDialogue(fileName);
        if (data == null) return;

        foreach (var line in data.dialogues)
        {
            if (!string.IsNullOrEmpty(line.id))
            {
                dialogueMap[line.id] = line;
            }
        }

        if (dialogueMap.TryGetValue("1", out DialogueLine firstLine))
        {
            dialogueQueue.Enqueue(firstLine);
            dialoguePanel.SetActive(true);
            bgImage.SetActive(true);
            DisplayNextSentence();
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

    private IEnumerator TypeSentence(DialogueLine line)
    {
        isTyping = true;
        dialogueText.text = "";
        titleName.text = line.name;

        imagePanda.SetActive(line.avatar == "ImagePanda");
        imageSamurai.SetActive(line.avatar == "ImageSamurai");

        foreach (char letter in line.text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;

        if (line.choices != null && line.choices.Count > 0)
        {
            isWaitingForChoice = true;
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E)); 
            ShowChoices(line.choices);
        }
    }

    private void ShowChoices(List<DialogueChoice> choices)
    {
        if (choices == null || choices.Count == 0) return;
        
        dialogueText.text = ""; 
        titleName.text = "Samurai"; 
        imagePanda.SetActive(false);
        imageSamurai.SetActive(true);

        // Визуальная маркировка кнопок
        ColorButton(button1, Color.red);
        ColorButton(button2, Color.green);
        ColorButton(button3, Color.blue);

        Debug.Log($"<color=yellow>Соответствие кнопок:</color>\n" +
                  $"Button1 ({button1.transform.position.x}) → {choices[0].nextId}\n" +
                  $"Button2 ({button2.transform.position.x}) → {choices[1].nextId}\n" +
                  $"Button3 ({button3.transform.position.x}) → {choices[2].nextId}");

        choicePanel.SetActive(true);
        isWaitingForChoice = true;

        // Установка текста
        text1.text = choices.Count > 0 ? choices[0].text : "";
        text2.text = choices.Count > 1 ? choices[1].text : "";
        text3.text = choices.Count > 2 ? choices[2].text : "";

        // Активация кнопок
        button1.gameObject.SetActive(choices.Count > 0);
        button2.gameObject.SetActive(choices.Count > 1);
        button3.gameObject.SetActive(choices.Count > 2);

        // Очистка старых обработчиков
        button1.onClick.RemoveAllListeners();
        button2.onClick.RemoveAllListeners();
        button3.onClick.RemoveAllListeners();

        // Привязка новых обработчиков с проверкой
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

        Debug.Log($"<color=yellow>Варианты выбора:</color>\n" +
                 $"1. {text1.text} → {choices[0].nextId}\n" +
                 $"2. {text2.text} → {choices[1]?.nextId}\n" +
                 $"3. {text3.text} → {choices[2]?.nextId}");
    }

    private void ColorButton(Button button, Color color)
    {
        if (button != null && button.image != null)
        {
            button.image.color = color;
        }
    }

    private void HandleChoice(int buttonNumber, string nextId)
    {
        var clickedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        Debug.Log($"<color=magenta>Нажата: {clickedButton.name}</color> " +
                  $"({buttonNumber} в коде) → nextId: {nextId}");
        Debug.Log($"<color=magenta>Нажата кнопка {buttonNumber}</color> ({(buttonNumber == 1 ? "Красная" : buttonNumber == 2 ? "Зеленая" : "Синяя")}), nextId: {nextId}");
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

        if (!dialogueMap.ContainsKey(nextId))
        {
            Debug.LogError($"Диалог с ID {nextId} не найден!");
            EndDialogue();
            return;
        }

        choicePanel.SetActive(false);
        isWaitingForChoice = false;

        dialogueQueue.Clear();
        dialogueQueue.Enqueue(dialogueMap[nextId]);
        DisplayNextSentence();
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
    }

    private void Update()
    {
        if (dialoguePanel.activeSelf && Input.GetKeyDown(KeyCode.E) && !isTyping && !isWaitingForChoice)
        {
            DisplayNextSentence();
        }
    }
}