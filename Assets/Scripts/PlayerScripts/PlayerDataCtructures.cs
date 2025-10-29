using System;
using UnityEngine;


public static class PlayerDataStructures
{
    // Структуры только для передачи данных через события
    //Структура для основных ресурсов - хп, мана, выносливость
    public readonly struct ResourceData
    {
        public readonly float Current;
        public readonly float Max;
        public readonly ResourceType Type; // Добавляем тип
        
        public ResourceData(float current, float max, ResourceType type)
        {
            this.Current = current;
            this.Max = max;
            this.Type = type;
        }
    }

    public enum ResourceType
    {
        Health,
        Mana,
        Stamina,
    }

    public enum ResourceValueType
    {
        Maximum,
        Current,
    }
    
    public readonly struct AttackModifiersData
    {
        public readonly float Damage;
        public readonly float AttackSpeed;
        public readonly float AttackRange;

        public AttackModifiersData(float damage, float attackSpeed, float attackRange)
        {
            this.Damage = damage;
            this.AttackSpeed = attackSpeed;
            this.AttackRange = attackRange;
        }
    }
    
    public readonly struct MovementModifiersData
    {
        public readonly float PlayerSpeed;
        public readonly float SprintMultiplier;
        public readonly float JumpHeight;
        public readonly float DashSpeed;
        public readonly float DashCooldown;
        public readonly float GravityScale;

        public MovementModifiersData(float moveSpeed, float sprintMultiplier, float jumpHeight,
                                     float dashSpeed, float dashCooldown,
                                     float gravityScale)
        {
            this.PlayerSpeed = moveSpeed;
            this.SprintMultiplier = sprintMultiplier;
            this.JumpHeight = jumpHeight;
            this.DashSpeed = dashSpeed;
            this.DashCooldown = dashCooldown;
            this.GravityScale = gravityScale;
        }
    }
}
