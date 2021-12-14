using System;
using UnityEngine;

namespace UnityCommonFeatures
{
    public class State
    {
        private readonly StateMachine _stateMachine;
        private Action _onEnter;
        private Func<State> _onUpdate;
        private Action _onExit;

        private float _startTime = 0f;

        public static readonly State NoTransition = null;
        public static readonly State CompleteState = new State(null);

        public float StateStartTime => _startTime;
        public float TimeSinceStateStart => Time.time - _startTime;

        public State(StateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void InitialiseActions(Action onEnter = null, Func<State> onUpdate = null, Action onExit = null)
        {
            _onEnter = onEnter;
            _onUpdate = onUpdate;
            _onExit = onExit;
        }

        public void InitialiseActions(IStateBehaviour stateBehaviour) =>
            InitialiseActions(stateBehaviour.OnEnterState, stateBehaviour.OnUpdateState, stateBehaviour.OnExitState);


        public void EnterState()
        {
            _startTime = Time.time;
            _onEnter?.Invoke();
        }

        public void UpdateState()
        {
            State nextState = _onUpdate();

            if (nextState == null)
            {
                // continuing with current state
                return;
            }

            if (_stateMachine.IsCompleteState(nextState))
            {
                // fsm completed
                StateCompleted();
                return;
            }

            _stateMachine.ChangeState(nextState);
        }

        private void StateCompleted()
        {
            _stateMachine.SetAsCompleted();
        }

        public void ExitState()
        {
            _onExit?.Invoke();
        }
    }
}