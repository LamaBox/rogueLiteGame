using System.Collections;
using UnityEngine;

public class BugBoss : BotBase
{
    [Header("Boss Settings")]
    [SerializeField] private GameObject _acidProjectilePrefab;
    [SerializeField] private Transform _firePoint;
    
    [Header("Distances (From collider edge!)")]
    [SerializeField] private float _closeRange = 2.0f;
    [SerializeField] private float _mediumRange = 5.0f;
    // Новая настройка погрешности по X
    [Tooltip("Погрешность по горизонтали. Если игрок ближе этого расстояния по X, босс не поворачивается и не двигается.")]
    [SerializeField] private float _horizontalDeadZone = 0.5f; 

    [Header("Charge Settings")]
    [SerializeField] private float _chargeSpeed = 15f; 
    [SerializeField] private float _chargeDelay = 1.0f; 
    [SerializeField] private float _chargeDuration = 3.0f; 
    [SerializeField] private float _chargeDamage = 20f;
    [SerializeField] private float _impactStunDuration = 0.7f; 
    [SerializeField] private LayerMask _chargeStopLayers;
    
    [Header("Damage Settings")]
    [SerializeField] private float _biteDamage = 10f;
    [SerializeField] private float _slashDamage = 20f;

    [Header("AI Probabilities")]
    [Range(0f, 1f)] [SerializeField] private float _mediumRangeAcidChance = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float _farRangeChargeChance = 0.4f; 
    [Range(0f, 1f)] [SerializeField] private float _farRangeAcidChance = 0.3f;
    [Range(0f, 1f)] [SerializeField] private float _chargeComboHpThreshold = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float _chargeComboChance = 0.6f;

    [Header("Child Colliders")]
    [SerializeField] private Collider2D _biteCollider;
    [SerializeField] private Collider2D _clawCollider;       // Передний удар
    [SerializeField] private Collider2D _upperSlashCollider; // НОВЫЙ КОЛЛАЙДЕР (Удар вверх)
    
    [SerializeField] private float _visionRadius = 20f; 

    private Transform _targetPlayer;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer; 

    private bool _isAttacking = false;
    private bool _isCharging = false; 
    private bool _isChargeMoving = false; 
    private bool _isDead = false;

    private float _recoveryTimer = 0f;
    private bool _queueSlashAfterAction = false; 
    private bool _queueChargeAfterSlash = false; 

    private float _currentSpeedMultiplier = 1.0f; 
    private float _currentDelayMultiplier = 1.0f; 

    private float _nextDecisionTime = 0f;

    private Coroutine _chargeCoroutine;
    private Coroutine _watchdogCoroutine; 

    protected override void Start()
    {
        base.Start(); 
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_acidProjectilePrefab == null) Debug.LogError("Acid Prefab is missing!");

