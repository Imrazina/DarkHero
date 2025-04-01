using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitchTrigger : MonoBehaviour
{
    public Transform newCameraPosition; // Точка, куда переместится камера

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Проверяем, это ли игрок
        {
            Camera.main.transform.position = new Vector3(
                newCameraPosition.position.x, 
                newCameraPosition.position.y, 
                Camera.main.transform.position.z
            );
        }
    }
}
