Shader "KBBeat PostEffects/levels/level1/reverse circles"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Frequency ("Circle Frequency", float) = 10.0
        
        _MulColor ("MulColor", Color) = (1, 1, 1, 1)
        _MulFactor ("MulFactor", float) = 1
        _Threshold ("Threshold", float) = 0.6

        _Theta ("Theta", float) = 0
        _DistanceLimit ("DistanceLimit", float) = 0
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

            float _Frequency;
            fixed4 _MulColor;
            float _Threshold;
            float _MulFactor;
            sampler2D _MainTex;
            float _Theta;
            float _DistanceLimit;

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

                if (dist < _DistanceLimit) {
                    return tex2D(_MainTex, i.uv);
                }

                float circles = sin(sqrt(dist) * _Frequency);

                fixed4 color = tex2D(_MainTex, i.uv);

                if (circles > _Threshold) {
                    float2 centerToPixel = pixelPos - center;

                    const float cosTheta = cos(_Theta);
                    const float sinTheta = sin(_Theta);

                    float x = centerToPixel.x;
                    float y = centerToPixel.y;

                    centerToPixel = float2(
                        x * cosTheta - y * sinTheta,
                        x * sinTheta + y * cosTheta
                    );

                    float2 newUV = (center + centerToPixel) / screenSize;

                    return tex2D(_MainTex, newUV);
                }
                else
                {
                    return tex2D(_MainTex, i.uv);
                }
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
