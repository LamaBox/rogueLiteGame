using UnityEngine;

public interface IDamageable
{
    // Метод получения урона. 
    // float damage — количество урона.
    void TakeDamage(float damage);
}

