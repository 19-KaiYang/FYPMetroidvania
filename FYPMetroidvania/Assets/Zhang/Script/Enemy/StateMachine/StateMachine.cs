using System.Collections.Generic;
using System;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public IState CurrentState { get; private set; }

    //public List<IState> states;
    public event Action<IState> stateChanged;

    //public StateMachine()
    //{
    //    states = new List<IState>();
    //}

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
