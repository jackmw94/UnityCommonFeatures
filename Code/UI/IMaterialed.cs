using UnityEngine;

namespace UnityCommonFeatures
{
    public interface IMaterialed
    {
        void SetMaterial(Material material);
        Material GetMaterial();
    }
}