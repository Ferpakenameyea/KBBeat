using KBBeat;
using KBBeat.Core;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
internal class FineWarner : MonoBehaviour
{
    [SerializeField] private float lerpSpeed = 30f;
    [SerializeField] private Color onWarningColor;
    private RawImage rawImage;
    private readonly Color idleColor = new Color(1, 1, 1, 0);

    private void Start()
    {
        this.rawImage = GetComponent<RawImage>();
        ScoreCounter.OnFine += OnFineAction;
        ScoreCounter.OnHeavy += OnFineAction;
        ScoreCounter.OnWeak += OnFineAction;
    }

    private void Update()
    {
        this.rawImage.color = Color.Lerp(
            this.rawImage.color,
            this.idleColor,
            this.lerpSpeed * Time.deltaTime);
    }

    private void OnDestroy()
    {
        ScoreCounter.OnFine -= OnFineAction;
        ScoreCounter.OnHeavy -= OnFineAction;
        ScoreCounter.OnWeak -= OnFineAction;
    }

    private void OnFineAction()
    {
        this.rawImage.color = this.onWarningColor;
    }
}
