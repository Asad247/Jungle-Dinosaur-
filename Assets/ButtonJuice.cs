using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // Required for DOTween
using UnityEngine.EventSystems;

public class ButtonJuice : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Settings")]
    public float punchAmount = 0.2f;
    public float duration = 0.2f;
    public int vibrato = 5;

    private Vector3 _initialScale;

    void Start()
    {
        _initialScale = transform.localScale;
    }

    // Runs when you press the mouse down
    public void OnPointerDown(PointerEventData eventData)
    {
        // "Punch" the scale to make it look like it's being pressed
        transform.DOPunchScale(new Vector3(-punchAmount, -punchAmount, 0), duration, vibrato)
                 .SetEase(Ease.OutQuad);
    }

    // Runs when you let go
    public void OnPointerUp(PointerEventData eventData)
    {
        // Ensure it returns to exactly the original size smoothly
        transform.DOScale(_initialScale, duration).SetEase(Ease.OutBack);
    }

    // Optional: Kill tweens on destroy to prevent memory leaks
    void OnDestroy()
    {
        transform.DOKill();
    }
}