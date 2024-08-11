using KBBeat;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
internal class EnterGameButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    private Image imageRenderer;
    [SerializeField] private float targetAlpha = 255f;
    [SerializeField] private float lerpSpeed;
    [SerializeField] private LevelSelector selector;
    public bool reactable = true;

    private void Start()
    {
        this.reactable = true;
        this.imageRenderer = GetComponent<Image>();
    }

    private void Update()
    {
        var color = Color.white;
        color.a = Mathf.Lerp(this.imageRenderer.color.a, this.targetAlpha, this.lerpSpeed * Time.deltaTime);
        this.imageRenderer.color = color;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!this.reactable)
        {
            return;
        }
        if (selector.Ready)
        {
            this.selector.SelectAndRun();
            this.reactable = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.targetAlpha = 0.5f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.targetAlpha = 1f;
    }
}
