using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class Glow : MonoBehaviour
{
    //特效Shader变量
    private Shader shader_BrightnessRange;
    //特效材质球变量
    private Material material_BrightnessRange;
    //特效Shader变量
    private Shader shader_Blur;
    //特效材质球变量
    private Material material_Blur;
    //特效Shader变量
    private Shader shader_Add;
    //特效材质球变量
    private Material material_Add;
    
    /*
    //HDR 渐变条
    [GradientUsage(true)]
    public Gradient gradient;
    */
    
    //HDR颜色
    [ColorUsageAttribute(true,true)]
    public Color GlowColor = Color.white;

    [Range(0f, 10f)]
    public float BrightnessRange = 1f;

    [Range(0f, 1f)]
    public float BlurRadius = 1f;
    [Range(1, 100)]
    public int LowPixel =1;
    [Range(0f, 10f)]
    public int BlurConvolution = 3;

    //程序启动处理部分
    private void Start() 
    {
        //依据材质球路径和名称获取shader
        shader_BrightnessRange = Shader.Find("lvl1 assets/CRLuo/CRLuo_CFX_BrightnessRange");
        //依据Shader创建材质球
        material_BrightnessRange = new Material(shader_BrightnessRange);

        //依据材质球路径和名称获取shader
        shader_Blur = Shader.Find("lvl1 assets/CRLuo/CRLuo_CFX_Blur");
        //依据Shader创建材质球
        material_Blur = new Material(shader_Blur);

        //依据材质球路径和名称获取shader
        shader_Add = Shader.Find("lvl1 assets/CRLuo/CRLuo_CFX_BlendAdd");
        //依据Shader创建材质球
        material_Add = new Material(shader_Add);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

        if (material_BrightnessRange)
        {
            //创建两个暂存渲染图的临时变量。
            RenderTexture temp1 = RenderTexture.GetTemporary(source.width/ LowPixel, source.height/ LowPixel, 0, source.format);
            RenderTexture temp2 = RenderTexture.GetTemporary(source.width/ LowPixel, source.height/ LowPixel, 0, source.format);

            //获取辉光亮度范围
            material_BrightnessRange.SetFloat("_BrightnessRange", BrightnessRange);
            Graphics.Blit(source, temp1, material_BrightnessRange);

            //循环模糊辉光
            material_Blur.SetFloat("_BlurRadius", BlurRadius);
            for (int i=0;i< BlurConvolution; i++)
            {
                //经过两次Shader渲染，把数据传回临时变量 temp1
                Graphics.Blit(temp1, temp2, material_Blur);
                Graphics.Blit(temp2, temp1, material_Blur);
            }

            //混合辉光
            material_Add.SetColor("_BlendCol", GlowColor); 
            material_Add.SetTexture("_BlendTex", temp1); 
            Graphics.Blit(source, destination, material_Add);

            //释放申请的两个临时变量 
            RenderTexture.ReleaseTemporary(temp1);
            RenderTexture.ReleaseTemporary(temp2);
        }
    }
}