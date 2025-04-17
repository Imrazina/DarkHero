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

        Vector3 targetPos = playerX > triggerX ? 
            cameraPositionBackward.position : 
            cameraPositionForward.position;

        MoveCamera(targetPos);
        GameStateManager.Instance.CurrentState.lastCameraPosition = targetPos;
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