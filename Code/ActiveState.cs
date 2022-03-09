using System;
using System.Diagnostics;
// ReSharper disable InvalidXmlDocComment

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
    ///
    /// </summary>
    /// <example>
    /// For example, the active state enum for a HUD could be:
    /// <code>
    /// [Flags]
    /// public enum HudActiveStates
    /// {
    ///     None = 0,
    ///     EnabledThroughSettings = 1 << 0,
    ///     IsInGameplay = 1 << 1,
    ///     PauseMenuHidden = 1 << 2
    /// }
    /// </code>
    /// It would only declare itself active when each of the enum values were set. When going into gameplay we could
    /// call _hudActiveState.SetUnsetReason(HudActiveStates.IsInGameplay) and make similar calls for the other enum values.
    /// This would mean the _hudActiveState.IsActive would only declare itself true when ALL of the values are set.
    /// You can override IsActiveInternal for a custom definition of the active state to suit your needs.
    /// </example>
    /// <typeparam name="T">The enum (with [Flags] attribute) stating the reasons that control active state</typeparam>
    public class ActiveState<T> where T : Enum
    {
        private T _currentState;
        
        // optimisation: if we know the on state then we don't have to iterate through the enum values to check they're all set
        private int? _onState = null;

        public bool IsActive => IsActiveInternal(_currentState);

        [Conditional("UNITY_EDITOR")]
        public void Validate()
        {
            Debug.Assert(typeof(T).IsDefined(typeof(FlagsAttribute), false), "The generic type of ActiveState needs to be an enum type with the 'flags' attribute");
        }

        /// <summary>
        /// Used to short circuit iterating through all values within the enum
        /// If your flags enum has a MAX value constructed of bitwise OR-ing the other values
        /// then you can pass it to this function to short circuit the regular iterative IsActive check
        /// </summary>
        /// <param name="onState">The enum value that we're stating means that the object is 'active'</param>
        public void SetPredefinedOnState(T onState)
        {
            _onState = (int)(object)onState;
        }

        /// <summary>
        /// Set or unset an active flag value
        /// </summary>
        public void SetUnsetReason(T reason, bool set)
        {
            int currentActiveStateValue = (int)(object)_currentState;
            int reasonValue = (int)(object)reason;
            if (set)
            {
                currentActiveStateValue |= reasonValue;
            }
            else
            {
                currentActiveStateValue &= ~reasonValue;
            }

            _currentState = (T)(currentActiveStateValue as object);
        }

        /// <summary>
        /// Checks whether a given flag is set
        /// </summary>
        public bool IsActiveForReason(T reason)
        {
            int currentActiveStateValue = (int)(object)_currentState;
            int reasonValue = (int)(object)reason;
            return (currentActiveStateValue & reasonValue) != 0;
        }

        /// <summary>
        /// Returns whether the value of the active state should mean the controlled element is active.
        /// Override this if you want anything other than "Active" to be when all flags are set
        /// (or, if defined, when the on value we set equals the current value)
        /// </summary>
        protected virtual bool IsActiveInternal(T currentValue)
        {
            if (_onState != null)
            {
                return _onState == (int)(object)currentValue;
            }
            
            Array values = Enum.GetValues(typeof(T));
            foreach (T value in values)
            {
                if (!currentValue.HasFlag(value))
                {
                    return false;
                }
            }
            return true;
        }
    }
}