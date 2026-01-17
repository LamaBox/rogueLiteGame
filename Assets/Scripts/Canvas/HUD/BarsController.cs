using UnityEngine;
using UnityEngine.UI; // Обязательно для работы с компонентом Image
using static PlayerDataStructures; // Чтобы видеть типы ResourceData и ResourceType

public class BarsController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image healthBarImage;
    [SerializeField] private Image manaBarImage;
    // [SerializeField] private Image staminaBarImage; // Можно раскомментировать, если понадобится стамина

    private PlayerData _playerData;

    private void Start()
    {
        InitializeConnection();
    }

    private void InitializeConnection()
    {
        // 1. Находим объект по тегу Player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj == null)
        {
            Debug.LogError("BarsController: Объект с тегом 'Player' не найден на сцене!");
            return;
        }

        // 2. Получаем компонент PlayerData
        _playerData = playerObj.GetComponent<PlayerData>();

        if (_playerData == null)
        {
            Debug.LogError("BarsController: На объекте Player не найден скрипт PlayerData!");
            return;
        }

        // 3. Подписываемся на событие изменения ресурсов
        _playerData.OnResourceChanged += OnResourceChangedHandler;

        // 4. Принудительно обновляем полоски значениями "на старте", 
        // чтобы они не были пустыми до первого получения урона/траты маны.
        // Используем конструктор ResourceData, который виден в твоем коде PlayerData
        UpdateBarVisuals(new ResourceData(_playerData.GetCurrentHealth(), _playerData.GetMaxHealth(), ResourceType.Health));
        UpdateBarVisuals(new ResourceData(_playerData.GetCurrentMana(), _playerData.GetMaxMana(), ResourceType.Mana));
    }

    // Метод-обработчик события
    private void OnResourceChangedHandler(ResourceData data)
    {
        UpdateBarVisuals(data);
    }

    // Логика обновления Fill Amount
    private void UpdateBarVisuals(ResourceData data)
    {
        // Вычисляем процент заполнения (защита от деления на ноль)
        float fillAmount = (data.Max > 0) ? data.Current / data.Max : 0;

        switch (data.Type)
        {
            case ResourceType.Health:
                if (healthBarImage != null)
                {
                    healthBarImage.fillAmount = fillAmount;
                }
                break;

            case ResourceType.Mana:
                if (manaBarImage != null)
                {
                    manaBarImage.fillAmount = fillAmount;
                }
                break;

            case ResourceType.Stamina:
                // Если добавишь полоску стамины:
                // if (staminaBarImage != null) staminaBarImage.fillAmount = fillAmount;
                break;
        }
    }

    private void OnDestroy()
    {
        // Обязательно отписываемся от события при уничтожении объекта UI,
        // чтобы избежать ошибок "MissingReferenceException"
        if (_playerData != null)
        {
            _playerData.OnResourceChanged -= OnResourceChangedHandler;
        }
    }
}