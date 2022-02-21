using UnityEngine;

namespace UnityCommonFeatures
{
    public interface IColourable
    {
        void SetColour(Color colour);
        Color GetColour();
    }
}