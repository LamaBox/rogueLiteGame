using System;
using UnityEngine;

public class BotMelee : BotBase
{
    [SerializeField] private Transform[] _waypoints;
    [SerializeField] private Collider2D _visionCollider; // любой 2D коллайдер (дочерний объект)
    [SerializeField] private Collider2D _attackCollider; // любой 2D коллайдер (дочерний объект)

    private int _currentWaypointIndex = 0;
    private bool _isMovingToPlayer = false;
    private Transform _targetPlayer = null;
    private bool _isAttacking = false;
    private bool _isMovingToLastPlayerPosition = false;
    private Vector2 _lastPlayerPosition;

    private Animator _animator;

    private bool _isStunned = false; // флаг стана

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
        if (_isStunned) return; // НИЧЕГО не делаем, если в стане

        if (_isAttacking) return; // не двигаемся, если атакуем

        if (_targetPlayer != null)
        {
            // Проверяем, находится ли игрок всё ещё в зоне видимости
            if (IsPlayerInVision())
            {
                // Проверяем, находится ли игрок в пределах дистанции атаки по X
                if (IsPlayerInAttackDistance())
                {
                    // Запускаем анимацию атаки
                    StartAttack();
                }
                else
                {
                    // Если игрок не в зоне атаки - идём к нему
                    MoveTowardsPlayer();
                }
            }
            else
            {
                // Если игрок вышел из зоны видимости - сбрасываем цель
                _lastPlayerPosition = _targetPlayer.position; // запоминаем последнюю позицию
                _targetPlayer = null;
                _isMovingToPlayer = false;
                _isMovingToLastPlayerPosition = true; // переходим к последней позиции игрока
            }
        }
        else
        {
            // Проверяем, есть ли игрок в зоне видимости
            CheckForPlayerInVision();

            if (_isMovingToLastPlayerPosition)
            {
                // Двигаемся к последней позиции игрока
                MoveToLastPlayerPosition();
            }
            else if (!_isMovingToPlayer && _waypoints.Length > 0)
            {
                // Двигаемся по точкам
                MoveToWaypoint();
            }
        }

        // Обновляем аниматор
        if (_animator != null)
        {
            // Проверяем, движется ли бот (если скорость по X > 0.1f, считаем, что движется)
            bool isCurrentlyMoving = Mathf.Abs(Rb2d.linearVelocity.x) > 0.1f;
            _animator.SetBool("IsMoving", isCurrentlyMoving);
        }

