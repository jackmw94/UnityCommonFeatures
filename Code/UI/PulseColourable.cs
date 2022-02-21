using System.Collections;
using UnityEngine;
using UnityExtras.Core;

namespace UnityCommonFeatures
{
    public class PulseColourable : MonoBehaviour
    {
        [SerializeReference, SerializeField] private IColourable _colourable;
        [SerializeField] private AnimationCurve _animationCurve;
        [SerializeField] private Color _colour;

        private Color _defaultColour;
        private Coroutine _pulseCoroutine = null;

        private void Awake()
        {
            _defaultColour = _colourable.GetColour();
        }

        public void PulseColour()
        {
            this.RestartCoroutine(ref _pulseCoroutine, PulseCoroutine());
        }

        private IEnumerator PulseCoroutine()
        {
            yield return Utilities.LerpOverTime(0f, _animationCurve.GetCurveDuration(), 1f, f =>
            {
                float curvedF = _animationCurve.Evaluate(f);
                _colourable.SetColour(Color.Lerp(_defaultColour, _colour, curvedF));
            });
            _colourable.SetColour(_defaultColour);
        }
    }
}