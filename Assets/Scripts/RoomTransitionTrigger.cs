using UnityEngine;

public class RoomTransitionTrigger : MonoBehaviour
{
    [Header("Target Room")]
    public GameObject targetRoom; // Ссылка на комнату на сцене (не префаб!)
    public GameObject playerSpawnPosition; // Где появится игрок в целевой комнате
    public string direction = "none"; // "up", "down", "left", "right"

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
}
