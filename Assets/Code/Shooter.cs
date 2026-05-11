using DG.Tweening;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject bulletPrefab, typeTextPrefab;
    public Transform shootPoint;

    public KeyCode startChargeKey = KeyCode.P;
    public KeyCode addPowerKey = KeyCode.B;

    public float baseSpeed = 20f;
    public float maxChargeTime = 1f;
    public float maxSpeed = 200f;
    public float growthRate = 0.35f;

    private float currentChargeTime = 0f;
    private int bPressCount = 0;
    private bool isCharging = false;

    private Camera mainCam;
    private float FOV, wideFOV;
    private bool fired;
    private float timePassed;
    private Quaternion currentRotation;

    public bool useCustomDirection = false;// 是否使用自定义方向
    public Vector3 customDirection = Vector3.forward;

    private void Awake()
    {
        mainCam = Camera.main;
        FOV = mainCam.fieldOfView;
        wideFOV = FOV + 20f;
        currentRotation = mainCam.transform.rotation;
    }

    private void Update()
    {
        // 开始蓄力
        if (Input.GetKeyDown(startChargeKey) && !isCharging)
        {
            StartCharge();
        }

        if (isCharging)
        {
            currentChargeTime += Time.deltaTime;

            // 按 B 增加蓄力
            if (Input.GetKeyDown(addPowerKey))
            {
                bPressCount++;
                GameObject textInstance = Instantiate(typeTextPrefab, new Vector3(0, 0, -100f), Quaternion.identity);
                textInstance.GetComponent<TypeText>().TextToShow = "B";
                textInstance.GetComponent<TypeText>().bScale = 1f + bPressCount/10f;
                mainCam.fieldOfView = Mathf.Lerp(FOV, wideFOV, currentChargeTime / maxChargeTime);
                mainCam.transform.rotation = Quaternion.Lerp(currentRotation, Quaternion.Euler(28f, 0, 0), currentChargeTime/maxChargeTime);
            }

            // 到时间自动发射
            if (currentChargeTime >= maxChargeTime)
            {
                fired = true;
                Fire();
                ResetCharge();
            }
        }

        if (fired)
        {
            timePassed += Time.deltaTime;
            float t = timePassed / 6f;
            mainCam.fieldOfView = Mathf.Lerp(wideFOV, FOV, t);
            mainCam.transform.rotation = Quaternion.Lerp(Quaternion.Euler(28f, 0, 0), currentRotation, t);
            if (t >= 1f) { fired = false; }
        }
        else timePassed = 0f;
    }

    void StartCharge()
    {
        isCharging = true;
        currentChargeTime = 0f;
        bPressCount = 0;
        GameObject textInstance = Instantiate(typeTextPrefab, new Vector3(0, 0, -100f), Quaternion.identity);
        textInstance.GetComponent<TypeText>().TextToShow = "P";
    }

    void ResetCharge()
    {
        isCharging = false;
        currentChargeTime = 0f;
        bPressCount = 0;
    }

    void Fire()
    {
        mainCam.fieldOfView = FOV;
        mainCam.transform.rotation = currentRotation;

        if (bulletPrefab == null || shootPoint == null) return;

        Vector3 dir = GetShootDirection();

        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            float speed = CalculateSpeed();
            rb.velocity = dir.normalized * speed;
        }

        bullet.transform.forward = dir.normalized;
    }

    float CalculateSpeed()
    {
        if (bPressCount <= 0)
        {
            return baseSpeed;
        }

        float t = 1f - Mathf.Exp(-growthRate * bPressCount);

        float speed = Mathf.Lerp(baseSpeed, maxSpeed, t);

        return Mathf.Min(speed, maxSpeed);
    }

    private Vector3 GetShootDirection()
    {
        if (useCustomDirection)
            return customDirection.normalized;
        else
            return shootPoint.forward;
    }

    // ---------- 外部接口 ----------
    public void SetCustomDirection(Vector3 newDirection)
    {
        useCustomDirection = true;
        customDirection = newDirection.normalized;
    }

    public void UseShootPointForward()
    {
        useCustomDirection = false;
    }

    public void RotateShootPoint(Quaternion newRotation)
    {
        if (shootPoint != null)
            shootPoint.rotation = newRotation;
    }
}