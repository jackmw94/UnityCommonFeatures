using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityExtras.Core;
using static UnityExtras.Code.Optional.EasingFunctions.EasingFunctions;
using Debug = UnityEngine.Debug;

namespace UnityCommonFeatures
{
    [DefaultExecutionOrder(-1), RequireComponent(typeof(CanvasGroup))]
    public class UIPanel : MonoBehaviour, IActivateable
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
            [SerializeField, FormerlySerializedAs("MinAnchors")] private Vector2 _minAnchors = Vector3.zero;
            [SerializeField, FormerlySerializedAs("MaxAnchors")] private  Vector2 _maxAnchors = Vector3.one;
            
            public Vector2 MinAnchors => _minAnchors;
            public Vector2 MaxAnchors => _maxAnchors;

            public float Left => _leftTop.x;
            public float Top => _leftTop.y;
            public float Right => _rightBottom.x;
            public float Bottom => _rightBottom.y;

            public void SetValuesFromRectTransform(RectTransform rectTransform)
            {
                _leftTop = rectTransform.offsetMin;
                _rightBottom = rectTransform.offsetMax;
                _minAnchors = rectTransform.anchorMin;
                _maxAnchors = rectTransform.anchorMax;
            }
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
            HorizontalScale = 1 << 6,
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
        [SerializeField] private float _showDuration = 0.33f;
        [SerializeField] private AnimateType _showAnimateType = AnimateType.Fade;
        [SerializeField] private EasingType _showEasingType = EasingType.Sine;
        [SerializeField] private EasingDirection _showEasingDirection = EasingDirection.InAndOut;

        [Header("Hide")]
        [SerializeField] private float _hideDuration = 0.33f;
        [SerializeField] private AnimateType _hideAnimateType = AnimateType.Fade;
        [SerializeField] private EasingType _hideEasingType = EasingType.Sine;
        [SerializeField] private EasingDirection _hideEasingDirection = EasingDirection.InAndOut;
    
        [Header("Other")]
        [SerializeField] private StartBehaviour _startBehaviour = StartBehaviour.HideInstant;

        [SerializeField, Tooltip("The reduction in distance travelled for directional show hide animations")] 
        private float _directionalShowHideOffset = 0.9f;

        [SerializeField] private Positioning _panelPositioning = new Positioning();

        private Coroutine _showHideCoroutine = null;
        private float _currShowAmt = 0f;
    
        private readonly Dictionary<UnityEngine.UI.Selectable,SelectableColourBlocks> _selectablesInPanel = new Dictionary<UnityEngine.UI.Selectable,SelectableColourBlocks>();
    
        private static readonly Dictionary<Type,UIPanel> _activePanels = new Dictionary<Type, UIPanel>();
    
        public float CurrentShowAmount => _currShowAmt;
        public bool IsShowing => CurrentPanelState == VisibleState.Visible || CurrentPanelState == VisibleState.ChangingToVisible;
        public VisibleState CurrentPanelState { get; private set; } = VisibleState.Hidden;

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

