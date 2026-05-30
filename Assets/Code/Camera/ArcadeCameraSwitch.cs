using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class ArcadeCameraSwitch : MonoBehaviour
{
    [Header("Transform")]
    [SerializeField] public Transform targetPoint;
    [SerializeField] public float cameraMoveDuration = 1.0f;
    [SerializeField] public Ease easeType = Ease.InOutQuad;

    [Header("Toggle")]
    [SerializeField] public MonoBehaviour[] scriptsToToggle; // Assign CameraFollowTopDown + Shooter

    [Header("Rotation")]
    [SerializeField] public Vector3 originalPosition;
    [SerializeField] public Quaternion originalRotation;
    [SerializeField] public bool isAtTarget = false;
    [SerializeField] private Tween moveTween;
    [SerializeField] private Tween rotateTween;

    [Header("Input")]
    public bool escEnabled = false;

    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        GameState.Phase = GamePhase.Menu;

        // Disable gameplay scripts at start since we begin at the slot machine
        foreach (var script in scriptsToToggle) script.enabled = false;
    }

    public void Update()
    {
        if (!escEnabled) return; // ← guard
        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleCamera();
    }

    public void ToggleCamera()
    {
        moveTween?.Kill();
        rotateTween?.Kill();

        if (!isAtTarget)
        {
            // Panning TO slot machine — disable gameplay scripts immediately
            GameState.Phase = GamePhase.Menu;
            foreach (var script in scriptsToToggle) script.enabled = false;

            moveTween = transform.DOMove(targetPoint.position, cameraMoveDuration).SetEase(easeType);
            rotateTween = transform.DORotateQuaternion(targetPoint.rotation, cameraMoveDuration)
                .SetEase(easeType)
                .OnComplete(() =>
                {
                    // Only clear rice if a round just finished, not a casual ESC mid-game
                    if (GameState.Phase == GamePhase.Results)
                        ClearRiceGrains();

                    FindObjectOfType<SlotMachine>()?.ResetForNewRound();
                });
        }
        else
        {
            // Panning BACK to game — re-enable scripts once arrived
            moveTween = transform.DOMove(originalPosition, cameraMoveDuration).SetEase(easeType);
            rotateTween = transform.DORotateQuaternion(originalRotation, cameraMoveDuration)
                .SetEase(easeType)
                .OnComplete(() =>
                {
                    foreach (var script in scriptsToToggle) script.enabled = true;
                    GameState.Phase = GamePhase.Shooting;
                    RoundManager.Instance?.StartRound();
                });
        }

        isAtTarget = !isAtTarget;
    }

    void ClearRiceGrains()
    {
        GameObject[] grains = GameObject.FindGameObjectsWithTag("RiceGrain");
        foreach (var grain in grains)
            Destroy(grain);
    }
    void ClearAllRice()
    {
        foreach (var grain in GameObject.FindGameObjectsWithTag("RiceGrain"))
            Destroy(grain);

        foreach (var scored in GameObject.FindGameObjectsWithTag("Score"))
            Destroy(scored);

        FishSpawner.Instance?.ClearFish();
    }
}