using System.Collections;
using UnityEngine;

public class CameraSwitchTrigger : MonoBehaviour
{
    public Transform cameraTargetForward;
    public Transform cameraTargetBackward;

    private Vector3 lastPlayerPosition;
    
    public float deadZone = 0.1f; 
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        lastPlayerPosition = other.transform.position;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Vector3 currentPos = other.transform.position;
        Vector3 direction = currentPos - lastPlayerPosition;
        
        if (direction.magnitude < deadZone) return;

        Transform target;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            target = direction.x > 0 ? cameraTargetForward : cameraTargetBackward;
        }
        else
        {
            target = direction.y > 0 ? cameraTargetForward : cameraTargetBackward;
        }

        if (target != null)
        {
            MoveCamera(target.position);
            StartCoroutine(DelayedSaveCameraPosition(target.position, 0.75f));
        }

        lastPlayerPosition = currentPos;
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

        GameStateManager.Instance.CurrentState.lastCameraPosition = fixedPosition;
        GameStateManager.Instance.CurrentState.cameraState = new CameraState 
        {
            position = fixedPosition,
            isOrthographic = Camera.main.orthographic
        };
        //       GameStateManager.Instance.SaveGame();
    }
}