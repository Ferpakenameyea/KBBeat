using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal class MenuBall : MonoBehaviour
{
    public bool Switching { get; private set; } = false;
    [SerializeField] private GameObject showerPrefab;
    [SerializeField] private RawImage mainShower;
    [SerializeField] private float lerpSpeed;
    [SerializeField] private float switchTime;
    [SerializeField] private Transform mask;

    public enum BallSwitchDirection
    {
        Left, Right
    }

    internal void SwitchLeft(Texture newTexture)
    {
        if (Switching)
        {
            return;
        }
        this.Switching = true;
        this.StartCoroutine(SwitchCoroutine(BallSwitchDirection.Left, newTexture));
    }

    internal void SwitchRight(Texture newTexture)
    {
        if (Switching)
        {
            return;
        }

        this.Switching = true;
        this.StartCoroutine(SwitchCoroutine(BallSwitchDirection.Right, newTexture));
    }
    public IEnumerator SwitchCoroutine(BallSwitchDirection direction, Texture newShowerTexture)
    {
        var alignmentType = direction == BallSwitchDirection.Left ? SquareAlignmentUGUI.AlignmentType.Right : SquareAlignmentUGUI.AlignmentType.Left;
        var extraShower = Instantiate(this.showerPrefab, this.mask).GetComponent<RawImage>();
        extraShower.texture = newShowerTexture;
        var dest = mainShower.rectTransform.localPosition;
        var timer = 0f;
        dest.x += direction == BallSwitchDirection.Left ?
            -mainShower.rectTransform.rect.width :
            mainShower.rectTransform.rect.width;
        while (timer < this.switchTime)
        {
            mainShower.rectTransform.localPosition = Vector3.Lerp(
                mainShower.rectTransform.localPosition,
                dest,
                this.lerpSpeed * Time.deltaTime
                );
            SquareAlignmentUGUI.Align(extraShower, mainShower, alignmentType);
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(this.mainShower.gameObject);
        extraShower.rectTransform.localPosition = Vector3.zero;
        this.mainShower = extraShower;
        this.Switching = false;
        yield break;
    }

    public void SetCenterTexture(Texture texture)
    {
        this.mainShower.texture = texture;
    }
}

internal static class SquareAlignmentUGUI
{
    public static void Align(RawImage target, RawImage baseShower, AlignmentType type)
    {
        target.rectTransform.sizeDelta = baseShower.rectTransform.sizeDelta;
        target.rectTransform.localScale = baseShower.rectTransform.localScale;

        var rect = baseShower.rectTransform.rect;
        var pos = baseShower.rectTransform.localPosition;
        switch (type)
        {
            case AlignmentType.Top:
                pos.y += rect.height;
                break;
            case AlignmentType.Bottom:
                pos.y -= rect.height;
                break;
            case AlignmentType.Left:
                pos.x -= rect.width;
                break;
            case AlignmentType.Right:
                pos.x += rect.width;
                break;
        }

        target.rectTransform.localPosition = pos;
    }

    public enum AlignmentType
    {
        Top,
        Bottom,
        Left,
        Right
    }
}

public class SimpleEventEntryBuilder
{
    private EventTrigger.Entry entry = new();

    public SimpleEventEntryBuilder SetID(EventTriggerType type)
    {
        this.entry.eventID = type;
        return this;
    }

    public SimpleEventEntryBuilder SetCallBack(UnityAction<BaseEventData> callback)
    {
        entry.callback.AddListener(callback);
        return this;
    }

    public EventTrigger.Entry Get()
    {
        return this.entry;
    }
}