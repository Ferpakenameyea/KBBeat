Shader "lvl1 assets/fading flower"
{
    Properties
    {
        _MainColor("MainColor", Color) = (0, 0, 0, 1)
        _NoiseColor("NoiseColor", Color) = (1, 1, 1, 1)
        _Threshold("Noise threshold", Float) = 0.5
        _Speed("Speed", Float) = 1
        _VectorWeight("Vector weight", Vector) = (1, 0, 0)
        _FadingColor("FadingColor", Color) = (1, 1, 1, 0)
        _FadingDistance("Distance", Float) = 200
        _FadingSpeed("Fading Speed", Float) = 0.01
    }

    SubShader
    {
        Pass
        {
            Cull Back
            
            CGPROGRAM


            #pragma fragment frag
            #pragma vertex vert

            float _Threshold;
            float _Speed;

            fixed4 _MainColor;
            fixed4 _NoiseColor;
            float3 _VectorWeight;

            fixed4 _FadingColor;
            float _FadingDistance;
            float _FadingSpeed;

            inline float randomNoise(float2 seed)
            {
                return frac(sin(0.0001 * dot(seed * floor(_Time.y * _Speed), float2(17.13, 3.71))) * 43758.5453123);
            }

            float4 vert(float4 vertex : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(vertex);
            }

            fixed4 frag(float4 vertex : SV_POSITION) : SV_TARGET
            {
                fixed4 c;

                if (randomNoise(133 + 
                    vertex.x * _VectorWeight.x + 
                    vertex.y * _VectorWeight.y + 
                    vertex.z * _VectorWeight.z) > _Threshold) 
                {
                    c = _MainColor;
                }  
                else 
                {
                    c = _NoiseColor;
                }

                float distance = length(vertex.xyz);
                if (distance < _FadingDistance) {
                    return c;
                }
                else 
                {
                    float weight = 1 - exp(-(distance - _FadingDistance) / _FadingSpeed);
                    return c * (1 - weight) + _FadingColor * weight;
                }
            }

            ENDCG
        }
    }
}