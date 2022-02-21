using UnityEngine;

namespace UnityCommonFeatures
{
    public interface ITexturable
    {
        void SetTexture(Texture texture);
        Texture GetTexture();
    }
}