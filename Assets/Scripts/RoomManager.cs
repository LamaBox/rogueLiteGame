using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [Header("References")]
    public Transform player;
    public Camera mainCamera;

    [Header("Current Room")]
    public GameObject currentRoom; // Текущая активная комната на сцене

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Метод перехода в существующую комнату на сцене
    public void TransitionToRoom(GameObject targetRoom, Vector3 spawnPosition, string direction = "none")
    {
        // Проверяем: можно ли выйти из текущей комнаты?
        //if (currentRoom != null)
        //{
         //   Debug.Log("Нельзя выйти — в комнате ещё есть живые враги!");
         //   return;
       // }

        // Обновляем текущую комнату
        currentRoom = targetRoom;

        // Телепортируем игрока
        player.position = spawnPosition;

        // Применяем импульс, если переход вверх
        if (direction == "up" && player.TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.x + 30f); // сохраняем горизонтальную скорость
        }

        // Перемещаем камеру к центру комнаты
        CenterCameraOnRoom();
    }

    //private bool HasAliveEnemies(GameObject room)
    //{
        
    //}

    public void CenterCameraOnRoom()
    {
        if (currentRoom == null) return;
        mainCamera.transform.position = new Vector3(currentRoom.transform.position.x, currentRoom.transform.position.y, -10);
    }
}
