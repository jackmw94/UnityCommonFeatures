using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace UnityCommonFeatures
{
    /// <summary>
    /// Useful to remove some of the repetitive code involved with setting up a class
    /// whose primary purpose is to execute some function on a button click
    /// </summary>
    [RequireComponent(typeof(Button))]
    public abstract class ButtonBehaviour : MonoBehaviour
    {
        [SerializeField] private Button _button;

        [Conditional("UNITY_EDITOR")]
        protected virtual void OnValidate()
        {
            if (!_button)
            {
                _button = GetComponent<Button>();
            }
        }

        protected virtual void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        protected virtual void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }
        
        protected abstract void OnButtonClicked();
    }
}