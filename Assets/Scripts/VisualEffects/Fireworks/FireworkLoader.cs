using System.Collections.Generic;
using UnityEngine;
namespace KBbeat
{
    public class FireworkLoader : MonoBehaviour
    {
        [SerializeField] private List<Sprite> fireworkShapes;
        [SerializeField] private float pixelSize;
        [SerializeField] private GameObject particlePrefab;
        [SerializeField] private GameObject launcherPrefab;

        private void Awake()
        {
            Firework.shapes.Clear();
            Firework.PixelSize = pixelSize;
            Firework.launcherPrefab = launcherPrefab;


            Firework.particlePool = DynamicObjectPoolFactory.Get("firework_particles", particlePrefab, 50, 20);

            foreach (var shape in fireworkShapes)
            {
                var width = (float)shape.texture.width;
                var height = (float)shape.texture.height;

                var offset = new Vector3(-(width / 2) * pixelSize, -(height / 2) * pixelSize);

                var list = new List<Vector3>();
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (shape.texture.GetPixel(x, y).a > 0.8f)
                        {
                            list.Add(new Vector3(x, y) * pixelSize + offset);
                        }
                    }
                }
                var pattern = new FireworkShape(list);
                Firework.shapes.Add(shape.texture.name, pattern);
            }

            Debug.Log($"Successfully loaded {Firework.shapes.Count} firework patterns");

            Destroy(gameObject);
        }
    }
}
