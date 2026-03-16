using UnityEngine;

public class MouseHider : MonoBehaviour
{
    void Start()
    {
        // Hides the cursor
        Cursor.visible = false;

        // Locks the cursor to the center of the screen 
        // Prevents the mouse from clicking outside the game window
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Safety check: Press Escape to bring the mouse back during testing
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}