using UnityEngine;

public interface IBotState
{
    void Enter();
    void Exit();
    void Update();
    BotStateType GetStateType();
}

public enum BotStateType
{
    Patrol,
    Rest,
    Alert,
    Attack,
    Chase,
    Search,
    Dialogue,
    Death,
    None
}

public abstract class BotStateComponent : MonoBehaviour, IBotState
{
    public abstract void Enter();
    public abstract void Exit();
    public abstract void Update();
    public abstract BotStateType GetStateType();
}
