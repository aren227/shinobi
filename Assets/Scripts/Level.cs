using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public const int xSize = 300, ySize = 300, zSize = 300;
    public const int gridSize = 20;

    const int width = xSize/gridSize;
    const int height = zSize/gridSize;

    bool[,] valid; // true if walkable.

    List<Vector2Int> availGrids = new List<Vector2Int>();
    List<Vector2Int> objectiveGrids = new List<Vector2Int>();
    List<Vector2Int> objectiveAdjacentGrids = new List<Vector2Int>();

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

    public int TaxiDist(Vector2Int a, Vector2Int b) {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public Vector2Int GetNextGrid(Vector2Int curr, Vector2Int dest) {
        if (curr == dest) return dest;

        int min = int.MaxValue;
        Vector2Int next = curr;

        {
            Vector2Int cand = curr + new Vector2Int(-1, 0);
            if (IsValidGrid(cand) && TaxiDist(cand, dest) < min) {
                min = TaxiDist(cand, dest);
                next = cand;
            }
        }
        {
            Vector2Int cand = curr + new Vector2Int(1, 0);
            if (IsValidGrid(cand) && TaxiDist(cand, dest) < min) {
                min = TaxiDist(cand, dest);
                next = cand;
            }
        }
        {
            Vector2Int cand = curr + new Vector2Int(0, -1);
            if (IsValidGrid(cand) && TaxiDist(cand, dest) < min) {
                min = TaxiDist(cand, dest);
                next = cand;
            }
        }
        {
            Vector2Int cand = curr + new Vector2Int(0, 1);
            if (IsValidGrid(cand) && TaxiDist(cand, dest) < min) {
                min = TaxiDist(cand, dest);
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
            (gridPos.x * Random.Range(0f, 0.999f)) * gridSize,
            Random.Range(50f, ySize - 50f),
            (gridPos.y * Random.Range(0f, 0.999f)) * gridSize
        );
    }

    public Vector2Int GetRandomObjectiveGrid() {
        return objectiveAdjacentGrids[Random.Range(0, objectiveAdjacentGrids.Count)];
    }

    public void SpawnEnemy() {
        // @Todo
    }

    public Vector3 GetObjectivePosition() {
        // @Todo
        return Random.onUnitSphere * 10;
    }
}
