using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
  public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject pausePanel;
    public GameObject settingsPanel;

    [Header("Buttons")]
    public Button continueButton;

    private bool isGamePaused = false;
    
    private bool openedFromPause = false;
    
    private static bool isStartingNewGame = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        string saveFilePath = Application.persistentDataPath + "/savegame.json";
        continueButton.interactable = File.Exists(saveFilePath);

        if (isStartingNewGame)
        {
            mainMenuPanel.SetActive(false);
            pausePanel.SetActive(false);
            settingsPanel.SetActive(false);
            Time.timeScale = 1f;

            isStartingNewGame = false; // сбрасываем флаг
            return;
        }

        // обычная логика
        Time.timeScale = 0f;
        mainMenuPanel.SetActive(true);
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !mainMenuPanel.activeSelf)
        {
            TogglePause();
        }
    }

    public void NewGame()
    {
        Debug.Log("NewGame() вызван!");
        isStartingNewGame = true;

        GameStateManager.Instance.ResetGame();

        Time.timeScale = 1f;
        isGamePaused = false;

        StartCoroutine(StartIntroAfterFrame());
    }

    private IEnumerator StartIntroAfterFrame()
    {
        yield return null; // ждём 1 кадр, чтобы всё прогрузилось

        LevelStart levelStart = FindObjectOfType<LevelStart>();
        if (levelStart != null)
        {
            Debug.Log("LevelStart найден, запускаю интро!");
            mainMenuPanel.SetActive(false);
            isGamePaused = false;
            levelStart.StartIntro();
        }
        else
        {
            Debug.LogWarning("LevelStart не найден!");
        }
    }

    public void ContinueGame()
    {
        GameStateManager.Instance.LoadGame();

        mainMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isGamePaused = false;
        
        FindObjectOfType<ScreenFade>()?.FadeIn(1f);

        if (!GameStateManager.Instance.CurrentState.hasPlayedIntro)
        {
            Debug.Log("Intro hasn't been played yet — launching it.");
            FindObjectOfType<LevelStart>()?.StartIntro();
        }
        else
        {
            Debug.Log("Intro already played — skipping it.");
        }
    }
    
    public void SaveGame()
    {
        GameStateManager.Instance.SaveGame();
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }

    // -------- Пауза --------

    public void TogglePause()
    {
        isGamePaused = !isGamePaused;

        if (isGamePaused)
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void ResumeGame()
    {
        isGamePaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ExitToMenu()
    {
        Time.timeScale = 0f;
        mainMenuPanel.SetActive(true);
        pausePanel.SetActive(false);
    }

    // -------- Настройки --------

    public void OpenSettings()
    {
        openedFromPause = pausePanel.activeSelf;

        settingsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);

        if (openedFromPause)
            pausePanel.SetActive(true);
        else
            mainMenuPanel.SetActive(true);
    }
}
