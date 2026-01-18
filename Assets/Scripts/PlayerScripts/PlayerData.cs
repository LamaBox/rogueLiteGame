using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static PlayerDataStructures;

//класс в котором хранятся все данные игрока
public class PlayerData : MonoBehaviour
{
    #region События
    // События для уведомления других систем
    public event Action<ResourceData> OnResourceChanged;
    public event Action<MovementModifiersData> OnMovementModifiersChanged;
    public event Action OnDataInitialized;
    public event Action<AttackModifiersData> OnAttackModifiersChanged;
    public event Action OnDead;
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

    [Header(("Base Attack Stats"))] 
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseAttackSpeed = 1f;
    [SerializeField] private float baseAttackRange = 1.5f;
    
    [Header("Heal Settings")]
    [SerializeField] private float healHealthPerSec = 1f;
    [SerializeField] private float healManaPerSec = 0f;
    
    
    [Header("Debug Settings")]
    [SerializeField] private bool debugMode; // Новый параметр для отладки
    #endregion

    #region Текущие значения ресурсов
    private float currentHealth;
    private float currentMana;
    private float currentStamina;
    #endregion
    
    private float _regenTimer = 0f;

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
    }

    private void Update()
    {
        // Накапливаем время
        _regenTimer += Time.deltaTime;

        // Если прошла 1 секунда
        if (_regenTimer >= 1f)
        {
            _regenTimer = 0f; // Сбрасываем таймер

            // --- ХИЛ (Выполняем, только если здоровье не полное) ---
            if (currentHealth < maxHealth && healHealthPerSec > 0 && currentHealth > 0)
            {
                // Тут уже не умножаем на Time.deltaTime, так как мы ждали ровно 1 секунду
                currentHealth += healHealthPerSec; 
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
                
                // Отправляем событие только раз в секунду и только если что-то изменилось
                BroadcastResourceChange(currentHealth, maxHealth, ResourceType.Health);
            }
            
            // --- МАНА (Выполняем, только если мана не полная) ---
            if (currentMana < maxMana && healManaPerSec > 0)
            {
                currentMana += healManaPerSec;
                currentMana = Mathf.Clamp(currentMana, 0, maxMana);
                
                BroadcastResourceChange(currentMana, maxMana, ResourceType.Mana);
            }
        }
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
        OnAttackModifiersChanged?.Invoke(GetCurrentAttackModifiers());
        OnDataInitialized?.Invoke();
        
    }
    #endregion

    #region Методы для работы с ресурсами
    //type - тип ресурса, который нужно изменить, valueType - мы берём максимальное или актуальное значение для изменения
    //isAddition - мы добавляем значение(true) или изменяем на конкретное(false).
    public void ChangeValueResource(float value, ResourceType type, ResourceValueType valueType, bool isAddition = false)
    {
        switch (type)
        {
            case ResourceType.Health:
                if (valueType == ResourceValueType.Maximum)
                {
                    this.maxHealth = isAddition ? this.maxHealth + value : value;
                }
                else
                {
                    this.currentHealth = isAddition ? this.currentHealth + value : value;
                    this.currentHealth = Mathf.Clamp(this.currentHealth, 0, maxHealth);
                }
                if (debugMode)
                    Debug.Log($"Health changed: {this.currentHealth} -> {this.maxHealth}");
                BroadcastResourceChange(this.currentHealth, this.maxHealth, ResourceType.Health);

                if (currentHealth == 0)
                {
                    OnDead?.Invoke();
                }
                break;
            
            case ResourceType.Mana:
                if (valueType == ResourceValueType.Maximum)
                {
                    this.maxMana = isAddition ? this.maxMana + value : value;
                }
                else
                {
                    this.currentMana = isAddition ? this.currentMana + value : value;
                    this.currentMana = Mathf.Clamp(this.currentMana, 0, maxMana);
                }
                if (debugMode)
                    Debug.Log($"Mana changed: {this.currentMana} -> {this.maxMana}");
                BroadcastResourceChange(this.currentMana, maxMana, ResourceType.Mana);
                break;
            
            case ResourceType.Stamina:
                if (valueType == ResourceValueType.Maximum)
                {
                    this.maxStamina =  isAddition ? this.maxStamina + value : value;
                }
                else
                {
                    this.currentStamina = isAddition ? this.currentStamina + value : value;
                    this.currentStamina = Mathf.Clamp(this.currentStamina, 0, maxStamina);
                }
                if (debugMode)
                    Debug.Log($"Stamina changed: {this.currentStamina} -> {this.maxStamina}");
                BroadcastResourceChange(this.currentStamina, maxStamina, ResourceType.Stamina);
                break;
        }
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

    private AttackModifiersData GetCurrentAttackModifiers()
    {
        return new AttackModifiersData(baseDamage, baseAttackSpeed, baseAttackRange);
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
        ChangeValueResource(maxHealth, ResourceType.Health, ResourceValueType.Current);
        ChangeValueResource(maxMana, ResourceType.Mana, ResourceValueType.Current);
        ChangeValueResource(maxStamina, ResourceType.Stamina, ResourceValueType.Current);
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

    [ContextMenu("DebugLog Actual Recources")]
    public void DebugLogRecources()
    {
        if (debugMode)
        {
            Debug.Log($"Actual Recources: Health: {this.currentHealth} Mana: {this.currentMana} Stamina: {this.currentStamina}");
        }    
    }
    
    [ContextMenu("Broadcast Attack Data Only")]
    public void DebugBroadcastAttack()
    {
        if (debugMode)
        {
            OnAttackModifiersChanged?.Invoke(GetCurrentAttackModifiers());
            Debug.Log("Attack data broadcasted");
        }
    }
    #endregion
}
