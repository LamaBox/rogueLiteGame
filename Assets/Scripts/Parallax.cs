using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Transform playerTransform; // Ссылка на игрока
    public float parallaxSpeed = 0.5f; // Коэффициент скорости параллакса

    private Vector3 lastPlayerPosition; // Последняя позиция игрока
    public Vector3 basePosition;        // Базовая позиция (публична для удобства отладки)

    void Start()
    {
        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        
            if (playerObject == null)
            {
                Debug.LogError($"[Parallax] Игрок с тегом 'Player' не найден на сцене! Объект: {name}");
                return;
            }

            playerTransform = playerObject.transform;
            Debug.Log($"[Parallax] Игрок найден: {playerObject.name}");
        }

        // Сохраняем начальную позицию как базовую
        basePosition = transform.position;
        lastPlayerPosition = playerTransform.position;
    }

    void LateUpdate()
    {
        if (playerTransform == null) return;

        Vector3 deltaMovement = playerTransform.position - lastPlayerPosition;

        // Двигаем фон в ОБРАТНУЮ сторону от движения игрока (только по X)
        transform.position -= new Vector3(deltaMovement.x * parallaxSpeed, 0f, 0f);

        lastPlayerPosition = playerTransform.position;
    }
}