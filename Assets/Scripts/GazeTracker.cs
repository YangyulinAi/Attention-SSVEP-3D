using Tobii.G2OM;
using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间来访问Image组件


namespace Tobii.XR.Examples.GettingStarted
{
    // Monobehaviour which implements the "IGazeFocusable" interface, meaning it will be called on when the object receives focus
    public class GazeTracker : MonoBehaviour, IGazeFocusable
    {
        public Color highlightColor = Color.red;
        public float animationTime = 0.1f;

        private Image _image; 
        private Color _originalColor;
        private Color _targetColor;

        public bool IsFocused { get; private set; } // 公开当前的注视状态

        // The method of the "IGazeFocusable" interface, which will be called when this object receives or loses focus
        public void GazeFocusChanged(bool hasFocus)
        {
            IsFocused = hasFocus; // 更新注视状态

            Debug.Log("Debug" + hasFocus);


            // If this object received focus, fade the object's color to highlight color
            if (hasFocus)
            {
                _targetColor = highlightColor;
            }
            // If this object lost focus, fade the object's color to its original color
            else
            {
                _targetColor = _originalColor;
            }
        }

        private void Start()
        {
            _image = GetComponent<Image>();
            if (_image == null)
            {
                Debug.LogError("Image component is missing!");
                return;
            }

            _originalColor = _image.color;
            _targetColor = _originalColor;
        }

        private void Update()
        {
            if (_image != null)
            {
                _image.color = Color.Lerp(_image.color, _targetColor, Time.deltaTime * (1 / animationTime));
            }
        }
    }
}
