using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 시야에 없으면 마지막으로 보인 위치로 이동
// 그래도 찾지 못하면 목표물로 이동해서 공격

// 미사일, 총알이 날라오면 랜덤하게 딜레이를 둬서 피함 (대시)
// 적이 시야에 있는 동안, 일정 거리를 유지하도록 노력 (너무 멀면 가까이, 너무 가까우면 멀리, 대시 활용)
// 적이 시야에 있는 동안, 공격

public class EnemyMechController : MonoBehaviour
{
    Mech mech;
    Level level;

    Mech player;

    const float fovWhenAttack = 80;
    const float fovWhenNotAttack = 80;

    const float missileLaunchMinDelay = 1f;
    const float missileLaunchMaxDelay = 3f;

    public State state = State.OBJECTIVE;

    Vector3 targetPos;

    Vector3 playerOffset;

    const float properDistance = 50;
    const float boostMinDist = 30;
    const float minMoveDist = 5;

    float stateDelay = 0;

    const float shootObjectiveAfter = 3;

    const float missileNoticeDistance = 10;
    // @Todo: Currently we evaluate prob per frame. Not good.
    const float missileAvoidProb = 0.33f;

    const float minReactionTime = 0.5f;
    const float maxReactionTime = 1.5f;
    float nextReactionTime;

    float nextOffsetDelay = 0;

    float noticingPlayerTime;

    int searchAttempt;
    float searchMaxTime;

    float lastShoot;
    float delay;

    public bool onlyShootSpaceship;

    // List with only one element.
    List<Transform> playerTargetList = new List<Transform>();
    List<Transform> objectiveTargetList = new List<Transform>();

    Vector3 timeOffset;

    const float attackPerlinNoiseSpeed = 0.2f;
    const float objectivePerlinNoiseSpeed = 0.05f;

    float perlinNoiseTime = 0;

    Vector2Int nextGrid = new Vector2Int(-1000, -1000);
    Vector3 nextGridTargetPos;

    public float overrideTargetPosFor = 0;
    public Vector3 overrideTargetPos;

    void SetRandomOffset() {
        // Vector3 sphere = Random.onUnitSphere;
        // sphere.y *= 0.2f;
        // playerOffset = sphere.normalized;

        // @Todo: Is this really 0.5 centered noise?
        Vector3 sphere = new Vector3(
            Mathf.PerlinNoise(perlinNoiseTime + timeOffset.x, 0f) - 0.5f,
            Mathf.PerlinNoise(perlinNoiseTime + timeOffset.y, 0f) - 0.5f,
            Mathf.PerlinNoise(perlinNoiseTime + timeOffset.z, 0f) - 0.5f
        ).normalized;

        sphere.y *= 0.2f;

        playerOffset = sphere.normalized;

        if (state == State.OBJECTIVE) perlinNoiseTime += Time.deltaTime * objectivePerlinNoiseSpeed;
        else perlinNoiseTime += Time.deltaTime * attackPerlinNoiseSpeed;
    }

    void SetReactionTime() {
        nextReactionTime = Random.Range(minReactionTime, maxReactionTime);
    }

    void Awake() {
        mech = GetComponent<Mech>();
        level = Level.Instance;

        player = GameManager.Instance.player;

        playerTargetList.Add(player.skeleton.cockpit.transform);
        objectiveTargetList.Add(GameManager.Instance.objective.transform);

        timeOffset = new Vector3(
            Random.Range(0f, 100f),
            Random.Range(0f, 100f),
            Random.Range(0f, 100f)
        );

        SetRandomOffset();
        SetReactionTime();

        noticingPlayerTime = float.PositiveInfinity;
    }

    void Start() {
        foreach (Inventory.Slot slot in new Inventory.Slot[] {
            Inventory.Slot.LEFT_SHOULDER,
            Inventory.Slot.RIGHT_SHOULDER,
        }) {
            GameObject missileWeapon = Instantiate(PrefabRegistry.Instance.missileWeapon);

            missileWeapon.GetComponent<Weapon>().ammo = 200;

            mech.Equip(missileWeapon.GetComponent<Item>(), slot);
        }

        if (Random.Range(0f, 1f) < 0.2f) {
            GameObject bulletWeapon = Instantiate(PrefabRegistry.Instance.bulletWeapon);

            bulletWeapon.GetComponent<Weapon>().ammo = 1000;

            mech.Equip(bulletWeapon.GetComponent<Item>(), Inventory.Slot.RIGHT_HAND);
        }
        if (Random.Range(0f, 1f) < 0.2f * 0.2f) {
            GameObject bulletWeapon = Instantiate(PrefabRegistry.Instance.bulletWeapon);

            bulletWeapon.GetComponent<Weapon>().ammo = 1000;

            mech.Equip(bulletWeapon.GetComponent<Item>(), Inventory.Slot.LEFT_HAND);
        }

        // Set initial state.
        state = State.OBJECTIVE;
        stateDelay = 0;

        targetPos = level.GetRandomPosInGrid(level.GetRandomObjectiveGrid());
    }

    bool CanSeePlayer() {
        // @Todo: Raycast hit with level.
        return
            !player.isHided
            && Vector3.Distance(mech.transform.position, player.transform.position) < 100
            && Vector3.Angle(mech.skeleton.headBone.forward, player.transform.position - mech.transform.position) <= (state == State.ATTACK ? fovWhenAttack : fovWhenNotAttack);
    }

    public void OverrideTargetPos(Vector3 target, float time) {
        overrideTargetPosFor = time;
        overrideTargetPos = target;
    }

