using UnityEngine;

public class AudioEffectSystem : MonoBehaviour
{
    [Header("Sound Effects")] 
    [SerializeField] private AudioClip[] audioClips;
    
    private AudioSource _audioSource;
    
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            Debug.LogError(this.name + ": Audio Source not found.");
        }
    }

    public void PlayAudioClip(int clipIndex)
    {
        if (audioClips == null || audioClips.Length == 0)
        {
            Debug.LogWarning("AudioEffectPlayer: AudioClips array is empty or null.");
            return;
        }

        if (clipIndex < 0 || clipIndex >= audioClips.Length)
        {
            Debug.LogWarning($"AudioEffectPlayer: Index {clipIndex} is out of range for audio clips array (length: {audioClips.Length}).");
            return;
        }

        AudioClip clip = audioClips[clipIndex];
        if (clip != null)
        {
            _audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"AudioEffectPlayer: AudioClip at index {clipIndex} is null.");
        }
    }
}
