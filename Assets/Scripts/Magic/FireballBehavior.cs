using UnityEngine;
using System.Collections;

namespace Magic
{
    public class FireballController : MonoBehaviour
    {
        [Header("Настройки движения")]
        [Tooltip("Скорость полета")]
        [SerializeField] private float speed = 15f;
        [Tooltip("Максимальное время полета")]
        [SerializeField] private float lifetime = 0.7f;

        [Header("Настройки взрыва")]
        [Tooltip("Длительность анимации взрыва")]
        [SerializeField] private float growthDuration = 0.07f;
        [SerializeField] private float explosionSize = 3f;

        [Header("Настройки урона")]
        [SerializeField] private float damage = 20f;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private LayerMask obstacleLayer;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        private bool hasExploded = false;
        private Vector2 flightDirection;
        private AnimationCurve growthCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        void Start()
        {
            InitializeFireball();
            StartCoroutine(FireballLifecycleRoutine());
        }

        void InitializeFireball()
        {
            flightDirection = transform.right; // летит по X
        }

        IEnumerator FireballLifecycleRoutine()
        {
            // Полёт
            yield return StartCoroutine(FlightPhaseRoutine());

            // Взрыв по таймеру
            if (!hasExploded)
            {
                yield return StartCoroutine(ExplodeRoutine());
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

            if (debugMode) Debug.Log("Файербол взрывается");

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

            ApplyExplosionDamage();

            yield return new WaitForSeconds(0.1f);
            Destroy(gameObject);
        }

        private void ApplyExplosionDamage()
        {
            float radius = explosionSize * 0.5f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    BotBase enemy = hit.GetComponent<BotBase>();
                    if (enemy != null)
                        enemy.TakeDamage(damage);
                }
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (hasExploded) return;

            if (other.CompareTag("Player")
                || other.CompareTag("Enviroment")
                || other.CompareTag("BotZone")) return;

            if (debugMode) Debug.Log($"Столкновение с {other.name}");
            StartCoroutine(ExplodeRoutine());
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionSize * 0.5f);
        }
    }
}
