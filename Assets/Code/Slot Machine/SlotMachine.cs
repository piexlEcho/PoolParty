using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotMachine : MonoBehaviour
{
    [Header("Fish Types")]
    public FishData[] fishTypes;

    [Header("Slot UI")]
    public Image[] slotImages;
    public TextMeshProUGUI[] slotLabels;
    public TextMeshProUGUI multiplierText;

    [Header("Buttons")]
    public Button spinButton;
    public Button backButton;

    [Header("Spin Settings")]
    public float spinDuration = 2f;
    public float spinTickInterval = 0.08f;
    public float autoConfirmDelay = 1.5f;

    [Header("References")]
    public ArcadeCameraSwitch arcadeCamera;
    public FishSpawner fishSpawner;

    private FishData _result;
    private bool _isSpinning = false;

    void Start()
    {
        SetSlotsVisible(false);
        spinButton.onClick.AddListener(OnSpin);
    }

    public void OnSpin()
    {
        if (_isSpinning) return;

        spinButton.interactable = false;
        if (backButton != null) backButton.interactable = false;
        arcadeCamera.escEnabled = false;
        SetSlotsVisible(true);

        if (multiplierText != null)
            multiplierText.text = "?";

        RoundManager.Instance?.SpawnRiceBall();
        StartCoroutine(SpinRoutine());
    }

    IEnumerator SpinRoutine()
    {
        _isSpinning = true;
        _result = PickWeightedFish();

        float elapsed = 0f;
        while (elapsed < spinDuration)
        {
            foreach (var img in slotImages)
            {
                if (img != null)
                    img.sprite = fishTypes[Random.Range(0, fishTypes.Length)].icon;
            }
            foreach (var label in slotLabels)
            {
                if (label != null)
                    label.text = fishTypes[Random.Range(0, fishTypes.Length)].fishName;
            }

            yield return new WaitForSeconds(spinTickInterval);
            elapsed += spinTickInterval;
        }

        foreach (var img in slotImages)
            if (img != null) img.sprite = _result.icon;

        foreach (var label in slotLabels)
            if (label != null) label.text = _result.fishName;

        if (multiplierText != null)
            multiplierText.text = $"{_result.multiplier}x";

        _isSpinning = false;

        yield return new WaitForSeconds(autoConfirmDelay);

        AutoConfirm();
    }
    void AutoConfirm()
    {
        if (_result == null) return;

        ScoreManager.Instance?.SetFishMultiplier(_result.multiplier);

        int fishIndex = System.Array.IndexOf(fishTypes, _result);
        fishSpawner?.SpawnFish(fishIndex);

        arcadeCamera.escEnabled = true;
        arcadeCamera?.ToggleCamera();
    }
    public void EnableBackButton()
    {
        if (backButton != null)
            backButton.interactable = true;
    }
    void SetSlotsVisible(bool visible)
    {
        foreach (var img in slotImages)
            if (img != null) img.enabled = visible;
        foreach (var label in slotLabels)
            if (label != null) label.enabled = visible;
    }
    FishData PickWeightedFish()
    {
        float total = 0f;
        foreach (var f in fishTypes) total += f.weight;

        float roll = Random.Range(0f, total);
        float cumulative = 0f;

        foreach (var f in fishTypes)
        {
            cumulative += f.weight;
            if (roll <= cumulative) return f;
        }

        return fishTypes[fishTypes.Length - 1];
    }

    public void ResetForNewRound()
    {
        if (spinButton == null) return;

        spinButton.interactable = true;
        if (backButton != null) backButton.interactable = true;
        SetSlotsVisible(false);

        if (multiplierText != null)
            multiplierText.text = "?";

        foreach (var img in slotImages)
            if (img != null) img.sprite = null;

        foreach (var label in slotLabels)
            if (label != null) label.text = "-";

        _result = null;
        _isSpinning = false;
    }
}