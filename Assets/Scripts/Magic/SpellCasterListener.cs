using Magic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpellCastListener : MonoBehaviour
{
    [Header("Настройки слушателя")]
    [SerializeField]
    private float spawnOffset = 1f;
    
    private float lastNonZeroDirection = 1f;
    private Keyboard keyboard;
    
    void Start()
    {
        keyboard = Keyboard.current;
    }
    
    void Update()
    {
        // Постоянно обновляем направление в Update
        if (keyboard != null)
        {
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                lastNonZeroDirection = 1f;
            else if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                lastNonZeroDirection = -1f;
            // Если ничего не нажато - сохраняем предыдущее направление
        }
    }
    
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

        // Просто используем lastNonZeroDirection, который постоянно обновляется в Update
        Vector3 spawnPos = spellData.caster.transform.position + 
                           Vector3.right * spawnOffset * Mathf.Sign(lastNonZeroDirection);
        
        Quaternion rotation = lastNonZeroDirection < 0 ? 
            Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;

        GameObject spell = Instantiate(spellData.spellPrefab, spawnPos, rotation);
        
        // if (spell.GetComponent<FireballController>() != null)
        //     Debug.Log($"Заклинание создано в направлении: {(lastNonZeroDirection > 0 ? "вправо" : "влево")}");
    }
}