using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target; //объект за которым надо следить
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10); //Смещение камеры

    [Header("Smooth Settings")]
    [SerializeField] private float smoothSpeed = 0.15f; //Плавность следования
    [SerializeField] private bool useSmooth = true; //Использовать плавное следование

    [Header("Dead Zone Settings")]
    [SerializeField] private bool useDeadZone = true; // использовать мертвую зону
    [SerializeField] private Vector2 deadZoneSize = new Vector2(3f, 2f); // размер мертвой зоны (ширина, высота)
    
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition; // позиция, за которой следует камера

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Target not assigned to FollowCamera on " + gameObject.name);
        }

        // Инициализируем начальную позицию камеры
        targetPosition = target.position + offset;
    }

    void LateUpdate()
    {
        Follow();
    }

    //метод следования камеры
    private void Follow()
    {
        if (target == null) return;

        if (useDeadZone)
        {
            HandleDeadZone();
        }
        else
        {
            // Обычное следование без dead zone
            targetPosition = target.position + offset;
        }

        // Двигаем камеру
        if (useSmooth)
        {
            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothSpeed);
            transform.position = smoothedPosition;
        }
        else
        {
            transform.position = targetPosition;
        }
    }

    private void HandleDeadZone()
    {
        Vector3 targetWorldPosition = target.position + offset;

        // Получаем текущие границы dead zone в мировых координатах
        float deadZoneLeft = transform.position.x - deadZoneSize.x * 0.5f;
        float deadZoneRight = transform.position.x + deadZoneSize.x * 0.5f;
        float deadZoneBottom = transform.position.y - deadZoneSize.y * 0.5f;
        float deadZoneTop = transform.position.y + deadZoneSize.y * 0.5f;

        // Проверяем, вышел ли игрок за границы dead zone
        if (targetWorldPosition.x < deadZoneLeft)
        {
            // Игрок слева от dead zone
            targetPosition.x = targetWorldPosition.x + deadZoneSize.x * 0.5f;
        }
        else if (targetWorldPosition.x > deadZoneRight)
        {
            // Игрок справа от dead zone
            targetPosition.x = targetWorldPosition.x - deadZoneSize.x * 0.5f;
        }

        if (targetWorldPosition.y < deadZoneBottom)
        {
            // Игрок снизу от dead zone
            targetPosition.y = targetWorldPosition.y + deadZoneSize.y * 0.5f;
        }
        else if (targetWorldPosition.y > deadZoneTop)
        {
            // Игрок сверху от dead zone
            targetPosition.y = targetWorldPosition.y - deadZoneSize.y * 0.5f;
        }

        // Сохраняем Z координату
        targetPosition.z = targetWorldPosition.z;
    }

    // Метод для установки размера dead zone
    public void SetDeadZoneSize(Vector2 size)
    {
        deadZoneSize = size;
    }
    
    // Визуализация dead zone в редакторе
    private void OnDrawGizmosSelected()
    {
        if (useDeadZone)
        {
            Gizmos.color = Color.yellow;
            Vector3 deadZoneCenter = transform.position;
            Vector3 size = new Vector3(deadZoneSize.x, deadZoneSize.y, 0.1f);
            Gizmos.DrawWireCube(deadZoneCenter, size);
        }
    }
}