using UnityEngine;
using UnityEngine.UI;

namespace UnityCommonFeatures
{
    public class ColourableImage : Image, IColourable, IMaterialed
    {
        public void SetColour(Color colour)
        {
            color = colour;
        }

        public Color GetColour()
        {
            return color;
        }
        
        public void SetMaterial(Material mat)
        {
            material = mat;
        }

        public Material GetMaterial()
        {
            return material;
        }
    }
}