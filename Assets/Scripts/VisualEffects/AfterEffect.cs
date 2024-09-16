using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class AfterEffect : MonoBehaviour 
{
    [SerializeField] private Material material;

    private void OnRenderImage(RenderTexture src, RenderTexture dest) 
    {

        if (material == null) 
        {
            Graphics.Blit(src, dest);
            return;
        }

        Graphics.Blit(src, dest, material);
    }
}