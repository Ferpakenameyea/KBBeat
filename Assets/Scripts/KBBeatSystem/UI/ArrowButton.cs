using KBBeat;
using KBBeat.Audio;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
internal class ArrowButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [SerializeField] private float lerpSpeed;
    [SerializeField] private Color idleColor;
    [SerializeField] private Color enterColor;
    [SerializeField] private ArrowType type;
    private Color targetColor;
    private RawImage rawImage;
    public LevelSelector selector;

    private void Start()
    {
        this.rawImage = GetComponent<RawImage>();
        this.targetColor = idleColor;
        this.rawImage.color = this.targetColor;
    }

    private void Update()
    {
        this.rawImage.color = Color.Lerp(
            this.rawImage.color,
            this.targetColor,
            this.lerpSpeed * Time.deltaTime
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.Instance.PlayClip("select", Channel.Effect);
        this.targetColor = enterColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (this.type)
        {
            case ArrowType.Left:
                this.selector.SwitchLeft();
                break;
            case ArrowType.Right:
                this.selector.SwitchRight();
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SoundManager.Instance.PlayClip("unselect", Channel.Effect);
        this.targetColor = idleColor;
    }

    [Serializable]
    private enum ArrowType
    {
        Left, Right
    }
}
