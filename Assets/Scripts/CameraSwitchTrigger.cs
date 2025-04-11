using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitchTrigger : MonoBehaviour
{
    public Transform cameraPositionForward;
    public Transform cameraPositionBackward;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        float playerX = other.transform.position.x;
        float triggerX = transform.position.x;

        if (playerX > triggerX)
        {
            MoveCamera(cameraPositionBackward.position);
        }
        else
        {
            MoveCamera(cameraPositionForward.position);
        }
    }

    private void MoveCamera(Vector3 targetPos)
    {
        Camera.main.transform.position = new Vector3(
            targetPos.x,
            targetPos.y,
            Camera.main.transform.position.z
        );
    }
}