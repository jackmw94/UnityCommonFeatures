﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using UnityExtras.Code.Core;
using static UnityExtras.Code.Optional.EasingFunctions.EasingFunctions;
using Debug = UnityEngine.Debug;

namespace UnityCommonFeatures
{
    [DefaultExecutionOrder(-1), RequireComponent(typeof(CanvasGroup))]
    public class UIPanel : MonoBehaviour
    {
        private class NoAnimationTypeException : Exception
        {
            public NoAnimationTypeException(string message) : base(message) { }
        }
    
        [Serializable]
        private class Positioning
        {
            [SerializeField] private Vector2 _leftTop = Vector3.zero;
            [SerializeField] private  Vector2 _rightBottom = Vector3.zero;
            public Vector2 MinAnchors = Vector2.zero;
            public Vector2 MaxAnchors = Vector2.one;

            public float Left => _leftTop.x;
            public float Top => _leftTop.y;
            public float Right => _rightBottom.x;
            public float Bottom => _rightBottom.y;
        }

        private struct SelectableColourBlocks
        {
            public ColorBlock AllowDisabledColours;
            public ColorBlock ConsistentColour;
        }
    
        [Flags]
        private enum AnimateType
        {
            None = 0,
            Fade = 1 << 0,
            Left = 1 << 1,
            Right = 1 << 2,
            Bottom = 1 << 3,
            Top = 1 << 4,
            VerticalScale = 1 << 5,
        }

        private enum StartBehaviour
        {
            HideInstant,
            ShowInstant,
            ShowAnimated,
        }

        [Header("References")]
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] protected CanvasGroup _canvasGroup;

        [Header("Show")]
        [SerializeField] private float _showDuration = 0.5f;
        [SerializeField] private AnimateType _showAnimateType = AnimateType.Fade;
        [SerializeField] private EasingType _showEasingType = EasingType.Linear;
        [SerializeField] private EasingDirection _showEasingDirection = EasingDirection.Out;

        [Header("Hide")]
        [SerializeField] private float _hideDuration = 0.25f;
        [SerializeField] private AnimateType _hideAnimateType = AnimateType.Fade;
        [SerializeField] private EasingType _hideEasingType = EasingType.Linear;
        [SerializeField] private EasingDirection _hideEasingDirection = EasingDirection.In;
    
        [Header("Other")]
        [SerializeField] private StartBehaviour _startBehaviour = StartBehaviour.HideInstant;

        [SerializeField, Tooltip("The reduction in distance travelled for directional show hide animations")] 
        private float _directionalShowHideOffset = 0.9f;

        [SerializeField] private Positioning _panelPositioning;

        private Coroutine _showHideCoroutine = null;
        private float _currShowAmt = 0f;
    
        private readonly Dictionary<UnityEngine.UI.Selectable,SelectableColourBlocks> _selectablesInPanel = new Dictionary<UnityEngine.UI.Selectable,SelectableColourBlocks>();
    
        private static readonly Dictionary<Type,UIPanel> _activePanels = new Dictionary<Type, UIPanel>();
    
        public float ShowAmount => _currShowAmt;
        public bool IsShowing => _currShowAmt > float.Epsilon;

