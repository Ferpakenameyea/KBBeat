using System;
using KBBeat.Audio;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CustomColliderImage))]
internal class MenuButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    private Vector3 basePosition;
    [SerializeField] private Vector3 offsetOnEnter;
    private Vector3 target;
    [SerializeField] private float lerpSpeed;
    public Action OnClick { get; set; } = null;

    private void Start()
    {
        this.basePosition = this.transform.localPosition;
        this.target = this.basePosition;
    }

    private void Update()
    {
        this.transform.localPosition = Vector3.Lerp(
                this.transform.localPosition,
                this.target,
                this.lerpSpeed * Time.deltaTime
            );
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SoundManager.Instance.PlayClip("switch", Channel.Effect);
        this.OnClick?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.Instance.PlayClip("select", Channel.Effect);
        this.target = this.basePosition + offsetOnEnter;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SoundManager.Instance.PlayClip("unselect", Channel.Effect);
        this.target = this.basePosition;
    }
}
