using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MusicSystem : MonoBehaviour
{
    public static MusicSystem Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("Время плавного перехода между треками (секунды)")]
    [SerializeField] private float _crossfadeDuration = 1.0f;
    
    // Это значение теперь будет обновляться из VolumeSettings
    [SerializeField] private float _masterVolume = 0.5f;

    [Header("Tracks Collection")]
    [SerializeField] private AudioClip[] _musicTracks;

    private AudioSource _audioSource;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _audioSource = GetComponent<AudioSource>();
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
    }

    void Start()
    {
        // --- ИНТЕГРАЦИЯ С VOLUMESETTINGS ---
        if (VolumeSettings.Instance != null)
        {
            // 1. Подписываемся на событие изменения громкости музыки
            VolumeSettings.Instance.OnMusicVolumeChanged += OnVolumeChanged;
            
            // 2. Инициализируем громкость текущим значением из настроек
            _masterVolume = VolumeSettings.Instance.GetMusicVolume();
        }
        else
        {
            Debug.LogError("VolumeSettings не обнаружен");
        }
        
        _audioSource.volume = _masterVolume;
        
        PlayMusic(0);
    }

    private void OnDestroy()
    {
        // Обязательно отписываемся, чтобы избежать ошибок при перезагрузке
        if (VolumeSettings.Instance != null)
        {
            VolumeSettings.Instance.OnMusicVolumeChanged -= OnVolumeChanged;
        }
    }

    // Обработчик события (подписчик)
    private void OnVolumeChanged(float newVolume)
    {
        SetVolume(newVolume);
    }

    public void PlayMusic(int trackIndex)
    {
        if (trackIndex < 0 || trackIndex >= _musicTracks.Length)
        {
            Debug.LogWarning($"MusicSystem: Track index {trackIndex} out of range.");
            return;
        }

        AudioClip nextClip = _musicTracks[trackIndex];

        if (_audioSource.clip == nextClip && _audioSource.isPlaying) return;

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeToNewTrack(nextClip));
    }

    public void StopMusic()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeToNewTrack(null));
    }

    public void SetVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
        
        // Если сейчас НЕ идет плавный переход, меняем громкость AudioSource напрямую
        if (_fadeCoroutine == null)
        {
            _audioSource.volume = _masterVolume;
        }
        // Если переход идет, корутина сама подхватит новое значение _masterVolume в процессе Lerp
    }

    private IEnumerator FadeToNewTrack(AudioClip newClip)
    {
        float timer = 0f;
        float startVolume = _audioSource.volume;

        if (_audioSource.isPlaying)
        {
            while (timer < _crossfadeDuration)
            {
                timer += Time.deltaTime;
                _audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / _crossfadeDuration);
                yield return null;
            }
        }

        _audioSource.volume = 0f;
        _audioSource.Stop();

        if (newClip == null) 
        {
            _fadeCoroutine = null;
            yield break;
        }

        _audioSource.clip = newClip;
        _audioSource.Play();

        timer = 0f;
        while (timer < _crossfadeDuration)
        {
            timer += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(0f, _masterVolume, timer / _crossfadeDuration);
            yield return null;
        }

        _audioSource.volume = _masterVolume;
        _fadeCoroutine = null;
    }
}