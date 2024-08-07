using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KBbeat
{
    public class Firework : MonoBehaviour
    {
        public static float PixelSize { get; internal set; }
        internal static Dictionary<string, FireworkShape> shapes = new();
        internal static DynamicObjectPool particlePool = null;
        internal static GameObject launcherPrefab = null;

        public static void Launch(string pattern, Vector3 launchPosition, float height, float time)
        {
            var firework = Instantiate(launcherPrefab).AddComponent<Firework>();
            firework._Launch_Implement(pattern, launchPosition, height, time);
        }

        private void _Launch_Implement(string pattern, Vector3 launchPosition, float height, float time) => StartCoroutine(LaunchCoroutine(pattern, launchPosition, height, time));

        private IEnumerator LaunchCoroutine(string pattern, Vector3 launchPosition, float height, float time)
        {
            var exist = shapes.TryGetValue(pattern, out var shape);
            if (!exist)
            {
                Debug.LogWarning($"pattern {pattern} not exist, firework will be disabled");
                yield break;
            }

            var dest = launchPosition + new Vector3(0, height, 0);
            var g = new Vector3(0, -(2 * height) / (time * time), 0);
            var speed = -g * time;
            while (speed.y > 0)
            {
                this.transform.position += speed * Time.deltaTime;
                speed += Time.deltaTime * g;
                yield return null;
            }


            this.Explode(shape);
            yield break;
        }

        private void Explode(FireworkShape shape)
        {
            foreach (var pos in shape.shape)
            {
                var obj = particlePool.Get(true);
                FireworkParticle particle = null;
                if ((particle = obj.GetComponent<FireworkParticle>()) == null)
                {
                    particle = obj.AddComponent<FireworkParticle>();
                }

                particle.Fire(this.transform.position + pos, this.transform.position);
            }
        }
    }

    internal class FireworkShape
    {
        public readonly List<Vector3> shape;
        internal FireworkShape(List<Vector3> shape)
        {
            this.shape = shape;
        }
    }
}