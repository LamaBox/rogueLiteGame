using System;
using UnityEngine;

public static class PlayerDataStructures
{
    // Структуры только для передачи данных через события
    //Структура для основных ресурсов - хп, мана, выносливость
    public readonly struct ResourceData
    {
        public readonly float current;
        public readonly float max;
        public readonly ResourceType type; // Добавляем тип
        
        public ResourceData(float current, float max, ResourceType type)
        {
            this.current = current;
            this.max = max;
            this.type = type;
        }
    }

    public enum ResourceType
    {
        Health,
        Mana,
        Stamina,
    }
    
    public readonly struct MovementModifiersData
    {
        public readonly float playerSpeed;
        public readonly float sprintMultiplier;
        public readonly float jumpHeight;
        public readonly float dashSpeed;
        public readonly float dashCooldown;
        public readonly float gravityScale;

        public MovementModifiersData(float moveSpeed, float sprintMultiplier, float jumpHeight,
                                     float dashSpeed, float dashCooldown,
                                     float gravityScale)
        {
            this.playerSpeed = moveSpeed;
            this.sprintMultiplier = sprintMultiplier;
            this.jumpHeight = jumpHeight;
            this.dashSpeed = dashSpeed;
            this.dashCooldown = dashCooldown;
            this.gravityScale = gravityScale;
        }
    }
}
