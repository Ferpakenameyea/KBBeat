Shader "lvl1 assets/CRLuo/CRLuo_CFX_BlendAdd"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlendTex ("AddTexture", 2D) = "white" {}
        [HDR]_BlendCol ("AddColor", COLOR) = (1,1,1,1)
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
            float4 _BlendCol;
            sampler2D _BlendTex;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                 col.rgb += tex2D(_BlendTex, i.uv).rgb*_BlendCol.rgb;
                return col;
            }
            ENDCG
        }
    }
}