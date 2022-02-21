using System;
using UnityEngine;

namespace UnityCommonFeatures
{
    [Serializable]
    public class FloatWithInertia
    {
        [SerializeField] private float _inertia = 0.98f;
        [SerializeField] private float _damping = 0.11f;

        private float _target = 0f;
        private float _current = 0f;
        private float _previousChange = 0f;

        public float Value => _current;
        
        public void SetTargetValue(float target)
        {
            _target = target;
        }

        public void Tick()
        {
            _current = GetChange(_inertia, _current, _target, _previousChange, out _previousChange);
        }
        
        private float GetChange(float inertia, float current, float target, float previousChange, out float change)
        {
            change = inertia * previousChange + (target - current) * (1f - inertia);
            float next = current + change;
            return Mathf.Lerp(next, target, _damping);
        }

        public void SetInertia(float inertia)
        {
            _inertia = inertia;
        }

        public void SetDamping(float damping)
        {
            _damping = damping;
        }
        
        public override string ToString()
        {
            return $"Current = {_current}, Target = {_target}";
        }

        public void CopyFrom(FloatWithInertia other)
        {
            _inertia = other._inertia;
            _damping = other._damping;
        }
        
        public void SetCurrentValueInstant(float currentValue)
        {
            _current = currentValue;
        }
    }
}