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

    [Header("Тайминги")]
    [SerializeField]
    private float castDelay = 0.5f; // ЗАДЕРЖКА ПЕРЕД КАСТОМ (Время произнесения)
    [SerializeField]
    private float spellCooldown = 1f; // Общее время перезарядки
    
    private float currentCooldown = 0f; // Текущее оставшееся время перезарядки
    private bool canCast = true;        // Флаг перезарядки
    private bool isCasting = false;     // Флаг: идет ли сейчас подготовка к выстрелу?

    // Событие для каста заклинания
    public delegate void SpellCastEventHandler(SpellCastData spellData);
    public static event SpellCastEventHandler OnSpellCast;
    
    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false;
    
    private Mouse mouse;
    private float scrollAccumulator;
    
    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;

    void Start()
    {
        mouse = Mouse.current;
        currentCooldown = 0f;
        
        playerMovement = GetComponent<PlayerMovement>();
        playerAttack = GetComponent<PlayerAttack>();
    }

    void Update()
    {
        if (mouse == null) return;

        UpdateCooldown();

        // Обработка прокрутки (смены заклинания)
        // Разрешаем менять заклинание только если НЕ кастуем сейчас
        if (!isCasting) 
        {
            Vector2 scroll = mouse.scroll.ReadValue();
            scrollAccumulator += scroll.y;

            if (Mathf.Abs(scrollAccumulator) >= 0.1f)
            {
                int direction = scrollAccumulator > 0 ? 1 : -1;
                ChangeSpell(direction);
                scrollAccumulator = 0f;
            }
        }

        // Обработка ПКМ
        // Проверяем: нажата кнопка + нет КД + не кастуем прямо сейчас + есть заклинания
        if (mouse.rightButton.wasPressedThisFrame && canCast && !isCasting && availableSpells.Length > 0)
        {
            StartCoroutine(CastSpellRoutine());
        }
    }

    // Корутина с задержкой
    IEnumerator CastSpellRoutine()
    {
        isCasting = true;

        // БЛОКИРУЕМ ВСЁ
        if (playerMovement != null) playerMovement.LockMovement();
        if (playerAttack != null) playerAttack.LockAttack();

        yield return new WaitForSeconds(castDelay);

        PerformSpellCast();
        StartCooldown();

        // РАЗБЛОКИРУЕМ ВСЁ
        if (playerMovement != null) playerMovement.UnlockMovement();
        if (playerAttack != null) playerAttack.UnlockAttack();
    
        isCasting = false;
    }

    // Метод, который непосредственно отправляет событие
    void PerformSpellCast()
    {
        SpellData selectedSpell = availableSpells[currentSpellIndex];
        
        SpellCastData spellData = new SpellCastData
        {
            spellPrefab = selectedSpell.spellPrefab,
            caster = gameObject,
        };

        OnSpellCast?.Invoke(spellData);
        
        if (debugMode)
            Debug.Log($"Заклинание ВЫПУЩЕНО: {selectedSpell.spellName}");
    }

    void UpdateCooldown()
    {
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
            
            if (currentCooldown <= 0f)
            {
                currentCooldown = 0f;
                canCast = true;
                
                if (debugMode) Debug.Log("Перезарядка завершена");
            }
        }
    }

    void ChangeSpell(int direction)
    {
        if (availableSpells.Length == 0) return;
        
        int newIndex = currentSpellIndex + direction;
        
        if (newIndex < 0) newIndex = availableSpells.Length - 1;
        else if (newIndex >= availableSpells.Length) newIndex = 0;
        
        currentSpellIndex = newIndex;
        
        if (debugMode) Debug.Log($"Выбрано заклинание: {availableSpells[currentSpellIndex].spellName}");
    }

    void StartCooldown()
    {
        canCast = false;
        currentCooldown = spellCooldown;
    }

    public float GetCooldownProgress()
    {
        if (spellCooldown <= 0f) return 1f;
        return 1f - (currentCooldown / spellCooldown);
    }
    
    // Полезно для UI: показываем, что идет каст или кулдаун
    public bool IsBusy() 
    {
        return !canCast || isCasting;
    }

    public float GetRemainingCooldown() => currentCooldown;
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