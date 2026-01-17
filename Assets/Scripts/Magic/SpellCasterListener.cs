using Magic;
using UnityEngine;
// using UnityEngine.InputSystem; // Больше не нужно, так как берем направление из трансформа

public class SpellCastListener : MonoBehaviour
{
    [Header("Настройки слушателя")]
    [SerializeField]
    private float spawnOffset = 1f;
    
    // Start и Update больше не нужны, так как мы не храним состояние кнопок
    
    void OnEnable() => SpellCaster.OnSpellCast += HandleSpellCast;
    void OnDisable() => SpellCaster.OnSpellCast -= HandleSpellCast;

    void HandleSpellCast(SpellCastData spellData) => CreateSpell(spellData);

    void CreateSpell(SpellCastData spellData)
    {
        if (spellData.spellPrefab == null || spellData.caster == null)
        {
            Debug.LogWarning("Префаб или кастер не задан!");
            return;
        }

        // Получаем X масштаб кастера
        float scaleX = spellData.caster.transform.localScale.x;

        // Логика согласно твоему ТЗ:
        // Если Scale X положительный (> 0) -> Направление ВЛЕВО (-1)
        // Если Scale X отрицательный (< 0) -> Направление ВПРАВО (1)
        float directionMultiplier = (scaleX > 0) ? -1f : 1f;

        // Вычисляем позицию спавна
        Vector3 spawnPos = spellData.caster.transform.position + 
                           Vector3.right * spawnOffset * directionMultiplier;
        
        // Вычисляем поворот
        // Если летим влево (-1), то разворачиваем на 180 градусов.
        // Если вправо (1), то поворот 0.
        Quaternion rotation = directionMultiplier < 0 ? 
            Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;

        Instantiate(spellData.spellPrefab, spawnPos, rotation);
        
        // Debug.Log($"Scale X: {scaleX}, Direction: {directionMultiplier}");
    }
}