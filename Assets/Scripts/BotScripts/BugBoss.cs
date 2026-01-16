using UnityEngine;

public class BugBoss : BotBase
{
    [Header("Boss Settings")]
    [SerializeField] private GameObject _acidProjectilePrefab; // Префаб плевка
    [SerializeField] private Transform _firePoint; // Точка вылета плевка (рот)
    [SerializeField] private float _closeRange = 2.0f; // Дистанция укуса
    [SerializeField] private float _mediumRange = 5.0f; // Дистанция удара когтем
    [SerializeField] private float _chargeSpeed = 15f; // Скорость рывка

    [Header("Child Colliders")]
    [SerializeField] private Collider2D _biteCollider; // Коллайдер зоны укуса
    [SerializeField] private Collider2D _clawCollider; // Коллайдер зоны удара когтем
    
    // Для поиска игрока (можно использовать общий, можно отдельный огромный триггер)
    [SerializeField] private float _visionRadius = 20f; 

    private Transform _targetPlayer;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer; // Для разворота (или через transform)

    // Состояния босса
    private bool _isAttacking = false;
    private bool _isChasing = false;
    private bool _isCharging = false;
    private bool _isDead = false;

    // Логические флаги для комбо
    private bool _queueSlashAfterAction = false; // Нужно ли ударить когтем после текущего действия
    private bool _queueChargeAfterSlash = false; // Нужно ли сделать рывок после удара когтем

