using TMPro;
using UnityEngine;

namespace UnityCommonFeatures
{
    public class PulseTextColour : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private AnimationCurve _animationCurve;
        [SerializeField] private Color _colour;

        private Color _defaultColour;
        private Coroutine _pulseCoroutine = null;

        private void Awake()
        {
            _defaultColour = _text.color;
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
                _text.color = Color.Lerp(_defaultColour, _colour, curvedF);
            });
            _text.color = _defaultColour;
        }
    }
}