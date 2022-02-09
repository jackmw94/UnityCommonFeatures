using System;
using UnityEngine;

namespace UnityCommonFeatures
{
    public abstract class StateMachine : MonoBehaviour
    {
        private class InvalidStateException : Exception
        {
            public InvalidStateException(string message) : base(message)
            {
            }
        }

        protected State StartingState { get; set; } = null;

        private State CurrentState { get; set; } = null;

        public bool IsComplete { get; private set; } = false;

        public bool IsCompleteState(State state) => state == State.CompleteState;

        private void OnEnable()
        {
            if (StartingState == null)
            {
                throw new InvalidStateException("Starting state was not initialised! Plz set the starting state in Awake");
            }

            Initialize(StartingState);
        }

        protected abstract void Awake();

        private void Initialize(State startingState)
        {
            CurrentState = startingState;
            startingState.EnterState();
        }

        protected virtual void Update()
        {
            if (IsComplete)
            {
                return;
            }

            CurrentState?.UpdateState();
        }

        public void ChangeState(State newState)
        {
            CurrentState?.ExitState();

            CurrentState = newState;

            if (newState != null)
            {
                newState.EnterState();
            }
            else
            {
                throw new InvalidStateException("Trying to enter a null state. This is not possible and probably should have been caught before now");
            }
        }

        public void SetAsCompleted()
        {
            CurrentState.ExitState();
            CurrentState = null;

            IsComplete = true;

            OnStateMachineComplete();
        }

        protected virtual void OnStateMachineComplete()
        {
        }
    }
}