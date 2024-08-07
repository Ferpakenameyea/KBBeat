using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KBbeat.Debugger
{
    public class DebugShower : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textArea;
        [SerializeField] private string targetLevel;
        [SerializeField] private Button playButton;
        [SerializeField] private Slider slider;

        private void Awake()
        {
            if (!BuiltInSettings.Debug)
            {
                try
                {
                    this.enabled = false;
                    this.textArea.enabled = false;
                    this.playButton.enabled = false;
                    this.slider.enabled = false;
                }
                catch (Exception) { }
            }
        }

        private void Start()
        {
            this.textArea.text = Scores.zero.ToString();
            ScoreCounter.Counter.OnScoresChanged += (score, combo) =>
            {
                this.textArea.text = score.ToString() + $"\nCombo: {combo}";
            };
        }
    }
}