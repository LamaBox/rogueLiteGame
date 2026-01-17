using UnityEngine;

public class PlayMusic2 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (RunContextSystem.Instance != null)
        {
            MusicSystem.Instance.PlayMusic(3);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
