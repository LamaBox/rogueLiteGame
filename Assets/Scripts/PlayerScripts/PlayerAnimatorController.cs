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
        // Получаем текущие состояния
        bool isWalking = Mathf.Abs(playerMovement.GetAxis()) > 0.1f && !playerMovement.GetIsDashing();
        bool isSprinting = isWalking && playerMovement.GetSpeedUp();
        bool isJumping = rb2d.linearVelocityY > 0.1f;
        bool isFalling = rb2d.linearVelocityY < -0.1f;
        bool isAttacking = playerAttack != null && !playerAttack.GetCanAttack(); // если атака в кулдауне — значит, она выполняется

        // Передаём в аниматор
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isSprinting", isSprinting);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isFalling", isFalling);
        animator.SetBool("isAttacking", isAttacking);
        //Debug.Log($"isWalking: {isWalking}, isSprinting: {isSprinting}, isJumping: {isJumping}, isFalling: {isFalling}, isAttacking: {isAttacking}");

        // Для Idle — можно не ставить отдельный параметр, он активен, когда остальные false
    }
}
