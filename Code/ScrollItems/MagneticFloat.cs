using System;
using UnityEngine;

namespace UnityCommonFeatures
{
    [Serializable]
    public class MagneticFloat
    {
        [SerializeField] private AnimationCurve _targetCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float _targetSpeed = 0.01f;
        [SerializeField] private float _edgeSpeed = 0.2f;
        [SerializeField] private float _inertia = 0.95f;
        [Space(5)]
        [SerializeField] private float[] _targets;

        private float _velocity = 0f;
        private float? _previous = 0f;
        
        public bool UpdateEnabled { get; set; } = true;

        public void SetTargets(float[] targets)
        {
            _targets = targets;
        }

        public float UpdateCurrentValue(float currentValue)
        {
            _previous ??= currentValue;
            
            if (!UpdateEnabled)
            {
                UpdateVelocity(currentValue);
                return currentValue;
            }
            
            float movement = GetMovement(currentValue, _targets, _targetCurve, _targetSpeed, _edgeSpeed);
            currentValue += movement + _velocity * _inertia;

            UpdateVelocity(currentValue);

            return currentValue;
        }


        private void UpdateVelocity(float currentValue)
        {
            if (!_previous.HasValue)
            {
                throw new ArgumentException("Cannot update velocity while previous is null. This should have been initialised");
            }
            
            _velocity = currentValue - _previous.Value;
            _previous = currentValue;
        }

        private static float GetMovement(float current, float[] targets, AnimationCurve animationCurve, float speed, float edgeSpeed)
        {
            (float? previous, float? next) = GetPreviousAndNextTargets(current, targets);
        
            // can assume at this point one of previous or next is not null, would have thrown exception

            if (!previous.HasValue)
            {
                return Time.deltaTime * edgeSpeed * (next.Value - current);
            }
        
            if (!next.HasValue)
            { 
                return -Time.deltaTime * edgeSpeed * (current - previous.Value);
            }
        
            float distanceToPrevious = current - previous.Value;
            float distanceToNext = next.Value - current;
        
            float halfWayPoint = Mathf.Lerp(previous.Value, next.Value, 0.5f);
            bool previousIsCloser = distanceToPrevious < distanceToNext;
        
            float lerpValue = Mathf.InverseLerp(previousIsCloser ? previous.Value : next.Value, halfWayPoint, current);
            lerpValue = Mathf.Clamp01(lerpValue);
        
            float movement = animationCurve.Evaluate(lerpValue) * speed * Time.deltaTime * (previousIsCloser ? -1f : 1f);
            return movement;
        }

        private static (float?, float?) GetPreviousAndNextTargets(float current, float[] targets)
        {
            float? previous = null;
            float? next = null;
            foreach (float target in targets)
            {
                if (target < current)
                {
                    previous = target;
                }
                else
                {
                    next = target;
                    break;
                }
            }

            if (!previous.HasValue && !next.HasValue)
            {
                throw new ArgumentException($"Could not find either next or previous targets from {targets.Length} targets");
            }

            return (previous, next);
        }
    }
}