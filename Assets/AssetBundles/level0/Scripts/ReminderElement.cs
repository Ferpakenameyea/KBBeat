using UnityEngine;

namespace AssetBundles.levels.level0
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ReminderElement : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;

        [SerializeField] private KeyCode listeningKey;
        [SerializeField] private Color idleColor;
        [SerializeField] private Color hitColor;
        [SerializeField] private float lerp;

        private void Start()
        {
            this.spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(listeningKey))
            {
                this.spriteRenderer.color = hitColor;
            }
            this.spriteRenderer.color = Color.Lerp(this.spriteRenderer.color, idleColor, lerp * Time.deltaTime);
        }
    }
}

