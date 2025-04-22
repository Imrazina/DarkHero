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
    public float segmentWidth = 16f;
    public float segmentHeight = 9f;

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
    }

    private void InitializeStartSegment()
    {
        currentGridPos = Vector2Int.zero;
        occupiedPositions.Add(currentGridPos);

        if (spawnPoint.childCount > 0)
        {
            GameObject startSegment = spawnPoint.GetChild(0).gameObject;
            placedSegments[currentGridPos] = startSegment;

            // Добавим directions для старта вручную, если нужно
            if (startSegment.GetComponent<SegmentInfo>() == null)
            {
                var info = startSegment.AddComponent<SegmentInfo>();
                info.directions = new[] { Direction.Right }; // старт направо
            }
        }

        currentDirection = Direction.Right;
    }

    private void GenerateMainLine()
    {
        for (int i = 0; i < horizontalSegments; i++)
        {
            Vector2Int prevPos = currentGridPos;

            if (!GenerateSegment(currentGridPos, currentDirection, isMainLine: true))
                break;

            mainLinePositions.Add(prevPos);
        }
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

    private bool GenerateSegment(Vector2Int fromPos, Direction direction, bool isMainLine)
    {
        Vector2Int nextPos = fromPos + DirectionToOffset(direction);

        if (occupiedPositions.Contains(nextPos)) return false;

        var validPrefabs = GetValidPrefabs(direction);
        if (validPrefabs.Count == 0) return false;

        var selected = validPrefabs[Random.Range(0, validPrefabs.Count)];
        var spawnPos = CalculateWorldPosition(nextPos);

        GameObject segment = Instantiate(selected.prefab, spawnPos, Quaternion.identity, transform);

        // Сохраняем информацию о выходах
        var info = segment.AddComponent<SegmentInfo>();
        info.directions = selected.availableDirections;

        placedSegments[nextPos] = segment;
        occupiedPositions.Add(nextPos);
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

        return true;
    }

    private List<PrefabDirections> GetValidPrefabs(Direction entryDirection)
    {
        var validPrefabs = new List<PrefabDirections>();
        Direction requiredEntry = GetOppositeDirection(entryDirection);

        foreach (var prefabDir in levelPrefabs)
        {
            if (System.Array.Exists(prefabDir.availableDirections, dir => dir == requiredEntry))
            {
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
}
