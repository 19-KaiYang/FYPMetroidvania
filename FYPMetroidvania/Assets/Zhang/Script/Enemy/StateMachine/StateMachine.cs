using System.Collections.Generic;
using System;
using UnityEngine;

public class StateMachine
{
    public IState CurrentState { get; private set; }

    public event Action<IState> stateChanged;

    public void Initialize(IState state)
    {
        CurrentState = state;
        CurrentState.OnEnter();
    }

    public void ChangeState(IState state)
    {
        CurrentState.OnExit();
        CurrentState = state;
        CurrentState.OnEnter();

        stateChanged?.Invoke(CurrentState);
    }

    public void Update()
    {
        CurrentState.OnUpdate();
    }
}
