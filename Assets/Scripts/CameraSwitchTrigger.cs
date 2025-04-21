using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitchTrigger : MonoBehaviour
{
   public Transform cameraTargetUp;
    public Transform cameraTargetDown;
    public Transform cameraTargetLeft;
    public Transform cameraTargetRight;

    private void Awake()
    {
        if (cameraTargetUp == null) cameraTargetUp = transform.Find("CameraTarget_Up");
        if (cameraTargetDown == null) cameraTargetDown = transform.Find("CameraTarget_Down");
        if (cameraTargetLeft == null) cameraTargetLeft = transform.Find("CameraTarget_Left");
        if (cameraTargetRight == null) cameraTargetRight = transform.Find("CameraTarget_Right");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
    
        bool isPlayerOnRight = other.transform.position.x > transform.position.x;
        bool isPlayerOnTop = other.transform.position.y > transform.position.y;

        Transform target = null;

         if (Mathf.Abs(other.transform.position.x - transform.position.x) > 
            Mathf.Abs(other.transform.position.y - transform.position.y))
        {
             target = isPlayerOnRight ? cameraTargetRight : cameraTargetLeft;
        }

        else
        {
            target = isPlayerOnTop ? cameraTargetUp : cameraTargetDown;
        }

        if (target != null)
        {
            MoveCamera(target.position);
            StartCoroutine(DelayedSaveCameraPosition(target.position, 0.75f));
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
        GameStateManager.Instance.SaveGame();
    }
}