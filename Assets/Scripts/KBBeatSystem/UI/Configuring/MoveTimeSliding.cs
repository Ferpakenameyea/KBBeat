using System;
using System.Collections;
using System.Collections.Generic;
using KBBeat.Configuring;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

internal class MoveTimeSliding : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;

    private void Start()
    {
        this.slider.value = Configurator.Instance.Config.noteMoveTime;
        FlushText();
        slider.onValueChanged.AddListener((value) => {
            FlushText();
            Configurator.Instance.Config.noteMoveTime = value;
        });

        increaseButton.onClick.AddListener(() =>
        {
            slider.value += 0.1f;
        });

        decreaseButton.onClick.AddListener(() =>
        {
            slider.value -= 0.1f;
        });
    }

    private void FlushText()
    {
        if (valueText != null)
        {
            valueText.text = string.Format("{0:F1}s", slider.value);
        }
    }
}
