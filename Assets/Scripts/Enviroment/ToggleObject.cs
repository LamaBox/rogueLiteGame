using UnityEngine;

public class Bonfire : MonoBehaviour
{
    public Animator animator;
    bool isOn = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerAttack"))
        {
            isOn = !isOn;
            animator.SetBool("IsOn", false);
        }
    }
}