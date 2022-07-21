using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<GameManager>();
            }
            return _instance;
        }
    }

    static GameManager _instance;

    public Mech player;

    public List<Mech> meches = new List<Mech>();

    public Objective objective;

    public GameState state { get; private set; }

    const int defenseTime = 60 * 5;
    const int enemySpawnCountPerWave = 5;
    const int enemySpawnDelay = 1;
    const int waveDelay = 30;

    public float stateBeginTime;

    Level level;

    void Awake() {
        level = FindObjectOfType<Level>();
    }

    void Start() {
        BeginState(GameState.FIGHT);
    }

    public void BeginState(GameState state) {
        stateBeginTime = Time.time;
        this.state = state;

        if (state == GameState.PREPARE) {
            // @Todo
        }
        if (state == GameState.FIGHT) {
            StartCoroutine(SpawnScheduler());
        }
        if (state == GameState.LEAVE) {
            // @Todo
        }
    }

    IEnumerator SpawnScheduler() {
        while (Time.time - stateBeginTime <= defenseTime) {
            Debug.Log("Begin wave");

            for (int i = 0; i < enemySpawnCountPerWave; i++) {
                level.SpawnEnemy();
                yield return new WaitForSeconds(enemySpawnDelay);
            }

            yield return new WaitForSeconds(waveDelay);
        }

        BeginState(GameState.LEAVE);
    }
}

public enum GameState {
    PREPARE,
    FIGHT,
    FAILED,
    LEAVE,
}