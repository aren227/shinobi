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

    const float fovWhenAttack = 60;
    const float fovWhenNotAttack = 30;

    const float missileLaunchMinDelay = 1f;
    const float missileLaunchMaxDelay = 3f;

    State state = State.OBJECTIVE;

    Vector3 targetPos;

    Vector3 playerOffset;

    const float properDistance = 50;
    const float boostMinDist = 30;
    const float minMoveDist = 5;

    float stateDelay = 0;

    const float shootObjectiveAfter = 7;

    const float missileNoticeDistance = 10;
    // @Todo: Currently we evaluate prob per frame. Not good.
    const float missileAvoidProb = 0.2f;

    const float minReactionTime = 0.3f;
    const float maxReactionTime = 1.5f;
    float nextReactionTime;

    float nextOffsetDelay = 0;

    float noticingPlayerTime;

    int searchAttempt;
    float searchMaxTime;

    float lastShoot;
    float delay;

    // List with only one element.
    List<Transform> playerTargetList = new List<Transform>();
    List<Transform> objectiveTargetList = new List<Transform>();

    Vector3 timeOffset;

    const float perlinNoiseSpeed = 0.2f;

    void SetRandomOffset() {
        // Vector3 sphere = Random.onUnitSphere;
        // sphere.y *= 0.2f;
        // playerOffset = sphere.normalized;

        // @Todo: Is this really 0.5 centered noise?
        Vector3 sphere = new Vector3(
            Mathf.PerlinNoise(Time.time * perlinNoiseSpeed + timeOffset.x, 0f) - 0.5f,
            Mathf.PerlinNoise(Time.time * perlinNoiseSpeed + timeOffset.y, 0f) - 0.5f,
            Mathf.PerlinNoise(Time.time * perlinNoiseSpeed + timeOffset.z, 0f) - 0.5f
        ).normalized;

        sphere.y *= 0.2f;

        playerOffset = sphere.normalized;
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

            missileWeapon.GetComponent<Weapon>().ammo = 300;

            mech.Equip(missileWeapon.GetComponent<Item>(), slot);
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
            && Vector3.Angle(mech.skeleton.headBone.forward, player.transform.position - mech.transform.position) <= (state == State.ATTACK ? fovWhenAttack : fovWhenNotAttack);
    }

    void Update() {
        if (mech.isKilled) return;

        List<Transform> currentTargets = null;

        if (state == State.ATTACK) {
            targetPos = player.transform.position + playerOffset * properDistance;

            if (!CanSeePlayer()) {
                if (stateDelay > 1) {
                    state = State.SEARCH;

                    stateDelay = 0;
                    searchAttempt = Random.Range(1, 4);
                    searchMaxTime = Random.Range(10f, 30f);
                }
            }
            else {
                stateDelay = 0;
            }

            currentTargets = playerTargetList;
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
                    searchMaxTime = Random.Range(10f, 30f);
                }
            }
        }
        else if (state == State.OBJECTIVE) {
            if (stateDelay > shootObjectiveAfter) {
                currentTargets = objectiveTargetList;
            }
        }

        float minTargetedMissileDist = float.PositiveInfinity;
        foreach (Missile missile in mech.targetedMissiles) {
            minTargetedMissileDist = Mathf.Min(minTargetedMissileDist, Vector3.Distance(missile.transform.position, missile.target.transform.position));
        }

        if (
            state == State.ATTACK
            && (
                Vector3.Distance(targetPos, mech.transform.position) > boostMinDist
                || (minTargetedMissileDist < missileNoticeDistance && Random.Range(0f, 1f) < missileAvoidProb)
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
            && (
                CanSeePlayer()
                || noticingPlayerTime < Time.time
                || minTargetedMissileDist < missileNoticeDistance
            )
        ) {
            state = State.ATTACK;

            stateDelay = 0;
        }

        mech.hitByPlayerFlag = false;

        float distToTarget = Vector3.Distance(targetPos, mech.transform.position);
        // if (state == State.ATTACK && distToTarget < minMoveDist) {
        //     // Set another random offset to move continuously while attacking.
        //     SetRandomOffset();
        // }

        if (distToTarget > minMoveDist) {
            mech.Move((targetPos - mech.transform.position).normalized);
        }
        else {
            mech.Move(Vector3.zero);
        }

        if (state == State.ATTACK) {
            mech.Aim(player.skeleton.cockpit.transform.position);
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
