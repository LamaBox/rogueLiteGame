using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb2d;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAttack playerAttack;

    private void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb2d == null) rb2d = GetComponent<Rigidbody2D>();

        // ПОДПИСЫВАЕМСЯ НА СОБЫТИЕ АТАКИ
        // Как только в PlayerAttack произойдет атака, мы дернем триггер
        if (playerAttack != null)
        {
            playerAttack.OnAttackPerformed += PlayAttackAnimation;
        }
    }

    private void OnDestroy() // Обязательно отписываемся при уничтожении
    {
        if (playerAttack != null)
        {
            playerAttack.OnAttackPerformed -= PlayAttackAnimation;
        }
    }

    private void PlayAttackAnimation()
    {
        // Используем SetTrigger вместо SetBool
        // Убедись, что в Аниматоре создан параметр "Attack" (Trigger)
        animator.SetTrigger("Attack"); 
    }

    private void Update()
    {
        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        bool isDashing = playerMovement.GetIsDashing();
        bool isWalking = Mathf.Abs(playerMovement.GetAxis()) > 0.1f && !isDashing;
        bool isJumping = rb2d.linearVelocityY > 0.1f && !isDashing;
        bool isFalling = rb2d.linearVelocityY < -0.1f && !isDashing;

        // Мы УБРАЛИ отсюда isAttacking, так как теперь это событие, а не состояние
        
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isFalling", isFalling);
        animator.SetBool("isDashing", isDashing);
    }
}