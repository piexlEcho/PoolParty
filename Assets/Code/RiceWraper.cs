using System.Collections.Generic;
using UnityEngine;

public class RiceWraper : MonoBehaviour
{
    [System.Serializable]
    public enum ShapeType
    {
        Sphere,
        Ellipsoid,
        Box
    }

    [Header("米粒设置")]
    [Tooltip("米粒预制体数组，生成时随机选择")]
    public GameObject[] ricePrefabs = new GameObject[0];
    [Tooltip("米粒数量")]
    public int riceCount = 100;
    [Tooltip("形状类型")]
    public ShapeType shape = ShapeType.Sphere;
    [Tooltip("球形半径（仅 Sphere 模式）")]
    public float sphereRadius = 1.5f;
    [Tooltip("椭球/长方体半轴尺寸 (x, y, z)")]
    public Vector3 boxSize = new Vector3(1.5f, 0.8f, 1.5f);

    [Header("散开设置")]
    [Tooltip("爆炸力强度")]
    public float explosionForce = 5f;
    [Tooltip("爆炸影响半径")]
    public float explosionRadius = 2f;

    [Header("调试")]
    [SerializeField] private bool triggerScatter = false; // 勾选后立即散开
    public bool IsScattered { get; private set; } = false;

    // 内部数据
    private List<GameObject> riceGrains = new List<GameObject>();
    private GameObject fishPiece;

    void Update()
    {
        // 调试触发散开
        if (triggerScatter && !IsScattered)
        {
            Scatter();
            triggerScatter = false; // 重置
        }
    }

    /// <summary>
    /// 创建寿司（米饭 + 鱼肉）
    /// </summary>
    /// <param name="position">世界坐标</param>
    /// <param name="shapeType">形状</param>
    /// <param name="shapeSize">形状尺寸（球形取x，椭球/长方体取完整Vector3）</param>
    /// <param name="fishPrefab">鱼肉预制体（需带Rigidbody）</param>
    /// <param name="riceCountOverride">米粒数量（可选）</param>
    /// <param name="fishHeightOffset">鱼肉相对米饭顶部的偏移量</param>
    public void BuildSushi(Vector3 position, ShapeType shapeType, Vector3 shapeSize, GameObject fishPrefab, int riceCountOverride = -1, float fishHeightOffset = 0.2f)
    {
        // 清除已有内容
        Clear();

        // 设置参数
        transform.position = position;
        shape = shapeType;
        if (riceCountOverride > 0) riceCount = riceCountOverride;

        switch (shapeType)
        {
            case ShapeType.Sphere:
                sphereRadius = shapeSize.x;
                break;
            default:
                boxSize = shapeSize;
                break;
        }

        // 生成米饭
        GenerateRice();

        // 生成鱼肉
        if (fishPrefab != null)
        {
            fishPiece = Instantiate(fishPrefab, transform);
            fishPiece.transform.localPosition = GetFishLocalPosition(fishHeightOffset);
            // 不修改旋转，保持预制体原有方向
            // fishPiece.transform.localRotation = Quaternion.identity; // 去掉这行

            // 设置为 kinematic，使其跟随父物体
            Rigidbody fishRb = fishPiece.GetComponent<Rigidbody>();
            if (fishRb != null) fishRb.isKinematic = true;
            else Debug.LogWarning("鱼肉预制体缺少 Rigidbody 组件！");
        }

        IsScattered = false;
    }

    /// <summary>
    /// 散开寿司（米饭和鱼肉飞溅）
    /// </summary>
    /// <param name="explosionCenter">爆炸中心（世界坐标），可选</param>
    public void Scatter(Vector3? explosionCenter = null)
    {
        if (IsScattered) return;
        IsScattered = true;

        Vector3 center = explosionCenter ?? transform.position;

        // 散开米饭
        foreach (GameObject rice in riceGrains)
        {
            if (rice == null) continue;

            rice.transform.SetParent(null);
            Rigidbody rb = rice.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                AddExplosionForce(rb, rice.transform.position, center);
            }
        }

        // 散开鱼肉
        if (fishPiece != null)
        {
            fishPiece.transform.SetParent(null);
            Rigidbody fishRb = fishPiece.GetComponent<Rigidbody>();
            if (fishRb != null)
            {
                fishRb.isKinematic = false;
                AddExplosionForce(fishRb, fishPiece.transform.position, center);
            }
        }