            if (!Application.isPlaying)
            {
                _panelPositioning.SetValuesFromRectTransform(_rectTransform);
            }
        }

        private void Awake()
        {
            CacheSelectablesColours();

            Type type = GetType();
            if (type != typeof(UIPanel))
            {
                if (!_activePanels.ContainsKey(type))
                {
                    // There is no case where we should be getting a UIPanel base class instance this way
                    _activePanels.Add(type, this);
                }
                else
                {
                    _activePanels[type] = null;
                }
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
            if (!_activePanels.TryGetValue(typeof(T), out UIPanel panel))
            {
                throw new ArgumentException($"No UI panel exists for type {typeof(T)}. If it definitely exists and is enabled, check it's not implementing Awake without calling base.Awake()");
            }
            Debug.Assert(panel, "Panel doesn't exist despite being in the active panels dictionary. This happens if there are multiple instances of this panel in the scene.");
            return panel as T;
        }
        
        public void SetActivated(bool activated) => ShowHide(activated);

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
            _showHideCoroutine = StartCoroutine(ShowCoroutine(instant, onComplete));
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
            _showHideCoroutine = StartCoroutine(HideCoroutine(instant, onComplete));
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

            _rectTransform.offsetMin = new Vector2(_panelPositioning.Left, _panelPositioning.Top);
            _rectTransform.offsetMax = new Vector2(_panelPositioning.Right, _panelPositioning.Bottom);

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
        
            Vector2 anchorMin = _rectTransform.anchorMin;
            Vector2 anchorMax = _rectTransform.anchorMax;
        
            anchorMin.x = left - 1f + _panelPositioning.MinAnchors.x;
            anchorMax.x = left - (1f - _panelPositioning.MaxAnchors.x);

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
        }
    
        private void SetRightAmount(float right)
        {
            // convert right into reduced lerp value
            right = Mathf.Lerp(_directionalShowHideOffset, 1f, right);
        
            Vector2 anchorMin = _rectTransform.anchorMin;
            Vector2 anchorMax = _rectTransform.anchorMax;
        
            anchorMin.x = 1f - right + _panelPositioning.MinAnchors.x;
            anchorMax.x = 2f - right - (1f - _panelPositioning.MaxAnchors.x);

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
        }
    
        private void SetBottomAmount(float bottom)
        {
            // convert bottom into reduced lerp value
            bottom = Mathf.Lerp(_directionalShowHideOffset, 1f, bottom);
        
            Vector2 anchorMin = _rectTransform.anchorMin;
            Vector2 anchorMax = _rectTransform.anchorMax;
        
            anchorMin.y = bottom - 1f + _panelPositioning.MinAnchors.y;
            anchorMax.y = bottom - (1f - _panelPositioning.MaxAnchors.y);

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
        }
    
        private void SetTopAmount(float top)
        {
            // Convert top into reduced lerp value
            top = Mathf.Lerp(_directionalShowHideOffset, 1f, top);
        
            Vector2 anchorMin = _rectTransform.anchorMin;
            Vector2 anchorMax = _rectTransform.anchorMax;
        
            anchorMin.y = 1f - top + _panelPositioning.MinAnchors.y;
            anchorMax.y = 2f - top - (1f - _panelPositioning.MaxAnchors.y);

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
        }

        private void SetVerticalScaleAmount(float verticalScale)
        {
            _rectTransform.localScale = _rectTransform.localScale.ModifyVectorElement(1, verticalScale);
        }
        
        private void SetHorizontalScaleAmount(float horizontalScale)
        {
            _rectTransform.localScale = _rectTransform.localScale.ModifyVectorElement(0, horizontalScale);
        }
    
        private void SetFadeAmount(float fade)
        {
            _canvasGroup.alpha = fade;
            OnPanelAlphaSet(fade);
        }

        #endregion
    

        #region Show or Hide Events

        protected virtual void OnShowStarted()
        {
            SetSelectablesColour(true);
            CurrentPanelState = VisibleState.ChangingToVisible;
        }

        protected virtual void OnShowCompleted()
        {
            SetSelectablesColour(false);
            CurrentPanelState = VisibleState.Visible;
        }

        protected virtual void OnHideStarted()
        {
            CurrentPanelState = VisibleState.ChangingToHidden;
        }

        protected virtual void OnHideCompleted()
        {
            CurrentPanelState = VisibleState.Hidden;
        }
    
        protected virtual void OnShowAmountChanged(float showAmt) { }
        
        protected virtual void OnPanelAlphaSet(float alpha) { }

        #endregion


        #region Enum-To-Function Converters

        private void SetAnimateAmount(AnimateType animateType, float showingAmt)
        {
            switch (animateType)
            {
                case AnimateType.None:
                    return;
                case AnimateType.Fade:
                    SetFadeAmount(showingAmt);
                    return;
                case AnimateType.Left:
                    SetLeftAmount(showingAmt);
                    return;
                case AnimateType.Right:
                    SetRightAmount(showingAmt);
                    return;
                case AnimateType.Bottom:
                    SetBottomAmount(showingAmt);
                    return;
                case AnimateType.Top:
                    SetTopAmount(showingAmt);
                    return;
                case AnimateType.VerticalScale:
                    SetVerticalScaleAmount(showingAmt);
                    return;
                case AnimateType.HorizontalScale:
                    SetHorizontalScaleAmount(showingAmt);
                    return;
                default:
                    throw new NoAnimationTypeException($"Can't animate panel for animate type {animateType}");
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