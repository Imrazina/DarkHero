using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;

    public TMP_Text coinsText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddCoins(int amount)
    {
        GameStateManager.Instance.CurrentState.totalCoins += amount;
        UpdateUI();
    }

    public void SetCoins(int amount)
    {
        GameStateManager.Instance.CurrentState.totalCoins = amount;
        UpdateUI();
    }

    public int GetCoins()
    {
        return GameStateManager.Instance.CurrentState.totalCoins;
    }

    public void UpdateUI()
    {
        if (coinsText != null)
        {
            coinsText.text = GameStateManager.Instance.CurrentState.totalCoins.ToString();
        }
    }
}
