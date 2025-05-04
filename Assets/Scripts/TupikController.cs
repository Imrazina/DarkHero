using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TupikController : MonoBehaviour
{
    [Header("Plank to open after boss death")]
    public Transform exitPlank;
    public float exitMoveDistance = 2f;

    [Header("Plank to close when boss fight starts")]
    public Transform entrancePlank;
    public float entranceMoveDistance = 2f;

    public float speed = 2f;

    private bool openExit = false;
    private bool closeEntrance = false;

    private Vector3 exitTargetPosition;
    private Vector3 entranceStartPosition;

    void Start()
    {
        if (exitPlank != null)
            exitTargetPosition = exitPlank.position + Vector3.up * exitMoveDistance;

        if (entrancePlank != null)
            entranceStartPosition = entrancePlank.position;
    }

    void Update()
    {
        if (openExit && exitPlank != null)
        {
            exitPlank.position = Vector3.MoveTowards(exitPlank.position, exitTargetPosition, speed * Time.deltaTime);
        }

        if (closeEntrance && entrancePlank != null)
        {
            Vector3 target = entranceStartPosition + Vector3.down * entranceMoveDistance;
            entrancePlank.position = Vector3.MoveTowards(entrancePlank.position, target, speed * Time.deltaTime);
        }
    }

    public void CloseEntrance()
    {
        closeEntrance = true;
    }

    public void OpenExit()
    {
        openExit = true;
    }
}
