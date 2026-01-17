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

    // Переменные рывка
    private float dashCooldownTimer = 0f;
    
    // Глобальный флаг "Мы в режиме рывка" (блокирует управление, включает анимацию)
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
        if (isDashing)
        {
            // Если физика активирована ивентом - летим
            if (isDashPhysicsActive)
            {
                rb2d.linearVelocity = new Vector2(lockedDashDirection * playerDashSpeed, 0);
            }
            else
            {
                // Фаза "Замаха" (Wind-up): Анимация уже идет, но движения еще нет.
                // Останавливаем персонажа, чтобы он не скользил по инерции.
                rb2d.linearVelocity = Vector2.zero;
                rb2d.gravityScale = 0f; // Висим в воздухе во время подготовки
            }
        }
        else
        {
            Move();
        }
    }

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
        if (context.performed && CheckGround() && !isDashing)
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
        // Проверки: не дешим сейчас и кулдаун прошел
        if (!isDashing && dashCooldownTimer <= 0)
        {
            // 1. Включаем режим рывка (блокируем управление, включаем анимацию в контроллере)
            isDashing = true;
            isDashPhysicsActive = false; // Физика пока выключена, ждем ивента!
            
            dashCooldownTimer = playerdashCooldown;
            dashSafetyTimer = MAX_DASH_SAFETY_TIME;

            // 2. Фиксируем направление строго в момент нажатия
            // isFacingRight == true -> спрайт смотрит влево (из-за инверсии) -> летим влево (-1)
            // isFacingRight == false -> спрайт смотрит вправо -> летим вправо (1)
            lockedDashDirection = isFacingRight ? -1f : 1f;
        }
    }

    // --- EVENTS FOR ANIMATION ---

    // СОБЫТИЕ 1: Поставить в НАЧАЛО момента движения в анимации
    public void StartDashPhysics()
    {
        if (!isDashing) return; // Если рывок уже отменился, не запускаем
        
        isDashPhysicsActive = true;
        rb2d.gravityScale = 0f; 
        rb2d.linearVelocityY = 0f;
        // Debug.Log("Dash Physics Started via Event");
    }

    // СОБЫТИЕ 2: Поставить в КОНЕЦ анимации рывка
    public void StopDash()
    {
        if (!isDashing) return;

        isDashing = false;
        isDashPhysicsActive = false;
        
        rb2d.gravityScale = playergravityScale; 
        rb2d.linearVelocity = Vector2.zero; 
        // Debug.Log("Dash Stopped via Event");
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