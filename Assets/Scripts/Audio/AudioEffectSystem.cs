using UnityEngine;

public class AudioEffectSystem : MonoBehaviour
{
    [Header("Sound Effects")] 
    [SerializeField] private AudioClip[] audioClips;

    [Header("Settings")]
    [Tooltip("Если true, звук создается в точке пространства и продолжает играть после уничтожения объекта.")]
    [SerializeField] private bool playDetached = false;
    
    [Tooltip("Локальный множитель громкости для этого конкретного объекта")]
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
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        _audioSource.spatialBlend = spatialBlend;
        _audioSource.minDistance = 1f; // Чуть увеличил minDistance, чтобы не глохло в упор
        _audioSource.maxDistance = maxDistance;
        _audioSource.rolloffMode = AudioRolloffMode.Linear;

        // --- ИНТЕГРАЦИЯ С VOLUMESETTINGS ---
        if (VolumeSettings.Instance != null)
        {
            // 1. Подписка на изменение громкости звуков (SFX)
            VolumeSettings.Instance.OnSFXVolumeChanged += OnGlobalSFXChanged;
            
            // 2. Инициализация (берем глобальную громкость и умножаем на локальный множитель)
            UpdateVolume(VolumeSettings.Instance.GetSFXVolume());
        }
        else
        {
            // Если настроек нет, просто ставим по локальному множителю
            _audioSource.volume = volumeMult;
        }
    }

    private void OnDestroy()
    {
        // Отписка
        if (VolumeSettings.Instance != null)
        {
            VolumeSettings.Instance.OnSFXVolumeChanged -= OnGlobalSFXChanged;
        }
    }

    // Обработчик события
    private void OnGlobalSFXChanged(float globalVolume)
    {
        UpdateVolume(globalVolume);
    }

    private void UpdateVolume(float globalVolume)
    {
        if (_audioSource != null)
        {
            // Итоговая громкость = НастройкаМеню * НастройкаОбъекта
            _audioSource.volume = Mathf.Clamp01(globalVolume * volumeMult);
        }
    }

    // Этот метод можно оставить для ручного управления (например, затухание при удалении)
    public void SetVolume(float volume)
    {
        if (_audioSource != null)
        {
            _audioSource.volume = Mathf.Clamp01(volume);
        }
    }

    public void PlayAudioClip(int clipIndex)
    {
        if (audioClips == null || audioClips.Length == 0) return;

        if (clipIndex < 0 || clipIndex >= audioClips.Length)
        {
            Debug.LogWarning($"AudioEffectPlayer: Index {clipIndex} out of range.");
            return;
        }

        AudioClip clip = audioClips[clipIndex];
        if (clip != null)
        {
            if (playDetached)
            {
                // PlayClipAtPoint создает новый временный AudioSource.
                // Мы передаем ему текущую громкость нашего _audioSource, 
                // которая уже настроена правильно (Глобальная * Локальная).
                AudioSource.PlayClipAtPoint(clip, transform.position, _audioSource.volume);
            }
            else
            {
                _audioSource.PlayOneShot(clip);
            }
        }
    }
}