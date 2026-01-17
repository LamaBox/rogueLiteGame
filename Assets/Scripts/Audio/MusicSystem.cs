using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MusicSystem : MonoBehaviour
{
    // Публичная статическая ссылка для доступа отовсюду: MusicSystem.Instance.PlayMusic(...)
    public static MusicSystem Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("Время плавного перехода между треками (секунды)")]
    [SerializeField] private float _crossfadeDuration = 1.0f;
    
    [Tooltip("Громкость музыки по умолчанию (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float _masterVolume = 0.5f;

    [Header("Tracks Collection")]
    [SerializeField] private AudioClip[] _musicTracks;

    private AudioSource _audioSource;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        // Реализация Синглтона
        if (Instance == null)
        {
            Instance = this;
            // Делаем так, чтобы этот объект не удалялся при смене сцен
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Если такой объект уже есть (пришли с другой сцены), удаляем дубликат
            Destroy(gameObject);
            return;
        }

        _audioSource = GetComponent<AudioSource>();
        _audioSource.loop = true; // Музыка всегда зациклена
        _audioSource.volume = _masterVolume;
    }

    void Start()
    {
        PlayMusic(0);
    }
    
    /// <summary>
    /// Запускает музыку по индексу с плавным переходом.
    /// </summary>
    public void PlayMusic(int trackIndex)
    {
        if (trackIndex < 0 || trackIndex >= _musicTracks.Length)
        {
            Debug.LogWarning($"MusicSystem: Track index {trackIndex} out of range.");
            return;
        }

        AudioClip nextClip = _musicTracks[trackIndex];

        // Если этот трек уже играет — ничего не делаем (чтобы не сбрасывать песню в начало)
        if (_audioSource.clip == nextClip && _audioSource.isPlaying)
        {
            return;
        }

        // Запускаем корутину смены трека
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeToNewTrack(nextClip));
    }

    /// <summary>
    /// Полная остановка музыки (с затуханием).
    /// </summary>
    public void StopMusic()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeToNewTrack(null));
    }

    /// <summary>
    /// Установка общей громкости (например, из настроек).
    /// </summary>
    public void SetVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
        // Если прямо сейчас не идет переход, применяем громкость сразу
        if (_fadeCoroutine == null)
        {
            _audioSource.volume = _masterVolume;
        }
    }

    // Корутина для плавного затухания старого и появления нового трека
    private IEnumerator FadeToNewTrack(AudioClip newClip)
    {
        float timer = 0f;
        float startVolume = _audioSource.volume;

        // 1. Затухание (Fade Out) текущего трека
        if (_audioSource.isPlaying)
        {
            while (timer < _crossfadeDuration)
            {
                timer += Time.deltaTime;
                // Lerp от текущей громкости до 0
                _audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / _crossfadeDuration);
                yield return null;
            }
        }

        _audioSource.volume = 0f;
        _audioSource.Stop();

        // Если новый трек null, значит мы просто хотели выключить музыку
        if (newClip == null) 
        {
            _fadeCoroutine = null;
            yield break;
        }

        // 2. Смена клипа
        _audioSource.clip = newClip;
        _audioSource.Play();

        // 3. Нарастание (Fade In) нового трека
        timer = 0f;
        while (timer < _crossfadeDuration)
        {
            timer += Time.deltaTime;
            // Lerp от 0 до целевой громкости (_masterVolume)
            _audioSource.volume = Mathf.Lerp(0f, _masterVolume, timer / _crossfadeDuration);
            yield return null;
        }

        _audioSource.volume = _masterVolume;
        _fadeCoroutine = null;
    }
}