    void Update() {
        if (GameManager.Instance.isPaused) return;
        if (mech.isKilled) return;

        List<Transform> currentTargets = null;

        if (state == State.ATTACK) {
            if (!CanSeePlayer()) {
                if (stateDelay > 1) {
                    // state = State.SEARCH;

                    // stateDelay = 0;
                    // searchAttempt = 1;
                    // searchMaxTime = Random.Range(5f, 15f);

                    state = State.OBJECTIVE;

                    stateDelay = 0;

                    targetPos = level.GetRandomPosInGrid(level.GetRandomObjectiveGrid());
                }
            }
            else {
                targetPos = player.transform.position + playerOffset * properDistance;

                stateDelay = 0;

                currentTargets = playerTargetList;
            }
            stateDelay = 0;
        }
        else if (state == State.SEARCH) {
            if (Vector3.Distance(targetPos, transform.position) < 3 || stateDelay >= searchMaxTime) {
                if (searchAttempt <= 0) {
                    state = State.OBJECTIVE;

                    stateDelay = 0;

                    targetPos = level.GetRandomPosInGrid(level.GetRandomObjectiveGrid());
                }
                else {
                    searchAttempt--;

                    targetPos = level.GetRandomPosInGrid(level.GetRandomValidGrid());

                    stateDelay = 0;
                    searchMaxTime = Random.Range(5f, 15f);
                }
            }
        }
        else if (state == State.OBJECTIVE) {
            targetPos = GameManager.Instance.objective.transform.position + playerOffset * 30;

            if (stateDelay > shootObjectiveAfter) {
                currentTargets = objectiveTargetList;
            }
        }

        if (overrideTargetPosFor > 0) {
            targetPos = overrideTargetPos;
            overrideTargetPosFor = Mathf.Max(overrideTargetPosFor - Time.deltaTime, 0);
        }

        float minTargetedMissileDist = float.PositiveInfinity;
        Missile minDistMissile = null;
        foreach (Missile missile in mech.targetedMissiles) {
            float dist = Vector3.Distance(missile.transform.position, missile.target.transform.position);
            if (minTargetedMissileDist > dist) {
                minTargetedMissileDist = dist;
                minDistMissile = missile;
            }
        }

        if (
            (
                Vector3.Distance(targetPos, mech.transform.position) > boostMinDist
                || Vector3.Distance(player.transform.position, mech.transform.position) < 15f
                || (minDistMissile != null && minTargetedMissileDist < missileNoticeDistance && minDistMissile.randomValue < missileAvoidProb)
            )
         ) {
                if (!mech.boost) mech.BeginBoost();
        }
        else {
            mech.EndBoost();
        }

        if (state != State.ATTACK) {
            if (mech.hitByPlayerFlag && float.IsInfinity(noticingPlayerTime)) {
                noticingPlayerTime = Time.time + nextReactionTime;
                SetReactionTime();
            }
        }
        else {
            noticingPlayerTime = float.PositiveInfinity;
        }
        mech.hitByPlayerFlag = false;

        if (
            state != State.ATTACK
            && !onlyShootSpaceship
            && (
                CanSeePlayer()
                || noticingPlayerTime < Time.time
                || minTargetedMissileDist < missileNoticeDistance
            )
        ) {
            state = State.ATTACK;

            stateDelay = 0;
        }

        float distToTarget = Vector3.Distance(targetPos, mech.transform.position);
        // if (state == State.ATTACK && distToTarget < minMoveDist) {
        //     // Set another random offset to move continuously while attacking.
        //     SetRandomOffset();
        // }

        if (distToTarget > minMoveDist) {
            Vector2Int gridCurr = level.GetGridAt(mech.transform.position);
            Vector2Int gridTar = level.GetGridAt(targetPos);

            if (gridCurr == gridTar) {
                mech.Move((targetPos - mech.transform.position).normalized);
            }
            else {
                Vector2Int nextGrid = level.GetNextGrid(gridCurr, gridTar);
                if (nextGrid != this.nextGrid) {
                    this.nextGrid = nextGrid;
                    nextGridTargetPos = level.GetRandomPosInGrid(nextGrid);
                }

                Vector3 fromToTar = targetPos - mech.transform.position;
                Vector3 fromToRandomGrid = nextGridTargetPos - mech.transform.position;

                Vector3 projected = Vector3.Project(fromToRandomGrid, fromToTar);

                fromToRandomGrid.y = projected.y;

                // Debug.DrawRay(transform.position, fromToRandomGrid, Color.blue);
                // Debug.DrawRay(transform.position, fromToTar, Color.green);

                // Debug.DrawLine(level.GetCenterPosInGrid(gridCurr), level.GetCenterPosInGrid(nextGrid), Color.red);

                // Debug.Log(gridCurr + " -> " + nextGrid);

                mech.Move(fromToRandomGrid.normalized);
            }
        }
        else {
            mech.Move(Vector3.zero);
        }

        if (state == State.ATTACK) {
            if (!player.isHided) mech.Aim(player.skeleton.cockpit.transform.position);
        }
        else if (state == State.SEARCH) {
            mech.Aim(targetPos);
        }
        else if (state == State.OBJECTIVE) {
            mech.Aim(objectiveTargetList[0].position);
        }

        if (currentTargets != null && Time.time - lastShoot > delay) {
            mech.LaunchMissiles(currentTargets);

            lastShoot = Time.time;
            delay = Random.Range(missileLaunchMinDelay, missileLaunchMaxDelay);
        }

        stateDelay += Time.deltaTime;

        SetRandomOffset();
    }

    public enum State {
        SEARCH,
        ATTACK,
        OBJECTIVE,
    }
}
