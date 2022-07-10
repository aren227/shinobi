using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadObjects : MonoBehaviour
{
    List<Vector3> points = new List<Vector3>();

    const int COUNT = 200;
    const int VIEW_RADIUS = 300;
    const int UNIT = VIEW_RADIUS * 2;

    public List<GameObject> prefabs;

    List<GameObject> spawned = new List<GameObject>();
    List<GameObject> pool = new List<GameObject>();

    Mech mech;
    Vector3 prevCenter;

    void Awake() {
        for (int i = 0; i < COUNT; i++) {
            points.Add(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)) * UNIT);
        }

        mech = FindObjectOfType<Mech>();
    }

    void Start() {
        Vector3 currCenter = mech.transform.position;

        int x, y, z;
        x = Mathf.FloorToInt(currCenter.x / UNIT);
        y = Mathf.FloorToInt(currCenter.y / UNIT);
        z = Mathf.FloorToInt(currCenter.z / UNIT);

        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                for (int k = 0; k < 3; k++) {
                    int dx = i-1, dy = j-1, dz = k-1;

                    Vector3 origin = new Vector3(x+dx, y+dy, z+dz) * UNIT;

                    for (int l = 0; l < COUNT; l++) {
                        Vector3 pos = points[l] + origin;
                        if (Vector3.Distance(currCenter, pos) <= VIEW_RADIUS) {
                            Spawn(pos);
                        }
                    }
                }
            }
        }
    }

    void Spawn(Vector3 pos) {
        if (pool.Count == 0) {
            if (prefabs.Count == 0) return;

            int index = Random.Range(0, prefabs.Count);
            GameObject cloned = Instantiate(prefabs[index]);
            cloned.SetActive(false);

            pool.Add(cloned);
        }

        GameObject obj = pool[pool.Count-1];
        pool.RemoveAt(pool.Count-1);

        obj.transform.position = pos;

        const float minScale = 1f;
        const float maxScale = 15f;

        float scale = Mathf.Lerp(minScale, maxScale, Mathf.Pow(Random.Range(0f, 1f), 2));
        obj.transform.localScale = Vector3.one * scale;
        obj.transform.localRotation = Random.rotation;

        obj.GetComponent<Rigidbody>().velocity = Vector3.zero;
        obj.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        obj.SetActive(true);

        spawned.Add(obj);
    }

    void Update() {
        Vector3 currCenter = mech.transform.position;

        for (int i = spawned.Count-1; i >= 0; i--) {
            if (Vector3.Distance(spawned[i].transform.position, currCenter) > VIEW_RADIUS) {
                spawned[i].SetActive(false);
                pool.Add(spawned[i]);
                spawned.RemoveAt(i);
            }
        }

        int x, y, z;
        x = Mathf.FloorToInt(currCenter.x / UNIT);
        y = Mathf.FloorToInt(currCenter.y / UNIT);
        z = Mathf.FloorToInt(currCenter.z / UNIT);

        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                for (int k = 0; k < 3; k++) {
                    int dx = i-1, dy = j-1, dz = k-1;

                    Vector3 origin = new Vector3(x+dx, y+dy, z+dz) * UNIT;

                    for (int l = 0; l < COUNT; l++) {
                        Vector3 pos = points[l] + origin;
                        if (Vector3.Distance(prevCenter, pos) > VIEW_RADIUS && Vector3.Distance(currCenter, pos) <= VIEW_RADIUS) {
                            Spawn(pos);
                        }
                    }
                }
            }
        }

        prevCenter = currCenter;
    }
}
