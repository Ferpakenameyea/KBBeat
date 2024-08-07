using System.Collections;
using UnityEngine;


namespace KBbeat
{
    public class FireworkParticle : MonoBehaviour
    {
        private Vector3 dest;
        private MeshRenderer meshRenderer;
        public static float RunTime = 0.2f;
        public static float FadeTime = 1f;
        public static float DropSpeed = 0.01f;

        private void Start()
        {
            this.meshRenderer = GetComponent<MeshRenderer>();
        }

        public void Fire(Vector3 dest, Vector3 startFrom)
        {
            this.transform.position = startFrom;
            var offset = new Vector3(
                    Random.Range(-0.01f, 0.01f),
                    Random.Range(-0.01f, 0.01f),
                    Random.Range(-0.01f, 0.01f)
                );
            this.dest = dest + offset;

            StartCoroutine(FireCoroutine());
        }

        private IEnumerator FireCoroutine()
        {
            var dis = this.dest - this.transform.position;
            var a = -(2 * dis) / (RunTime * RunTime);
            var lastFrameSpeed = Vector3.zero;
            var speed = -a * RunTime;
            while (
                lastFrameSpeed == Vector3.zero ||
                Vector3.Dot(lastFrameSpeed, speed) > 0
                )
            {
                this.transform.position += speed * Time.deltaTime;
                speed += a * Time.deltaTime;

                lastFrameSpeed = speed;
                yield return null;
            }

            var timer = 0f;
            var shrinkSpeed = -this.transform.localScale / FadeTime;
            var fadeSpeed = -this.meshRenderer.material.color.a / FadeTime;
            while (timer < FadeTime)
            {
                this.transform.localScale += shrinkSpeed * Time.deltaTime;
                this.transform.position += new Vector3(0, -DropSpeed, 0) * Time.deltaTime;

                var color = this.meshRenderer.material.color;
                color.a -= Time.deltaTime * fadeSpeed;
                this.meshRenderer.material.color = color;

                timer += Time.deltaTime;
                yield return null;
            }

            this.Release();
            yield break;
        }

        private void Release()
        {
            Firework.particlePool.Release(this.gameObject);
        }
    }
}