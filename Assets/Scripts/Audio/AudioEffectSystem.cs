using UnityEngine;

public class AudioEffectSystem : MonoBehaviour
{
    [Header("Sound Effects")] 
    [SerializeField] private AudioClip[] audioClips;

    [Header("Settings")]
    [Tooltip("Если true, звук создается в точке пространства и продолжает играть после уничтожения объекта. Если false - играет на этом объекте.")]
    [SerializeField] private bool playDetached = false;
    
    [Range(0f, 1f)]
    [SerializeField] private float volumeMult = 0.6f;
    
    [Range(0f, 1f)]
    [SerializeField] private float spatialBlend = 1f;
    
    [SerializeField] private float maxDistance = 30f;
    
    private AudioSource _audioSource;
    
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            // Если компонента нет, добавим его, чтобы хранить в нем настройки громкости
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        _audioSource.spatialBlend = spatialBlend;
        _audioSource.minDistance = 10f;
        _audioSource.maxDistance = maxDistance;
        _audioSource.volume *= volumeMult;
        _audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    /// <summary>
    /// Устанавливает громкость эффектов.
    /// </summary>
    /// <param name="volume">Значение от 0.0 до 1.0</param>
    public void SetVolume(float volume)
    {
        if (_audioSource != null)
        {
            _audioSource.volume = Mathf.Clamp01(volume * volumeMult);
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
            if (playDetached)
            {
                // Создает временный объект AudioSource в точке взрыва.
                // Он автоматически уничтожится после проигрывания.
                // Громкость берем из нашего AudioSource.
                AudioSource.PlayClipAtPoint(clip, transform.position, _audioSource.volume);
            }
            else
            {
                // Стандартный способ (звук прервется, если объект уничтожить)
                _audioSource.PlayOneShot(clip);
            }
        }
        else
        {
            Debug.LogWarning($"AudioEffectPlayer: AudioClip at index {clipIndex} is null.");
        }
    }
}