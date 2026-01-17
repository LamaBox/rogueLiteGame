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
        if (animator == null)
            animator = GetComponent<Animator>();
        
        if (rb2d == null)
            rb2d = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        bool isDashing = playerMovement.GetIsDashing();

        // Если мы в рывке, ходьба и прыжки не должны перебивать анимацию
        bool isWalking = Mathf.Abs(playerMovement.GetAxis()) > 0.1f && !isDashing;
        bool isJumping = rb2d.linearVelocityY > 0.1f && !isDashing;
        bool isFalling = rb2d.linearVelocityY < -0.1f && !isDashing;
        bool isAttacking = playerAttack != null && !playerAttack.GetCanAttack();

        // Передаем параметры в аниматор
        animator.SetBool("isDashing", isDashing);
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isFalling", isFalling);
        animator.SetBool("isAttacking", isAttacking);
        
        // Важно: в Аниматоре должен быть Bool параметр "isDashing".
        // Переход в Dash происходит, когда true. Обратно, когда false.
    }
}
