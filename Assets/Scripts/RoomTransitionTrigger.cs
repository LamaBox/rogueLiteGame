using UnityEngine;

public class RoomTransitionTrigger : MonoBehaviour
{
    [Header("Target Room")]
    public GameObject targetRoom; // Ссылка на комнату на сцене (не префаб!)
    public GameObject playerSpawnPosition; // Где появится игрок в целевой комнате
    public string direction = "none"; // "up", "down", "left", "right"

    private BoxCollider2D boxCollider; // Для управления IsTrigger
    
    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            Debug.LogError("BoxCollider2D not found on RoomTransitionTrigger!");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Проверка: можно ли выйти из текущей комнаты?
            //if (RoomManager.Instance.currentRoom != null)
            //{
            //    Debug.Log("Вы не можете покинуть комнату — враги ещё живы!");
            //    return;
            //}

            // Выполняем переход
            RoomManager.Instance.TransitionToRoom(targetRoom, playerSpawnPosition.transform.position, direction);
        }
    }
    
    // Метод для установки состояния триггера
    public void SetTriggerState(bool isTrigger)
    {
        if (boxCollider != null)
        {
            boxCollider.isTrigger = isTrigger;
        }
    }
}
