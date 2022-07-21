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
    const int enemySpawnCountPerWave = 2;
    const int enemySpawnDelay = 1;
    const int waveDelay = 30;

    public float stateBeginTime;

    Level level;

    Spaceship spaceship;

    Coroutine fightStateCoroutine;

    void Awake() {
        level = FindObjectOfType<Level>();
        spaceship = FindObjectOfType<Spaceship>();
    }

    void Start() {
        BeginState(GameState.PREPARE);
    }

    public void BeginState(GameState state) {
        stateBeginTime = Time.time;
        this.state = state;

        if (state == GameState.PREPARE) {
            StartCoroutine(Prepare());
        }
        if (state == GameState.FIGHT) {
            fightStateCoroutine = StartCoroutine(SpawnScheduler());
        }
        if (state == GameState.FAILED) {
            StopCoroutine(fightStateCoroutine);

            StartCoroutine(Explosion());
        }
        if (state == GameState.LEAVE) {
            StartCoroutine(Leave());
        }
    }

    IEnumerator Prepare() {
        spaceship.Arrive();

        yield return new WaitForSeconds(15);

        BeginState(GameState.FIGHT);
    }

    IEnumerator Leave() {
        spaceship.Depart();

        yield return new WaitForSeconds(15);

        Debug.Log("Game done");
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

    IEnumerator Explosion() {
        spaceship.Explode();

        yield return null;
    }
}

public enum GameState {
    PREPARE,
    FIGHT,
    FAILED,
    LEAVE,
}