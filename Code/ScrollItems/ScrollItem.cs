using System.Collections;
using TMPro;
using UnityEngine;
using UnityExtras.Code.Core;

namespace UnityCommonFeatures
{
    public class ScrollItem : Selectable
    {
        [SerializeField] private TextMeshProUGUI _text;

        private Coroutine _colourChangeCoroutine = null;
        private bool? _wasOn = null;

        public TextMeshProUGUI Text => _text;
        public string Value => _text.text;
        
        public override void SetOnOff(bool isOn, Color colour, float duration)
        {
            if (isOn == _wasOn)
            {
                return;
            }

            _wasOn = isOn;
        
            if (_colourChangeCoroutine != null)
            {
                StopCoroutine(_colourChangeCoroutine);
            }
            _colourChangeCoroutine = StartCoroutine(SetColour(colour, duration));
        }

        private IEnumerator SetColour(Color colour, float duration)
        {
            Color startColour = _text.color;
            yield return Utilities.LerpOverTime(0f, 1f, duration, f =>
            {
                _text.color = Color.Lerp(startColour, colour, f);
            });
        }
    }
}