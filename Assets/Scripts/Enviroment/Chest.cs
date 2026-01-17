using System;
using UnityEngine;

public class Chest : MonoBehaviour, IDamageable
{
    [SerializeField] private int scoreReward = 50; // Сколько очков давать за сундук
    
    public Animator animator;
    private bool isOn = true; // true = сундук закрыт, false = открыт\
    
    private Collider2D col2d;

    private void Start()
    {
        col2d = this.GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Проверяем, что это атака игрока И что сундук еще закрыт (isOn)
        if (collision.CompareTag("PlayerAttack") && isOn)
        {
            OpenChest();
        }
    }

    public void TakeDamage(float damage)
    {
        // Если сундук уже открыт, ничего не делаем
        if (!isOn) return;

        OpenChest();
    }

    private void OpenChest()
    {
        isOn = false;
        animator.SetBool("IsOn", isOn);
        
        col2d.enabled = false;

        // --- НАЧИСЛЕНИЕ ОЧКОВ ---
        if (ScoreCounter.Instance != null)
        {
            // Если сундук "хороший" или просто даем очки всем сундукам
            // (можно добавить проверку if (isGood))
            ScoreCounter.Instance.AddScore(scoreReward);
        }
        else
        {
            Debug.LogWarning("ScoreCounter не найден на сцене!");
        }
        
        // Тут можно добавить звук открытия
        // AudioEffectSystem.Instance?.PlayAudioClip(0);
    }
}