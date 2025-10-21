using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private PlayerMovement playerMovement;

    private void Awake()
    {
        InitializeComponents();
        SubscribeToEvents();
        BroadcastAllData();
    }

    //проверяет что все компоненты игрока назначены в инспекторе
    private void InitializeComponents()
    {
        if (playerData == null)
        {
            Debug.LogError("PlayerData component not found on " + gameObject.name);
            return;
        }

        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement component not found on " + gameObject.name);
            return;
        }
    }

    //подключает элементы к событиям
    private void SubscribeToEvents()
    {
        playerData.OnMovementModifiersChanged += playerMovement.OnMovementModifiersChanged;
        playerData.OnDataInitialized += OnDataInitialized;
    }

    //запускает событие с передачей всех данных в PlayerData
    private void BroadcastAllData()
    {
        playerData.BroadcastAllData();
    }

    //выводит сообщение, когда данные инициализированы
    private void OnDataInitialized()
    {
        Debug.Log("Player fully initialized!");
    }
    
    //удаляет подписки
    private void OnDestroy()
    {
        playerData.OnMovementModifiersChanged -= playerMovement.OnMovementModifiersChanged;
        playerData.OnDataInitialized -= OnDataInitialized;
    }
}
