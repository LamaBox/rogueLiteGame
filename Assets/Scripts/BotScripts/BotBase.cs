using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class BotBase : MonoBehaviour
{
    //ScriptableObject
    [SerializeField] protected BotDataSO botDataSO;
    
    public event Action<float, float, float> OnHealthChanged; //current health, maximum health, percentage(current/max)
    
    protected Rigidbody2D Rb2d;
    protected Transform Transf;
    
    //Transform игрока, чтобы бот знал координаты
    protected Transform PlayerTransform;
    protected Vector2 PlayerPosition => PlayerTransform != null ? PlayerTransform.position : Vector2.zero;

    protected float MaxHealth => this.botDataSO.maxHealth;
    protected float Damage => this.botDataSO.damage;
    protected float MoveSpeed => this.botDataSO.moveSpeed;
    protected float AttackSpeed => this.botDataSO.attackSpeed;
    protected float AttackDistance => this.botDataSO.attackDistance;
    
    protected float CurrentHealth;

    // Ссылка на коллайдер для изменения радиуса
    protected CircleCollider2D circleCollider;
    protected float initialColliderRadius;

    protected virtual void Start()
    {
        if (this.botDataSO is null)
        {
            Debug.LogError($"{nameof(this.gameObject)} Bot DataSO is null");
            this.gameObject.SetActive(false);
        }
        
        Transf = GetComponent<Transform>();
        Rb2d = GetComponent<Rigidbody2D>();
        
        // Получаем CircleCollider2D и сохраняем начальный радиус
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            initialColliderRadius = circleCollider.radius;
        }
        else
        {
            Debug.LogError($"{nameof(this.gameObject)} - CircleCollider2D not found!");
        }
        
        CurrentHealth = this.botDataSO.maxHealth;
        
        // Обновляем коллайдер при старте
        UpdateColliderSize();
    }

    protected virtual void Update()
    {
        // Обновляем размер коллайдера каждый кадр (если нужно динамическое изменение)
        UpdateColliderSize();
    }

    /// <summary>
    /// Обновляет размер коллайдера в соответствии с масштабом объекта
    /// </summary>
    protected virtual void UpdateColliderSize()
    {
        if (circleCollider != null)
        {
            // Используем среднее значение масштаба по осям X и Y для равномерного изменения
            float averageScale = (Transf.localScale.x + Transf.localScale.y) / 2f;
            circleCollider.radius = initialColliderRadius * averageScale;
        }
    }

    public virtual void TakeDamage(float damageInp)
    {
        if (damageInp >= 0)
        {
            CurrentHealth -= damageInp;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth, CurrentHealth/MaxHealth);
        }
        else
            Debug.LogError($"{nameof(this.gameObject)} - Damage is negative ({damageInp}), can't take!");

        if (CurrentHealth == 0) //смерть
        {
            Debug.Log($"{nameof(this.gameObject)} Bot is dead");
            CurrentHealth = MaxHealth;
        }
    }

    public virtual void SetPlayerTransform(Transform player)
    {
        if (player is not null)
            PlayerTransform = player;
        else
            Debug.LogError($"{nameof(this.gameObject)}  - PlayerTransform is null! Error in SetPlayerTransform!");
    }
    
    public float GetCurrentHealth() => CurrentHealth;
    public float GetMaxHealth() => MaxHealth;
    public Transform GetPlayerTransform() => PlayerTransform;
    
    [ContextMenu("Kill Bot")]
    public void KillBot()
    {
        TakeDamage(MaxHealth);
    }

    [ContextMenu("Log Health")]
    public void LogHealth()
    {
        Debug.Log(CurrentHealth);
    }

    [ContextMenu("Double Size")]
    public void DoubleSize()
    {
        // Метод для тестирования изменения размера
        Transf.localScale *= 2f;
        UpdateColliderSize();
    }

    [ContextMenu("Reset Size")]
    public void ResetSize()
    {
        // Метод для сброса размера
        Transf.localScale = Vector3.one;
        UpdateColliderSize();
    }
}