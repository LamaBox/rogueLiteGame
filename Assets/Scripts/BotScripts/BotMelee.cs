using System.Collections; // System.Random может конфликтовать с UnityEngine.Random, поэтому лучше не использовать using System; если не обязательно
using UnityEngine;

public class BotMelee : BotBase
{
    [SerializeField] private Transform[] _waypoints;
    [SerializeField] private Collider2D _visionCollider;
    [SerializeField] private Collider2D _attackCollider;
    [SerializeField] private RoomExitController _roomExitController;

    private int _currentWaypointIndex = 0;
    private bool _isMovingToPlayer = false;
    private Transform _targetPlayer = null;
    private bool _isAttacking = false;
    private bool _isMovingToLastPlayerPosition = false;
    private Vector2 _lastPlayerPosition;

    private Animator _animator;

    private bool _isStunned = false;
    
    // Новая переменная для подсчета полученных ударов
    private int _damageHitCounter = 0;

    private void Start()
    {
        base.Start();

        OnDead += Death;
        OnDamagedEvent += OnDamaged;

        if (_waypoints == null || _waypoints.Length == 0)
        {
            Debug.LogError($"{nameof(BotMelee)} - Waypoints array is null or empty!");
        }

        if (_visionCollider == null)
        {
            Debug.LogError($"{nameof(BotMelee)} - Vision collider is null! Please assign it in the inspector.");
        }

        if (_attackCollider == null)
        {
            Debug.LogError($"{nameof(BotMelee)} - Attack collider is null! Please assign it in the inspector.");
        }

        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError($"{nameof(BotMelee)} - Animator component is missing!");
        }
    }

    private void Update()
    {
        if (_isStunned) return;

        if (_isAttacking) return;

        if (_targetPlayer != null)
        {
            if (IsPlayerInVision())
            {
                if (IsPlayerInAttackDistance())
                {
                    StartAttack();
                }
                else
                {
                    MoveTowardsPlayer();
                }
            }
            else
            {
                _lastPlayerPosition = _targetPlayer.position;
                _targetPlayer = null;
                _isMovingToPlayer = false;
                _isMovingToLastPlayerPosition = true;
            }
        }
        else
        {
            CheckForPlayerInVision();

            if (_isMovingToLastPlayerPosition)
            {
                MoveToLastPlayerPosition();
            }
            else if (!_isMovingToPlayer && _waypoints.Length > 0)
            {
                MoveToWaypoint();
            }
        }

        if (_animator != null)
        {
            bool isCurrentlyMoving = Mathf.Abs(Rb2d.linearVelocity.x) > 0.1f;
            _animator.SetBool("IsMoving", isCurrentlyMoving);
        }

        RotateToDirection();
    }

    private void StartAttack()
    {
        if (_isAttacking || _isStunned) return;

        if (_targetPlayer != null)
        {
            float playerX = _targetPlayer.position.x;
            float botX = transform.position.x;

            if (playerX > botX)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (playerX < botX)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }

        _isAttacking = true;

        if (_animator != null)
        {
            _animator.SetTrigger("IsAttack");
        }
    }

    private void RotateToDirection()
    {
        if (_isStunned) return;

        float velocityX = Rb2d.linearVelocity.x;

        if (velocityX > 0.1f)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (velocityX < -0.1f)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    private void CheckForPlayerInVision()
    {
        if (_isStunned) return;

        if (_visionCollider == null) return;

        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(LayerMask.GetMask("Player"));
        contactFilter.useTriggers = true;

        Collider2D[] overlappingColliders = new Collider2D[10];
        int numColliders = _visionCollider.Overlap(contactFilter, overlappingColliders);

        for (int i = 0; i < numColliders; i++)
        {
            Collider2D col = overlappingColliders[i];
            if (col.gameObject.layer == 8) // слой Player
            {
                _targetPlayer = col.transform;
                _isMovingToPlayer = true;
                _isMovingToLastPlayerPosition = false;
                return;
            }
        }
    }

    private bool IsPlayerInVision()
    {
        if (_targetPlayer == null || _visionCollider == null) return false;

        Collider2D playerCollider = _targetPlayer.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            return _visionCollider.IsTouching(playerCollider);
        }
        return false;
    }

    private bool IsPlayerInAttackDistance()
    {
        if (_targetPlayer == null) return false;

        float distanceToPlayerX = Mathf.Abs(_targetPlayer.position.x - transform.position.x);
        return distanceToPlayerX <= AttackDistance;
    }

    private bool IsPlayerInAttackRange()
    {
        if (_targetPlayer == null || _attackCollider == null) return false;

        Collider2D playerCollider = _targetPlayer.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            return _attackCollider.IsTouching(playerCollider);
        }
        return false;
    }

    private void MoveToWaypoint()
    {
        if (_isStunned) return;

        if (_waypoints.Length == 0) return;

        float targetX = _waypoints[_currentWaypointIndex].position.x;
        float currentX = transform.position.x;

        float directionX = targetX - currentX;

        if (Mathf.Abs(directionX) <= 0.1f)
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;
            return;
        }

        directionX = Mathf.Sign(directionX);

        Vector2 newVelocity = Rb2d.linearVelocity;
        newVelocity.x = directionX * MoveSpeed;
        Rb2d.linearVelocity = newVelocity;
    }

    private void MoveToLastPlayerPosition()
    {
        if (_isStunned) return;

        float targetX = _lastPlayerPosition.x;
        float currentX = transform.position.x;

        float directionX = targetX - currentX;

        if (Mathf.Abs(directionX) <= 0.1f)
        {
            _isMovingToLastPlayerPosition = false;
            return;
        }

        directionX = Mathf.Sign(directionX);

        Vector2 newVelocity = Rb2d.linearVelocity;
        newVelocity.x = directionX * MoveSpeed;
        Rb2d.linearVelocity = newVelocity;
    }

    private void MoveTowardsPlayer()
    {
        if (_isStunned) return;

        if (_targetPlayer == null) return;

        float targetX = _targetPlayer.position.x;
        float currentX = transform.position.x;

        float directionX = targetX - currentX;
        float distanceToPlayerX = Mathf.Abs(directionX);

        if (distanceToPlayerX > AttackDistance)
        {
            directionX = Mathf.Sign(directionX);
            Vector2 newVelocity = Rb2d.linearVelocity;
            newVelocity.x = directionX * MoveSpeed;
            Rb2d.linearVelocity = newVelocity;
        }
        else
        {
            Vector2 newVelocity = Rb2d.linearVelocity;
            newVelocity.x = 0f;
            Rb2d.linearVelocity = newVelocity;
        }
    }

    public void PerformAttack()
    {
        if (_isStunned) return;

        if (_targetPlayer != null && IsPlayerInAttackRange())
        {
            PlayerData playerData = _targetPlayer.GetComponent<PlayerData>();
            if (playerData != null)
            {
                DealDamagePlayer(playerData);
            }
        }
    }

    public void EndAttack()
    {
        if (_isStunned) return;

        _isAttacking = false;
    }

    public bool IsPlayerInRangeForAttack()
    {
        if (_targetPlayer == null || _attackCollider == null) return false;

        Collider2D playerCollider = _targetPlayer.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            return _attackCollider.IsTouching(playerCollider);
        }
        return false;
    }

    private void Death()
    {
        _animator.SetTrigger("IsDead");
    }

    public void OnDeath()
    {
        OnDead -= Death;
        OnDamagedEvent -= OnDamaged;

        if (_roomExitController != null)
        {
            _roomExitController.UpdateExitStates();
        }
        
        Destroy(this.gameObject);
    }

    public void OnDamaged()
    {
        _damageHitCounter++; // Увеличиваем счетчик ударов

        bool shouldStun = false;

        if (_damageHitCounter == 2)
        {
            // 2-й удар: шанс 70% (0.7)
            if (UnityEngine.Random.value <= 0.7f)
            {
                shouldStun = true;
            }
            // Если не повезло, счетчик остается 2, ждем 3-го удара
        }
        else if (_damageHitCounter >= 3)
        {
            // 3-й удар: шанс 30% (0.3)
            if (UnityEngine.Random.value <= 0.3f)
            {
                shouldStun = true;
            }
            
            // Если это был 3-й удар (или больше, для страховки) и мы не застанились,
            // или если застанились - в любом случае сбрасываем счетчик после обработки,
            // чтобы цикл начался заново со следующего удара.
            if (!shouldStun) 
            {
                _damageHitCounter = 0;
            }
        }

        // Применяем логику стана, если условия совпали
        if (shouldStun)
        {
            _damageHitCounter = 0; // Сбрасываем счетчик при успешном стане
            
            _isStunned = true;
            _isAttacking = false;
            _targetPlayer = null;
            _isMovingToPlayer = false;
            _isMovingToLastPlayerPosition = false;

            // Останавливаем движение
            Rb2d.linearVelocity = Vector2.zero;
            
            _animator.SetTrigger("OnDamaged");
        }
    }

    public void EndStun()
    {
        _isStunned = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (_waypoints != null && _waypoints.Length > 0)
        {
            for (int i = 0; i < _waypoints.Length; i++)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(_waypoints[i].position, 0.2f);

                if (i > 0)
                {
                    Gizmos.DrawLine(_waypoints[i - 1].position, _waypoints[i].position);
                }
            }

            if (_waypoints.Length > 1)
            {
                Gizmos.DrawLine(_waypoints[_waypoints.Length - 1].position, _waypoints[0].position);
            }
        }

        if (_attackCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_attackCollider.bounds.center, _attackCollider.bounds.size);
        }

        if (_visionCollider != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(_visionCollider.bounds.center, _visionCollider.bounds.size);
        }
    }
}