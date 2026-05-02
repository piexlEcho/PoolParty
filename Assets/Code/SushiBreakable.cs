using UnityEngine;
using System.Collections.Generic;

public class SushiBreakable : MonoBehaviour
{
    [Header("米粒生成")]
    public GameObject riceGrainPrefab;          // 米粒预制体
    public Transform[] spawnPoints;             // 米粒生成点位（局部坐标）

    [Header("爆炸力参数")]
    public float baseExplosionForce = 10f;      // 基础爆炸力
    public float randomForceRange = 3f;         // 随机力范围
    public float upwardBias = 2f;               // 向上偏移

    [Header("碰撞设置")]
    public string projectileTag = "Projectile"; // 能触发散开的标签

    private bool broken = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (broken) return;
        if (!collision.gameObject.CompareTag(projectileTag)) return;

        // 获取撞击信息
        ContactPoint contact = collision.GetContact(0);
        Vector3 impactPoint = contact.point;
        Vector3 impactDir = collision.relativeVelocity.normalized;
        float impactSpeed = collision.relativeVelocity.magnitude;

        BreakApart(impactPoint, impactDir, impactSpeed);
    }

    void BreakApart(Vector3 impactPoint, Vector3 impactDir, float impactStrength)
    {
        broken = true;

        // 记录寿司当前速度（用于继承）
        Vector3 inheritedVelocity = rb != null ? rb.velocity : Vector3.zero;
        Vector3 inheritedAngular = rb != null ? rb.angularVelocity : Vector3.zero;

        // 隐藏寿司整体
        SetWholeVisible(false);

        // 生成米粒
        List<GameObject> grains = new List<GameObject>();
        foreach (Transform pt in spawnPoints)
        {
            if (pt == null) continue;

            Vector3 worldPos = transform.TransformPoint(pt.localPosition);
            Quaternion worldRot = transform.rotation * pt.localRotation;

            GameObject grain = Instantiate(riceGrainPrefab, worldPos, worldRot);
            Rigidbody grainRb = grain.GetComponent<Rigidbody>();

            if (grainRb != null)
            {
                // 继承速度
                grainRb.velocity = inheritedVelocity;
                grainRb.angularVelocity = inheritedAngular;

                // 计算施加的力：基础方向力 + 随机偏移 + 向上分量
                Vector3 force = (impactDir * 1) * (impactStrength * baseExplosionForce);
                force += Random.insideUnitSphere * randomForceRange;
                force += Vector3.up * upwardBias;// 应用力

                grainRb.AddForce(force, ForceMode.Impulse);
            }

            grains.Add(grain);
        }
        // 通知摄像机开始俯视追踪
        CameraTopDownTracker tracker = Camera.main?.GetComponent<CameraTopDownTracker>();
        if (tracker != null)
        {
            tracker.StartTracking();
        }

        // 销毁寿司整体（延迟一点确保生成完成）
        Destroy(gameObject, 0.05f);
    }

    void SetWholeVisible(bool visible)
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = visible;
        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = visible;
        if (rb != null) rb.isKinematic = !visible;
    }

    [ContextMenu("Generate Spawn Points From Mesh")]
    void GenerateSpawnPointsFromMesh()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogError("需要 MeshFilter 和网格");
            return;
        }

        // 清除旧有点位子物体
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).name.StartsWith("SpawnPoint"))
                DestroyImmediate(transform.GetChild(i).gameObject);
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
        Debug.Log($"已生成 {spawnPoints.Length} 个点位");
    }
}