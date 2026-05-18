using System.Collections;
using UnityEngine;
using DG.Tweening;

public class CameraSequenceController : MonoBehaviour
{
    public static CameraSequenceController Instance { get; private set; }

    [Header("Camera")]
    public ArcadeCameraSwitch arcadeCamera;
    public CameraFollowTopDown cameraFollowTopDown;

    [Header("Attract Shot — camera watching rice ball up")]
    public Transform attractViewPoint;
    public float attractTransitionDuration = 1f;
    public Ease attractEase = Ease.InOutQuad;

    [Header("Scoreboard")]
    public Transform scoreboardViewPoint;
    public float scoreboardTransitionDuration = 1f;
    public Ease scoreboardEase = Ease.InOutQuad;
    public float scoreboardHoldDuration = 4f;

    [Header("Timing")]
    public float delayBeforeScoreboard = 1f;

    private Transform _cam;

    void Awake()
    {
        Instance = this;
        _cam = Camera.main.transform;
    }

    public void BeginEndSequence()
    {
        StartCoroutine(EndSequenceRoutine());
    }

    IEnumerator EndSequenceRoutine()
    {
        if (cameraFollowTopDown != null)
            cameraFollowTopDown.StopFollowing();

        if (cameraFollowTopDown != null)
            cameraFollowTopDown.enabled = false;

        yield return PanCamera(attractViewPoint, attractTransitionDuration, attractEase);

        yield return new WaitForSeconds(delayBeforeScoreboard);

        yield return PanCamera(scoreboardViewPoint, scoreboardTransitionDuration, scoreboardEase);

        yield return new WaitForSeconds(scoreboardHoldDuration);

        GameState.Phase = GamePhase.Results;

        arcadeCamera?.ToggleCamera();
    }

    IEnumerator PanCamera(Transform target, float duration, Ease ease)
    {
        if (target == null) yield break;

        Tween moveTween = _cam.DOMove(target.position, duration).SetEase(ease);
        Tween rotTween = _cam.DORotateQuaternion(target.rotation, duration).SetEase(ease);

        yield return moveTween.WaitForCompletion();
    }
}