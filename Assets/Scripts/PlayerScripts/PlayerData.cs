using System;
using System.Collections.Generic;
using UnityEngine;
using static PlayerDataStructures;

//класс в котором хранятся все данные игрока
public class PlayerData : MonoBehaviour
{
    #region События
    // События для уведомления других систем
    public static event Action<ResourceData> OnResourceChanged;
    public static event Action<MovementModifiersData> OnMovementModifiersChanged;
    public static event Action OnDataInitialized;
    #endregion

    #region Инспекторные поля - базовые значения
    [Header("Base Resources")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float maxStamina = 100f;

    [Header("Base Movement Stats")]
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float baseSprintMultiplier = 1.5f;
    [SerializeField] private float baseJumpHeight = 10f;
    [SerializeField] private float baseDashSpeed = 15f;
    [SerializeField] private float baseDashCooldown = 2f;
    [SerializeField] private float baseGravityScale = 10f;

    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false; // Новый параметр для отладки
    #endregion

    #region Текущие значения ресурсов
    private float currentHealth;
    private float currentMana;
    private float currentStamina;
    #endregion

    #region Жизненный цикл
    private void Awake()
    {
        InitializeResources();
    }

    private void Start()
    {
        if (debugMode)
        {
            Debug.Log("PlayerData initialized with debug mode ON");
        }

        BroadcastAllData();
    }

    private void InitializeResources()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;
    }

    public void BroadcastAllData()
    {
        // Отправляем все начальные данные
        BroadcastResourceChange(currentHealth, maxHealth, ResourceType.Health);
        BroadcastResourceChange(currentMana, maxMana, ResourceType.Mana);
        BroadcastResourceChange(currentStamina, maxStamina, ResourceType.Stamina);
        OnMovementModifiersChanged?.Invoke(GetCurrentMovementModifiers());
        OnDataInitialized?.Invoke();
    }
    #endregion

    #region Методы для работы с ресурсами
    public void SetHealth(float value)
    {
        float oldValue = currentHealth;
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        if (Math.Abs(oldValue - currentHealth) > 0.01f)
        {
            if (debugMode)
            {
                Debug.Log($"Health changed: {oldValue} -> {currentHealth}");
            }
            BroadcastResourceChange(currentHealth, maxHealth, ResourceType.Health);
        }
    }

    public void ModifyHealth(float amount)
    {
        SetHealth(currentHealth + amount);
    }

    public void SetMana(float value)
    {
        float oldValue = currentMana;
        currentMana = Mathf.Clamp(value, 0, maxMana);
        if (Math.Abs(oldValue - currentMana) > 0.01f)
        {
            if (debugMode)
            {
                Debug.Log($"Mana changed: {oldValue} -> {currentMana}");
            }
            BroadcastResourceChange(currentMana, maxMana, ResourceType.Mana);
        }
    }

    public void ModifyMana(float amount)
    {
        SetMana(currentMana + amount);
    }

    public void SetStamina(float value)
    {
        float oldValue = currentStamina;
        currentStamina = Mathf.Clamp(value, 0, maxStamina);
        if (Math.Abs(oldValue - currentStamina) > 0.01f)
        {
            if (debugMode)
            {
                Debug.Log($"Stamina changed: {oldValue} -> {currentStamina}");
            }
            BroadcastResourceChange(currentStamina, maxStamina, ResourceType.Stamina);
        }
    }

    public void ModifyStamina(float amount)
    {
        SetStamina(currentStamina + amount);
    }
    #endregion

    #region Геттеры для текущих значений
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => maxHealth > 0 ? currentHealth / maxHealth : 0;

    public float GetCurrentMana() => currentMana;
    public float GetMaxMana() => maxMana;
    public float GetManaPercentage() => maxMana > 0 ? currentMana / maxMana : 0;

    public float GetCurrentStamina() => currentStamina;
    public float GetMaxStamina() => maxStamina;
    public float GetStaminaPercentage() => maxStamina > 0 ? currentStamina / maxStamina : 0;

    private MovementModifiersData GetCurrentMovementModifiers()
    {
        return new MovementModifiersData(
            baseMoveSpeed,
            baseSprintMultiplier,
            baseJumpHeight,
            baseDashSpeed,
            baseDashCooldown,
            baseGravityScale
        );
    }
    #endregion

    #region Вспомогательные методы
    private void BroadcastResourceChange(float current, float max, ResourceType type)
    {
        OnResourceChanged?.Invoke(new ResourceData(current, max, type));
    }
    #endregion

    #region Дебаг методы
    // Кнопка в инспекторе для дебага
    [ContextMenu("Broadcast All Data (Debug)")]
    public void DebugBroadcastAllData()
    {
        if (debugMode)
        {
            Debug.Log("Broadcasting all data for debug...");
            BroadcastAllData();
        }
    }

    // Дополнительные дебаг методы
    [ContextMenu("Set All Resources to Max")]
    public void DebugSetAllToMax()
    {
        SetHealth(maxHealth);
        SetMana(maxMana);
        SetStamina(maxStamina);
        Debug.Log("All resources set to max values");
    }

    [ContextMenu("Broadcast Movement Data Only")]
    public void DebugBroadcastMovement()
    {
        if (debugMode)
        {
            OnMovementModifiersChanged?.Invoke(GetCurrentMovementModifiers());
            Debug.Log("Movement data broadcasted");
        }
    }

    [ContextMenu("Broadcast Health Data Only")]
    public void DebugBroadcastHealth()
    {
        if (debugMode)
        {
            OnResourceChanged?.Invoke(new ResourceData(currentHealth, maxHealth, ResourceType.Health));
            Debug.Log($"Health data broadcasted: {currentHealth}/{maxHealth}");
        }
    }
    #endregion
}
