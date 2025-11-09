using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class SpellCaster : MonoBehaviour
{
    [Header("Настройки заклинаний")]
    [SerializeField]
    private SpellData[] availableSpells;
    
    [Header("Текущее заклинание")]
    [SerializeField]
    private int currentSpellIndex = 0;

    [Header("Перезарядка")]
    [SerializeField]
    private float spellCooldown = 1f; // Общее время перезарядки
    private float currentCooldown = 0f; // Текущее оставшееся время перезарядки
    
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
        currentCooldown = 0f; // Инициализируем перезарядку
    }

    void Update()
    {
        if (mouse == null) return;

        // Обновление перезарядки
        UpdateCooldown();

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

        // Обработка ПКМ (теперь проверяем canCast)
        if (mouse.rightButton.wasPressedThisFrame && canCast && availableSpells.Length > 0)
        {
            CastSpell();
        }
    }

    void UpdateCooldown()
    {
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
            
            // Если перезарядка закончилась
            if (currentCooldown <= 0f)
            {
                currentCooldown = 0f;
                canCast = true;
                
                if (debugMode)
                    Debug.Log("Перезарядка завершена, можно кастовать заклинания");
            }
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
        if (availableSpells.Length == 0 || !canCast) return;

        SpellData selectedSpell = availableSpells[currentSpellIndex];
        
        SpellCastData spellData = new SpellCastData
        {
            spellPrefab = selectedSpell.spellPrefab,
            caster = gameObject,
        };

        OnSpellCast?.Invoke(spellData);
        
        // Запускаем перезарядку
        StartCooldown();
        
        if (debugMode)
            Debug.Log($"Создано событие создания заклинания: {availableSpells[currentSpellIndex].spellName}");
    }

    void StartCooldown()
    {
        canCast = false;
        currentCooldown = spellCooldown;
        
        if (debugMode)
            Debug.Log($"Начата перезарядка: {spellCooldown} секунд");
    }

    // Метод для получения информации о перезарядке (может пригодиться для UI)
    public float GetCooldownProgress()
    {
        if (spellCooldown <= 0f) return 1f;
        return 1f - (currentCooldown / spellCooldown);
    }

    public bool IsOnCooldown()
    {
        return !canCast;
    }

    public float GetRemainingCooldown()
    {
        return currentCooldown;
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