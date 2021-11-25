using UnityEngine;

namespace UnityCommonFeatures
{
    public abstract class Selectable : MonoBehaviour
    {
        public abstract void SetOnOff(bool isOn, Color colour, float duration);
    }
}