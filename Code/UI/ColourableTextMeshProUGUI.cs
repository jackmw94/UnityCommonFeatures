using TMPro;
using UnityEngine;

namespace UnityCommonFeatures
{
    public class ColourableTextMeshProUGUI : TextMeshProUGUI, IColourable
    {
        public void SetColour(Color colour)
        {
            color = colour;
        }

        public Color GetColour()
        {
            return color;
        }
    }
}