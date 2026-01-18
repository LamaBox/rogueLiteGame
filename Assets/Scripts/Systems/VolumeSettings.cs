using UnityEngine;
using UnityEngine.UI;
using System;

public class VolumeSettings : MonoBehaviour
{
    public static VolumeSettings Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    [Header("Settings Keys")]
    private const string MusicKey = "MusicVolume";
    private const string SFXKey = "SFXVolume";

    // События
    public event Action<float> OnMusicVolumeChanged;
    public event Action<float> OnSFXVolumeChanged;

    private float _currentMusicVolume = 1f;
    private float _currentSFXVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        LoadValues();
        InitializeSliders();
        
        // --- ЛОГИКА ПОДПИСКИ MUSICSYSTEM ---
        // Так как VolumeSettings пересоздается на новой сцене, он сам ищет MusicSystem
        if (MusicSystem.Instance != null)
        {
            // 1. Подписываем MusicSystem на изменения
            OnMusicVolumeChanged += MusicSystem.Instance.SetVolume;
            
            // 2. СРАЗУ отправляем актуальную громкость (синхронизация при смене сцены)
            MusicSystem.Instance.SetVolume(_currentMusicVolume);
        }
    }

    private void OnDestroy()
    {
        // Обязательно отписываем MusicSystem, когда этот UI уничтожается (при смене сцены)
        if (MusicSystem.Instance != null)
        {
            OnMusicVolumeChanged -= MusicSystem.Instance.SetVolume;
        }
    }

    private void LoadValues()
    {
        _currentMusicVolume = PlayerPrefs.GetFloat(MusicKey, 1f);
        _currentSFXVolume = PlayerPrefs.GetFloat(SFXKey, 1f);
        
        // Уведомляем остальных подписчиков (например, SFX), если они есть
        OnMusicVolumeChanged?.Invoke(_currentMusicVolume);
        OnSFXVolumeChanged?.Invoke(_currentSFXVolume);
    }

    private void InitializeSliders()
    {
        if (_musicSlider != null)
        {
            _musicSlider.value = _currentMusicVolume;
            _musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.value = _currentSFXVolume;
            _sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    public void SetMusicVolume(float value)
    {
        _currentMusicVolume = value;
        PlayerPrefs.SetFloat(MusicKey, _currentMusicVolume);
        OnMusicVolumeChanged?.Invoke(_currentMusicVolume);
    }

    public void SetSFXVolume(float value)
    {
        _currentSFXVolume = value;
        PlayerPrefs.SetFloat(SFXKey, _currentSFXVolume);
        OnSFXVolumeChanged?.Invoke(_currentSFXVolume);
    }

    public float GetMusicVolume() => _currentMusicVolume;
    public float GetSFXVolume() => _currentSFXVolume;
}