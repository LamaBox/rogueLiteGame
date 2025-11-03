using UnityEngine;

[CreateAssetMenu(fileName = "BotDataSO", menuName = "Scriptable Objects/BotDataSO")]
public class BotDataSO : ScriptableObject
{
    [Header("Basic Stats")] 
    public string botName;
    public float maxHealth = 100f;
    public float damage = 10f;
    public float moveSpeed = 20f;
    public float attackSpeed = 1f;
    public float attackDistance = 2f;
}
