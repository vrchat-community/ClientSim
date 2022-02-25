using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace VRC.SDK3.ClientSim.Tests
{
    [AddComponentMenu("")]
    public class ClientSimTestUIHandler : ClientSimBehaviour
    {
        public Button button;
        public Slider slider;
        public Transform sliderHandle;
        public Transform sliderLeftAnchor;
        public Transform sliderRightAnchor;
        
        public Action onButtonAction;
        public Action<float> onSliderAction;
        
        [PublicAPI]
        public void OnButton()
        {
            onButtonAction?.Invoke();
        }
        
        [PublicAPI]
        public void OnSlider(float value)
        {
            onSliderAction?.Invoke(value);
        }
    }
}