        [Conditional("UNITY_EDITOR")]
        protected virtual void OnValidate()
        {
            if (!_rectTransform)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            if (!_canvasGroup)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void Awake()
        {
            CacheSelectablesColours();

            if (GetType() != typeof(UIPanel))
            {
                // Adding base class just confuses things - there is no case where we should be getting a UIPanel base class instance this way
                _activePanels.Add(GetType(), this);
            }

            InternalAwake();
        }

        private void Start()
        {
            switch (_startBehaviour)
            {
                case StartBehaviour.HideInstant:
                    Hide(true);
                    break;
                case StartBehaviour.ShowInstant:
                    Show(true);
                    break;
                case StartBehaviour.ShowAnimated:
                    SetShowingAmount(_showAnimateType, 0f);
                    Show();
                    break;
            }
        
            InternalStart();
        }

        private void OnDestroy()
        {
            _activePanels.Remove(GetType());
            InternalOnDestroy();
        }

        protected virtual void InternalAwake(){}
        protected virtual void InternalStart(){}
        protected virtual void InternalOnDestroy(){}
    
        public static T GetPanel<T>() where T : UIPanel
        {
            if (_activePanels.TryGetValue(typeof(T), out var panel))
            {
                Debug.Assert(panel, "Panel doesn't exist despite being in the active panels dictionary. Did you implement OnDestroy without calling base.OnDestroy()?");
                return panel as T;
            }
            throw new ArgumentException($"No UI panel exists for type {typeof(T)}. If it definitely exists and is enabled, check it's not implementing Awake without calling base.Awake()");
        }

        public void ShowHide(bool show, bool instant = false, Action onComplete = null)
        {
            if (show)
            {
                Show(instant: instant, onComplete: onComplete);
            }
            else
            {
                Hide(instant: instant, onComplete: onComplete);
            }
        }

        /// <summary>
        /// Shows the UI panel. If overriding, you'll almost certainly want to call base.Show at some point
        /// Overriding is there to allow you to have control over when you show the regular UIPanel elements
        /// </summary>
        /// <param name="instant">Whether or not to show instantly, disregarding duration</param>
        /// <param name="onComplete">Function to call once show operation completed</param>
        public virtual void Show(bool instant = false, Action onComplete = null)
        {
            if (_showHideCoroutine != null)
            {
                StopCoroutine(_showHideCoroutine);
            }
            StartCoroutine(ShowCoroutine(instant, onComplete));
        }

        /// <summary>
        /// Hides the UI panel. If overriding, you'll almost certainly want to call base.Hide at some point
        /// Overriding is there to allow you to have control over when you hide the regular UIPanel elements
        /// </summary>
        /// <param name="instant">Whether or not to hide instantly, disregarding duration</param>
        /// <param name="onComplete">Function to call once hide operation completed</param>
        [ContextMenu(nameof(Hide))]
        public virtual void Hide(bool instant = false, Action onComplete = null)
        {
            if (_showHideCoroutine != null)
            {
                StopCoroutine(_showHideCoroutine);
            }
            StartCoroutine(HideCoroutine(instant, onComplete));
        }

        private void CacheSelectablesColours()
        {
            GetComponentsInChildren<UnityEngine.UI.Selectable>(true).ApplyFunction(selectable =>
            {
                ColorBlock useDisabledColour = selectable.colors;
                ColorBlock ignoreCanvasDisabling = selectable.colors;
                ignoreCanvasDisabling.disabledColor = ignoreCanvasDisabling.normalColor;
                _selectablesInPanel.Add(selectable, new SelectableColourBlocks
                {
                    AllowDisabledColours = useDisabledColour,
                    ConsistentColour = ignoreCanvasDisabling
                });
            });
        }

        private void SetSelectablesColour(bool ignoreCanvasDisabling)
        {
            _selectablesInPanel.ApplyFunction(selectableColoursPair =>
            {
                UnityEngine.UI.Selectable selectable = selectableColoursPair.Key;
                selectable.colors = ignoreCanvasDisabling ? selectableColoursPair.Value.ConsistentColour : selectableColoursPair.Value.AllowDisabledColours;
            });
        }

        private IEnumerator ShowCoroutine(bool instant, Action onComplete)
        {
            OnShowStarted();
        
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = true;
            gameObject.SetActiveSafe(true);

            _rectTransform.localPosition = Vector3.zero;
            _rectTransform.anchoredPosition = Vector2.zero;

            _rectTransform.offsetMin = new Vector2(_panelPositioning.Left, _panelPositioning.Bottom);
            _rectTransform.offsetMax = new Vector2(_panelPositioning.Right, _panelPositioning.Top);

            float duration = instant ? 0f : _showDuration;
            yield return Utilities.LerpOverTime(_currShowAmt, 1f, duration, f =>
            {
                float easedF = ConvertLinearToEased(_showEasingType, _showEasingDirection, f);
                SetShowingAmount(_showAnimateType, easedF);
            });
        
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        
            onComplete?.Invoke();
        
            OnShowCompleted();
        }
    
        private IEnumerator HideCoroutine(bool instant, Action onComplete)
        {
            OnHideStarted();
        
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = true;
        
            float duration = instant ? 0f : _hideDuration;
            yield return Utilities.LerpOverTime(_currShowAmt, 0f, duration, f =>
            {
                // Double "1f -" done in order to maintain easing direction. 
                float easedF = 1f - ConvertLinearToEased(_hideEasingType, _hideEasingDirection, 1f - f);
                SetShowingAmount(_hideAnimateType, easedF);
            });
        
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        
            onComplete?.Invoke();
        
            OnHideCompleted();
        }

        private void SetShowingAmount(AnimateType panelAnimateType, float showingAmt)
        {
            _currShowAmt = showingAmt;
            OnShowAmountChanged(_currShowAmt);
        
            foreach (AnimateType singleAnimateType in Enum.GetValues(typeof(AnimateType)))
            {
                if (panelAnimateType.HasFlag(singleAnimateType))
                {
                    SetAnimateAmount(singleAnimateType, showingAmt);
                }
            }
        }

        #region Animate Amount Setters

        private void SetLeftAmount(float left)
        {
            // convert left into reduced lerp value
            left = Mathf.Lerp(_directionalShowHideOffset, 1f, left);
        
            var anchorMin = _rectTransform.anchorMin;
            var anchorMax = _rectTransform.anchorMax;
        
            anchorMin.x = left - 1f + _panelPositioning.MinAnchors.x;
            anchorMax.x = left - (1f - _panelPositioning.MaxAnchors.x);

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
        }
    
        private void SetRightAmount(float right)
        {
            // convert right into reduced lerp value
            right = Mathf.Lerp(_directionalShowHideOffset, 1f, right);
        
            var anchorMin = _rectTransform.anchorMin;
            var anchorMax = _rectTransform.anchorMax;
        
            anchorMin.x = 1f - right + _panelPositioning.MinAnchors.x;
            anchorMax.x = 2f - right - (1f - _panelPositioning.MaxAnchors.x);

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
        }
    
        private void SetBottomAmount(float bottom)
        {
            // convert bottom into reduced lerp value
            bottom = Mathf.Lerp(_directionalShowHideOffset, 1f, bottom);
        
            var anchorMin = _rectTransform.anchorMin;
            var anchorMax = _rectTransform.anchorMax;
        
            anchorMin.y = bottom - 1f + _panelPositioning.MinAnchors.y;
            anchorMax.y = bottom - (1f - _panelPositioning.MaxAnchors.y);

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
        }
    
        private void SetTopAmount(float top)
        {
            // Convert top into reduced lerp value
            top = Mathf.Lerp(_directionalShowHideOffset, 1f, top);
        
            var anchorMin = _rectTransform.anchorMin;
            var anchorMax = _rectTransform.anchorMax;
        
            anchorMin.y = 1f - top + _panelPositioning.MinAnchors.y;
            anchorMax.y = 2f - top - (1f - _panelPositioning.MaxAnchors.y);

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
        }

        private void SetVerticalScaleAmount(float verticalScale)
        {
            _rectTransform.localScale = _rectTransform.localScale.ModifyVectorElement(1, verticalScale);
        }
    
        private void SetFadeAmount(float fade)
        {
            _canvasGroup.alpha = fade;
        }

        #endregion
    

        #region Show or Hide Events

        protected virtual void OnShowStarted()
        {
            SetSelectablesColour(true);
        }

        protected virtual void OnShowCompleted()
        {
            SetSelectablesColour(false);
        }
    
        protected virtual void OnHideStarted() { }
    
        protected virtual void OnHideCompleted() { }
    
        protected virtual void OnShowAmountChanged(float showAmt) { }

        #endregion


        #region Enum-To-Function Converters

        private void SetAnimateAmount(AnimateType animateType, float showingAmt)
        {
            switch (animateType)
            {
                case AnimateType.None:
                {
                    return;
                }
                case AnimateType.Fade:
                {
                    SetFadeAmount(showingAmt); 
                    return;
                }
                case AnimateType.Left:
                {
                    SetLeftAmount(showingAmt); 
                    return;
                }
                case AnimateType.Right:
                {
                    SetRightAmount(showingAmt); 
                    return;
                }
                case AnimateType.Bottom:
                {
                    SetBottomAmount(showingAmt); 
                    return;
                }
                case AnimateType.Top:
                {
                    SetTopAmount(showingAmt); 
                    return;
                }
                case AnimateType.VerticalScale:
                {
                    SetVerticalScaleAmount(showingAmt);
                    return;
                }
                default:
                {
                    throw new NoAnimationTypeException($"Can't animate panel for animate type {animateType}");
                }
            }
        }
    
        #endregion

        #region Editor

#if UNITY_EDITOR

        [ContextMenu(nameof(Show))]
        public void EditorShow() => Show();
    
        [ContextMenu(nameof(Hide))]
        public void EditorHide() => Hide();

#endif

        #endregion
    }
}