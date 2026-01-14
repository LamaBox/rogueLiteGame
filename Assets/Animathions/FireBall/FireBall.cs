using UnityEngine;
using System.Collections;

namespace Magic
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Fireball : MonoBehaviour
    {
        [Header("Полёт")]
        [SerializeField] private float speed = 15f;               // Скорость движения шара
        [SerializeField] private float lifetime = 0.7f;           // Время полёта до взрыва

        [Header("Взрыв")]
        [SerializeField] private float explosionDuration = 0.3f;  // Время анимации взрыва
        [SerializeField] private float explosionScale = 3f;       // Масштаб визуального взрыва
        [SerializeField] private float explosionRadius = 1.5f;    // Радиус коллайдера для нанесения урона

        [Header("Урон")]
        [SerializeField] private float damage = 20f;             // Урон по ботам

        [Header("Debug")]
        [SerializeField] private bool debugMode;

        private bool exploded = false;
        private Vector2 direction;
        private Animator animator;
        private Vector3 startScale;

        private CircleCollider2D circleCollider;
        private float startColliderRadius;

        private static readonly int ExplodedHash = Animator.StringToHash("exploded");

        private void Awake()
        {
            animator = GetComponent<Animator>();

            // Настройка Rigidbody
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            // Настройка коллайдера
            circleCollider = GetComponent<CircleCollider2D>();
            circleCollider.isTrigger = true;
            startColliderRadius = circleCollider.radius;
        }

        private void Start()
        {
            direction = transform.right;      // Летим по локальной оси X
            startScale = transform.localScale;

            StartCoroutine(LifeTimer());      // Таймер жизни перед взрывом
        }

        private void Update()
        {
            if (exploded) return;

            // Перемещение шара по направлению
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
        }

        // Таймер жизни шара
        private IEnumerator LifeTimer()
        {
            yield return new WaitForSeconds(lifetime);
            Explode();
        }

        // Обработка столкновений
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (exploded) return;

            // Игнорируем игрока, окружение и зоны ботов
            if (other.CompareTag("Player") ||
                other.CompareTag("Enviroment") ||
                other.CompareTag("BotZone"))
                return;

            Explode();
        }

        // Метод взрыва
        private void Explode()
        {
            if (exploded) return;
            exploded = true;

            if (debugMode)
                Debug.Log("Fireball exploded");

            // Переключаем анимацию взрыва
            animator.SetBool(ExplodedHash, true);

            // Увеличиваем радиус коллайдера для нанесения урона
            circleCollider.radius = explosionRadius;

            // Наносим урон всем ботам в радиусе
            ApplyDamage();

            // Запускаем анимацию визуального увеличения шара
            StartCoroutine(ExplosionScaleRoutine());
        }

        // Логика нанесения урона
        private void ApplyDamage()
        {
            // Получаем все коллайдеры в радиусе взрыва
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

            foreach (var hit in hits)
            {
                // Если объект имеет компонент BotBase — наносим урон
                BotBase bot = hit.GetComponent<BotBase>();
                if (bot != null)
                {
                    bot.TakeDamage(damage);

                    if (debugMode)
                        Debug.Log($"Fireball hit {bot.gameObject.name} for {damage} damage");
                }
            }
        }

        // Анимация визуального увеличения шара
        private IEnumerator ExplosionScaleRoutine()
        {
            float time = 0f;
            Vector3 targetScale = startScale * explosionScale;

            while (time < explosionDuration)
            {
                time += Time.deltaTime;
                float t = time / explosionDuration;
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            Destroy(gameObject);
        }

#if UNITY_EDITOR
        // Визуализация радиуса взрыва в редакторе
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
#endif
    }
}
