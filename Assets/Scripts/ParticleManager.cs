using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ParticleManager : MonoBehaviour
{
    public GameObject bulletImpact;
    public GameObject bulletTrail;
    public GameObject missile;
    public GameObject missileExplosion;

    // @Todo: Implement pooling.

    public void CreateBulletImpact(Vector3 pos, Vector3 normal) {
        GameObject cloned = GameObject.Instantiate(bulletImpact);

        cloned.transform.position = pos;
        cloned.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
    }

    public void CreateMissileExplosion(Vector3 pos) {
        GameObject cloned = GameObject.Instantiate(missileExplosion);

        cloned.transform.position = pos;
    }

    public void CreateBulletTrail(Vector3 start, Vector3 end) {
        GameObject obj = Instantiate(bulletTrail);

        LineRenderer lineRenderer = obj.GetComponent<LineRenderer>();

        lineRenderer.SetPositions(new Vector3[] {
            start, end
        });

        // @Hardcoded
        Color trailColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        const float lifetime = 1;

        lineRenderer.startColor = lineRenderer.endColor = trailColor;
        DOTween.To(() => lineRenderer.startColor, x => lineRenderer.startColor = x, new Color(trailColor.r, trailColor.g, trailColor.b, 0), lifetime);
        DOTween.To(() => lineRenderer.endColor, x => lineRenderer.endColor = x, new Color(trailColor.r, trailColor.g, trailColor.b, 0), lifetime).OnComplete(() => {
            Destroy(obj);
        });
    }

    // Dictionary<string, ParticlePool> pools = new Dictionary<string, ParticlePool>();

    // void Awake() {
    //     pools["bulletImpact"] = new ParticlePool("bulletImpact", bulletImpact);
    // }

    // public void CreateBulletImpact(Vector3 position, Vector3 normal) {
    //     GameObject obj = pools["bulletImpact"].Pop(position, Quaternion.FromToRotation(Vector3.up, normal));

    //     particleSystem.Play();

    //     return particleSystem;
    // }
}

// class ParticlePool {
//     public string name;
//     public GameObject prefab;
//     public float lifetime;
//     public List<GameObject> queue = new List<GameObject>();

//     public ParticlePool(string name, GameObject prefab, float lifetime) {
//         this.name = name;
//         this.prefab = prefab;
//         this.lifetime = lifetime;
//     }

//     public ParticleSystem Pop(Vector3 position, Quaternion rotation) {
//         ParticleSystem particleSystem = queue.Find(x => x.isStopped);

//         if (particleSystem == null) {
//             GameObject cloned = GameObject.Instantiate(prefab);

//             particleSystem = cloned.GetComponent<ParticleSystem>();

//             queue.Add(particleSystem);
//         }

//         particleSystem.transform.position = position;
//         particleSystem.transform.rotation = rotation;

//         return particleSystem;
//     }
// }