        // 清空引用
        riceGrains.Clear();
        fishPiece = null;

        // 可选：禁用此脚本，避免重复散开
        // enabled = false;
    }

    // ------------------- 私有方法 -------------------

    private void GenerateRice()
    {
        if (ricePrefabs.Length == 0)
        {
            Debug.LogError("RiceWraper: 未设置米粒预制体数组！");
            return;
        }

        for (int i = 0; i < riceCount; i++)
        {
            // 随机选择米粒外观
            GameObject prefab = ricePrefabs[Random.Range(0, ricePrefabs.Length)];
            if (prefab == null) continue;

            Vector3 localPos = GetRandomSurfacePoint();
            GameObject rice = Instantiate(prefab, transform);
            rice.transform.localPosition = localPos;
            rice.transform.localRotation = Random.rotation; // 米粒随机旋转，增加自然感

            Rigidbody rb = rice.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            else Debug.LogWarning("米粒预制体缺少 Rigidbody 组件！");

            riceGrains.Add(rice);
        }
    }

    private Vector3 GetRandomSurfacePoint()
    {
        Vector3 point = Vector3.zero;
        switch (shape)
        {
            case ShapeType.Sphere:
                point = Random.onUnitSphere * sphereRadius;
                break;
            case ShapeType.Ellipsoid:
                Vector3 unit = Random.onUnitSphere;
                point.x = unit.x * boxSize.x;
                point.y = unit.y * boxSize.y;
                point.z = unit.z * boxSize.z;
                break;
            case ShapeType.Box:
                // 随机选择一个面
                int face = Random.Range(0, 6);
                float hx = boxSize.x, hy = boxSize.y, hz = boxSize.z;
                switch (face)
                {
                    case 0: point = new Vector3(Random.Range(-hx, hx), Random.Range(-hy, hy), hz); break;
                    case 1: point = new Vector3(Random.Range(-hx, hx), Random.Range(-hy, hy), -hz); break;
                    case 2: point = new Vector3(Random.Range(-hx, hx), hy, Random.Range(-hz, hz)); break;
                    case 3: point = new Vector3(Random.Range(-hx, hx), -hy, Random.Range(-hz, hz)); break;
                    case 4: point = new Vector3(hx, Random.Range(-hy, hy), Random.Range(-hz, hz)); break;
                    case 5: point = new Vector3(-hx, Random.Range(-hy, hy), Random.Range(-hz, hz)); break;
                }
                break;
        }
        return point;
    }

    private Vector3 GetFishLocalPosition(float heightOffset)
    {
        switch (shape)
        {
            case ShapeType.Sphere:
                return new Vector3(0, sphereRadius + heightOffset, 0);
            case ShapeType.Ellipsoid:
                return new Vector3(0, boxSize.y + heightOffset, 0);
            case ShapeType.Box:
                return new Vector3(0, boxSize.y + heightOffset, 0);
            default:
                return Vector3.zero;
        }
    }

    private void AddExplosionForce(Rigidbody rb, Vector3 position, Vector3 center)
    {
        Vector3 dir = position - center;
        float dist = dir.magnitude;
        if (dist < explosionRadius)
        {
            float force = explosionForce * (1 - dist / explosionRadius);
            rb.AddForce(dir.normalized * force, ForceMode.Impulse);
        }
        else
        {
            // 半径外给予微小随机力
            rb.AddForce(Random.insideUnitSphere * 1.5f, ForceMode.Impulse);
        }
    }

    private void Clear()
    {
        foreach (GameObject rice in riceGrains)
            if (rice != null) Destroy(rice);
        riceGrains.Clear();

        if (fishPiece != null) Destroy(fishPiece);
        fishPiece = null;
    }

    // ------------------- 编辑器辅助 -------------------
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        switch (shape)
        {
            case ShapeType.Sphere:
                Gizmos.DrawWireSphere(transform.position, sphereRadius);
                break;
            case ShapeType.Ellipsoid:
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, boxSize * 2);
                Gizmos.DrawWireSphere(Vector3.zero, 1f);
                Gizmos.matrix = Matrix4x4.identity;
                break;
            case ShapeType.Box:
                Gizmos.DrawWireCube(transform.position, boxSize * 2);
                break;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
}