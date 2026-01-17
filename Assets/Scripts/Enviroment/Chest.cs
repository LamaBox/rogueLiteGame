using UnityEngine;

public class Chest : MonoBehaviour, IDamageable
{
    public Animator animator;
    bool isOn = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerAttack"))
        {
            isOn = !isOn;
            animator.SetBool("IsOn", isOn);
        }
    }

    public void TakeDamage(float damage)
    {
        isOn = !isOn;
        animator.SetBool("IsOn", isOn);
    }
}