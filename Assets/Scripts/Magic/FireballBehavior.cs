using UnityEngine;
using System.Collections;

namespace Magic
{
    public class FireballController : MonoBehaviour
    {
        [Header("Настройки движения")]
        [Tooltip("Время нарастания файербола на месте перед полетом")]
        [SerializeField] private float castTime = 0.5f;
        [Tooltip("Скорость полета")]
        [SerializeField] private float speed = 15f;
        [Tooltip("Максимальное время полета")]
        [SerializeField] private float lifetime = 0.7f;
        [Tooltip("Длительность анимации взрыва")]
        [SerializeField] private float growthDuration = 0.07f;

        [Header("Настройки размера")]
        [SerializeField] private float startSize = 0.1f;
        [SerializeField] private float castSize = 1f;
        [SerializeField] private float explosionSize = 3f;

        [Header("Настройки урона и фильтры")]
        [SerializeField] private float damage = 20f;
        [Tooltip("Слой врагов")]
        [SerializeField] private LayerMask enemyLayer;
        [Tooltip("Слой препятствий (стены, пол)")]
        [SerializeField] private LayerMask obstacleLayer;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        private bool hasExploded = false;
        private bool isCasting = true;
        private Vector2 flightDirection;
        private AnimationCurve growthCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        void Start()
        {
            InitializeFireball();
        }

        void InitializeFireball()
        {
            transform.localScale = Vector3.one * startSize;
            flightDirection = transform.right; // Летит в сторону "красной" оси X
            StartCoroutine(FireballLifecycleRoutine());
        }

        IEnumerator FireballLifecycleRoutine()
        {
            // 1. Фаза каста (зарядка)
            yield return StartCoroutine(CastPhaseRoutine());

            // 2. Проверка "в упор" сразу после каста
            // Если мы уже внутри врага или стены — взрываемся не летя
            if (!hasExploded)
            {
                CheckImmediateCollision();
            }

            // 3. Фаза полета
            if (!hasExploded)
            {
                yield return StartCoroutine(FlightPhaseRoutine());
            }

            // 4. Взрыв по истечению времени (если еще не взорвались)
            if (!hasExploded)
            {
                yield return StartCoroutine(ExplodeRoutine());
            }
        }

        IEnumerator CastPhaseRoutine()
        {
            float castTimer = 0f;
            Vector3 initialScale = transform.localScale;
            Vector3 targetCastScale = Vector3.one * castSize;

            while (castTimer < castTime && !hasExploded)
            {
                castTimer += Time.deltaTime;
                float progress = castTimer / castTime;
                transform.localScale = Vector3.Lerp(initialScale, targetCastScale, growthCurve.Evaluate(progress));
                yield return null;
            }
            isCasting = false;
        }

        private void CheckImmediateCollision()
        {
            float checkRadius = castSize * 0.5f;
            // Проверяем и врагов, и препятствия
            Collider2D hit = Physics2D.OverlapCircle(transform.position, checkRadius, enemyLayer | obstacleLayer);
            
            if (hit != null)
            {
                if (debugMode) Debug.Log($"Объект {hit.name} обнаружен сразу после каста. Взрыв.");
                StartCoroutine(ExplodeRoutine());
            }
        }

        IEnumerator FlightPhaseRoutine()
        {
            float flightTime = 0f;
            while (flightTime < lifetime && !hasExploded)
            {
                transform.position += (Vector3)(flightDirection * speed * Time.deltaTime);
                flightTime += Time.deltaTime;
                yield return null;
            }
        }

        IEnumerator ExplodeRoutine()
        {
            if (hasExploded) yield break;
            hasExploded = true;

            if (debugMode) Debug.Log("Файербол детонирует");

            // Анимация расширения взрыва
            float growthTime = 0f;
            Vector3 initialScale = transform.localScale;
            Vector3 targetScale = Vector3.one * explosionSize;

            while (growthTime < growthDuration)
            {
                growthTime += Time.deltaTime;
                float progress = growthTime / growthDuration;
                transform.localScale = Vector3.Lerp(initialScale, targetScale, growthCurve.Evaluate(progress));
                yield return null;
            }

            // Нанесение урона
            ApplyExplosionDamage();

            yield return new WaitForSeconds(0.1f); // Короткая пауза, чтобы игрок увидел размер взрыва
            Destroy(gameObject);
        }

        private void ApplyExplosionDamage()
        {
            float explosionRadius = explosionSize * 0.5f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    BotBase enemyBot = hit.GetComponent<BotBase>();
                    if (enemyBot != null)
                    {
                        enemyBot.TakeDamage(damage);
                    }
                }
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (hasExploded) return;

            // 1. Игнорируем игрока
            if (other.CompareTag("Player") || other.CompareTag("Enviroment")) return;

            // 2. Взрыв об врага
            if (other.CompareTag("Enemy"))
            {
                StartCoroutine(ExplodeRoutine());
                return;
            }
            
            if (debugMode) Debug.Log($"Удар о препятствие: {other.name}");
            StartCoroutine(ExplodeRoutine());
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, castSize * 0.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionSize * 0.5f);
        }
    }
}