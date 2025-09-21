using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float playerSpeed = 10; //обычная скорость
    [SerializeField] private float playerSprintMult = 1.3f; //модификатор ускорения
    [SerializeField] private float playerJumpSpeed = 15; //скорость прыжка
    [SerializeField] private float playerDashSpeed = 50; //скорость рывка
    [SerializeField] private float dashDuration = 0.13f; //длительность дэша
    [SerializeField] private float dashCooldown = 1f; //кулдаун рывка
    [SerializeField] private float gravityScale = 10f; //множитель гравитации

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
    private bool isDashing = false; //флал для рывка

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.gravityScale = gravityScale; //устанавливаем гравитацию

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
        dashCooldownTimer = dashCooldown;
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
        rb2d.gravityScale = gravityScale; // Восстанавливаем гравитацию
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
}
