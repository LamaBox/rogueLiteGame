using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerDataStructures;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    // [SerializeField] private float playerSpeed = 10; //обычная скорость
    // [SerializeField] private float playerSprintMult = 1.3f; //модификатор ускорения
    // [SerializeField] private float playerJumpSpeed = 15; //скорость прыжка
    // [SerializeField] private float playerDashSpeed = 50; //скорость рывка
    [SerializeField] private float dashDuration = 0.13f; //длительность дэша
    // [SerializeField] private float playerdashCooldown = 1f; //кулдаун рывка
    // [SerializeField] private float playergravityScale = 10f; //множитель гравитаци

    [Header("GroundCheck Settings")]
    [SerializeField] private Transform groundCheck; //местоположение объекта для проверки земли под ногами
    [SerializeField] private float groundCheckRadius = 0.2f; //радиус проверки земли
    [SerializeField] private LayerMask groundLayer = 1; //слои земли

    private Rigidbody2D rb2d; //переменная для физического компонента игрока

    //переменные связанные с перемещением
    private float axis = 0; //переменная для определения направления движения. -1 влево, 1 вправо
    private bool speedUp = false; //флаг для спринта

    //переменные связанные в рывком
    private float dashDirection = 0; //-1 влево, 1 вправо
    private float dashTimer = 0f; //таймер для рывка
    private float dashCooldownTimer = 0f; //таймер кулдауна для рывка
    private bool isDashing = false; //флаг для рывка

    private float playerSpeed = 0;
    private float playerSprintMult = 0;
    private float playerJumpSpeed = 0;
    private float playerDashSpeed = 0;
    private float playerdashCooldown = 0;
    private float playergravityScale = 0;

    public bool GetIsDashing()
    {
        return isDashing;
    }

    public bool GetSpeedUp()
    {
        return speedUp;
    }

    public float GetAxis()
    {
        return axis;
    }


    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.gravityScale = playergravityScale; //устанавливаем гравитацию

        if (groundCheck == null)
            Debug.LogError("GroundCheck object not assigned to " + gameObject.name);
    }

    void Update()
    {
        UpdateTimers();
    }

    void FixedUpdate()
    {
        Move();
    }

    //метод движения влево/вправо.
    public void Move()
    {
        //движение во время рывка
        if (isDashing)
        {
            // Во время рывка двигаемся с постоянной скоростью
            rb2d.linearVelocityX = dashDirection * playerDashSpeed;
            rb2d.linearVelocityY = 0;
        }
        //обычное передвижение
        else
        {
            float currentSpeed = speedUp ? playerSpeed * playerSprintMult : playerSpeed;
            rb2d.linearVelocityX = axis * currentSpeed;
        }
        if (axis != 0f)
        {
            // Меняем масштаб по X, чтобы "перевернуть" спрайт
            Vector3 scale = transform.localScale;
            scale.x = -(Mathf.Sign(axis) * Mathf.Abs(scale.x));
            transform.localScale = scale;
        }
    }

    //метод получения информации о направлении движения(-1, 0, 1)
    public void MoveInput(InputAction.CallbackContext context)
    {
        axis = context.ReadValue<float>();
    }

    //метод получения информации о прыжке(0, 1)
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && CheckGround() && !isDashing)
        {
            rb2d.linearVelocityY = playerJumpSpeed;
        }
    }

    //метод получения информации об ускорении(0, 1)
    public void Sprint(InputAction.CallbackContext context)
    {
        speedUp = context.performed;
    }

    //метод, который отвечает за получение данных для рывка
    public void DashInput(InputAction.CallbackContext context)
    {
        if (context.started && !isDashing)
        {
            dashDirection = context.ReadValue<float>();
            //Debug.Log("started" + dashDirection);
        }

        if (context.performed && !isDashing)
        {
            //Debug.Log("performed" + dashDirection);
            if (dashCooldownTimer <= 0)
            {
                StartDash();
            }
        }
    }

    //метод запускающий рывок
    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = playerdashCooldown;
        rb2d.gravityScale = 0.2f; // отключаем гравитацию во время рывка
        //Debug.Log("Dash started!");
    }

    //обновляеет таймеры для рывка(в будущем можно сделать не только для рывка)
    private void UpdateTimers()
    {
        // Обновляем таймер рывка
        if (dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                EndDash();
            }
        }

        // Обновляем cooldown рывка
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void EndDash()
    {
        isDashing = false;
        dashTimer = 0f;
        rb2d.gravityScale = playergravityScale; // Восстанавливаем гравитацию
        //Debug.Log("Dash ended!");
    }

    //проверка на наличие земли под ногами
    private bool CheckGround()
    {
        if (groundCheck != null)
            return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        return false;
    }

    //Визуализация зоны проверки на землю
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius,
                groundLayer);
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
                if (contact.normal.y > 0.7f) // контакт сверху
                {
                    float lookDir = -Mathf.Sign(transform.localScale.x);
                    var v = new Vector2(lookDir * 300f, 2f);
                    rb2d.AddForce(v, ForceMode2D.Force);
                    return; // хватит одного выталкивания
                }
            }
        }
    }

    #region EventsMetods
    // Метод для обработки изменений модификаторов движения
    public void OnMovementModifiersChanged(MovementModifiersData data)
    {
        UpdateMovementValues(data);
    }

    // Обновляем локальные переменные из данных PlayerData
    private void UpdateMovementValues(MovementModifiersData data)
    {
        playerSpeed = data.PlayerSpeed;
        playerSprintMult = data.SprintMultiplier;
        playerJumpSpeed = data.JumpHeight;
        playerDashSpeed = data.DashSpeed;
        playerdashCooldown = data.DashCooldown;
        playergravityScale = data.GravityScale;

        // Обновляем гравитацию в реальном времени
        if (rb2d != null && !isDashing)
        {
            rb2d.gravityScale = playergravityScale;
        }
    }
    #endregion
}
