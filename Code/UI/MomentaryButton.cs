using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityCommonFeatures
{
    public class MomentaryButton : Button
    {
        public bool IsHeld { get; private set; }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            IsHeld = true;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            IsHeld = false;
        }
    }
}