        OnDead += Death;
        UpdateRageStats();
    }

    protected override void Update()
    {
        base.Update(); 
        if (_isDead) return;

        if (Time.time < _recoveryTimer) return;

        if (_targetPlayer == null) FindPlayer();

        if (_targetPlayer != null && !_isAttacking && !_isCharging)
        {
            FaceTarget();
            DecideNextAction();
        }
    }

    private void DecideNextAction()
    {
        if (Time.time < _recoveryTimer) return;

        float distanceX = Mathf.Abs(transform.position.x - _targetPlayer.position.x);
        float distanceY = Mathf.Abs(transform.position.y - _targetPlayer.position.y);

        // 1. ПРОВЕРКА ВЕРХНЕЙ АТАКИ (Если игрок на платформе)
        // Если игрок касается верхнего коллайдера - бьем мечом
        if (IsPlayerInZone(_upperSlashCollider))
        {
            StopMovement(); // Остановиться, чтобы ударить точно
            StartSlashAttack(false);
            return;
        }

        // Если игрок просто высоко (но не в зоне удара), пытаемся подойти под него
        if (distanceY > 2.0f) // Чуть снизил порог высоты
        {
            MoveTowardsPlayer();
            // Если в процессе движения мы зацепили игрока обычным когтем (вдруг он низко висит)
            if (IsPlayerInZone(_clawCollider)) StartSlashAttack(false); 
            return;
        }

        // 2. Ближняя дистанция (по X)
        if (distanceX <= _closeRange) 
        {
            // Если мы близко, проверяем, можем ли ударить
            if (IsPlayerInZone(_clawCollider)) StartSlashAttack(false);
            else StartBiteAttack();
        }
        // 3. Средняя дистанция
        else if (distanceX <= _mediumRange)
        {
            bool doAcidCombo = UnityEngine.Random.value <= _mediumRangeAcidChance;
            if (doAcidCombo) StartAcidAttack(true); 
            else StartSlashAttack(CheckLowHpCharge()); 
        }
        // 4. Дальняя дистанция
        else
        {
            MoveTowardsPlayer();

            if (Time.time >= _nextDecisionTime)
            {
                _nextDecisionTime = Time.time + (1.0f * _currentDelayMultiplier);
                float roll = UnityEngine.Random.value; 

                if (roll <= _farRangeChargeChance) StartChargeAttack();
                else if (UnityEngine.Random.value <= _farRangeAcidChance) StartAcidAttack(false);
            }
        }
    }
    
    // --- ИЗМЕНЕНИЯ В ДВИЖЕНИИ И ПОВОРОТЕ (Лечим "дребезг") ---

    private void FaceTarget() 
    {
        // Вычисляем разницу по X
        float diff = _targetPlayer.position.x - transform.position.x;

        // Если разница меньше погрешности - НЕ поворачиваемся. 
        // Это убирает бесконечный флип, когда игрок ровно над боссом.
        if (Mathf.Abs(diff) < _horizontalDeadZone) return;

        if (diff > 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        else transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
    }

    private void MoveTowardsPlayer()
    {
        if (_targetPlayer == null) return;
        
        float diff = _targetPlayer.position.x - transform.position.x;

        // Если мы достаточно близко по X (в пределах погрешности), останавливаемся по горизонтали.
        // Это нужно, чтобы босс не дергался влево-вправо под игроком.
        if (Mathf.Abs(diff) < _horizontalDeadZone)
        {
            StopMovement();
            return;
        }

        float direction = Mathf.Sign(diff);
        float finalSpeed = MoveSpeed * _currentSpeedMultiplier;
        
        Rb2d.linearVelocity = new Vector2(direction * finalSpeed, Rb2d.linearVelocity.y);
        _animator.SetBool("IsMoving", true);
    }

    // --- ИЗМЕНЕНИЯ В НАНЕСЕНИИ УРОНА ---

    // Вызывается из анимации удара (Slash)
    public void AnimEvent_SlashHit() 
    { 
        bool hit = false;

        // 1. Проверяем передний коллайдер
        if (IsPlayerInZone(_clawCollider)) hit = true;
        
        // 2. Проверяем верхний коллайдер (для платформ)
        // Если игрок был в любом из них - наносим урон
        if (!hit && IsPlayerInZone(_upperSlashCollider)) hit = true;

        if (hit)
        {
            DealDamagePlayer(_targetPlayer.GetComponent<PlayerData>(), _slashDamage); 
        }
    }

    // --- ОСТАЛЬНОЙ КОД БЕЗ ИЗМЕНЕНИЙ ---

    private void Death()
    {
        _isDead = true;
        StopAllCoroutines();
        StopMovement();
        _isCharging = false;
        _isChargeMoving = false;
        _animator.SetTrigger("IsDead");
        Debug.Log("BOSS: Death sequence started.");
    }

    public void OnDeath()
    {
        OnDead -= Death;
        var col = GetComponent<Collider2D>();
        if(col != null) col.enabled = false;
        Rb2d.simulated = false;
        Rb2d.linearVelocity = Vector2.zero;
        Debug.Log("BOSS: Completely disabled.");
    }

    private void OnDestroy() { OnDead -= Death; }

    private void UpdateRageStats()
    {
        if (MaxHealth == 0) return; 
        float hpPercent = CurrentHealth / MaxHealth; 
        float lostHpPercent = 1.0f - hpPercent;      
        int steps = Mathf.FloorToInt(lostHpPercent * 10f); 
        _currentSpeedMultiplier = 1.0f + (steps * 0.1f);
        _currentDelayMultiplier = 1.0f - (steps * 0.05f);
        _currentDelayMultiplier = Mathf.Max(_currentDelayMultiplier, 0.1f);
        if (_animator != null) _animator.speed = _currentSpeedMultiplier;
    }

    public override void TakeDamage(float damageInp)
    {
        if (_isDead) return;
        base.TakeDamage(damageInp); 
        UpdateRageStats(); 
        _animator.SetTrigger("OnDamaged");
    }

    private bool CheckLowHpCharge()
    {
        float hpPercent = CurrentHealth / MaxHealth;
        return hpPercent < _chargeComboHpThreshold && UnityEngine.Random.value <= _chargeComboChance;
    }

    private void StopMovement()
    {
        Rb2d.linearVelocity = new Vector2(0, Rb2d.linearVelocity.y);
        _animator.SetBool("IsMoving", false);
    }

    private void TriggerAttack(string triggerName)
    {
        StopMovement();
        _isAttacking = true;
        _animator.ResetTrigger("IsBite");
        _animator.ResetTrigger("IsSlashing");
        _animator.ResetTrigger("IsAcid");
        _animator.SetTrigger(triggerName);

        if (_watchdogCoroutine != null) StopCoroutine(_watchdogCoroutine);
        _watchdogCoroutine = StartCoroutine(AttackWatchdog());
    }

    private void StartBiteAttack() { TriggerAttack("IsBite"); Debug.Log("BOSS: Start Bite"); }
    private void StartSlashAttack(bool queueCharge) { _queueChargeAfterSlash = queueCharge; TriggerAttack("IsSlashing"); Debug.Log("BOSS: Start Slash"); }
    private void StartAcidAttack(bool queueSlash) { _queueSlashAfterAction = queueSlash; TriggerAttack("IsAcid"); Debug.Log("BOSS: Start Acid"); }

    private void StartChargeAttack()
    {
        StopMovement();
        if (_chargeCoroutine != null) StopCoroutine(_chargeCoroutine);
        _chargeCoroutine = StartCoroutine(ChargeRoutine());
    }

    private IEnumerator ChargeRoutine()
    {
        _isCharging = true; 
        float currentDelay = _chargeDelay * _currentDelayMultiplier;
        Debug.Log($"BOSS: Charge preparing... wait {currentDelay} sec");
        yield return new WaitForSeconds(currentDelay);

        _isChargeMoving = true;
        _animator.SetBool("IsCharging", true); 
        Debug.Log("BOSS: CHARGE GO!");

        float dir = transform.localScale.x > 0 ? 1 : -1;
        float timer = 0f;
        float finalChargeSpeed = _chargeSpeed * _currentSpeedMultiplier;

        while (timer < _chargeDuration && _isChargeMoving)
        {
            Rb2d.linearVelocity = new Vector2(dir * finalChargeSpeed, Rb2d.linearVelocity.y);
            timer += Time.deltaTime;
            yield return null; 
        }
        StopCharge();
    }

    private void StopCharge()
    {
        if (!_isCharging) return;
        if (_chargeCoroutine != null) StopCoroutine(_chargeCoroutine);
        _isCharging = false;
        _isChargeMoving = false;
        _animator.SetBool("IsCharging", false);
        _animator.Play("Idle"); 
        Rb2d.linearVelocity = Vector2.zero; 
        float currentStun = _impactStunDuration * _currentDelayMultiplier;
        _recoveryTimer = Time.time + currentStun;
        AnimEvent_AttackFinished();
    }

    private IEnumerator AttackWatchdog()
    {
        yield return new WaitForSeconds(3.0f * _currentDelayMultiplier);
        if (_isAttacking)
        {
            Debug.LogWarning("BOSS: Attack Watchdog triggered!");
            _isAttacking = false;
            _animator.Play("Idle");
            _queueSlashAfterAction = false;
            _queueChargeAfterSlash = false;
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isChargeMoving) return;
        if ((_chargeStopLayers.value & (1 << collision.gameObject.layer)) > 0) StopCharge();

        if (collision.gameObject.TryGetComponent(out PlayerData player))
        {
            DealDamagePlayer(player, _chargeDamage); 
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb)
            {
                Vector2 dir = (player.transform.position - transform.position).normalized;
                playerRb.AddForce(dir * 10f + Vector2.up * 5f, ForceMode2D.Impulse);
            }
             StopCharge(); 
        }
    }

    public void AnimEvent_AttackFinished()
    {
        if (_isCharging) return;
        _isAttacking = false;
        Rb2d.linearVelocity = Vector2.zero; 
        if (_watchdogCoroutine != null) StopCoroutine(_watchdogCoroutine);

        if (Time.time < _recoveryTimer) { _queueSlashAfterAction = false; _queueChargeAfterSlash = false; return; }
        if (_queueSlashAfterAction) { _queueSlashAfterAction = false; StartSlashAttack(CheckLowHpCharge()); return; }
        if (_queueChargeAfterSlash) { _queueChargeAfterSlash = false; StartChargeAttack(); return; }
    }

    public void AnimEvent_BiteHit() 
    { 
        if (IsPlayerInZone(_biteCollider)) 
            DealDamagePlayer(_targetPlayer.GetComponent<PlayerData>(), _biteDamage); 
    }

    public void AnimEvent_SpawnAcid() { 
        if (_acidProjectilePrefab && _firePoint) {
            var acid = Instantiate(_acidProjectilePrefab, _firePoint.position, Quaternion.identity);
            Vector3 dir = (_targetPlayer.position - _firePoint.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            acid.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void FindPlayer() {
        var hit = Physics2D.OverlapCircle(transform.position, _visionRadius, LayerMask.GetMask("Player"));
        if (hit) _targetPlayer = hit.transform;
    }
    
    // В IsPlayerInZone добавлена проверка на null, чтобы не падало, если коллайдер не назначен
    private bool IsPlayerInZone(Collider2D z) => _targetPlayer && z && z.IsTouching(_targetPlayer.GetComponent<Collider2D>());
    
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, _visionRadius);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, _closeRange);
        Gizmos.color = Color.blue; Gizmos.DrawWireSphere(transform.position, _mediumRange);
    }
    
    protected void DealDamagePlayer(PlayerData playerData, float damageAmount)
    {
         playerData.ChangeValueResource(-damageAmount, PlayerDataStructures.ResourceType.Health, PlayerDataStructures.ResourceValueType.Current, true);
    }
}