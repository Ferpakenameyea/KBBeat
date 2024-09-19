Shader "KBBeat PostEffects/levels/level1/circles"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Frequency ("Circle Frequency", float) = 10.0
        
        _MulColor ("MulColor", Color) = (1, 1, 1, 1)
        _MulFactor ("MulFactor", float) = 1
        _Threshold ("Threshold", float) = 0.6
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // 属性声明
            float _Frequency;
            fixed4 _MulColor;
            float _Threshold;
            float _MulFactor;
            sampler2D _MainTex;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 screenSize = float2(_ScreenParams.x, _ScreenParams.y);

                float2 pixelPos = i.uv * screenSize;

                float2 center = screenSize * 0.5;

                float dist = distance(pixelPos, center);

                float circles = sin(sqrt(dist) * _Frequency);

                fixed4 color = tex2D(_MainTex, i.uv);

                if (circles > _Threshold) {
                    return (color * _MulColor * _MulFactor);
                }

                float lerpValue = (circles - (-1)) / 2;

                return lerp(color, (color * _MulColor * _MulFactor), lerpValue);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
