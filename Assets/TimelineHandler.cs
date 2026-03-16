using UnityEngine;
using UnityEngine.Playables;

public class TimelineHandler : MonoBehaviour
{
    [SerializeField] private GameObject[] enablee;
    public PlayableDirector director;

    void Start()
    {
        if (director != null)
        {
            // Subscribe to the stopped event
            director.stopped += OnTimelineComplete;
        }
    }

    private void OnTimelineComplete(PlayableDirector aDirector)
    {
        // Only run if it's the timeline we are watching
        if (aDirector == director)
        {
            Debug.Log("Timeline has finished!");

            // Trigger your custom logic here
            TriggerNextEvent();
        }
        gameObject.SetActive(false);
    }

    private void TriggerNextEvent()
    {
        // Add your logic (e.g., spawn enemies, change scenes, etc.)
        foreach (GameObject x in enablee)
        {
            x.SetActive(true);
        }

    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks or errors
        if (director != null)
        {
            director.stopped -= OnTimelineComplete;
        }
    }
}