using System;
using UnityEngine;

namespace KBBeat.Core
{
    public class ExperimentTrack : DefaultTrack
    {
        [SerializeField]
        private MeshRenderer clickEffect;

        [SerializeField]
        private Color onIdleColor;

        [SerializeField]
        private Color onClickColor;

        [SerializeField]
        private float lerpSpeed;

        private void ColorChangeOnClick()
        {
            clickEffect.material.color = this.onClickColor;
        }

        private new void Start()
        {
            base.Start();
            try
            {
                this.parent.OnPressDownGroup += ColorChangeOnClick;
            }
            catch (NullReferenceException)
            {
                Debug.LogWarning("Failed to subscribe parent event!");
            }
        }

        private void OnDestroy()
        {
            if (this.parent != null)
            {
                this.parent.OnPressDownGroup -= ColorChangeOnClick;
            }
        }

        private void Update()
        {
            clickEffect.material.color = Color.Lerp(
                clickEffect.material.color,
                onIdleColor,
                lerpSpeed * Time.deltaTime
            );
        }
    }
}
