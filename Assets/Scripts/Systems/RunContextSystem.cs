using UnityEngine;
using static PlayerDataStructures; // Для доступа к ResourceType и ResourceValueType

public class RunContextSystem : MonoBehaviour
{
    public static RunContextSystem Instance { get; private set; }

    // --- Хранимые данные ---
    private float _savedCurrentHealth;
    private float _savedMaxHealth;
    
    private float _savedCurrentMana;
    private float _savedMaxMana;
    
    private int _savedScore;

    // Флаг, чтобы не загружать пустые данные (0) при первом старте игры
    private bool _hasSavedData = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Вызывать В КОНЦЕ уровня (перед загрузкой следующей сцены).
    /// Собирает данные с текущего игрока и счетчика.
    /// </summary>
    public void SaveRunContext()
    {
        // 1. Сохраняем данные игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerData data = player.GetComponent<PlayerData>();
            if (data != null)
            {
                _savedCurrentHealth = data.GetCurrentHealth();
                _savedMaxHealth = data.GetMaxHealth();
                
                _savedCurrentMana = data.GetCurrentMana();
                _savedMaxMana = data.GetMaxMana();
            }
        }
        else
        {
            Debug.LogWarning("RunContextSystem: Игрок не найден при сохранении!");
        }

        // 2. Сохраняем очки
        if (ScoreCounter.Instance != null)
        {
            _savedScore = ScoreCounter.Instance.GetScore();
        }

        _hasSavedData = true;
        Debug.Log("RunContextSystem: Данные забега сохранены.");
    }

    /// <summary>
    /// Вызывать В НАЧАЛЕ уровня (в Start менеджера уровня или самого игрока).
    /// Применяет сохраненные данные к новому игроку.
    /// </summary>
    public void LoadRunContext()
    {
        if (!_hasSavedData)
        {
            Debug.Log("RunContextSystem: Нет сохраненных данных (первый уровень?), загрузка пропущена.");
            return;
        }

        // 1. Загружаем данные в игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerData data = player.GetComponent<PlayerData>();
            if (data != null)
            {
                // Важно: Сначала восстанавливаем МАКСИМУМЫ, потом ТЕКУЩИЕ значения.
                // false в конце означает isAddition = false (то есть перезаписываем значение, а не добавляем)
                
                // Здоровье
                data.ChangeValueResource(_savedMaxHealth, ResourceType.Health, ResourceValueType.Maximum, false);
                data.ChangeValueResource(_savedCurrentHealth, ResourceType.Health, ResourceValueType.Current, false);

                // Мана
                data.ChangeValueResource(_savedMaxMana, ResourceType.Mana, ResourceValueType.Maximum, false);
                data.ChangeValueResource(_savedCurrentMana, ResourceType.Mana, ResourceValueType.Current, false);
                
                // Принудительно обновляем UI после загрузки
                data.BroadcastAllData();
            }
        }
        else
        {
            Debug.LogWarning("RunContextSystem: Игрок не найден при загрузке!");
        }

        // 2. Загружаем очки
        if (ScoreCounter.Instance != null)
        {
            ScoreCounter.Instance.SetScore(_savedScore);
        }
        
        Debug.Log("RunContextSystem: Данные забега загружены.");
    }

    /// <summary>
    /// Вызывать при смерти игрока или полном перезапуске игры.
    /// Уничтожает этот объект, сбрасывая прогресс.
    /// </summary>
    public void ResetRun()
    {
        Debug.Log("RunContextSystem: Контекст забега уничтожен.");
        Destroy(gameObject);
    }
}
