using UnityEngine;

namespace UnityCommonFeatures
{
    public interface IMaterialed
    {
        void SetMaterial(Material mat);
        Material GetMaterial();
    }
}