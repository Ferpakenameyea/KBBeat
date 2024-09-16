Shader "KBBeat PostEffects/levels/level1/glitch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Amplitude ("Amplitude", Range(-1, 0)) = -0.15
        _Amount ("Amount", Range(-5, 5)) = 0.5
        _BlockSize ("Block Size", Range(0, 1)) = 0.05
        _Speed ("Speed", Range(0, 100)) = 10
        _BlockPow ("Block Size Pow", Vector) = (3, 3, 0, 0)

        _MulColor ("Mul Color", Color) = (1, 1, 1, 1)
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Amplitude;
            float _Amount;
            float _BlockSize;
            float _Speed;
            float4 _BlockPow;

            fixed4 _MulColor;

            inline float randomNoise(float2 seed)
            {
                return frac(sin(dot(seed * floor(_Time.y * _Speed), float2(17.13, 3.71))) * 43758.5453123);
            }

            float Noise()
            {
                float _TimeX = _Time.y;
                float splitAmout = (1.0 + sin(_TimeX * 6.0)) * 0.5;
                splitAmout *= 1.0 + sin(_TimeX * 16.0) * 0.5;
                splitAmout *= 1.0 + sin(_TimeX * 19.0) * 0.5;
                splitAmout *= 1.0 + sin(_TimeX * 27.0) * 0.5;
                splitAmout = pow(splitAmout, _Amplitude);
                splitAmout *= (0.05 * _Amount);
                return splitAmout;
            }

            float ImageBlockIntensity(v2f i)
            {
                //float2 block = randomNoise(floor(i.uv * _BlockSize * 100));
                //return (block.x);
                float2 size = lerp(1, _MainTex_TexelSize.xy, 1 - _BlockSize);
                size = floor((i.uv) / size);
                float noiseBlock = randomNoise(size);
                float displaceNoise = pow(noiseBlock.x, _BlockPow.x) * pow(noiseBlock.x, _BlockPow.y);
                return displaceNoise + 1.0;
            }

            half4 SplitRGB(v2f i)
            {
                float splitAmout = Noise() * ImageBlockIntensity(i);
                half3 finalColor;
                finalColor.r = tex2D(_MainTex, fixed2(i.uv.x + splitAmout * randomNoise(13.0), i.uv.y)).r;
                finalColor.g = tex2D(_MainTex, i.uv).g;
                finalColor.b = tex2D(_MainTex, fixed2(i.uv.x - splitAmout * randomNoise(123.0), i.uv.y)).b;

                return half4(finalColor, 1.0);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return SplitRGB(i) * _MulColor;
            }
            ENDCG
        }
    }
}

