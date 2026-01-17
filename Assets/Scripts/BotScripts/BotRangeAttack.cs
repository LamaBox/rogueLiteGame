using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class BotRangeAttack : MonoBehaviour
{
    [Header("Полёт")]
    [SerializeField] private float speed = 15f;               // Скорость снаряда
    [SerializeField] private float lifetime = 5f;             // Время жизни (чтобы не летел вечно)

    [Header("Взрыв")]
    [SerializeField] private float explosionDuration = 0.5f;  // Время анимации взрыва
    [SerializeField] private float explosionScale = 2f;       // Визуальное увеличение при взрыве
    [SerializeField] private float explosionRadius = 1.0f;    // Радиус поражения
    [SerializeField] private LayerMask whatIsPlayer;          // Маска слоя игрока (для оптимизации)

    [Header("Урон")]
    [SerializeField] private float damage = 15f;              // Урон по игроку

    [Header("Debug")]
    [SerializeField] private bool debugMode;

    private bool exploded = false;
    private Animator animator;
    private Vector3 startScale;

    private CircleCollider2D circleCollider;
    private Rigidbody2D rb;

    private static readonly int ExplodedHash = Animator.StringToHash("exploded");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();

        // Гарантируем настройки физики
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        circleCollider.isTrigger = true;
    }

    private void Start()
    {
        startScale = transform.localScale;
        
        // Запускаем таймер самоуничтожения (если никуда не попал)
        StartCoroutine(LifeTimer());
    }

    private void Update()
    {
        if (exploded) return;

        // Движение вперед относительно своего поворота (transform.right)
        // Так как ты задаешь rotation при спавне, снаряд полетит в нужном направлении
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(lifetime);
        // Если время вышло, просто взрываемся
        Explode();
    }

    // Обработка столкновений
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (exploded) return;

        // 1. Игнорируем самого себя, других врагов и зоны триггеров ботов
        if (other.CompareTag("Enemy") || other.CompareTag("BotZone")) 
            return;

        // 2. Если попали в стены/пол (Environment) или Игрока - взрываемся
        // (Предполагается, что у стен стоит тег Environment или слой Default)
        Explode();
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;

        if (debugMode) Debug.Log("Projectille Exploded");

        // Останавливаем движение (хотя Update уже заблокирован флагом, для надежности)
        speed = 0f; 

        // Запуск анимации взрыва
        animator.SetBool(ExplodedHash, true);

        // Наносим урон
        ApplyDamage();

        // Визуальный эффект расширения
        StartCoroutine(ExplosionScaleRoutine());
    }

    private void ApplyDamage()
    {
        // Ищем коллайдеры в радиусе взрыва.
        // Если whatIsPlayer не задан (Nothing), ищем по всем слоям, иначе фильтруем.
        Collider2D[] hits;
        
        if (whatIsPlayer.value != 0)
            hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, whatIsPlayer);
        else
            hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (var hit in hits)
        {
            // Пытаемся найти компонент игрока
            PlayerData player = hit.GetComponent<PlayerData>();
            
            // Если компонента нет на самом объекте, ищем на родителях (на случай сложной иерархии)
            if (player == null) 
                player = hit.GetComponentInParent<PlayerData>();

            if (player != null)
            {
                // Используем логику нанесения урона из твоего проекта
                player.ChangeValueResource(-damage, PlayerDataStructures.ResourceType.Health, PlayerDataStructures.ResourceValueType.Current, true);
                
                if (debugMode) Debug.Log($"Hit Player for {damage}");
                
                // Важно: наносим урон только один раз за взрыв одному игроку.
                // Если у игрока несколько коллайдеров, break предотвратит двойной урон.
                break; 
            }
        }
    }

    private IEnumerator ExplosionScaleRoutine()
    {
        float time = 0f;
        Vector3 targetScale = startScale * explosionScale;

        // Плавное увеличение
        while (time < explosionDuration)
        {
            time += Time.deltaTime;
            float t = time / explosionDuration;
            // Можно добавить Easing для красоты, но Lerp тоже пойдет
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        // Уничтожаем объект после окончания анимации
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
#endif
}