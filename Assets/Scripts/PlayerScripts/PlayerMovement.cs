using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerDataStructures;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float playerDashSpeed = 0; 
    [SerializeField] private float playerdashCooldown = 0; 
    [SerializeField] private float playergravityScale = 0; 

    [Header("GroundCheck Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer = 1;

    private Rigidbody2D rb2d;
    
    // Переменные движения
    private float axis = 0; 
    private bool isFacingRight = true; 

    // БЛОКИРОВКА ДВИЖЕНИЯ
    private bool isMovementLocked = false; 

    // Переменные рывка
    private float dashCooldownTimer = 0f;
    
    // Глобальный флаг "Мы в режиме рывка" (блокирует управление, включаем анимацию)
    private bool isDashing = false; 
    
    // Флаг физики "Прикладываем силу" (включается Ивентом)
    private bool isDashPhysicsActive = false;

    private float lockedDashDirection = 0f; 
    
    // Страховка
    private float dashSafetyTimer = 0f; 
    private const float MAX_DASH_SAFETY_TIME = 0.5f; 
    
    private float playerSpeed = 0;
    private float playerJumpSpeed = 0;

    public bool GetIsDashing() => isDashing;
    public float GetAxis() => axis;

    // Геттер для аниматора или других систем, чтобы знать, заблокированы ли мы
    public bool IsMovementLocked() => isMovementLocked;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        if(rb2d != null) rb2d.gravityScale = playergravityScale;

        if (groundCheck == null)
            Debug.LogError("GroundCheck object not assigned to " + gameObject.name);
    }

    void Update()
    {
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        // Страховка: если анимация зависла и не вызвала StopDash
        if (isDashing)
        {
            dashSafetyTimer -= Time.deltaTime;
            if (dashSafetyTimer <= 0)
            {
                Debug.LogWarning("Dash Safety Triggered! Stop Event missed.");
                StopDash();
            }
        }
    }

    void FixedUpdate()
    {
        // 1. Если движение заблокировано (каст магии, стан и т.д.)
        if (isMovementLocked)
        {
            // Если мы в этот момент НЕ в рывке (рывок имеет инерцию, его лучше не стопорить резко, если не надо)
            if (!isDashing)
            {
                rb2d.linearVelocityX = 0; // Останавливаемся по горизонтали
                // Гравитация (VelocityY) продолжает работать штатно
                return;
            }
            // Если мы заблокировали движение ВО ВРЕМЯ рывка, можно либо прервать рывок (StopDash),
            // либо дать ему долететь. Обычно при стане рывок прерывают, при касте - каст не должен работать в рывке.
            // Пока оставим выполнение рывка, если он уже начался.
        }

        // 2. Логика рывка
        if (isDashing)
        {
            // Если физика активирована ивентом - летим
            if (isDashPhysicsActive)
            {
                rb2d.linearVelocity = new Vector2(lockedDashDirection * playerDashSpeed, 0);
            }
            else
            {
                // Фаза "Замаха"
                rb2d.linearVelocity = Vector2.zero;
                rb2d.gravityScale = 0f; 
            }
        }
        // 3. Обычное движение
        else
        {
            Move();
        }
    }

    // --- ПУБЛИЧНЫЕ МЕТОДЫ БЛОКИРОВКИ ---

    /// <summary>
    /// Запрещает двигаться, прыгать и делать рывок.
    /// Вызывать перед началом каста.
    /// </summary>
    public void LockMovement()
    {
        isMovementLocked = true;
        // Сразу гасим инерцию, чтобы персонаж не "скользил" во время каста
        if (rb2d != null && !isDashing) 
        {
            rb2d.linearVelocityX = 0;
        }
    }

    /// <summary>
    /// Разрешает движение.
    /// Вызывать после окончания каста или прерывания.
    /// </summary>
    public void UnlockMovement()
    {
        isMovementLocked = false;
    }

    // ------------------------------------

    public void Move()
    {
        rb2d.linearVelocityX = axis * playerSpeed;

        if (axis > 0 && isFacingRight) Flip();
        else if (axis < 0 && !isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void MoveInput(InputAction.CallbackContext context)
    {
        axis = context.ReadValue<float>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        // Добавил проверку !isMovementLocked
        if (context.performed && CheckGround() && !isDashing && !isMovementLocked)
        {
            rb2d.linearVelocityY = playerJumpSpeed;
        }
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.performed) TryStartDash();
    }
    
    public void DashInput(InputAction.CallbackContext context)
    {
        if (context.performed) TryStartDash();
    }

    private void TryStartDash()
    {
        // Добавил проверку !isMovementLocked
        // Проверки: не дешим сейчас, кулдаун прошел, движение разрешено
        if (!isDashing && dashCooldownTimer <= 0 && !isMovementLocked)
        {
            isDashing = true;
            isDashPhysicsActive = false; 
            
            dashCooldownTimer = playerdashCooldown;
            dashSafetyTimer = MAX_DASH_SAFETY_TIME;

            lockedDashDirection = isFacingRight ? -1f : 1f;
        }
    }

    // --- EVENTS FOR ANIMATION ---

    public void StartDashPhysics()
    {
        if (!isDashing) return; 
        
        isDashPhysicsActive = true;
        rb2d.gravityScale = 0f; 
        rb2d.linearVelocityY = 0f;
    }

    public void StopDash()
    {
        if (!isDashing) return;

        isDashing = false;
        isDashPhysicsActive = false;
        
        rb2d.gravityScale = playergravityScale; 
        rb2d.linearVelocity = Vector2.zero; 
    }

    // --- REST OF THE CODE ---

    private bool CheckGround()
    {
        if (groundCheck != null)
            return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.7f) 
                {
                    float pushDir = isFacingRight ? 1 : -1; 
                    var v = new Vector2(pushDir * 300f, 2f);
                    rb2d.AddForce(v, ForceMode2D.Force);
                    return; 
                }
            }
        }
    }

    #region EventsMetods
    public void OnMovementModifiersChanged(MovementModifiersData data)
    {
        UpdateMovementValues(data);
    }

    private void UpdateMovementValues(MovementModifiersData data)
    {
        playerSpeed = data.PlayerSpeed;
        playerJumpSpeed = data.JumpHeight;
        playerDashSpeed = data.DashSpeed;
        playerdashCooldown = data.DashCooldown;
        playergravityScale = data.GravityScale;

        if (rb2d != null && !isDashing)
        {
            rb2d.gravityScale = playergravityScale;
        }
    }
    #endregion
}