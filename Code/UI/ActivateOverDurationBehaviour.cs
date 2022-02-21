using System;
using System.Collections;
using UnityEngine;
using UnityExtras.Core;
using UnityExtras.Code.Optional.EasingFunctions;

namespace UnityCommonFeatures
{
    public abstract class ActivateOverDurationBehaviour : MonoBehaviour
    {
        [SerializeField] private bool _startVisible;
        [SerializeField] private float _duration;
        [SerializeField] private EasingFunctions.EasingType _easingType = EasingFunctions.EasingType.Linear;
        [SerializeField] private EasingFunctions.EasingDirection _easingDirection = EasingFunctions.EasingDirection.InAndOut;

        private VisibleState _visibleState;
        private Coroutine _activateCoroutine;
        private float _currentActivatedAmount = 0f;

        public VisibleState CurrentVisibleState => _visibleState;
        public float CurrentActivatedAmount => _currentActivatedAmount;

        protected virtual void Awake()
        {
            ActivateDeactivate(_startVisible, true);
        }

        public void ActivateDeactivate(bool activate, bool instant = false, Action onComplete = null)
        {
            this.RestartCoroutine(ref _activateCoroutine, ActivateDeactivateCoroutine(activate, instant, onComplete));
        }

        private IEnumerator ActivateDeactivateCoroutine(bool activate, bool instant, Action onComplete)
        {
            float startingAmount = _currentActivatedAmount;
            float targetAmount = activate ? 1f : 0f;
            float transitionDuration = instant ? 0f : _duration;

            _visibleState = activate ? VisibleState.ChangingToVisible : VisibleState.ChangingToHidden;
            
            OnActivateDeactivateStarted();
            
            yield return Utilities.LerpOverTime(startingAmount, targetAmount, transitionDuration, f =>
            {
                _currentActivatedAmount = f;
                float easedF = EasingFunctions.ConvertLinearToEased(_easingType, _easingDirection, f);
                SetActivatedAmount(easedF);
            });

            _visibleState = activate ? VisibleState.Visible : VisibleState.Hidden;

            OnActivateDeactivateFinished();
            onComplete?.Invoke();
        }

        protected virtual void OnActivateDeactivateStarted(){}
        protected virtual void OnActivateDeactivateFinished(){}
        
        protected abstract void SetActivatedAmount(float amount);
        
#if UNITY_EDITOR
        [ContextMenu(nameof(DebugActivate))]
        private void DebugActivate() => ActivateDeactivate(true);
        
        [ContextMenu(nameof(DebugDeactivate))]
        private void DebugDeactivate() => ActivateDeactivate(false);
#endif
    }
}