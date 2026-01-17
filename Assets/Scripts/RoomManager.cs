using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [Header("References")]
    public Transform player;
    public Camera mainCamera;
    public GameObject backgroundHolder;

    public GameObject[] leftPictures;
    public GameObject[] centerPictures;
    public GameObject[] rightPictures;

    [Header("Current Room")]
    public GameObject currentRoom; // Текущая активная комната на сцене

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Переход в указанную комнату с телепортацией игрока и центрированием камеры/фона.
    /// </summary>
    public void TransitionToRoom(GameObject targetRoom, Vector3 spawnPosition, string direction = "none")
    {
        // TODO: Раскомментировать, когда добавите проверку врагов
        // if (currentRoom != null && HasAliveEnemies(currentRoom))
        // {
        //     Debug.Log("Нельзя выйти — в комнате ещё есть живые враги!");
        //     return;
        // }

        currentRoom = targetRoom;

        // Телепортируем игрока
        player.position = spawnPosition;

        // Применяем импульс при прыжке вверх
        if (direction == "up" && player.TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y + 10f);
        }

        // Центрируем камеру и фон
        CenterCameraOnRoom();
        CenterBackgroundOnRoom();
    }

    // private bool HasAliveEnemies(GameObject room) { /* реализация */ }

    public void CenterCameraOnRoom()
    {
        if (currentRoom == null || mainCamera == null) return;
        mainCamera.transform.position = new Vector3(currentRoom.transform.position.x, currentRoom.transform.position.y, -10f);
    }

    public void CenterBackgroundOnRoom()
    {
        if (currentRoom == null) return;

        // Обновляем позиции и базовые позиции для левых фонов
        foreach (var bg in leftPictures)
        {
            if (bg == null) continue;
            Vector3 newPos = new Vector3(currentRoom.transform.position.x - 27.2f, currentRoom.transform.position.y - 0.37f, bg.transform.position.z);
            bg.transform.position = newPos;

            Parallax p = bg.GetComponent<Parallax>();
            if (p != null)
                p.basePosition = newPos;
        }

        // Центральные фоны
        foreach (var bg in centerPictures)
        {
            if (bg == null) continue;
            Vector3 newPos = new Vector3(currentRoom.transform.position.x, currentRoom.transform.position.y - 0.37f, bg.transform.position.z);
            bg.transform.position = newPos;

            Parallax p = bg.GetComponent<Parallax>();
            if (p != null)
                p.basePosition = newPos;
        }

        // Правые фоны
        foreach (var bg in rightPictures)
        {
            if (bg == null) continue;
            Vector3 newPos = new Vector3(currentRoom.transform.position.x + 27.2f, currentRoom.transform.position.y - 0.37f, bg.transform.position.z);
            bg.transform.position = newPos;

            Parallax p = bg.GetComponent<Parallax>();
            if (p != null)
                p.basePosition = newPos;
        }
    }
}
