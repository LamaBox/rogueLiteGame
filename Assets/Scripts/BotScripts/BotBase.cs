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
    protected Vector2 PlayerPosition => PlayerTransform.position; //? PlayerTransform.position : Vector2.zero;

    protected float MaxHealth => this.botDataSO.maxHealth;
    protected float Damage => this.botDataSO.damage;
    protected float MoveSpeed => this.botDataSO.moveSpeed;
    protected float AttackSpeed => this.botDataSO.attackSpeed;
    protected float AttackDistance => this.botDataSO.attackDistance;
    
    protected float CurrentHealth;

    protected virtual void Start()
    {
        if (this.botDataSO is null)
        {
            Debug.LogError($"{nameof(this.gameObject)} Bot DataSO is null");
            this.gameObject.SetActive(false);
        }
        
        Transf = GetComponent<Transform>();
        Rb2d = GetComponent<Rigidbody2D>();
        CurrentHealth = this.botDataSO.maxHealth;
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
            Destroy(this.gameObject);
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
}
