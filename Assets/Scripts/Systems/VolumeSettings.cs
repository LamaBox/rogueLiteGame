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

    // События, на которые подпишутся MusicSystem и AudioEffectSystem
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
        // Если настройки должны жить между сценами вместе с UI, раскомментируй:
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadValues();
        InitializeSliders();
    }

    private void LoadValues()
    {
        // Загружаем из PlayerPrefs, по умолчанию 1.0 (максимум)
        _currentMusicVolume = PlayerPrefs.GetFloat(MusicKey, 1f);
        _currentSFXVolume = PlayerPrefs.GetFloat(SFXKey, 1f);
        
        // Сразу отправляем события, чтобы системы звука обновились при старте игры
        // (даже если меню настроек еще не открывали)
        OnMusicVolumeChanged?.Invoke(_currentMusicVolume);
        OnSFXVolumeChanged?.Invoke(_currentSFXVolume);
    }

    private void InitializeSliders()
    {
        // Настраиваем слайдер музыки
        if (_musicSlider != null)
        {
            _musicSlider.value = _currentMusicVolume;
            _musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        // Настраиваем слайдер звуков
        if (_sfxSlider != null)
        {
            _sfxSlider.value = _currentSFXVolume;
            _sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    // Метод, вызываемый слайдером Музыки
    public void SetMusicVolume(float value)
    {
        _currentMusicVolume = value;
        
        // Сохраняем
        PlayerPrefs.SetFloat(MusicKey, _currentMusicVolume);
        
        // Уведомляем подписчиков
        OnMusicVolumeChanged?.Invoke(_currentMusicVolume);
    }

    // Метод, вызываемый слайдером Звуков
    public void SetSFXVolume(float value)
    {
        _currentSFXVolume = value;
        
        // Сохраняем
        PlayerPrefs.SetFloat(SFXKey, _currentSFXVolume);
        
        // Уведомляем подписчиков
        OnSFXVolumeChanged?.Invoke(_currentSFXVolume);
    }

    // Публичные методы для получения текущих значений (на всякий случай)
    public float GetMusicVolume() => _currentMusicVolume;
    public float GetSFXVolume() => _currentSFXVolume;
}