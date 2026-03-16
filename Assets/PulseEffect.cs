using UnityEngine;
using DG.Tweening;

public class PulseEffect : MonoBehaviour
{
    public float scaleAmount = 1.1f; // The scale to reach
    public float duration = 1.0f;    // Speed of one pulse

    void Start()
    {
        // Animate the localScale to scaleAmount, then back to original
        transform.DOScale(scaleAmount, duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
}