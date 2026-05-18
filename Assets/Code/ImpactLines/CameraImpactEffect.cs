using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraImpactEffect : MonoBehaviour
{
    [Header("Freeze")]
    public float freezeDuration = 0.03f;

    [Header("Impact Shader")]
    public Material impactMaterial;

    [Header("Shader Timing")]
    public float effectInTime = 0.02f;

    public float effectOutTime = 0.12f;

    [Header("FOV Punch")]
    public float fovReduction = 3f;

    [Header("Shake")]
    public float shakeDuration = 0.08f;

    public float shakeStrength = 0.08f;

    private Material _mat;

    private Camera _cam;

    private float _intensity = 0f;

    private float _baseFOV;

    private bool _isPlaying = false;

    private Vector3 _originalLocalPos;

    public static CameraImpactEffect Instance;

    void Awake()
    {
        Instance = this;

        _cam = GetComponent<Camera>();

        _mat = impactMaterial;

        _baseFOV = _cam.fieldOfView;

        _originalLocalPos = transform.localPosition;

        enabled = false;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (_mat != null && _intensity > 0f)
        {
            _mat.SetFloat("_Intensity", _intensity);

            Graphics.Blit(src, dest, _mat);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }

    public void TriggerImpact()
    {
        if (_isPlaying) return;

        StartCoroutine(ImpactRoutine());
    }

    IEnumerator ImpactRoutine()
    {
        _isPlaying = true;

        enabled = true;

        _baseFOV = _cam.fieldOfView;

        float targetFOV =
            _baseFOV - fovReduction;

        StartCoroutine(
            ShakeRoutine(shakeDuration, shakeStrength)
        );

        // IN
        float elapsed = 0f;

        while (elapsed < effectInTime)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = elapsed / effectInTime;

            _intensity = Mathf.Lerp(0f, 1f, t);

            _cam.fieldOfView =
                Mathf.Lerp(_baseFOV, targetFOV, t);

            yield return null;
        }

        _intensity = 1f;

        // Freeze
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(
            freezeDuration
        );

        Time.timeScale = 1f;

        // OUT
        elapsed = 0f;

        while (elapsed < effectOutTime)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / effectOutTime;

            _intensity = Mathf.Lerp(1f, 0f, t);

            _cam.fieldOfView =
                Mathf.Lerp(targetFOV, _baseFOV, t);

            yield return null;
        }

        _intensity = 0f;

        _cam.fieldOfView = _baseFOV;

        _isPlaying = false;

        enabled = false;
    }

    IEnumerator ShakeRoutine(
        float duration,
        float strength
    )
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            Vector3 randomOffset =
                Random.insideUnitSphere * strength;

            randomOffset.z = 0f;

            transform.localPosition =
                _originalLocalPos + randomOffset;

            yield return null;
        }

        transform.localPosition =
            _originalLocalPos;
    }
}