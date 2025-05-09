using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_HealthDisplay : MonoBehaviour
{
    public Image heart1;
    public Image heart2;
    public Image heart3;

    [SerializeField] private int maxHealth = 500;
    private Character playerCharacter;
    
    private void Start()
    {
        if (GameStateManager.Instance != null)
        {
            UpdateHearts(GameStateManager.Instance.CurrentState.currentHealth);
        }
        else
        {
            var player = FindObjectOfType<Character>();
            if (player != null) 
                UpdateHearts(player.currentHealth);
        }
    }
    
    public void UpdateHearts(int currentHealth)
    {
        if (heart1 == null || heart2 == null || heart3 == null) return;
    
        float normalized = Mathf.Clamp01((float)currentHealth / maxHealth);
        heart1.fillAmount = Mathf.Clamp01(normalized * 3f);
        heart2.fillAmount = Mathf.Clamp01((normalized - 0.33f) * 3f);
        heart3.fillAmount = Mathf.Clamp01((normalized - 0.66f) * 3f);
    }

    private void UpdateHeart(Image heart, float normalizedHealth, float heartStart)
    {
        float fill = Mathf.Clamp01((normalizedHealth - heartStart) * 3f);
        StartCoroutine(SmoothFill(heart, fill));
    }
    
    IEnumerator SmoothFill(Image image, float targetFill)
    {
        float startFill = image.fillAmount;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            image.fillAmount = Mathf.Lerp(startFill, targetFill, t);
            yield return null;
        }
        image.fillAmount = targetFill;
    }
}