using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [System.Serializable]
    public class PrefabDirections
    {
        public GameObject prefab;
        public Direction[] availableDirections;
    }

    public PrefabDirections[] levelPrefabs;
    public Transform spawnPoint;
    public int horizontalSegments = 7;
    public int maxVerticalDepth = 10;
    public float segmentWidth = 18f;
    public float segmentHeight = 11f;

    private Vector2Int currentGridPos;
    private Direction currentDirection;
    private readonly HashSet<Vector2Int> occupiedPositions = new();
    private readonly Dictionary<Vector2Int, GameObject> placedSegments = new();
    private List<Vector2Int> mainLinePositions = new();

    private void Start()
    {
        GenerateLevel();
    }

    private void GenerateLevel()
    {
        InitializeStartSegment();
        GenerateMainLine();
        GenerateAllBranches();
        FillDeadEnds(); 
        GenerateMissingCameraTriggers();
    }

    private void InitializeStartSegment()
    {
        currentGridPos = Vector2Int.zero;

        GameObject startSegment;

        if (spawnPoint.childCount > 0)
        {
            startSegment = spawnPoint.GetChild(0).gameObject;
        }
        else
        {
            var defaultPrefab = levelPrefabs[0].prefab;
            startSegment = Instantiate(defaultPrefab, spawnPoint.position, Quaternion.identity, transform);
        }

        placedSegments[currentGridPos] = startSegment;
        occupiedPositions.Add(currentGridPos);
        
        if (startSegment.GetComponent<SegmentInfo>() == null)
        {
            var info = startSegment.AddComponent<SegmentInfo>();
            info.directions = new[] { Direction.Right }; 
            info.GridPosition = currentGridPos;
        }

        currentDirection = Direction.Right;

        Transform cameraStartTarget = startSegment.transform.Find("CameraTarget_Left");
        if (cameraStartTarget != null)
        {
            Camera.main.transform.position = new Vector3(
                cameraStartTarget.position.x,
                cameraStartTarget.position.y,
                Camera.main.transform.position.z
            );
        }
        else
        {
            Debug.LogWarning("CameraStartTarget not found in start segment!");
        }
    }

    private void GenerateMainLine()
    {
        for (int i = 0; i < horizontalSegments; i++)
        {
            Vector2Int prevPos = currentGridPos;
            if (!GenerateSegment(prevPos, currentDirection, isMainLine: true, mainLine: mainLinePositions))
            {
                // Попытка найти альтернативный префаб с выходом направо
                var fallbackPrefab = FindFallbackMainLinePrefab(currentDirection);
                if (fallbackPrefab != null)
                {
                    SpawnFallbackSegment(prevPos, fallbackPrefab);
                    continue;
                }
                Debug.LogWarning($"Main line generation failed at step {i} from {prevPos}");
                break;
            }
        }
    }
    
    private PrefabDirections FindFallbackMainLinePrefab(Direction entryDirection)
    {
        Direction requiredEntry = GetOppositeDirection(entryDirection);
        return System.Array.Find(levelPrefabs, prefab =>
            System.Array.Exists(prefab.availableDirections, dir => dir == requiredEntry) &&
            System.Array.Exists(prefab.availableDirections, dir => dir == Direction.Right)
        );
    }

    private void SpawnFallbackSegment(Vector2Int gridPos, PrefabDirections prefabDir)
    {
        Vector3 spawnPos = CalculateWorldPosition(gridPos);
        GameObject segment = Instantiate(prefabDir.prefab, spawnPos, Quaternion.identity, transform);

        var info = segment.AddComponent<SegmentInfo>();
        info.directions = prefabDir.availableDirections;
        info.GridPosition = gridPos;

        placedSegments[gridPos] = segment;
        occupiedPositions.Add(gridPos);
        mainLinePositions.Add(gridPos);
        currentDirection = Direction.Right;
    }

    private void GenerateAllBranches()
    {
        List<Vector2Int> positionsToCheck = new List<Vector2Int>(placedSegments.Keys);

        foreach (var pos in positionsToCheck)
        {
            CheckAndGenerateBranch(pos, Direction.Up);
            CheckAndGenerateBranch(pos, Direction.Down);
        }
    }
    
    private void FillDeadEnds()
    {
        Vector2Int lastMainLinePos = mainLinePositions.Count > 0 ? mainLinePositions[mainLinePositions.Count - 1] : Vector2Int.zero;

        List<Vector2Int> positionsToCheck = new List<Vector2Int>(placedSegments.Keys);

        foreach (var pos in positionsToCheck)
        {
            foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
            {
                if (!HasExitAtPosition(pos, dir)) continue;

                Vector2Int neighborPos = pos + DirectionToOffset(dir);
                if (occupiedPositions.Contains(neighborPos)) continue;
                
                if (mainLinePositions.Contains(pos))
                {
                    continue;
                }

                var deadEnd = GetDeadEndPrefab(dir);
                if (deadEnd == null)
                {
                    Debug.LogWarning($"No dead-end prefab found for direction {dir}");
                    continue;
                }

                Vector3 spawnPos = CalculateWorldPosition(neighborPos);
                GameObject newSegment = Instantiate(deadEnd.prefab, spawnPos, Quaternion.identity, transform);

                SegmentInfo newInfo = newSegment.AddComponent<SegmentInfo>();
                newInfo.directions = new[] { GetOppositeDirection(dir) };

                placedSegments[neighborPos] = newSegment;
                occupiedPositions.Add(neighborPos);
            }
        }
    }

    private PrefabDirections GetDeadEndPrefab(Direction entryDirection)
    {
        Direction requiredExit = GetOppositeDirection(entryDirection);
        return System.Array.Find(levelPrefabs, prefab =>
            prefab.availableDirections.Length == 1 &&
            prefab.availableDirections[0] == requiredExit
        );
    }
    
    private void CheckAndGenerateBranch(Vector2Int startPos, Direction direction)
    {
        if (!HasExitAtPosition(startPos, direction)) return;

        currentGridPos = startPos;
        currentDirection = direction;
        int depth = 0;

        while (depth < maxVerticalDepth)
        {
            if (!GenerateSegment(currentGridPos, currentDirection, isMainLine: false))
                break;

            if (!HasExitAtPosition(currentGridPos, direction))
                break;

            depth++;
        }
    }

    private bool GenerateSegment(Vector2Int fromPos, Direction direction, bool isMainLine, List<Vector2Int> mainLine = null)
    {
        Vector2Int nextPos = fromPos + DirectionToOffset(direction);

        if (occupiedPositions.Contains(nextPos)) return false;

        var validPrefabs = GetValidPrefabs(direction, isMainLine);

        validPrefabs = validPrefabs.FindAll(prefab =>
        {
            if (!System.Array.Exists(prefab.availableDirections, dir => dir == GetOppositeDirection(direction)))
                return false;

            foreach (var exitDir in prefab.availableDirections)
            {
                if (exitDir == GetOppositeDirection(direction)) continue;

                Vector2Int targetPos = nextPos + DirectionToOffset(exitDir);
                if (occupiedPositions.Contains(targetPos))
                    return false;
            }

            return true;
        });

        if (validPrefabs.Count == 0) return false;

        var selected = validPrefabs[Random.Range(0, validPrefabs.Count)];
        var spawnPos = CalculateWorldPosition(nextPos);

        GameObject segment = Instantiate(selected.prefab, spawnPos, Quaternion.identity, transform);

        var info = segment.AddComponent<SegmentInfo>();
        info.directions = selected.availableDirections;
        info.GridPosition = nextPos; 

        placedSegments[nextPos] = segment;
        occupiedPositions.Add(nextPos);
        
        if (isMainLine && mainLine != null)
            mainLine.Add(nextPos);

        CreateCameraTrigger(fromPos, nextPos, spawnPos);

        currentGridPos = nextPos;

        if (isMainLine)
        {
            currentDirection = Direction.Right;
        }
        else
        {
            var exits = GetAvailableExits(selected, direction);
            currentDirection = exits.Count > 0 ? exits[Random.Range(0, exits.Count)] : direction;
        }
        Debug.Log($"[Segment] Placed segment at {nextPos} from {fromPos} going {direction}");

        return true;
    }

    private List<PrefabDirections> GetValidPrefabs(Direction entryDirection, bool isMainLine = false)
    {
        var validPrefabs = new List<PrefabDirections>();
        Direction requiredEntry = GetOppositeDirection(entryDirection);

        foreach (var prefabDir in levelPrefabs)
        {
            if (System.Array.Exists(prefabDir.availableDirections, dir => dir == requiredEntry))
            {
                if (isMainLine && !System.Array.Exists(prefabDir.availableDirections, dir => dir == Direction.Right))
                {
                    continue;
                }
                validPrefabs.Add(prefabDir);
            }
        }

        return validPrefabs;
    }

    private bool HasExitAtPosition(Vector2Int gridPos, Direction direction)
    {
        if (!placedSegments.TryGetValue(gridPos, out var segment)) return false;

        var info = segment.GetComponent<SegmentInfo>();
        if (info == null) return false;

        return System.Array.Exists(info.directions, d => d == direction);
    }

    private List<Direction> GetAvailableExits(PrefabDirections prefabDir, Direction entryDirection)
    {
        var exits = new List<Direction>();
        Direction forbiddenDirection = GetOppositeDirection(entryDirection);

        foreach (var dir in prefabDir.availableDirections)
        {
            if (dir != forbiddenDirection)
                exits.Add(dir);
        }

        return exits;
    }

    private Vector2Int DirectionToOffset(Direction dir)
    {
        return dir switch
        {
            Direction.Right => Vector2Int.right,
            Direction.Left => Vector2Int.left,
            Direction.Up => Vector2Int.up,
            Direction.Down => Vector2Int.down,
            _ => Vector2Int.zero
        };
    }

    private Vector3 CalculateWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(
            spawnPoint.position.x + gridPos.x * segmentWidth,
            spawnPoint.position.y + gridPos.y * segmentHeight,
            spawnPoint.position.z
        );
    }

    private Direction GetOppositeDirection(Direction dir)
    {
        return dir switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => Direction.Right
        };
    }

    private void CreateCameraTrigger(Vector2Int fromPos, Vector2Int toPos, Vector3 segmentWorldPos)
    {
        Vector3 fromWorldPos = CalculateWorldPosition(fromPos);
        Vector3 toWorldPos = CalculateWorldPosition(toPos);
        Vector3 triggerPos = (fromWorldPos + toWorldPos) / 2f;

        Debug.Log($"[CameraTrigger] Creating trigger between {fromPos} -> {toPos}");

        GameObject triggerObject = new GameObject("CameraSwitchTrigger");
        triggerObject.transform.position = triggerPos;
        triggerObject.transform.parent = transform;
        triggerObject.tag = "CameraTrigger";

        BoxCollider2D collider = triggerObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        bool isHorizontalTransition = fromPos.x != toPos.x;

        if (isHorizontalTransition)
        {
            collider.size = new Vector2(0.1f, segmentHeight);
        }
        else
        {
            collider.size = new Vector2(segmentWidth, 0.1f);
        }

        CameraSwitchTrigger switchTrigger = triggerObject.AddComponent<CameraSwitchTrigger>();

        if (!placedSegments.TryGetValue(toPos, out GameObject nextSegment) ||
            !placedSegments.TryGetValue(fromPos, out GameObject prevSegment))
        {
            Debug.LogWarning($"[CameraTrigger] Missing segment references at {fromPos} or {toPos}");
            return;
        }

        Transform forwardTarget = nextSegment.transform.Find("CameraTarget_Center");
        Transform backwardTarget = prevSegment.transform.Find("CameraTarget_Center");

        if (forwardTarget == null || backwardTarget == null)
        {
            Debug.LogWarning($"[CameraTrigger] Missing CameraTarget_Center in segments at {fromPos} or {toPos}");
        }
        else
        {
            switchTrigger.cameraTargetForward = forwardTarget;
            switchTrigger.cameraTargetBackward = backwardTarget;
        }

        Debug.Log($"[CameraTrigger] Trigger set with Forward: {forwardTarget?.name}, Backward: {backwardTarget?.name}");
    }

    
    private void GenerateMissingCameraTriggers()
    {
        foreach (var kvp in placedSegments)
        {
            Vector2Int pos = kvp.Key;
            foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
            {
                Vector2Int neighborPos = pos + DirectionToOffset(dir);

                if (!placedSegments.ContainsKey(neighborPos)) continue;
                
                Vector3 from = CalculateWorldPosition(pos);
                Vector3 to = CalculateWorldPosition(neighborPos);
                Vector3 triggerPos = (from + to) / 2f;

                Collider2D[] colliders = Physics2D.OverlapPointAll(triggerPos);
                bool triggerExists = false;
                foreach (var col in colliders)
                {
                    if (col.CompareTag("CameraTrigger"))
                    {
                        triggerExists = true;
                        break;
                    }
                }

                if (!triggerExists)
                {
                    CreateCameraTrigger(pos, neighborPos, triggerPos);
                }
            }
        }
    }
    public void SaveLevelState()
    {
        var levelState = GameStateManager.Instance.CurrentState.levelState;
        levelState.segments.Clear();
        levelState.currentGridPos = currentGridPos;
        levelState.currentDirection = currentDirection;

        foreach (var kvp in placedSegments)
        {
            var segment = kvp.Value.GetComponent<SegmentInfo>();
            if (segment != null)
            {
                levelState.segments.Add(new SavedLevelSegment
                {
                    gridPosition = kvp.Key,
                    availableDirections = segment.directions,
                    prefabIndex = GetPrefabIndex(kvp.Value)
                });
                
                var spawner = segment.GetComponentInChildren<LevelSegmentSpawner>();
                if (spawner != null)
                {
                    spawner.SaveSpawnedObjects();
                }
            }
        }
    }

    public void LoadLevelState()
    {
        var levelState = GameStateManager.Instance.CurrentState.levelState;
        if (levelState.segments.Count == 0) 
        {
            GenerateLevel();
            return;
        }

        ClearExistingLevel();

        currentGridPos = levelState.currentGridPos;
        currentDirection = levelState.currentDirection;

        foreach (var savedSegment in levelState.segments)
        {
            if (savedSegment.prefabIndex < 0 || savedSegment.prefabIndex >= levelPrefabs.Length)
                continue;

            var prefab = levelPrefabs[savedSegment.prefabIndex].prefab;
            var position = CalculateWorldPosition(savedSegment.gridPosition);
            var segment = Instantiate(prefab, position, Quaternion.identity, transform);

            var info = segment.AddComponent<SegmentInfo>();
            info.directions = savedSegment.availableDirections;
            info.GridPosition = savedSegment.gridPosition;

            placedSegments[savedSegment.gridPosition] = segment;
            occupiedPositions.Add(savedSegment.gridPosition);
        }

        GenerateMissingCameraTriggers();
    }

    private int GetPrefabIndex(GameObject segment)
    {
        for (int i = 0; i < levelPrefabs.Length; i++)
        {
            if (segment.name.StartsWith(levelPrefabs[i].prefab.name))
            {
                return i;
            }
        }
        return -1;
    }

    private void ClearExistingLevel()
    {
        foreach (var segment in placedSegments.Values)
        {
            if (segment != null)
            {
                Destroy(segment);
            }
        }

        placedSegments.Clear();
        occupiedPositions.Clear();
        mainLinePositions.Clear();

        var cameraTriggers = GameObject.FindGameObjectsWithTag("CameraTrigger");
        foreach (var trigger in cameraTriggers)
        {
            Destroy(trigger);
        }
    }
}