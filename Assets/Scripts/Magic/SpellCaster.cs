using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using static PlayerDataStructures; // Чтобы видеть типы ресурсов

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
    private float castDelay = 0.5f; 
    [SerializeField]
    private float spellCooldown = 1f; 
    
    private float currentCooldown = 0f; 
    private bool canCast = true;        
    private bool isCasting = false;     

    public delegate void SpellCastEventHandler(SpellCastData spellData);
    public static event SpellCastEventHandler OnSpellCast;
    
    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false;
    
    private Mouse mouse;
    private float scrollAccumulator;
    
    // Ссылки на компоненты
    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;
    private PlayerData playerData; // Ссылка на данные игрока (Мана)

    void Start()
    {
        mouse = Mouse.current;
        currentCooldown = 0f;
        
        playerMovement = GetComponent<PlayerMovement>();
        playerAttack = GetComponent<PlayerAttack>();
        playerData = GetComponent<PlayerData>(); // Получаем компонент PlayerData

        if (playerData == null)
            Debug.LogError("SpellCaster: PlayerData не найден на объекте!");
    }

    void Update()
    {
        if (mouse == null) return;

        UpdateCooldown();

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
        if (mouse.rightButton.wasPressedThisFrame && canCast && !isCasting && availableSpells.Length > 0)
        {
            // ПРОВЕРКА МАНЫ ПЕРЕД ЗАПУСКОМ
            if (HasEnoughMana())
            {
                StartCoroutine(CastSpellRoutine());
            }
            else
            {
                if (debugMode) Debug.Log("Недостаточно маны!");
                // Здесь можно добавить проигрывание звука ошибки или UI эффект
            }
        }
    }

    // Проверка, хватает ли маны на текущее заклинание
    private bool HasEnoughMana()
    {
        if (playerData == null) return false;

        float cost = availableSpells[currentSpellIndex].manaCost;
        return playerData.GetCurrentMana() >= cost;
    }

    IEnumerator CastSpellRoutine()
    {
        isCasting = true;

        if (playerMovement != null) playerMovement.LockMovement();
        if (playerAttack != null) playerAttack.LockAttack();

        yield return new WaitForSeconds(castDelay);

        // Еще раз проверяем ману (на случай, если за время каста её сняли чем-то другим)
        if (HasEnoughMana())
        {
            PerformSpellCast();
            StartCooldown();
        }
        else
        {
            if (debugMode) Debug.Log("Мана закончилась во время каста!");
        }

        if (playerMovement != null) playerMovement.UnlockMovement();
        if (playerAttack != null) playerAttack.UnlockAttack();
    
        isCasting = false;
    }

    void PerformSpellCast()
    {
        SpellData selectedSpell = availableSpells[currentSpellIndex];
        
        // 1. СПИСЫВАЕМ МАНУ
        // isAddition = true, значение отрицательное (-cost), чтобы отнять
        if (playerData != null)
        {
            playerData.ChangeValueResource(-selectedSpell.manaCost, ResourceType.Mana, ResourceValueType.Current, true);
        }

        // 2. Создаем заклинание
        SpellCastData spellData = new SpellCastData
        {
            spellPrefab = selectedSpell.spellPrefab,
            caster = gameObject,
        };

        OnSpellCast?.Invoke(spellData);
        
        if (debugMode)
            Debug.Log($"Заклинание ВЫПУЩЕНО: {selectedSpell.spellName}. Потрачено маны: {selectedSpell.manaCost}");
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
    
    [Header("Цена маны")]
    [Min(0)]
    public float manaCost = 10f; // Новое поле для настройки цены
}

public struct SpellCastData
{
    public GameObject spellPrefab;
    public GameObject caster;
}