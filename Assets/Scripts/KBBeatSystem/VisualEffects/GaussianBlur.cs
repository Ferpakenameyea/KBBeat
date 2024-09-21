using UnityEngine;

[ExecuteInEditMode] // 编辑态可以查看脚本运行效果
[RequireComponent(typeof(Camera))] // 需要相机组件
internal class GaussianBlur : MonoBehaviour
{
    [Range(0, 4)]
    public int Iterations = 3; // 高斯模糊迭代次数
    [Range(0f, 3.0f)]
    public float BlurSpread = 0.6f; // 每次迭代纹理坐标偏移的速度
    [Range(1, 8)]
    public int DownSample = 2; // 降采样比率
    private Material material = null; // 材质

    protected virtual void Start()
    {
        material = new Material(Shader.Find("MyShader/GaussianBlur"));
        material.hideFlags = HideFlags.DontSave;
    }

    protected virtual void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material != null)
        {
            int rtW = src.width / DownSample; // 降采样的纹理宽度
            int rtH = src.height / DownSample; // 降采样的纹理高度
            RenderTexture buffer0 = RenderTexture.GetTemporary(rtW, rtH, 0);
            buffer0.filterMode = FilterMode.Bilinear; // 滤波模式设置为双线性
            Graphics.Blit(src, buffer0);
            for (int i = 0; i < Iterations; i++)
            {
                material.SetFloat("_BlurSize", 1.0f + i * BlurSpread); // 设置模糊尺寸(纹理坐标的偏移量)
                RenderTexture buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);
                Graphics.Blit(buffer0, buffer1, material, 0); // 渲染垂直的Pass
                RenderTexture.ReleaseTemporary(buffer0);
                buffer0 = buffer1;
                buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);
                Graphics.Blit(buffer0, buffer1, material, 1); // 渲染水平的Pass
                RenderTexture.ReleaseTemporary(buffer0);
                buffer0 = buffer1;
            }
            Graphics.Blit(buffer0, dest);
            RenderTexture.ReleaseTemporary(buffer0);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
