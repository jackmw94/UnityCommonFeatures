using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UnityCommonFeatures
{
    [Serializable]
    public sealed class InterfaceComponentWrapper<T> where T : class
    {
        [SerializeField] private Component _componentOfType;

        private T _componentInstance = null;

        public T Component => _componentInstance ??= _componentOfType as T;

        [Conditional("UNITY_EDITOR")]
        public void Validate()
        {
            if (_componentOfType && !(_componentOfType is T))
            {
                Debug.LogError($"Referenced component ({_componentOfType}) is not of type {typeof(T)}. Setting reference to null");
                _componentOfType = null;
            }
        }
    }
}