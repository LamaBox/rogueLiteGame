using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))]
public class DestructibleObject : MonoBehaviour, IDamageable
{
    private Animator animator;
    private AudioEffectSystem audioEffectSystem;
    private bool isDestroyed = false;
    private Collider2D col2d;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        audioEffectSystem = GetComponent<AudioEffectSystem>();
        col2d = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    //public void TakeDamage
    public void TakeDamage(float damage)
    {
        if (!isDestroyed)
        {
            animator.SetTrigger("IsDestroy");
            isDestroyed = true;
            audioEffectSystem?.PlayAudioClip(0);
            col2d.enabled = false;
        }
        Debug.Log($"{this.name} damaged {damage}!");
    }
}
