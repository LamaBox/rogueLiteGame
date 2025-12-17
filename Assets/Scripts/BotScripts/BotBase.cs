using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public abstract class BotBase : MonoBehaviour
{
    //ScriptableObject
    [SerializeField] protected BotDataSO botDataSO;
    
    public event Action<float, float, float> OnHealthChanged; //current health, maximum health, percentage(current/max)
    public event Action OnDead;
    public event Action OnDamagedEvent;
    
    protected Rigidbody2D Rb2d;
    protected Transform Transf;

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
        
        CurrentHealth = MaxHealth;
        
    }

    protected virtual void Update()
    {
        
    }

    public virtual void TakeDamage(float damageInp)
    {
        if (damageInp >= 0)
        {
            CurrentHealth -= damageInp;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth, CurrentHealth/MaxHealth);
            OnDamagedEvent?.Invoke();
        }
        else
            Debug.LogError($"{nameof(this.gameObject)} - Damage is negative ({damageInp}), can't take!");

        if (CurrentHealth == 0) //смерть
        {
            Debug.Log($"{nameof(this.gameObject)} Bot is dead");
            OnDead?.Invoke();
        }
    }

    public void DealDamagePlayer(PlayerData playerData)
    {
        playerData.ChangeValueResource(-Damage, PlayerDataStructures.ResourceType.Health, PlayerDataStructures.ResourceValueType.Current, true);
    }
    
    public float GetCurrentHealth() => CurrentHealth;
    public float GetMaxHealth() => MaxHealth;
    
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
        Transf.localScale *= 2f;
    }

    [ContextMenu("Reset Size")]
    public void ResetSize()
    {
        Transf.localScale = Vector3.one;
    }
}