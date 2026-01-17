using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerDataStructures;

public class PlayerAttack : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerMovement playerMovement; // Ссылка на скрипт движения

    [Header("Attack Settings (will be overridden by PlayerData)")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float attackSpeed = 1f; // attacks per second
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private LayerMask damageableLayers;

    [Header("Attack Direction")]
    [SerializeField] private Transform attackPoint; // точка, откуда идёт атака
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Attack Visual")]
    [SerializeField] private SpriteRenderer attackVisual;
    private float visualAttackTimer = 0f;

    private float attackTimer = 0f;
    private bool canAttack = true;

    // Флаг блокировки атаки (для магии, катсцен и т.д.)
    private bool isAttackLocked = false;

    public bool GetCanAttack()
    {
        return canAttack;
    }

    // Событие: атака выполнена
    public event System.Action OnAttackPerformed;
    
    void Start()
    {
        // Автоматически ищем скрипт движения, если не назначен
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
            
        // if (attackPoint == null)
        // {
        //     attackPoint = transform;
        //     Debug.LogWarning("AttackPoint not assigned. Using player transform as attack origin.");
        // }
    }
    
    void Update()
    {
        if (!canAttack)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
                canAttack = true;
        }
        
        // Скрываем визуализацию, если таймер атаки закончился
        if (attackVisual != null && attackVisual.enabled && visualAttackTimer <= 0f)
        {
            attackVisual.enabled = false;
        }
        else if (attackVisual.enabled)
            visualAttackTimer -= Time.deltaTime;
    }

    public void OnAttackInput(InputAction.CallbackContext context)
    {
        // Проверяем:
        // 1. Кнопка нажата
        // 2. Кулдаун прошел (canAttack)
        // 3. Атака НЕ заблокирована (isAttackLocked)
        // 4. Мы НЕ в рывке (нельзя бить во время дэша)
        
        bool isDashing = playerMovement != null && playerMovement.GetIsDashing();
        
        if (context.performed && canAttack && !isAttackLocked && !isDashing)
        {
            PerformAttack();
        }
    }

    // --- ПУБЛИЧНЫЕ МЕТОДЫ БЛОКИРОВКИ ---
    
    public void LockAttack()
    {
        isAttackLocked = true;
    }

    public void UnlockAttack()
    {
        isAttackLocked = false;
    }
    
    // ------------------------------------

    private void PerformAttack()
    {
        canAttack = false;
        attackTimer = 1f / attackSpeed;
        
        // Показываем визуализацию
        if (attackVisual != null)
        {
            visualAttackTimer = 0.5f;
            attackVisual.transform.localScale = Vector3.one * attackRange * 2f; // масштабируем под радиус
            attackVisual.enabled = true;
        }
        
        OnAttackPerformed?.Invoke();
        
        // Находим всех врагов в радиусе
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, damageableLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            // Пытаемся получить интерфейс у объекта, в который попали
            if (enemy.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(damage);
                // Для дебага:
                Debug.Log($"Hit enemy: {enemy.name} with {damage} damage (commented out)");
            }
        }
        Debug.Log("Совершил атаку");
    }
    
    // Обновление параметров из PlayerData
    public void OnAttackModifiersChanged(AttackModifiersData data)
    {
        damage = data.Damage;
        attackSpeed = data.AttackSpeed;
        attackRange = data.AttackRange;
    }

    // Визуализация радиуса атаки в редакторе
    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}