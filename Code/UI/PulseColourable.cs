using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityExtras.Core;

namespace UnityCommonFeatures
{
    public class PulseColourable : MonoBehaviour, IActivateable, IColourable
    {
        [SerializeField] private InterfaceComponentWrapper<IColourable> _colourable = new InterfaceComponentWrapper<IColourable>();
        [SerializeField] private AnimationCurve _animationCurve;
        [SerializeField] private Color _colour;

        private Color _defaultColour;
        private Coroutine _pulseCoroutine = null;

        [Conditional("UNITY_EDITOR")]
        protected virtual void OnValidate()
        {
            _colourable.Validate();
        }

        protected virtual void Awake()
        {
            _defaultColour = _colourable.Component.GetColour();
        }

        public void PulseColour()
        {
            this.RestartCoroutine(ref _pulseCoroutine, PulseCoroutine());
        }

        public void StopPulse()
        {
            StopCoroutine(_pulseCoroutine);
            _colourable.Component.SetColour(_defaultColour);
        }

        private IEnumerator PulseCoroutine()
        {
            yield return Utilities.LerpOverTime(0f, _animationCurve.GetCurveDuration(), 1f, f =>
            {
                float curvedF = _animationCurve.Evaluate(f);
                _colourable.Component.SetColour(Color.Lerp(_defaultColour, _colour, curvedF));
            });
            _colourable.Component.SetColour(_defaultColour);
        }

        public void SetActivated(bool activated)
        {
            if (activated)
            {
                PulseColour();
            }
            else
            {
                StopPulse();
            }
        }

        public void SetColour(Color colour)
        {
            _colour = colour;
        }

        public Color GetColour()
        {
            return _colour;
        }
    }
}