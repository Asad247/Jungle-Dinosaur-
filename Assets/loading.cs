using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Loading : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private float loadTime = 7f;

    private float timer = 0f;
    private bool isLoading = true;

    [SerializeField] private string SceneName;

    void Start()
    {
        fillImage.fillAmount = 0f;
    }

    void OnEnable()
    {
        isLoading = true;
    }

    void Update()
    {
        if (!isLoading) return;

        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / loadTime);

        fillImage.fillAmount = progress;
        UpdateProgressText(progress);

        if (progress >= 1f)
        {
            isLoading = false;
            SceneManager.LoadScene(SceneName);
        }
    }

    private void UpdateProgressText(float progress)
    {
        if (progressText)
        {
            int percent = Mathf.RoundToInt(progress * 100);

            string message = progress switch
            {
                < 0.3f => "Loading assets...",
                < 0.6f => "Loading scene data...",
                < 0.9f => "Almost there...",
                _ => "Finalizing..."
            };

            progressText.text = $"{message} {percent}%";
        }
    }
}