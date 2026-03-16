using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelComplete : MonoBehaviour
{
    public void loadS()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