    protected override void Start()
    {
        base.Start(); // Инициализация BotBase (HP, RB и т.д.)
        
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_acidProjectilePrefab == null) Debug.LogError("Acid Prefab is missing!");
        if (_biteCollider == null) Debug.LogError("Bite Collider is missing!");
        if (_clawCollider == null) Debug.LogError("Claw Collider is missing!");
    }

    protected override void Update()
    {
        base.Update(); // Если в BotBase есть логика в Update

        if (_isDead) return;

        // Поиск игрока, если потеряли
        if (_targetPlayer == null)
        {
            FindPlayer();
        }

        if (_targetPlayer != null)
        {
            // Поворот к игроку (если не атакуем и не в рывке)
            if (!_isAttacking && !_isCharging)
            {
                FaceTarget();
            }

            // Логика поведения
            if (!_isAttacking && !_isCharging)
            {
                DecideNextAction();
            }
        }
    }

    // --- ЛОГИКА ПОВЕДЕНИЯ ---

    private void DecideNextAction()
    {
        float distanceX = Mathf.Abs(transform.position.x - _targetPlayer.position.x);
        float distanceY = Mathf.Abs(transform.position.y - _targetPlayer.position.y);

        // 1. Если игрок выше (на другой платформе)
        if (distanceY > 2.0f) // Условная высота
        {
            // Идем к игроку по X
            MoveTowardsPlayer();

            // Если попали в зону удара когтем по X, пробуем атаковать (вдруг достанем)
            if (IsPlayerInZone(_clawCollider))
            {
                StartSlashAttack(false); // Просто удар, без комбо
            }
            return;
        }

        // 2. Ближняя дистанция (вплотную)
        if (distanceX <= _closeRange)
        {
            StartBiteAttack();
        }
        // 3. Средняя дистанция
        else if (distanceX <= _mediumRange)
        {
            // Шанс 50% плюнуть кислотой перед ударом
            bool doAcidCombo = UnityEngine.Random.value > 0.5f;

            if (doAcidCombo)
            {
                StartAcidAttack(true); // true = после плевка будет удар когтем
            }
            else
            {
                StartSlashAttack(CheckLowHpCharge()); // Проверяем, нужен ли рывок после удара
            }
        }
        // 4. Дальняя дистанция
        else
        {
            // Просто плевок, либо рывок
            bool doCharge = UnityEngine.Random.value > 0.3f; // Например, 70% шанс рывка издалека (или настрой сам)
            
            if (doCharge)
            {
                StartChargeAttack();
            }
            else
            {
                StartAcidAttack(false);
            }
            
            // Если ничего не выбрали (кулдауны и т.д.), просто идем
            if (!_isAttacking) MoveTowardsPlayer();
        }
    }

    // Проверка на < 50% HP для рывка после удара
    private bool CheckLowHpCharge()
    {
        float hpPercent = CurrentHealth / MaxHealth;
        if (hpPercent < 0.5f)
        {
            return UnityEngine.Random.value <= 0.6f; // 60% шанс
        }
        return false;
    }

    private void MoveTowardsPlayer()
    {
        if (_targetPlayer == null) return;

        float direction = Mathf.Sign(_targetPlayer.position.x - transform.position.x);
        Rb2d.linearVelocity = new Vector2(direction * MoveSpeed, Rb2d.linearVelocity.y);

        // Анимация ходьбы
        // _animator.SetBool("IsWalking", true);
    }

    private void StopMovement()
    {
        Rb2d.linearVelocity = new Vector2(0, Rb2d.linearVelocity.y);
        // _animator.SetBool("IsWalking", false);
    }

    // --- ЗАПУСК АТАК (Вызываются из логики) ---

    private void StartBiteAttack()
    {
        StopMovement();
        _isAttacking = true;
        
        // Запуск анимации укуса. Тайминг управляется анимацией.
        // _animator.SetTrigger("CastBite");
        Debug.Log("BOSS: Start Bite");
    }

    private void StartSlashAttack(bool queueCharge)
    {
        StopMovement();
        _isAttacking = true;
        _queueChargeAfterSlash = queueCharge;

        // Запуск анимации когтя.
        // _animator.SetTrigger("CastSlash");
        Debug.Log($"BOSS: Start Slash (Charge next: {queueCharge})");
    }

    private void StartAcidAttack(bool queueSlash)
    {
        StopMovement();
        _isAttacking = true;
        _queueSlashAfterAction = queueSlash;

        // Запуск анимации плевка.
        // _animator.SetTrigger("CastAcid");
        Debug.Log($"BOSS: Start Acid (Slash next: {queueSlash})");
    }

    private void StartChargeAttack()
    {
        StopMovement();
        _isAttacking = true;
        _isCharging = true;

        // Анимация подготовки к рывку
        // _animator.SetTrigger("CastCharge");
        Debug.Log("BOSS: Prepare Charge");
    }


    // --- СОБЫТИЯ АНИМАЦИИ (ANIMATION EVENTS) ---
    // Эти методы ты должен добавить в Events внутри Animation Clip в Unity

    // 1. Событие в момент укуса (кадр нанесения урона)
    public void AnimEvent_BiteHit()
    {
        if (IsPlayerInZone(_biteCollider))
        {
            Debug.Log("BOSS: Bite Hit!");
            
            // Наносим урон
            PlayerData player = _targetPlayer.GetComponent<PlayerData>();
            if(player != null) DealDamagePlayer(player);

            // Логика отбрасывания (Knockback)
            Rigidbody2D playerRb = _targetPlayer.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 dir = (_targetPlayer.position - transform.position).normalized;
                // playerRb.AddForce(dir * 10f + Vector2.up * 5f, ForceMode2D.Impulse); // Пример
                // Или вызов метода: player.ApplyKnockback(...);
            }
        }
    }

    // 2. Событие в момент удара когтем
    public void AnimEvent_SlashHit()
    {
        if (IsPlayerInZone(_clawCollider))
        {
            Debug.Log("BOSS: Slash Hit!");
            
            PlayerData player = _targetPlayer.GetComponent<PlayerData>();
            if(player != null) DealDamagePlayer(player);

            // Логика оглушения (Stun)
            // player.ApplyStun(1.5f); // Твой метод стана игрока
        }
    }

    // 3. Событие в момент вылета кислоты
    public void AnimEvent_SpawnAcid()
    {
        if (_acidProjectilePrefab != null && _firePoint != null)
        {
            Debug.Log("BOSS: Acid Spit!");
            // Создаем префаб
            GameObject acid = Instantiate(_acidProjectilePrefab, _firePoint.position, Quaternion.identity);
            
            // Задаем направление (rotation). Например, в сторону игрока
            Vector3 dir = (_targetPlayer.position - _firePoint.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            acid.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    // 4. Событие начала движения рывка (после подготовки)
    public void AnimEvent_StartChargeMovement()
    {
        // Придаем ускорение
        float dir = transform.localScale.x > 0 ? 1 : -1; // Или использовать sprite flip
        Rb2d.linearVelocity = new Vector2(dir * _chargeSpeed, 0); 
    }

    // 5. Событие ОКОНЧАНИЯ любой атаки (в самом конце анимации)
    // ВАЖНО: Это событие должно быть в конце анимаций Bite, Slash, Acid, Charge (End)
    public void AnimEvent_AttackFinished()
    {
        _isAttacking = false;
        _isCharging = false;
        Rb2d.linearVelocity = Vector2.zero; // Останавливаемся после рывка/атаки

        // Проверка очередей (Комбо)
        
        if (_queueSlashAfterAction)
        {
            _queueSlashAfterAction = false;
            // Сразу переходим в атаку когтем, если было запланировано (Acid -> Slash)
            // Проверяем, нужно ли добавить рывок после этого удара (HP < 50%)
            StartSlashAttack(CheckLowHpCharge()); 
            return; 
        }

        if (_queueChargeAfterSlash)
        {
            _queueChargeAfterSlash = false;
            StartChargeAttack();
            return;
        }

        // Если очередей нет, босс возвращается в Idle/Move в следующем кадре Update
    }


    // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ---

    private void FindPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, _visionRadius, LayerMask.GetMask("Player"));
        if (hit != null)
        {
            _targetPlayer = hit.transform;
        }
    }

    private void FaceTarget()
    {
        if (_targetPlayer.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }
        else
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }
    }

    // Проверка, попадает ли игрок в конкретный коллайдер атаки
    private bool IsPlayerInZone(Collider2D attackZone)
    {
        if (_targetPlayer == null || attackZone == null) return false;

        Collider2D playerCol = _targetPlayer.GetComponent<Collider2D>();
        if (playerCol == null) return false;

        return attackZone.IsTouching(playerCol);
    }

    // Переопределение получения урона для реакции
    public override void TakeDamage(float damageInp)
    {
        base.TakeDamage(damageInp);
        
        // Тут можно добавить визуальный эффект попадания или звук
        // _animator.SetTrigger("GetHit"); 
    }

    private void OnDestroy()
    {
        OnDead -= HandleDeath;
    }

    private void HandleDeath()
    {
        _isDead = true;
        StopMovement();
        GetComponent<Collider2D>().enabled = false; // Отключаем физику
        
        // _animator.SetTrigger("IsDead");
        
        // Спавн лужи яда (можно тоже через Animation Event в конце анимации смерти)
        // Instantiate(_poisonPuddlePrefab, transform.position, Quaternion.identity);
    }
    
    // Для визуализации в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _visionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _closeRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _mediumRange);
    }
}
