namespace UnityCommonFeatures
{
    public interface IStateBehaviour
    {
        void OnEnterState();

        State OnUpdateState();

        void OnExitState();
    }
}