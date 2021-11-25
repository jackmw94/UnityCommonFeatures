using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace UnityCommonFeatures
{
    [DefaultExecutionOrder(-1), RequireComponent(typeof(ScrollRect))]
    public class MagneticScrollRect : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] protected ScrollRect _scrollRect;
        [SerializeField] protected RectTransform _root;
        [Space(15)]
        [SerializeField] private bool _isVerticalScroll = true;
        [SerializeField] private Transform _selector;
        [SerializeField] private float _magnetismFactor = 0.05f;
        [Space(15)]
        [SerializeField] private float _turnOnDuration;
        [SerializeField] private float _turnOffDuration;
        [SerializeField] private Color _unselectedColour;
        [SerializeField] private Color _selectedColour;

        private Selectable[] _items = new Selectable[0];
        private int _currentlySelectedIndex = -1;
        private bool _isScrollRectInteractedWith = false;

        public int NumberOfItems => _items.Length;
        public int CurrentlySelectedIndex => _currentlySelectedIndex;
        public Selectable CurrentlySelected => _items[_currentlySelectedIndex];

        [Conditional("UNITY_EDITOR")]
        private void OnValidate()
        {
            if (!_scrollRect)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }
        }

        private void Awake()
        {
            _items = GetScrollItems().OrderBy(p => p.transform.GetSiblingIndex()).ToArray();
            Logger.Assert(_items.Length > 0, "There are no selectable items beneath magnetic scroll rect}", this);
        }

        protected virtual Selectable[] GetScrollItems()
        {
            return GetComponentsInChildren<Selectable>();
        }

        private void Update()
        {
            if (_root.childCount == 0)
            {
                return;
            }
        
            DetermineSelectedItem();
            HandleMagnetism();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isScrollRectInteractedWith = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isScrollRectInteractedWith = false;
        }

        public void SetScrollingEnabled(bool scrollingEnabled)
        {
            _scrollRect.enabled = scrollingEnabled;
        }

        public void SetToItemAtIndex(int index)
        {
            int itemCount = _items.Length;
            int divisor = itemCount - 1;
            float perItemFraction = 1f / divisor;
            float normalisedScrollPosition = perItemFraction * index;

            if (_isVerticalScroll)
            {
                _scrollRect.verticalNormalizedPosition = 1f - normalisedScrollPosition;
            }
            else
            {
                _scrollRect.horizontalNormalizedPosition = normalisedScrollPosition;
            }
        }

        private void DetermineSelectedItem()
        {
            float selectorPosition = GetOneDimensionalPosition(_selector);
            float minimumDistanceFromSelector = float.MaxValue;
            _currentlySelectedIndex = -1;

            for (int i = 0; i < _root.childCount; i++)
            {
                Transform childTransform = _root.GetChild(i);
                float childPosition = GetOneDimensionalPosition(childTransform);
                float itemDistance = Mathf.Abs(childPosition - selectorPosition);
                if (itemDistance < minimumDistanceFromSelector)
                {
                    minimumDistanceFromSelector = itemDistance;
                    _currentlySelectedIndex = i;
                }
            }

            if (_currentlySelectedIndex == -1)
            {
                Debug.LogError($"Could not get a selected item despite there being {_root.childCount} items");
                return;
            }
            
            foreach (Selectable scrollItem in _items)
            {
                if (scrollItem == CurrentlySelected)
                {
                    scrollItem.SetOnOff(true, _selectedColour, _turnOnDuration);
                }
                else
                {
                    scrollItem.SetOnOff(false, _unselectedColour, _turnOffDuration);
                }
            }
        }

        private void HandleMagnetism()
        {
            Vector2 rootSizeDelta = _root.sizeDelta;
            float rootSizeAxis = _isVerticalScroll ? rootSizeDelta.y : rootSizeDelta.x;
            if (_isScrollRectInteractedWith || rootSizeAxis < float.Epsilon)
            {
                return;
            }

            Transform selectedItemTransform = _root.GetChild(_currentlySelectedIndex);
            float distanceToSelector = GetOneDimensionalPosition(_selector) - GetOneDimensionalPosition(selectedItemTransform);
            float fractionalDistance = distanceToSelector / rootSizeAxis;
            float moveDistance = _magnetismFactor * fractionalDistance;

            if (_isVerticalScroll)
            {
                _scrollRect.verticalNormalizedPosition -= moveDistance;
            }
            else
            {
                _scrollRect.horizontalNormalizedPosition -= moveDistance;
            }
        }

        private float GetOneDimensionalPosition(Transform objectTransform)
        {
            Vector3 position = objectTransform.position;
            return _isVerticalScroll ? position.y : position.x;
        }

        [ContextMenu(nameof(NameObjectsFromLabels))]
        private void NameObjectsFromLabels()
        {
            foreach (Transform child in _root)
            {
                TextMeshProUGUI label = child.GetComponentInChildren<TextMeshProUGUI>();
                child.gameObject.name = label.text;
            }
        }
    }
}