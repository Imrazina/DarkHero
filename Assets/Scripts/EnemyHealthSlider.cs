using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthSlider : MonoBehaviour
{
    public Slider healthSlider; 
    public Transform enemy;     
    public Vector3 offset;     

    void Update()
    {
        Vector3 sliderPos = Camera.main.WorldToScreenPoint(enemy.position + offset);
        
        healthSlider.transform.position = sliderPos;
    }

    public void UpdateHealthSlider(float healthPercent)
    {
        healthSlider.value = healthPercent;
    }
}
