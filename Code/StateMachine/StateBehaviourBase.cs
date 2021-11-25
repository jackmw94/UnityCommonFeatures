namespace UnityCommonFeatures
{
    public abstract class StateBehaviourBase<T> : IStateBehaviour where T : StateMachine
    {
        protected readonly State _nextState;
        protected readonly State _altState;
        protected readonly T _stateMachine;

        protected StateBehaviourBase(State nextState, T stateMachine)
        {
            _nextState = nextState;
            _stateMachine = stateMachine;
        }

        protected StateBehaviourBase(State nextState, State alternateState, T stateMachine)
        {
            _nextState = nextState;
            _altState = alternateState;
            _stateMachine = stateMachine;
        }

        public virtual void OnEnterState(){}

        public abstract State OnUpdateState();

        public virtual void OnExitState(){}
    }
}