using System.Collections;
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
    public int maxHorizontalSegments = 7; // Максимальное количество сегментов по горизонтали

    private Vector3 currentPosition;
    private Direction currentDirection;
    private int generatedHorizontalCount = 0;
    private float initialY; // Начальная позиция по Y, к которой будем стремиться
    private bool shouldAlignVertically = false; // Флаг для выравнивания по Y

    private readonly Dictionary<Direction, string> connectionPoints = new()
    {
        { Direction.Up, "Connection_Up" },
        { Direction.Down, "Connection_Down" },
        { Direction.Left, "Connection_Left" },
        { Direction.Right, "Connection_Right" }
    };

    void Start()
    {
        currentPosition = spawnPoint.position;
        currentPosition.z = 0f;
        initialY = currentPosition.y;
        currentDirection = Direction.Right;
        GenerateLabyrinth();
    }

    void GenerateLabyrinth()
    {
        // Основная горизонтальная генерация
        while (generatedHorizontalCount < maxHorizontalSegments)
        {
            GenerateNextSegment();
            generatedHorizontalCount++;
        }

        // После генерации горизонтали начинаем выравнивание по Y
        shouldAlignVertically = true;
        while (Mathf.Abs(currentPosition.y - initialY) > 0.1f)
        {
            GenerateNextSegment();
        }

        // Генерация вертикальных сегментов вниз, если есть выходы
        GenerateVerticalDown();
    }

    void GenerateNextSegment()
    {
        // Выбираем только подходящие префабы
        List<PrefabDirections> validPrefabs = new List<PrefabDirections>();
        foreach (var prefabDir in levelPrefabs)
        {
            if (System.Array.Exists(prefabDir.availableDirections, 
                dir => dir == GetOppositeDirection(currentDirection)))
            {
                // Если нужно выравнивание по Y, предпочитаем сегменты с нужным направлением
                if (shouldAlignVertically)
                {
                    if (currentPosition.y > initialY && 
                        System.Array.Exists(prefabDir.availableDirections, dir => dir == Direction.Down))
                    {
                        validPrefabs.Add(prefabDir);
                    }
                    else if (currentPosition.y < initialY && 
                        System.Array.Exists(prefabDir.availableDirections, dir => dir == Direction.Up))
                    {
                        validPrefabs.Add(prefabDir);
                    }
                    else if (Mathf.Abs(currentPosition.y - initialY) < 0.1f)
                    {
                        validPrefabs.Add(prefabDir);
                    }
                }
                else
                {
                    validPrefabs.Add(prefabDir);
                }
            }
        }

        if (validPrefabs.Count == 0)
        {
            Debug.LogError("No valid prefabs found for direction: " + currentDirection);
            return;
        }

        // Выбираем случайный подходящий префаб с учетом приоритетов
        PrefabDirections selectedPrefab = validPrefabs[Random.Range(0, validPrefabs.Count)];
        GameObject prefab = selectedPrefab.prefab;

        // Создаем временный объект для поиска точки входа
        GameObject temp = Instantiate(prefab);
        string entryPointName = "Connection_" + GetOppositeDirection(currentDirection);
        Transform entryPoint = temp.transform.Find(entryPointName);
        
        if (entryPoint == null)
        {
            Debug.LogError($"Prefab {prefab.name} doesn't have entry point {entryPointName}");
            Destroy(temp);
            return;
        }

        // Вычисляем позицию для спавна
        Vector3 offset = entryPoint.position - temp.transform.position;
        Vector3 spawnPos = currentPosition - offset;
        Destroy(temp);

        // Создаем финальный сегмент
        GameObject segment = Instantiate(prefab, spawnPos, Quaternion.identity);
        
        // Находим все возможные выходы
        List<Direction> availableExits = new List<Direction>();
        foreach (Direction dir in selectedPrefab.availableDirections)
        {
            if (dir != GetOppositeDirection(currentDirection))
            {
                string exitPointName = "Connection_" + dir;
                if (segment.transform.Find(exitPointName) != null)
                {
                    availableExits.Add(dir);
                }
            }
        }

        // Выбираем следующий направление с учетом стратегии
        if (availableExits.Count > 0)
        {
            if (shouldAlignVertically)
            {
                // При выравнивании выбираем направление, ведущее к initialY
                if (currentPosition.y > initialY && availableExits.Contains(Direction.Down))
                {
                    currentDirection = Direction.Down;
                }
                else if (currentPosition.y < initialY && availableExits.Contains(Direction.Up))
                {
                    currentDirection = Direction.Up;
                }
                else
                {
                    // Если нет нужного направления, выбираем случайное
                    currentDirection = availableExits[Random.Range(0, availableExits.Count)];
                }
            }
            else
            {
                // Обычный случай - случайный выбор
                currentDirection = availableExits[Random.Range(0, availableExits.Count)];
            }
            
            currentPosition = segment.transform.Find("Connection_" + currentDirection).position;
        }
        else
        {
            Debug.LogWarning("No valid exits found in prefab " + prefab.name);
            currentPosition = segment.transform.position;
        }

        SetupCameraTriggers(segment);
    }

    void GenerateVerticalDown()
{
    // Сохраняем текущее направление перед генерацией вертикальных веток
    Direction savedDirection = currentDirection; // Добавляем объявление переменной
    
    // Проверяем все сегменты на наличие выходов вниз
    List<GameObject> segmentsWithDownExit = new List<GameObject>();
    foreach (Transform child in transform)
    {
        if (child.Find("Connection_Down") != null)
        {
            segmentsWithDownExit.Add(child.gameObject);
        }
    }

    // Для каждого сегмента с выходом вниз генерируем вертикальную ветку
    foreach (var segment in segmentsWithDownExit)
    {
        Vector3 startPos = segment.transform.Find("Connection_Down").position;
        currentDirection = Direction.Down;
        currentPosition = startPos;

        // Генерируем вертикальную ветку до тупика
        bool hasExits = true;
        int safetyCounter = 0;
        int maxDepth = 3; // Максимальная глубина вертикальной ветки

        while (hasExits && safetyCounter < maxDepth)
        {
            // Выбираем только префабы без выхода вниз (если это последний сегмент)
            List<PrefabDirections> validPrefabs = new List<PrefabDirections>();
            foreach (var prefabDir in levelPrefabs)
            {
                if (System.Array.Exists(prefabDir.availableDirections, 
                    dir => dir == GetOppositeDirection(currentDirection)))
                {
                    // Если это предпоследняя итерация, выбираем префабы без выхода вниз
                    if (safetyCounter >= maxDepth - 1)
                    {
                        if (!System.Array.Exists(prefabDir.availableDirections, 
                            dir => dir == Direction.Down))
                        {
                            validPrefabs.Add(prefabDir);
                        }
                    }
                    else
                    {
                        validPrefabs.Add(prefabDir);
                    }
                }
            }

            if (validPrefabs.Count == 0) break;

            PrefabDirections selectedPrefab = validPrefabs[Random.Range(0, validPrefabs.Count)];
            GameObject prefab = selectedPrefab.prefab;

            GameObject temp = Instantiate(prefab);
            string entryPointName = "Connection_" + GetOppositeDirection(currentDirection);
            Transform entryPoint = temp.transform.Find(entryPointName);
            
            if (entryPoint == null)
            {
                Debug.LogError($"Prefab {prefab.name} doesn't have entry point {entryPointName}");
                Destroy(temp);
                break;
            }

            Vector3 offset = entryPoint.position - temp.transform.position;
            Vector3 spawnPos = currentPosition - offset;
            Destroy(temp);

            GameObject newSegment = Instantiate(prefab, spawnPos, Quaternion.identity);
            
            // Проверяем выходы
            List<Direction> availableExits = new List<Direction>();
            foreach (Direction dir in selectedPrefab.availableDirections)
            {
                if (dir != GetOppositeDirection(currentDirection))
                {
                    string exitPointName = "Connection_" + dir;
                    if (newSegment.transform.Find(exitPointName) != null)
                    {
                        availableExits.Add(dir);
                    }
                }
            }

            if (availableExits.Count > 0)
            {
                // В вертикальной ветке предпочитаем продолжать вниз
                if (availableExits.Contains(Direction.Down))
                {
                    currentDirection = Direction.Down;
                }
                else
                {
                    currentDirection = availableExits[Random.Range(0, availableExits.Count)];
                }
                currentPosition = newSegment.transform.Find("Connection_" + currentDirection).position;
            }
            else
            {
                hasExits = false;
            }

            SetupCameraTriggers(newSegment);
            safetyCounter++;
        }
    }
    currentDirection = savedDirection;
}

    Direction GetOppositeDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return Direction.Down;
            case Direction.Down: return Direction.Up;
            case Direction.Left: return Direction.Right;
            case Direction.Right: return Direction.Left;
            default: return Direction.Right;
        }
    }

    void SetupCameraTriggers(GameObject segment)
    {
        string[] possibleTriggers = { "ExitTrigger", "Colider_Up", "Colider_Right", "Colider_Down" };
        
        foreach (string triggerName in possibleTriggers)
        {
            Transform trigger = segment.transform.Find(triggerName);
            if (trigger != null)
            {
                CameraSwitchTrigger switcher = trigger.GetComponent<CameraSwitchTrigger>();
                if (switcher == null) switcher = trigger.gameObject.AddComponent<CameraSwitchTrigger>();
                
                switcher.cameraTargetUp = segment.transform.Find("CameraTarget_Up");
                switcher.cameraTargetDown = segment.transform.Find("CameraTarget_Down");
                switcher.cameraTargetLeft = segment.transform.Find("CameraTarget_Left");
                switcher.cameraTargetRight = segment.transform.Find("CameraTarget_Right");
            }
        }
    }
}