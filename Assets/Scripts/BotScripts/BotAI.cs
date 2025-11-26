using UnityEngine;
using System.Collections.Generic;

public class BotAI : BotBase
{
    [SerializeField] private BotStateComponent[] availableStates;

    private Dictionary<BotStateType, BotStateComponent> stateMap;
    private BotStateComponent currentState;

    protected override void Start()
    {
        base.Start();
        InitializeStateMap();
        SwitchState(BotStateType.Patrol); // начальное состояние
    }

    private void InitializeStateMap()
    {
        stateMap = new Dictionary<BotStateType, BotStateComponent>();

        foreach (var state in availableStates)
        {
            stateMap[state.GetStateType()] = state;
            state.enabled = false; // выключаем, пока не наступит состояние
        }
    }

    public void SwitchState(BotStateType newStateType)
    {
        if (currentState != null)
        {
            currentState.Exit();
            currentState.enabled = false;
        }

        if (stateMap.TryGetValue(newStateType, out var newState))
        {
            currentState = newState;
            currentState.enabled = true;
            currentState.Enter();
        }
    }

    protected override void Update()
    {
        base.Update();
        currentState?.Update();
    }
}