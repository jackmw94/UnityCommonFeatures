using UnityEngine;

namespace UnityCommonFeatures
{
    public class ScrollRectContentResizer : MonoBehaviour
    {
        [SerializeField] private RectTransform _viewport;
        [SerializeField] private RectTransform _content;
        [Space(15)]
        [SerializeField] private bool _isVertical = true;

        private void Start()
        {
            ResetSize();
        }

        [ContextMenu(nameof(ResetSize))]
        private void ResetSize()
        {
            int screenCount = _content.childCount;
            Vector2 viewportSizeDelta = _viewport.rect.size;
            
            float contentSizeMagnitude = (_isVertical ? viewportSizeDelta.y : viewportSizeDelta.x) * screenCount;
            RectTransform.Axis axis = _isVertical ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal;
            
            _content.SetSizeWithCurrentAnchors(axis, contentSizeMagnitude);
        }
    }
}