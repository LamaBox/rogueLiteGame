using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class SpellCaster : MonoBehaviour
{
    [Header("Настройки заклинаний")]
    public SpellData[] availableSpells;
    
    [Header("Текущее заклинание")]
    public int currentSpellIndex = 0;
    
    private bool canCast = true;

    // Событие для каста заклинания
    public delegate void SpellCastEventHandler(SpellCastData spellData);
    public static event SpellCastEventHandler OnSpellCast;
    
    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false;
    
    private Mouse mouse;
    private float scrollAccumulator;

    void Start()
    {
        mouse = Mouse.current;
    }

    void Update()
    {
        if (mouse == null) return;

        // Обработка прокрутки колесика - накапливаем значение
        Vector2 scroll = mouse.scroll.ReadValue();
        scrollAccumulator += scroll.y;

        // Если накопилось достаточно для смены заклинания
        if (Mathf.Abs(scrollAccumulator) >= 0.1f)
        {
            int direction = scrollAccumulator > 0 ? 1 : -1;
            ChangeSpell(direction);
            scrollAccumulator = 0f; // Сбрасываем аккумулятор
        }

        // Обработка ПКМ
        if (mouse.rightButton.wasPressedThisFrame && canCast && availableSpells.Length > 0)
        {
            CastSpell();
        }
    }

    void ChangeSpell(int direction)
    {
        if (availableSpells.Length == 0) return;
        
        int newIndex = currentSpellIndex + direction;
        
        // Зацикливание индекса
        if (newIndex < 0)
            newIndex = availableSpells.Length - 1;
        else if (newIndex >= availableSpells.Length)
            newIndex = 0;
        
        currentSpellIndex = newIndex;
        
        if (debugMode)
            Debug.Log($"Выбрано заклинание: {availableSpells[currentSpellIndex].spellName}, индекс: {currentSpellIndex}");
    }

    void CastSpell()
    {
        if (availableSpells.Length == 0) return;

        SpellData selectedSpell = availableSpells[currentSpellIndex];
        
        SpellCastData spellData = new SpellCastData
        {
            spellPrefab = selectedSpell.spellPrefab,
            caster = gameObject,
        };

        OnSpellCast?.Invoke(spellData);
        
        if (debugMode)
            Debug.Log($"Создано событие создания заклинания: {availableSpells[currentSpellIndex].spellName}");
    }
}

[System.Serializable]
public class SpellData
{
    public string spellName;
    public GameObject spellPrefab;
}

public struct SpellCastData
{
    public GameObject spellPrefab;
    public GameObject caster;
}