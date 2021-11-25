using System;

namespace UnityCommonFeatures
{
    /// <summary>
    /// This class is here for when something has multiple reasons to be off or on
    /// We use an enum with the [Flags] attribute containing each of the reasons
    /// 
    /// To demonstrate its use, it was first created when I had a feature that could be
    /// enabled or disabled by the user in the settings, was also only enabled during a
    /// level and when the level was not a replay
    /// 
    /// Instead of having the feature track multiple bools and only show when they were
    /// all true, we set and unset the flags of a value of the enum type T
    /// </summary>
    /// <typeparam name="T">The enum (with [Flags] attribute) stating the reasons that control active state</typeparam>
    public abstract class ActiveState<T> where T : Enum
    {
        private T _activeState;


        public bool IsActive => IsActiveInternal(_activeState);

        public void SetUnsetReason(T reason, bool set)
        {
            int currentActiveStateValue = (int)(object)_activeState;
            int reasonValue = (int)(object)reason;
            if (set)
            {
                currentActiveStateValue |= reasonValue;
            }
            else
            {
                currentActiveStateValue &= ~reasonValue;
            }

            _activeState = (T)(currentActiveStateValue as object);
        }

        public bool IsActiveForReason(T reason)
        {
            int currentActiveStateValue = (int)(object)_activeState;
            int reasonValue = (int)(object)reason;
            return (currentActiveStateValue & reasonValue) != 0;
        }

        /// <summary>
        /// Returns whether the value of the active state should mean the controlled element is active
        /// </summary>
        protected abstract bool IsActiveInternal(T currentValue);
    }
}