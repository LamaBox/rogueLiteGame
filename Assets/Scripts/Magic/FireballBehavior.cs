using UnityEngine;
using System.Collections;

namespace Magic
{
    public class FireballController : MonoBehaviour
    {
        [Header("Настройки движения")]
        public float speed = 15f;
        public float lifetime = 0.7f;
        
        [Header("Настройки размера")]
        public float startSize = 0.5f;
        public float explosionSize = 2f;
        public float growthDuration = 0.3f;
        
        [Header("Настройки урона")]
        public float damage = -1f;
        public float explosionDamageMultiplier = -1f;
        
        [SerializeField]
        private bool hasExploded = false;
        
        [Header("Debug Settings")]
        [SerializeField] private bool debugMode = false; 

        private AnimationCurve growthCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        void Start()
        {
            InitializeFireball();
        }
        
        void InitializeFireball()
        {
            // Устанавливаем начальный размер
            transform.localScale = Vector3.one * startSize;
            
            // Определяем направление на основе поворота объекта
            Vector2 forwardDirection = GetForwardDirection();
            
            StartCoroutine(FlightRoutine(forwardDirection));
        }
        
        Vector2 GetForwardDirection()
        {
            return transform.right;
        }
        
        IEnumerator FlightRoutine(Vector2 flightDirection)
        {
            float flightTime = 0f;
            
            while (flightTime < lifetime && !hasExploded)
            {
                transform.position += (Vector3)(flightDirection * speed * Time.deltaTime);
                flightTime += Time.deltaTime;
                yield return null;
            }
            
            if (!hasExploded)
            {
                StartCoroutine(ExplodeRoutine());
            }
        }
        
        IEnumerator ExplodeRoutine()
        {
            hasExploded = true;
            
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
            Destroy(gameObject);
        }
        
        void OnTriggerEnter2D(Collider2D other)
        {
            // if (!hasExploded)
            // {
            //     StartCoroutine(ExplodeRoutine());
            // }
        }
        
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionSize * 0.5f);
        }
    }
}