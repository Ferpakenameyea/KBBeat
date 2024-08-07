using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DelaySliding : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;

    private void Start()
    {
        this.slider.value = Configurator.Instance.Config.customOffsetSeconds * 1000f;
        FlushText();
        slider.onValueChanged.AddListener((value) => {
            FlushText();
            Configurator.Instance.Config.customOffsetSeconds = value / 1000f;
        });

        increaseButton.onClick.AddListener(() =>
        {
            slider.value += 1;
        });

        decreaseButton.onClick.AddListener(() =>
        {
            slider.value -= 1;
        });
    }

    private void FlushText()
    {
        if (valueText != null)
        {
            valueText.text = $"{(slider.value > 0 ? '+' : '-')}{Math.Abs(slider.value)}ms";
        }
    }
}
