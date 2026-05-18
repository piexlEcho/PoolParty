using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class StartupSequence : MonoBehaviour
{
    [Header("References")]
    public ArcadeCameraSwitch arcadeCamera;
    public MonoBehaviour[] allGameplayScripts; // Shooter, CameraFollowTopDown,
                                               // SlotMachine, FishSpawner, etc.
    [Header("Blur")]
    public Material blurMaterial;              // A simple blur post-process material
    public float blurAmount = 3f;              // Shader blur intensity property

    [Header("UI")]
    public CanvasGroup blackOverlay;           // A full screen black Image CanvasGroup
    public GameObject logoObject;              // Your game logo UI object
    public TextMeshProUGUI pressEscText;       // "Press ESC to begin" TMP text

    [Header("Timing")]
    public float fadeInDuration = 1.5f;        // Black fade in duration
    public float logoAppearDelay = 0.5f;       // Delay before logo appears
    public float logoFadeDuration = 1f;        // Logo fade in duration
    public float textAppearDelay = 1f;         // Delay before press ESC appears
    public float textFadeDuration = 1f;        // Press ESC fade in duration

    private bool _sequenceStarted = false;
    private CanvasGroup _logoCG;
    private CanvasGroup _textCG;

    void Awake()
    {
        // Block everything at startup
        GameState.Phase = GamePhase.Menu;
        foreach (var script in allGameplayScripts)
            if (script != null) script.enabled = false;

        // Also disable ArcadeCameraSwitch so ESC doesn't fire early
        arcadeCamera.enabled = false;
    }

    void Start()
    {
        // Set up UI starting states
        blackOverlay.alpha = 1f;
        blackOverlay.gameObject.SetActive(true);

        _logoCG = logoObject.GetComponent<CanvasGroup>();
        if (_logoCG == null) _logoCG = logoObject.AddComponent<CanvasGroup>();
        _logoCG.alpha = 0f;
        logoObject.SetActive(true);

        _textCG = pressEscText.GetComponent<CanvasGroup>();
        if (_textCG == null) _textCG = pressEscText.gameObject.AddComponent<CanvasGroup>();
        _textCG.alpha = 0f;

        StartCoroutine(IntroRoutine());
    }

    void Update()
    {
        // Only listen for ESC once the intro has fully played out
        if (_sequenceStarted) return;

        if (Input.GetKeyDown(KeyCode.Escape) && _textCG.alpha >= 1f)
        {
            _sequenceStarted = true;
            StartCoroutine(TransitionToSlotMachine());
        }
    }

    IEnumerator IntroRoutine()
    {
        // 1. Fade out black overlay to reveal blurred sea background
        yield return blackOverlay.DOFade(0f, fadeInDuration).WaitForCompletion();

        // 2. Fade in logo
        yield return new WaitForSeconds(logoAppearDelay);
        yield return _logoCG.DOFade(1f, logoFadeDuration).WaitForCompletion();

        // 3. Fade in "Press ESC to begin"
        yield return new WaitForSeconds(textAppearDelay);
        yield return _textCG.DOFade(1f, textFadeDuration).WaitForCompletion();

        // Now waiting for player to press ESC (handled in Update)
    }

    IEnumerator TransitionToSlotMachine()
    {
        // 1. Fade out UI
        _logoCG.DOFade(0f, 0.5f);
        yield return _textCG.DOFade(0f, 0.5f).WaitForCompletion();

        // 2. Fade to black briefly for clean transition
        yield return blackOverlay.DOFade(1f, 0.4f).WaitForCompletion();

        // 3. Enable ArcadeCameraSwitch and pan to slot machine
        arcadeCamera.enabled = true;
        arcadeCamera.ToggleCamera();

        // 4. Wait for camera to arrive at slot machine
        yield return new WaitForSeconds(arcadeCamera.cameraMoveDuration);

        // 5. Fade black back out
        yield return blackOverlay.DOFade(0f, 0.5f).WaitForCompletion();

        // 6. Enable slot machine scripts only
        EnableSlotMachineScripts();

        // 7. Hide overlay entirely
        blackOverlay.gameObject.SetActive(false);

        StartupBlur blur = Camera.main.GetComponent<StartupBlur>();
        if (blur != null)
        {
            float elapsed = 0f;
            float fadeDuration = 0.8f; // Adjust to taste
            float startBlur = blur.blurSize;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                blur.blurSize = Mathf.Lerp(startBlur, 0f, elapsed / fadeDuration);
                yield return null;
            }

            blur.blurSize = 0f;
            blur.enabled = false;
        }
    }

    void EnableSlotMachineScripts()
    {
        // Only enable slot machine related scripts here,
        // gameplay scripts stay off until confirm is pressed
        foreach (var script in allGameplayScripts)
        {
            if (script is SlotMachine || script is FishSpawner)
                script.enabled = true;
        }
    }
}