Shader "lvl1 assets/CRLuo/CRLuo_CFX_BrightnessRange"
{
    Properties
    {
		_MainTex("渲染图像", 2D) = "white" {}
        _BrightnessRange("亮度范围",float) = 0.5

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _BrightnessRange;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }



            float4 frag (v2f i) : SV_Target
            {
                //获取渲染图像
                float4 col = tex2D(_MainTex, i.uv);

                //用step函数 设置亮度接范围_BrightnessRange，限制图像RGB之和，RGB大于_BrightnessRange亮度为1，小于为0；
                float _range =step (_BrightnessRange,col.r+col.g+col.b);

                //使用0，1范围去掉图像暗部。
                col.rgb *= _range;

                return col;
            }
            ENDCG
        }
    }
}