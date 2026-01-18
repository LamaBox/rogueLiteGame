using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MusicSystem : MonoBehaviour
{
    public static MusicSystem Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("Время плавного перехода между треками (секунды)")]
    [SerializeField] private float _crossfadeDuration = 1.0f;
    
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
        // Убрали всю логику поиска VolumeSettings.
        // Теперь MusicSystem просто ждет команд.
        
        // Применяем локальную громкость (на случай если VolumeSettings еще не проинициализировал нас)
        _audioSource.volume = _masterVolume;
        
        PlayMusic(0);
    }
    
    // Этот метод вызывается снаружи (из VolumeSettings)
    public void SetVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
        
        if (_fadeCoroutine == null)
        {
            _audioSource.volume = _masterVolume;
        }
    }

    public void PlayMusic(int trackIndex)
    {
        if (trackIndex < 0 || trackIndex >= _musicTracks.Length) return;

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