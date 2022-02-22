using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityExtras.Core;

namespace UnityCommonFeatures
{
    public class TargetedScrollRect : ScrollRect
    {
        [SerializeField] private MagneticFloat _magneticFloatVertical;
        [SerializeField] private MagneticFloat _magneticFloatHorizontal;

        protected override void Awake()
        {
            base.Awake();
            
            SetTargetsByNumberOfContentChildren(_magneticFloatVertical);
            
            _magneticFloatHorizontal.UpdateEnabled = horizontal;
            _magneticFloatVertical.UpdateEnabled = vertical;
        }

        private void SetTargetsByNumberOfContentChildren(MagneticFloat magneticFloat)
        {
            int contentChildrenCount = content.childCount;
            float[] targets = new float[contentChildrenCount];
            targets.ApplyFunctionWithIndex((_, index) => targets[index] = index / (contentChildrenCount - 1f));
            
            magneticFloat.SetTargets(targets);
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            
            normalizedPosition = new Vector2(
                _magneticFloatHorizontal.UpdateCurrentValue(normalizedPosition.x), 
                _magneticFloatVertical.UpdateCurrentValue(normalizedPosition.y));
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            _magneticFloatVertical.UpdateEnabled = false;
            _magneticFloatHorizontal.UpdateEnabled = false;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            _magneticFloatVertical.UpdateEnabled = vertical;
            _magneticFloatHorizontal.UpdateEnabled = horizontal;
        }
    }
}