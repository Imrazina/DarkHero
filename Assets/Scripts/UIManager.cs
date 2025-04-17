using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
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

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public Button restartGameButton;
    public Button loadLastSaveButton;
    public Button exitToMenuFromGameOverButton;

    private bool isGamePaused = false;
    private bool openedFromPause = false;
    private static bool isStartingNewGame = false;

    private void Awake()
    {
        Debug.Log("Awake()");
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
        Debug.Log("Start()");
        string saveFilePath = Application.persistentDataPath + "/savegame.json";
        continueButton.interactable = File.Exists(saveFilePath);

        if (isStartingNewGame)
        {
            Debug.Log("Starting new game setup.");
            mainMenuPanel.SetActive(false);
            pausePanel.SetActive(false);
            settingsPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            Time.timeScale = 1f;
            isStartingNewGame = false; 
            return;
        }

        Time.timeScale = 0f;
        mainMenuPanel.SetActive(true);
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !mainMenuPanel.activeSelf)
        {
            Debug.Log("Escape pressed — toggling pause.");
            TogglePause();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("G pressed — showing Game Over.");
            ShowGameOver();
        }
    }

    public void NewGame()
    {
        Debug.Log("NewGame()");
        isStartingNewGame = true;
        GameStateManager.Instance.ResetGame();
        Time.timeScale = 1f;
        isGamePaused = false;
        StartCoroutine(StartIntroAfterFrame());
    }

    private IEnumerator StartIntroAfterFrame()
    {
        Debug.Log("StartIntroAfterFrame() started.");
        yield return null;

        LevelStart levelStart = FindObjectOfType<LevelStart>();
        if (levelStart != null)
        {
            Debug.Log("LevelStart найден — запускаю интро.");
            mainMenuPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            isGamePaused = false;
            levelStart.StartIntro();
        }
        else
        {
            Debug.LogWarning("LevelStart не найден.");
        }
    }

    public void ContinueGame()
    {
        GameStateManager.Instance.LoadGame();
    
        // Применяем все загруженные состояния
        var player = FindObjectOfType<Character>();
        if (player != null)
        {
            player.transform.position = GameStateManager.Instance.CurrentState.playerPosition;
        }

        // Восстанавливаем камеру
        if (GameStateManager.Instance.CurrentState.lastCameraPosition != Vector3.zero)
        {
            Camera.main.transform.position = GameStateManager.Instance.CurrentState.lastCameraPosition;
        }

        // Управление UI
        mainMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void SaveGame()
    {
        Debug.Log("SaveGame()");
        GameStateManager.Instance.SaveGame();
    }

    public void QuitGame()
    {
        Debug.Log("QuitGame()");
        Application.Quit();
        Debug.Log("Приложение завершено.");
    }

    public void TogglePause()
    {
        Debug.Log("TogglePause()");
        isGamePaused = !isGamePaused;

        if (isGamePaused)
        {
            Debug.Log("Игра на паузе.");
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            Debug.Log("Игра возобновлена.");
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void ResumeGame()
    {
        Debug.Log("ResumeGame()");
        isGamePaused = false;
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ExitToMenu()
    {
        Debug.Log("ExitToMenu()");
        Time.timeScale = 0f;
        mainMenuPanel.SetActive(true);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        Debug.Log("OpenSettings()");
        openedFromPause = pausePanel.activeSelf;

        settingsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void CloseSettings()
    {
        Debug.Log("CloseSettings()");
        settingsPanel.SetActive(false);

        if (openedFromPause)
        {
            Debug.Log("Возвращаюсь в паузу.");
            pausePanel.SetActive(true);
        }
        else
        {
            Debug.Log("Возвращаюсь в главное меню.");
            mainMenuPanel.SetActive(true);
        }
    }

    public void ShowGameOver()
    {
        Debug.Log("ShowGameOver()");
        Debug.Log($"Кнопка restartGameButton: {restartGameButton != null}");
        Debug.Log($"Кнопка interactable: {restartGameButton.interactable}");
        gameOverPanel.SetActive(true);
        
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(restartGameButton.gameObject);
        }
        else
        {
            Debug.LogError("EventSystem не найден!");
        }
    }

    public void GameOverWithDelay(float delay)
    {
        Debug.Log($"GameOverWithDelay({delay})");
        StartCoroutine(ShowGameOverAfterDelay(delay));
    }

    private IEnumerator ShowGameOverAfterDelay(float delay)
    {
        Debug.Log($"Ожидаю {delay} секунд перед показом Game Over...");
        yield return new WaitForSecondsRealtime(delay);
        ShowGameOver();
    }
}
