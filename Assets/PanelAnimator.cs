using UnityEngine;
using DG.Tweening;

public class PanelAnimator : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 0.4f;
    public Ease openEase = Ease.OutBack;
    public Ease closeEase = Ease.InBack; // InBack pulls it back before shrinking

    private Vector3 _originalScale;
    private CanvasGroup _canvasGroup;

    void Awake()
    {
        _originalScale = transform.localScale;
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        // Kill any existing tweens to prevent conflicts if opened/closed rapidly
        transform.DOKill();
        if (_canvasGroup != null) _canvasGroup.DOKill();

        transform.localScale = Vector3.zero;
        transform.DOScale(_originalScale, duration).SetEase(openEase);

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.DOFade(1f, duration).SetEase(Ease.OutQuad);
        }
    }

    // Call this INSTEAD of gameObject.SetActive(false)
    public void ClosePanel()
    {
        transform.DOKill();
        if (_canvasGroup != null) _canvasGroup.DOKill();

        if (_canvasGroup != null)
        {
            _canvasGroup.DOFade(0f, duration).SetEase(Ease.InQuad);
        }

        // Shrink to zero, then disable the object OnComplete
        transform.DOScale(Vector3.zero, duration)
                 .SetEase(closeEase)
                 .OnComplete(() => gameObject.SetActive(false));
    }

    void OnDisable()
    {
        transform.DOKill();
        if (_canvasGroup != null) _canvasGroup.DOKill();
    }
}