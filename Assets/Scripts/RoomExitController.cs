using UnityEngine;

public class RoomExitController : MonoBehaviour
{
    [Header("Exit Triggers")]
    public RoomTransitionTrigger[] exitTriggers; // Все триггеры выхода в этой комнате

    public int enemyCount = 0;

    private void Start()
    {
        // При старте комнаты — обновляем состояние выходов
        UpdateExitStates();
    }

    // Вызывается, когда враг умирает или появляется
    public void UpdateExitStates()
    {
        if (enemyCount != 0)
        {
            enemyCount--;
        }
        else
        {
            foreach (RoomTransitionTrigger trigger in exitTriggers)
            {
                if (trigger != null)
                {
                    trigger.SetTriggerState(enemyCount == 0);
                }
            }
        }
    }

    
}