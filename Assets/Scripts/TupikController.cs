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

    public bool openExit = false;
    public bool closeEntrance = false;

    private Vector3 exitTargetPosition;
    private Vector3 entranceStartPosition;
    private Vector3 exitStartPosition;
    private Vector3 entranceTargetPosition;


    void Start()
    {
        if (exitPlank != null)
        {
            exitStartPosition = exitPlank.position;
            exitTargetPosition = exitStartPosition + Vector3.up * exitMoveDistance;
        }

        if (entrancePlank != null)
        {
            entranceStartPosition = entrancePlank.position;
            entranceTargetPosition = entranceStartPosition + Vector3.down * entranceMoveDistance;
        }
        LoadPlanksState();
    }

    public void LoadPlanksState()
    {
        if (GameStateManager.Instance == null) return;

        var state = GameStateManager.Instance.CurrentState;
        if (state.isBossDead)
        {
            if (state.isExitPlankOpen && exitPlank != null)
            {
                exitPlank.position = exitTargetPosition;
                openExit = true;
            }

            if (state.isEntrancePlankClosed && entrancePlank != null)
            {
                entrancePlank.position = entranceTargetPosition;
                closeEntrance = true;
            }
        }
        else
        {
            if (exitPlank != null) exitPlank.position = exitStartPosition;
            if (entrancePlank != null) entrancePlank.position = entranceStartPosition;
            openExit = false;
            closeEntrance = false;
        }
    }

    public void SavePlanksStateIfBossDead()
    {
        if (GameStateManager.Instance == null) return;

        var state = GameStateManager.Instance.CurrentState;
        state.isExitPlankOpen = openExit;
        state.isEntrancePlankClosed = closeEntrance;
        
        Character player = FindObjectOfType<Character>();
        if (player != null)
        {
            state.playerPosition = player.transform.position;
            state.lastCameraPosition = Camera.main.transform.position;
        }
        
        GameStateManager.Instance.SaveGame();
    }

    public void CloseEntrance()
    {
        if (closeEntrance) return;
        
        closeEntrance = true;
    }

    public void OpenExit()
    {
        if (openExit) return;
        
        openExit = true;
    }

    void Update()
    {
        if (openExit && exitPlank != null)
        {
            exitPlank.position = Vector3.MoveTowards(
                exitPlank.position, 
                exitTargetPosition, 
                speed * Time.deltaTime
            );
        }

        if (closeEntrance && entrancePlank != null)
        {
            entrancePlank.position = Vector3.MoveTowards(
                entrancePlank.position, 
                entranceTargetPosition, 
                speed * Time.deltaTime
            );
        }
    }
}