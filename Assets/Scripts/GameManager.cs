using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

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

    public GlobalData globalData;

    public Mech player;

    UiManager uiManager;

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
    Door door;

    Coroutine fightStateCoroutine;

    public bool isPaused;

    void Awake() {
        level = FindObjectOfType<Level>();
        uiManager = FindObjectOfType<UiManager>();
        spaceship = FindObjectOfType<Spaceship>();
        door = FindObjectOfType<Door>();
    }

    void Start() {
        StartCoroutine(Prepare());
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            SetPause(!isPaused);
        }
    }

    public void BeginState(GameState state) {
        if (this.state == state) return;

        stateBeginTime = Time.time;
        this.state = state;

        if (state == GameState.PREPARE) {

        }
        if (state == GameState.FIGHT) {
            fightStateCoroutine = StartCoroutine(SpawnScheduler());
        }
        if (state == GameState.FAILED) {
            StopCoroutine(fightStateCoroutine);

            StartCoroutine(Explosion());

            Debug.Log("Mission failed.");
        }
        if (state == GameState.LEAVE) {
            StartCoroutine(Leave());
        }
        if (state == GameState.COMPLETED) {
            Debug.Log("Mission complete.");

            globalData.isComplete = true;

            SceneManager.LoadScene("Result");
        }
    }

    public void SetPause(bool isPaused) {
        this.isPaused = isPaused;

        DOTween.Kill("timeScale", true);

        if (isPaused) {
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
        }
        else {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
        }

        uiManager.SetPause(isPaused);
    }

    public void Restart() {
        SceneManager.LoadScene("SampleScene");
    }

    IEnumerator Prepare() {
        spaceship.Arrive();

        door.Open();

        yield return new WaitForSeconds(10);

        door.Close();

        yield return new WaitForSeconds(5);

        BeginState(GameState.FIGHT);
    }

    IEnumerator Leave() {
        door.Open();

        yield return new WaitForSeconds(5);

        spaceship.Depart();

        yield return new WaitForSeconds(3);

        door.RemoveBarrier();
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

        yield return new WaitForSeconds(5);

        globalData.isComplete = false;

        SceneManager.LoadScene("Result");
    }
}

public enum GameState {
    PREPARE,
    FIGHT,
    FAILED,
    LEAVE,
    COMPLETED,
}