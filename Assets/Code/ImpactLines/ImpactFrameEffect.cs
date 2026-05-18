using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class ImpactFrameEffect : MonoBehaviour
{
    [Header("Timing")]
    public float freezeDuration = 0.08f;
    public float effectInTime = 0.03f;
    public float effectOutTime = 0.18f;

    [Header("Shader")]
    public Material impactMaterial;

    [Header("FOV Punch")]
    public float fovReduction = 15f;   // How much FOV decreases
    public float fovInTime = 0.03f; // How fast it zooms in
    public float fovOutTime = 0.2f;  // How fast it eases back

    private Material _mat;
    private float _intensity = 0f;
    private bool _isPlaying = false;
    private Camera _cam;
    private float _baseFOV;

    private static ImpactFrameEffect _instance;
    public static ImpactFrameEffect Instance => _instance;

    void Awake()
    {
        _instance = this;
        _mat = impactMaterial;
        _cam = GetComponent<Camera>();
        _baseFOV = _cam.fieldOfView;
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

    public void TriggerImpactFrame()
    {
        if (_isPlaying) return;
        _baseFOV = _cam.fieldOfView;
        StartCoroutine(ImpactRoutine());
    }

    IEnumerator ImpactRoutine()
    {
        _isPlaying = true;
        enabled = true;

        float targetFOV = _baseFOV - fovReduction;

        // ramp + fov change
        float elapsed = 0f;
        while (elapsed < effectInTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / effectInTime;
            _intensity = Mathf.Lerp(0f, 1f, t);
            _cam.fieldOfView = Mathf.Lerp(_baseFOV, targetFOV, t);
            yield return null;
        }
        _intensity = 1f;
        _cam.fieldOfView = targetFOV;

        // freeze
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(freezeDuration);
        Time.timeScale = 1f;

        // fade out + fov decrease
        elapsed = 0f;
        while (elapsed < effectOutTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / effectOutTime;
            _intensity = Mathf.Lerp(1f, 0f, t);
            _cam.fieldOfView = Mathf.Lerp(targetFOV, _baseFOV, t);
            yield return null;
        }

        _intensity = 0f;
        _cam.fieldOfView = _baseFOV;
        _isPlaying = false;
        enabled = false;
    }
}