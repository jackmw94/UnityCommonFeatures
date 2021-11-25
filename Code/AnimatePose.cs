using System;
using System.Collections.Generic;
using UnityEngine;
using UnityExtras.Code.Core;
using UnityExtras.Code.Optional.EasingFunctions;

namespace UnityCommonFeatures
{
    public class AnimatePose : MonoBehaviour
    {
        public class PoseAnimationConfiguration
        {
            public enum PoseMode
            {
                RotationOnly,
                PositionOnly,
                UseAllPose
            }

            public Pose TargetPose { get; }
            public Transform TargetTransform { get; }
            public float Duration { get; }
            public float PauseAtStart { get; }
            public float PauseAtTarget { get; }
            public EasingFunctions.EasingType EasingType { get; }
            public EasingFunctions.EasingDirection EasingDirection { get; }
            public PoseMode AnimationPoseMode { get; }
            public Action OnStart { get; }
            public Action<float> OnUpdate { get; }
            public Action OnComplete { get; set; }
            public Action OnCancel { get; set; }

            public bool UsingStaticPose { get; }

            public PoseAnimationConfiguration(
                Pose targetPose,
                float duration = 1f,
                float pauseAtStart = 0f,
                float pauseAtTarget = 0.25f,
                EasingFunctions.EasingType easingType = EasingFunctions.EasingType.Sine,
                EasingFunctions.EasingDirection easingDirection = EasingFunctions.EasingDirection.InAndOut,
                PoseMode poseMode = PoseMode.UseAllPose,
                Action onStart = null,
                Action<float> onUpdate = null,
                Action onComplete = null,
                Action onCancel = null)
            {
                TargetPose = targetPose;
                Duration = duration;
                PauseAtStart = pauseAtStart;
                PauseAtTarget = pauseAtTarget;
                EasingType = easingType;
                EasingDirection = easingDirection;
                AnimationPoseMode = poseMode;
                OnStart = onStart;
                OnUpdate = onUpdate;
                OnComplete = onComplete;
                OnCancel = onCancel;

                UsingStaticPose = true;
            }

            public PoseAnimationConfiguration(
                Transform targetTransform,
                float duration = 1f,
                float pauseAtStart = 0f,
                float pauseAtTarget = 0.25f,
                EasingFunctions.EasingType easingType = EasingFunctions.EasingType.Sine,
                EasingFunctions.EasingDirection easingDirection = EasingFunctions.EasingDirection.InAndOut,
                PoseMode poseMode = PoseMode.UseAllPose,
                Action onStart = null,
                Action<float> onUpdate = null,
                Action onComplete = null)
            {
                TargetTransform = targetTransform;
                Duration = duration;
                PauseAtStart = pauseAtStart;
                PauseAtTarget = pauseAtTarget;
                EasingType = easingType;
                EasingDirection = easingDirection;
                AnimationPoseMode = poseMode;
                OnStart = onStart;
                OnUpdate = onUpdate;
                OnComplete = onComplete;

                UsingStaticPose = false;
            }
        }

        private class PoseAnimation
        {
            private PoseAnimationConfiguration _configuration;
            private Pose? _startingPose;
            private float _startingTime;

            private float AnimationTime => Time.time - _startingTime;
            private float LinearAnimationProgress => Mathf.Clamp01((AnimationTime - _configuration.PauseAtStart) / _configuration.Duration);
            public float EasedAnimationProgress => EasingFunctions.ConvertLinearToEased(_configuration.EasingType, _configuration.EasingDirection, LinearAnimationProgress);

            public bool IsComplete => AnimationTime > (_configuration.Duration + _configuration.PauseAtStart + _configuration.PauseAtTarget);

            private Pose GetCurrentPose(Pose currentPose, float lerpAmt)
            {
                if (!_startingPose.HasValue)
                {
                    throw new ArgumentException("Trying to get animation pose's current pose before having set it as started");
                }

                Pose targetPose = _configuration.UsingStaticPose ? _configuration.TargetPose : _configuration.TargetTransform.GetPose();

                Pose lerpedPose = Utilities.LerpPose(_startingPose.Value, targetPose, lerpAmt);

                switch (_configuration.AnimationPoseMode)
                {
                    case PoseAnimationConfiguration.PoseMode.PositionOnly:
                        lerpedPose.rotation = currentPose.rotation;
                        break;
                    case PoseAnimationConfiguration.PoseMode.RotationOnly:
                        lerpedPose.position = currentPose.position;
                        break;
                }

                return lerpedPose;
            }

            public PoseAnimation(PoseAnimationConfiguration configuration)
            {
                _configuration = configuration;
            }

            public void SetStarted(Transform transform)
            {
                _startingPose = transform.GetPose();
                _startingTime = Time.time;
                _configuration.OnStart?.Invoke();
            }

            public void UpdateProgress(Transform transform)
            {
                float animationProgress = EasedAnimationProgress;

                Pose previousPose = transform.GetPose();
                Pose currentPose = GetCurrentPose(previousPose, animationProgress);
                transform.SetPose(currentPose);

                _configuration.OnUpdate?.Invoke(animationProgress);
            }

            public void SetCompleted()
            {
                _configuration.OnComplete?.Invoke();
            }

            public void OnCancel()
            {
                _configuration.OnCancel?.Invoke();
            }

            public override string ToString()
            {
                return $"PoseAnim moving to {_configuration.TargetPose.position} in {_configuration.Duration} seconds";
            }

            public void DrawGizmo()
            {
                Vector3 targetPos = _configuration.UsingStaticPose ? _configuration.TargetPose.position : _configuration.TargetTransform.position;
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(targetPos, 0.01f);
            }
        }

        private bool _isPaused = false;
        private PoseAnimation _currentPoseAnimation = null;
        private readonly Queue<PoseAnimation> _animationsQueue = new Queue<PoseAnimation>();

        public bool IsComplete => _currentPoseAnimation == null && _animationsQueue.Count == 0;

        public void QueueTarget(PoseAnimationConfiguration configuration)
        {
            _animationsQueue.Enqueue(new PoseAnimation(configuration));
        }

        public void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;
        }

        public void Reset()
        {
            if (_currentPoseAnimation != null)
            {
                _currentPoseAnimation.OnCancel();
                _currentPoseAnimation = null;
            }

            foreach (PoseAnimation queuedAnimation in _animationsQueue)
            {
                queuedAnimation.OnCancel();
            }
            _animationsQueue.Clear();

            SetPaused(false);
        }

        private void Update()
        {
            if (_isPaused)
            {
                Debug.Assert(_currentPoseAnimation == null, "Don't think pausing mid-animation is supported. Need to account for lost time");
                return;
            }

            if (_currentPoseAnimation == null && _animationsQueue.Count > 0)
            {
                _currentPoseAnimation = _animationsQueue.Dequeue();
                _currentPoseAnimation.SetStarted(transform);
            }

            if (_currentPoseAnimation == null)
            {
                return;
            }

            _currentPoseAnimation.UpdateProgress(transform);

            if (_currentPoseAnimation.IsComplete)
            {
                _currentPoseAnimation.SetCompleted();
                _currentPoseAnimation = null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            _currentPoseAnimation?.DrawGizmo();
        }
    }
}
