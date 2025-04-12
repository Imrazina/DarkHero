using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_HealthDisplay : MonoBehaviour
{
    public Image heart1;
    public Image heart2;
    public Image heart3;

    private int maxHealth = 300;

    public void UpdateHearts(int currentHealth)
    {
        float normalized = Mathf.Clamp01(currentHealth / (float)maxHealth);

        // Каждое сердце — 1/3
        UpdateHeart(heart1, normalized, 0f);
        UpdateHeart(heart2, normalized, 1f / 3f);
        UpdateHeart(heart3, normalized, 2f / 3f);
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