        // Поворачиваем бота в сторону движения
        RotateToDirection();
    }

    private void StartAttack()
    {
        if (_isAttacking || _isStunned) return; // уже атакуем или в стане

        // Поворот в сторону игрока перед атакой
        if (_targetPlayer != null)
        {
            float playerX = _targetPlayer.position.x;
            float botX = transform.position.x;

            if (playerX > botX)
            {
                // Игрок справа - поворот на 0 градусов
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (playerX < botX)
            {
                // Игрок слева - поворот на 180 градусов
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            // Если playerX == botX, оставляем текущий поворот
        }

        _isAttacking = true;

        // Вызываем анимационный триггер атаки
        if (_animator != null)
        {
            _animator.SetTrigger("IsAttack");
        }
    }

    private void RotateToDirection()
    {
        if (_isStunned) return; // не поворачиваем, если в стане

        // Получаем текущую скорость по X
        float velocityX = Rb2d.linearVelocity.x;

        if (velocityX > 0.1f)
        {
            // Движемся вправо - поворот на 0 градусов
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (velocityX < -0.1f)
        {
            // Движемся влево - поворот на 180 градусов
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        // Если скорость близка к нулю, оставляем текущий поворот
    }

    private void CheckForPlayerInVision()
    {
        if (_isStunned) return; // не проверяем, если в стане

        if (_visionCollider == null) return;

        // Создаём ContactFilter2D и настраиваем его для фильтрации по слою "Player"
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(LayerMask.GetMask("Player"));
        contactFilter.useTriggers = true; // если коллайдеры триггеры

        // Получаем все коллайдеры, пересекающиеся с _visionCollider
        Collider2D[] overlappingColliders = new Collider2D[10]; // буфер для найденных коллайдеров
        int numColliders = _visionCollider.Overlap(contactFilter, overlappingColliders); // исправлено

        for (int i = 0; i < numColliders; i++)
        {
            Collider2D col = overlappingColliders[i];
            if (col.gameObject.layer == 8) // слой Player
            {
                _targetPlayer = col.transform;
                _isMovingToPlayer = true;
                _isMovingToLastPlayerPosition = false; // сбрасываем режим поиска
                return;
            }
        }
    }

    private bool IsPlayerInVision()
    {
        if (_targetPlayer == null || _visionCollider == null) return false;

        // Проверяем, всё ли ещё пересекаемся с коллайдером игрока
        Collider2D playerCollider = _targetPlayer.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            return _visionCollider.IsTouching(playerCollider);
        }
        return false;
    }

    // Проверяем, находится ли игрок в пределах AttackDistance по X
    private bool IsPlayerInAttackDistance()
    {
        if (_targetPlayer == null) return false;

        float distanceToPlayerX = Mathf.Abs(_targetPlayer.position.x - transform.position.x);
        return distanceToPlayerX <= AttackDistance;
    }

    // Проверяем, находится ли игрок в коллайдере атаки (для нанесения урона)
    private bool IsPlayerInAttackRange()
    {
        if (_targetPlayer == null || _attackCollider == null) return false;

        // Проверяем, пересекаемся ли с коллайдером игрока
        Collider2D playerCollider = _targetPlayer.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            return _attackCollider.IsTouching(playerCollider);
        }
        return false;
    }

    private void MoveToWaypoint()
    {
        if (_isStunned) return; // не двигаемся, если в стане

        if (_waypoints.Length == 0) return;

        // Используем только X-координату точки
        float targetX = _waypoints[_currentWaypointIndex].position.x;
        float currentX = transform.position.x;

        float directionX = targetX - currentX;

        // Проверяем, достигли ли мы точки по оси X
        if (Mathf.Abs(directionX) <= 0.1f) // достигли точки по X
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;
            return; // выходим, чтобы не двигаться в этот кадр
        }

        // Направление: 1 или -1
        directionX = Mathf.Sign(directionX);

        // Устанавливаем только X-составляющую скорости
        Vector2 newVelocity = Rb2d.linearVelocity;
        newVelocity.x = directionX * MoveSpeed;
        Rb2d.linearVelocity = newVelocity; // используем velocity, а не linearVelocity
    }

    private void MoveToLastPlayerPosition()
    {
        if (_isStunned) return; // не двигаемся, если в стане

        // Используем только X-координату последней позиции игрока
        float targetX = _lastPlayerPosition.x;
        float currentX = transform.position.x;

        float directionX = targetX - currentX;

        // Проверяем, достигли ли мы точки по оси X
        if (Mathf.Abs(directionX) <= 0.1f) // достигли точки по X
        {
            _isMovingToLastPlayerPosition = false; // возвращаемся к патрулированию
            return; // выходим, чтобы не двигаться в этот кадр
        }

        // Направление: 1 или -1
        directionX = Mathf.Sign(directionX);

        // Устанавливаем только X-составляющую скорости
        Vector2 newVelocity = Rb2d.linearVelocity;
        newVelocity.x = directionX * MoveSpeed;
        Rb2d.linearVelocity = newVelocity; // используем velocity
    }

    private void MoveTowardsPlayer()
    {
        if (_isStunned) return; // не двигаемся, если в стане

        if (_targetPlayer == null) return;

        // Используем только X-координату игрока
        float targetX = _targetPlayer.position.x;
        float currentX = transform.position.x;

        float directionX = targetX - currentX;
        float distanceToPlayerX = Mathf.Abs(directionX);

        if (distanceToPlayerX > AttackDistance)
        {
            // Двигаемся к игроку только по X
            directionX = Mathf.Sign(directionX);
            Vector2 newVelocity = Rb2d.linearVelocity;
            newVelocity.x = directionX * MoveSpeed;
            Rb2d.linearVelocity = newVelocity; // используем velocity
        }
        else
        {
            // Если близко к игроку по X, останавливаем движение по X
            Vector2 newVelocity = Rb2d.linearVelocity;
            newVelocity.x = 0f;
            Rb2d.linearVelocity = newVelocity;
        }
    }

    // Публичный метод для вызова из анимации
    public void PerformAttack()
    {
        if (_isStunned) return; // не атакуем, если в стане

        // ВАЖНО: Повторно проверяем, находится ли игрок в зоне атаки в момент вызова метода
        if (_targetPlayer != null && IsPlayerInAttackRange())
        {
            PlayerData playerData = _targetPlayer.GetComponent<PlayerData>();
            if (playerData != null)
            {
                DealDamagePlayer(playerData);
            }
        }
    }

    // Публичный метод для вызова из анимации
    public void EndAttack()
    {
        if (_isStunned) return; // не завершаем атаку, если в стане

        _isAttacking = false;
    }

    // Публичный метод для проверки, находится ли игрок в зоне атаки
    public bool IsPlayerInRangeForAttack()
    {
        if (_targetPlayer == null || _attackCollider == null) return false;

        // Проверяем, пересекаемся ли с коллайдером игрока
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
        Destroy(this.gameObject);
    }

    public void OnDamaged()
    {
        _isStunned = true;
        _isAttacking = false; // Прерываем атаку
        _targetPlayer = null; // Сбрасываем цель
        _isMovingToPlayer = false;
        _isMovingToLastPlayerPosition = false;

        // Останавливаем движение
        Rb2d.linearVelocity = Vector2.zero;
        
        _animator.SetTrigger("OnDamaged"); // или любой другой триггер для стана
    }

    // Публичный метод для вызова из анимации
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

        // Визуализация коллайдеров (опционально)
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