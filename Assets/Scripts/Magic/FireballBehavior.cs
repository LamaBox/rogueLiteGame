using UnityEngine;
using System.Collections;

namespace Magic
{
    public class FireballController : MonoBehaviour
    {
        [Header("Настройки движения")]
        [Tooltip("Время нарастания файербола на месте перед полетом (в секундах)")]
        [SerializeField] private float castTime = 0.5f;
        
        [Tooltip("Скорость полета файербола после каста")]
        [SerializeField] private float speed = 15f;
        
        [Tooltip("Максимальное время полета до автоматического взрыва")]
        [SerializeField] private float lifetime = 0.7f;
        
        [Tooltip("Длительность анимации взрыва")]
        [SerializeField] private float growthDuration = 0.07f;
        
        [Header("Настройки размера")]
        [Tooltip("Начальный размер файербола при создании")]
        [SerializeField] private float startSize = 0.1f;
        
        [Tooltip("Размер файербола в конце фазы каста (перед полетом)")]
        [SerializeField] private float castSize = 1f;
        
        [Tooltip("Финальный размер файербола при взрыве")]
        [SerializeField] private float explosionSize = 3f;
        
        [Header("Настройки урона")]
        [Tooltip("Урон от прямого попадания файербола")]
        [SerializeField] private float damage = 20f;
        
        [Tooltip("Флаг, указывающий что файербол уже взорвался")]
        [SerializeField] private bool hasExploded = false;
        
        [Header("Debug Settings")]
        [Tooltip("Включить вывод отладочных сообщений в консоль")]
        [SerializeField] private bool debugMode = false; 

        private AnimationCurve growthCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        private bool isCasting = true;
        private Vector2 flightDirection;
        
        void Start()
        {
            InitializeFireball();
        }
        
        void InitializeFireball()
        {
            // Устанавливаем начальный размер
            transform.localScale = Vector3.one * startSize;
            
            // Определяем направление полета
            flightDirection = GetForwardDirection();
            
            // Запускаем основную корутину
            StartCoroutine(FireballLifecycleRoutine());
        }
        
        Vector2 GetForwardDirection()
        {
            return transform.right;
        }
        
        IEnumerator FireballLifecycleRoutine()
        {
            // Фаза 1: Каст (нарастание на месте)
            yield return StartCoroutine(CastPhaseRoutine());
            
            // Фаза 2: Полет
            yield return StartCoroutine(FlightPhaseRoutine());
            
            // Фаза 3: Взрыв (если не взорвался ранее)
            if (!hasExploded)
            {
                yield return StartCoroutine(ExplodeRoutine());
            }
        }
        
        IEnumerator CastPhaseRoutine()
        {
            if (debugMode)
                Debug.Log("Начало фазы каста");
                
            float castTimer = 0f;
            Vector3 initialScale = transform.localScale;
            Vector3 targetCastScale = Vector3.one * castSize;
            
            while (castTimer < castTime)
            {
                castTimer += Time.deltaTime;
                float progress = castTimer / castTime;
                
                // Плавное увеличение размера во время каста
                transform.localScale = Vector3.Lerp(initialScale, targetCastScale, growthCurve.Evaluate(progress));
                
                yield return null;
            }
            
            isCasting = false;
            
            if (debugMode)
                Debug.Log("Фаза каста завершена, начало полета");
        }
        
        IEnumerator FlightPhaseRoutine()
        {
            float flightTime = 0f;
            
            while (flightTime < lifetime && !hasExploded)
            {
                // Двигаемся только если не в режиме каста
                if (!isCasting)
                {
                    transform.position += (Vector3)(flightDirection * speed * Time.deltaTime);
                    flightTime += Time.deltaTime;
                }
                yield return null;
            }
        }
        
        IEnumerator ExplodeRoutine()
        {
            hasExploded = true;
            
            if (debugMode)
                Debug.Log("Начало взрыва");
                
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
            
            yield return new WaitForSeconds(0.2f);
            
            if (debugMode)
                Debug.Log("Уничтожение файербола");
                
            Destroy(gameObject);
        }
        
        void OnTriggerEnter2D(Collider2D other)
        {
            // Не взрываемся во время каста
            if (!isCasting && !hasExploded && !other.CompareTag("Player"))
            {
                if (other.CompareTag("Enemy"))
                {
                    // Наносим урон врагу
                    BotBase enemyBot = other.GetComponent<BotBase>();
                    if (enemyBot != null)
                    {
                        enemyBot.TakeDamage(damage);
                        Debug.Log($"{name} нанес {damage} урона врагу {other.name}");
                    }
                }
        
                StartCoroutine(ExplodeRoutine());
            }
        }
        
        void OnDrawGizmosSelected()
        {
            // Визуализация размеров
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, castSize * 0.5f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionSize * 0.5f);
            
            // Направление полета
            Gizmos.color = Color.blue;
            Vector3 direction = transform.right * 2f;
            Gizmos.DrawLine(transform.position, transform.position + direction);
        }
        
        // Методы для получения статуса (могут пригодиться для визуальных эффектов)
        public bool IsCasting()
        {
            return isCasting;
        }
        
        public float GetCastProgress()
        {
            return isCasting ? 0f : 1f;
        }
    }
}