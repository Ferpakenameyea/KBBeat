Shader "KBBeat PostEffects/levels/level1/glitch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MulColor ("Mul Color", Color) = (1, 1, 1, 1)

        _A ("A", Float) = 1
        _Omega ("Omega", Float) = 1
        _T ("T", Float) = 0
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            float _A;
            float _Omega;
            float _T;
            sampler2D _MainTex;
            fixed4 _MulColor;

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

            float CalSine(float x) {
                return _A * sin(_Omega * x + _T);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                uv.y += CalSine(i.uv.x);
                fixed4 color = tex2D(_MainTex, uv);

                return color * _MulColor;
            }
            ENDCG
        }
    }
}

