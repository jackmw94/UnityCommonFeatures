using System;
using UnityEngine;

namespace UnityCommonFeatures
{
    [Serializable]
    public class ComponentOnObjectWrapper<T> where T : class
    {
        [SerializeField] private GameObject _objectContainingComponent;

        private T _componentInstance = null;

        public T Component => _componentInstance ??= _objectContainingComponent.GetComponent<T>();
    }
}