using UnityEngine;
using UnityEngine.SceneManagement;

public class restartScene : MonoBehaviour
{
    public void restartScenes()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
