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
        
        restartGameButton.onClick.AddListener(() => {
            Debug.Log("RESTART GAME clicked");
            NewGame(); // Используем тот же функционал, что и New Game
        });
        
        loadLastSaveButton.onClick.AddListener(() => {
            Debug.Log("LOAD GAME clicked");
    
            GameStateManager.Instance.LoadGame();
    
            var player = FindObjectOfType<Character>();
            if (player != null)
            {
                player.Resurrect();
                player.transform.position = GameStateManager.Instance.CurrentState.playerPosition;
            }
    
            // Восстанавливаем камеру
            if (GameStateManager.Instance.CurrentState.cameraState != null)
            {
                Camera.main.transform.position = GameStateManager.Instance.CurrentState.cameraState.position;
            }
    
            gameOverPanel.SetActive(false);
            Time.timeScale = 1f;
        });
        
        exitToMenuFromGameOverButton.onClick.AddListener(() => {
            Time.timeScale = 0f;
            mainMenuPanel.SetActive(true);
            gameOverPanel.SetActive(false);
        });

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
    }

    public void NewGame()
    {
        Debug.Log("Starting NEW GAME with full reset");
        
        GameStateManager.Instance.FullReset();
        
        var worldSwitcher = FindObjectOfType<WorldSwitcher>();
        if (worldSwitcher != null)
        {
            worldSwitcher.ResetWorlds();
        }
        
        isStartingNewGame = true;

        mainMenuPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
 
        StartCoroutine(StartGameAfterReset());
    }

    private IEnumerator StartGameAfterReset()
    {
        
        var levelStart = FindObjectOfType<LevelStart>();
        if (levelStart != null)
        {
            levelStart.StartIntro();
        }
        else
        {
            Debug.LogError("LevelStart не найден!");
        }
    
        Time.timeScale = 1f;
        yield return null;
    }

    public void ContinueGame()
    {
        GameStateManager.Instance.LoadGame();
        
        var player = FindObjectOfType<Character>();
        if (player != null)
        {
            player.transform.position = GameStateManager.Instance.CurrentState.playerPosition;
        }
        
        if (GameStateManager.Instance.CurrentState.lastCameraPosition != Vector3.zero)
        {
            Camera.main.transform.position = GameStateManager.Instance.CurrentState.lastCameraPosition;
        }
        
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
