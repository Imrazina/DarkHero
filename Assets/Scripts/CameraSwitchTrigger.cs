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

        Vector3 targetPos = other.transform.position.x > transform.position.x 
            ? cameraPositionBackward.position 
            : cameraPositionForward.position;

        MoveCamera(targetPos);
        StartCoroutine(DelayedSaveCameraPosition(targetPos, 0.75f));
    }

    private void MoveCamera(Vector3 targetPos)
    {
        Camera.main.transform.position = new Vector3(
            targetPos.x,
            targetPos.y,
            Camera.main.transform.position.z
        );
    }

    private IEnumerator DelayedSaveCameraPosition(Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay);
        Vector3 fixedPosition = new Vector3(position.x, position.y, -10f);

        GameStateManager.Instance.CurrentState.lastCameraPosition = position;
        GameStateManager.Instance.CurrentState.cameraState = new CameraState
        {
            position = fixedPosition,
            isOrthographic = Camera.main.orthographic
        };
        GameStateManager.Instance.SaveGame();
    }
}