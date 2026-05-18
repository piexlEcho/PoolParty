using UnityEngine;
using System.Collections.Generic;

public class SushiBreakable : MonoBehaviour
{
    [Header("Rice Grain")]
    public GameObject riceGrainPrefab;
    public Transform[] spawnPoints;

    [Header("Explosion Force")]
    public float baseExplosionForce = 10f;
    public float randomForceRange = 3f;
    public float upwardBias = 2f;

    [Header("Collision")]
    public string projectileTag = "Projectile";

    private bool broken = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (broken) return;

        if (!collision.gameObject.CompareTag(projectileTag))
            return;

        // 镜头冲击效果
        CameraImpactEffect.Instance?.TriggerImpact();

        // 获取碰撞信息
        ContactPoint contact = collision.GetContact(0);

        Vector3 impactPoint = contact.point;
        Vector3 impactDir = collision.relativeVelocity.normalized;
        float impactSpeed = collision.relativeVelocity.magnitude;

        BreakApart(impactPoint, impactDir, impactSpeed);
    }

    void BreakApart(
        Vector3 impactPoint,
        Vector3 impactDir,
        float impactStrength
    )
    {
        broken = true;

        // 继承寿司原本速度
        Vector3 inheritedVelocity =
            rb != null ? rb.velocity : Vector3.zero;

        Vector3 inheritedAngular =
            rb != null ? rb.angularVelocity : Vector3.zero;

        // 隐藏整体
        SetWholeVisible(false);

        List<GameObject> grains = new List<GameObject>();

        foreach (Transform pt in spawnPoints)
        {
            if (pt == null) continue;

            Vector3 worldPos =
                transform.TransformPoint(pt.localPosition);

            Quaternion worldRot =
                transform.rotation * pt.localRotation;

            GameObject grain = Instantiate(
                riceGrainPrefab,
                worldPos,
                worldRot
            );

            Rigidbody grainRb = grain.GetComponent<Rigidbody>();

            if (grainRb != null)
            {
                // 继承速度
                grainRb.velocity = inheritedVelocity;
                grainRb.angularVelocity = inheritedAngular;

                // 爆炸力
                Vector3 force =
                    impactDir * impactStrength * baseExplosionForce;

                // 随机扩散
                force += Random.insideUnitSphere * randomForceRange;

                // 向上偏移
                force += Vector3.up * upwardBias;

                grainRb.AddForce(force, ForceMode.Impulse);
            }

            grains.Add(grain);
        }

        // 找到速度最快的米粒
        GameObject mainGrain = null;
        float maxSpeed = 0f;

        foreach (GameObject grain in grains)
        {
            Rigidbody grainRb = grain.GetComponent<Rigidbody>();

            if (grainRb == null) continue;

            float speed = grainRb.velocity.magnitude;

            if (speed > maxSpeed)
            {
                maxSpeed = speed;
                mainGrain = grain;
            }
        }

        // Camera 跟随主米粒
        if (mainGrain != null)
        {
            CameraFollowTopDown follow =
                Camera.main
                .GetComponentInParent<CameraFollowTopDown>();

            if (follow != null)
            {
                follow.SetTarget(mainGrain.transform);
            }
        }

        Destroy(gameObject, 0.05f);
    }

    void SetWholeVisible(bool visible)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = visible;
        }

        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = visible;
        }

        if (rb != null)
        {
            rb.isKinematic = !visible;
        }
    }

    [ContextMenu("Generate Spawn Points From Mesh")]
    void GenerateSpawnPointsFromMesh()
    {
        MeshFilter mf = GetComponent<MeshFilter>();

        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogError("需要 MeshFilter 和 Mesh");
            return;
        }

        // 删除旧点位
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            if (child.name.StartsWith("SpawnPoint"))
            {
                DestroyImmediate(child.gameObject);
            }
        }

        Vector3[] verts = mf.sharedMesh.vertices;

        List<Transform> points = new List<Transform>();

        for (int i = 0; i < verts.Length; i++)
        {
            GameObject go = new GameObject($"SpawnPoint_{i}");

            go.transform.parent = transform;

            go.transform.localPosition = verts[i];
            go.transform.localRotation = Quaternion.identity;

            points.Add(go.transform);
        }

        spawnPoints = points.ToArray();

        Debug.Log($"已生成 {spawnPoints.Length} 个 SpawnPoints");
    }
}