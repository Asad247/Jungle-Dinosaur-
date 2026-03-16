using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ImageFader : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1.5f; // seconds
    private Image image;

    void Awake()
    {
        image = GetComponent<Image>();
        if (image == null)
        {
            Debug.LogError("ImageFader requires an Image component on the same GameObject.");
        }
    }

    void Start()
    {
        // Assume image starts fully opaque
        Color c = image.color;
        c.a = 1f;
        image.color = c;

        // Fade out automatically on scene load
        StartCoroutine(FadeOutCoroutine());
    }

    public void FadeIn()
    {
        StartCoroutine(FadeInCoroutine());
    }

    public void FadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeInCoroutine()
    {
        float elapsed = 0f;
        Color c = image.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / fadeDuration);
            image.color = c;
            yield return null;
        }

        c.a = 1f;
        image.color = c;
    }

    private IEnumerator FadeOutCoroutine()
    {
        float elapsed = 0f;
        Color c = image.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(1f - (elapsed / fadeDuration));
            image.color = c;
            yield return null;
        }

        c.a = 0f;
        image.color = c;
    }
}