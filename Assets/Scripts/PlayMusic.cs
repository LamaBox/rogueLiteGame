using UnityEngine;

public class PlayMusic : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MusicSystem.Instance.PlayMusic(2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
