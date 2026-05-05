using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
 
[RequireComponent(typeof(Image))]
public class JuicyButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler
{
    [Header("Hover - Edge Glow")]
    [SerializeField] private Image glowBorderImage;
    [SerializeField] private Color glowColorOff = new Color(0.4f, 0.4f, 0.8f, 0f);
    [SerializeField] private Color glowColorOn = new Color(0.6f, 0.6f, 1f, 0.7f);
    [SerializeField] private float glowDuration = 0.25f;

    [Header("Click - Slam / Punch")]
    [SerializeField] private Vector3 punchScale = new Vector3(0.14f, -0.22f, 0f);
    [SerializeField] private float punchDuration = 0.3f;
    [SerializeField] private int punchVibrato = 6;
    [SerializeField] private float punchElasticity = 0.5f;

    private RectTransform _rect;
    private Vector3 _baseScale;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _baseScale = _rect.localScale;

        if (glowBorderImage != null)
            glowBorderImage.color = glowColorOff;
    }

    private void OnDestroy()
    {
        DOTween.Kill(transform);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (glowBorderImage != null)
            glowBorderImage.DOColor(glowColorOn, glowDuration).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (glowBorderImage != null)
            glowBorderImage.DOColor(glowColorOff, glowDuration).SetUpdate(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _rect.DOKill();
        _rect.DOScale(_baseScale * 0.92f, 0.07f)
            .SetEase(Ease.InQuad)
            .SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _rect.DOKill();
        _rect.DOScale(_baseScale, 0.15f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // slam
        _rect.DOKill();
        _rect.localScale = _baseScale;
        _rect.DOPunchScale(punchScale, punchDuration, punchVibrato, punchElasticity)
            .SetUpdate(true);
    }
}
