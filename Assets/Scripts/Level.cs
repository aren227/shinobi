using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public static Level Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<Level>();
            }
            return _instance;
        }
    }

    static Level _instance;

    public const int xSize = 300, ySize = 90, zSize = 300;
    public const int gridSize = 10;

    const int width = xSize/gridSize;
    const int height = zSize/gridSize;

    bool[,] valid; // true if walkable.

    int enemyCount = 0;

    List<Vector2Int> availGrids = new List<Vector2Int>();
    List<Vector2Int> objectiveGrids = new List<Vector2Int>();
    List<Vector2Int> objectiveAdjacentGrids = new List<Vector2Int>();

    EnemySpawnPoint[] enemySpawnPoints;

    public MechStatus normalEnemyStatus;
    public MechStatus playerStatus;

    int[,,,] distTable;

    void Awake() {
        valid = new bool[width, height];

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (Physics.CheckBox(
                    new Vector3((i + 0.5f) * gridSize, ySize * 0.5f, (j + 0.5f) * gridSize),
                    new Vector3(gridSize * 0.5f - 2f, ySize * 0.5f - 2f, gridSize * 0.5f - 2f),
                    Quaternion.identity,
                    LayerMask.GetMask("Ground", "Objective")
                )) {
                    valid[i, j] = false;

                    if (Physics.CheckBox(
                        new Vector3((i + 0.5f) * gridSize, ySize * 0.5f, (j + 0.5f) * gridSize),
                        new Vector3(gridSize * 0.5f - 2f, ySize * 0.5f - 2f, gridSize * 0.5f - 2f),
                        Quaternion.identity,
                        LayerMask.GetMask("Objective")
                    )) {
                        objectiveGrids.Add(new Vector2Int(i, j));
                    }
                }
                else {
                    valid[i, j] = true;
                    availGrids.Add(new Vector2Int(i, j));
                }
            }
        }

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (!valid[i, j]) continue;
                if (
                    objectiveGrids.Contains(new Vector2Int(i, j) - new Vector2Int(-1, 0))
                    || objectiveGrids.Contains(new Vector2Int(i, j) - new Vector2Int(1, 0))
                    || objectiveGrids.Contains(new Vector2Int(i, j) - new Vector2Int(0, -1))
                    || objectiveGrids.Contains(new Vector2Int(i, j) - new Vector2Int(0, 1))
                ) {
                    objectiveAdjacentGrids.Add(new Vector2Int(i, j));
                }
            }
        }

        Debug.Log("Available grid count: " + availGrids.Count);
        Debug.Log("Objective adjacent grid count: " + objectiveAdjacentGrids.Count);

        distTable = new int[width, height, width, height];
        Queue<KeyValuePair<Vector2Int, int>> queue = new Queue<KeyValuePair<Vector2Int, int>>();
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (!valid[i, j]) continue;

                for (int k = 0; k < width; k++) {
                    for (int l = 0; l < height; l++) {
                        distTable[i, j, k, l] = int.MaxValue;
                    }
                }

                queue.Clear();
                queue.Enqueue(new KeyValuePair<Vector2Int, int>(new Vector2Int(i, j), 0));
                while (queue.Count > 0) {
                    KeyValuePair<Vector2Int, int> front = queue.Dequeue();
                    if (!IsValidGrid(front.Key)) continue;
                    if (distTable[i, j, front.Key.x, front.Key.y] <= front.Value) continue;
                    distTable[i, j, front.Key.x, front.Key.y] = front.Value;

                    queue.Enqueue(new KeyValuePair<Vector2Int, int>(new Vector2Int(front.Key.x+1, front.Key.y), front.Value+1));
                    queue.Enqueue(new KeyValuePair<Vector2Int, int>(new Vector2Int(front.Key.x-1, front.Key.y), front.Value+1));
                    queue.Enqueue(new KeyValuePair<Vector2Int, int>(new Vector2Int(front.Key.x, front.Key.y+1), front.Value+1));
                    queue.Enqueue(new KeyValuePair<Vector2Int, int>(new Vector2Int(front.Key.x, front.Key.y-1), front.Value+1));
                }
            }
        }

        enemySpawnPoints = FindObjectsOfType<EnemySpawnPoint>();
    }

    void Start() {
        playerStatus.Initialize(GameManager.Instance.player);
    }

    void Update() {
        // for (int i = 0; i < width; i++) {
        //     for (int j = 0; j < height; j++) {
        //         if (!valid[i, j]) continue;
        //         Vector3 center = GetCenterPosInGrid(new Vector2Int(i, j));
        //         Debug.DrawRay(center - Vector3.one * gridSize/2f, Vector3.one * gridSize, Color.yellow);
        //     }
        // }
    }

    public Mech SpawnEnemy() {
        EnemySpawnPoint spawnPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];

        GameObject cloned = Instantiate(PrefabRegistry.Instance.mech, spawnPoint.transform.position, Quaternion.identity);

        Mech mech = cloned.GetComponent<Mech>();

        normalEnemyStatus.Initialize(mech);

        // if (enemyCount % 3 == 0) mech.GetComponent<EnemyMechController>().onlyShootSpaceship = true;

        enemyCount++;

        return mech;
    }

    public Vector2Int GetGridAt(Vector3 pos) {
        int x = Mathf.FloorToInt(pos.x / gridSize);
        int z = Mathf.FloorToInt(pos.z / gridSize);
        x = Mathf.Clamp(x, 0, width-1);
        z = Mathf.Clamp(z, 0, height-1);
        return new Vector2Int(x, z);
    }

    public bool IsValidGrid(Vector2Int gridPos) {
        if (0 > gridPos.x || gridPos.x >= width || 0 > gridPos.y || gridPos.y >= height) return false;
        return valid[gridPos.x, gridPos.y];
    }

    public int GetGridDist(Vector2Int a, Vector2Int b) {
        return distTable[a.x, a.y, b.x, b.y];
    }

    public Vector2Int GetNextGrid(Vector2Int curr, Vector2Int dest) {
        if (curr == dest) return dest;

        int min = GetGridDist(curr, dest);
        Vector2Int next = curr;

        {
            Vector2Int cand = curr + new Vector2Int(-1, 0);
            if (IsValidGrid(cand) && GetGridDist(cand, dest) < min) {
                min = GetGridDist(cand, dest);
                next = cand;
            }
        }
        {
            Vector2Int cand = curr + new Vector2Int(1, 0);
            if (IsValidGrid(cand) && GetGridDist(cand, dest) < min) {
                min = GetGridDist(cand, dest);
                next = cand;
            }
        }
        {
            Vector2Int cand = curr + new Vector2Int(0, -1);
            if (IsValidGrid(cand) && GetGridDist(cand, dest) < min) {
                min = GetGridDist(cand, dest);
                next = cand;
            }
        }
        {
            Vector2Int cand = curr + new Vector2Int(0, 1);
            if (IsValidGrid(cand) && GetGridDist(cand, dest) < min) {
                min = GetGridDist(cand, dest);
                next = cand;
            }
        }

        return next;
    }

    public Vector2Int GetRandomValidGrid() {
        return availGrids[Random.Range(0, availGrids.Count)];
    }

    public Vector3 GetRandomPosInGrid(Vector2Int gridPos) {
        return new Vector3(
            (gridPos.x + Random.Range(0f, 0.999f)) * gridSize,
            Random.Range(50f, ySize - 50f),
            (gridPos.y + Random.Range(0f, 0.999f)) * gridSize
        );
    }

    public Vector3 GetCenterPosInGrid(Vector2Int gridPos) {
        return new Vector3(
            (gridPos.x + 0.5f) * gridSize,
            Random.Range(50f, ySize - 50f),
            (gridPos.y + 0.5f) * gridSize
        );
    }

    public Vector2Int GetRandomObjectiveGrid() {
        return objectiveAdjacentGrids[Random.Range(0, objectiveAdjacentGrids.Count)];
    }
}
