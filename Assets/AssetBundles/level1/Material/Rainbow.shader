Shader "lvl1 assets/rainbow"
{
    Properties
    {
        _Weights("Weights", Vector) = (1, 1, 1, 1)
        _A("Amplitude", Float) = 1
        _Omega("Frequency", Float) = 1
        _Phi("Phase", Float) = 0
    }

    SubShader
    {
        Pass
        {
            Cull Back
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float _A;
            float _Omega;
            float _Phi;
            float4 _Weights;

            struct v2f {
                float4 pos : SV_POSITION;
                float4 worldPos : POSITION1;
            };

            // HSV to RGB conversion
            fixed4 HSVtoRGB(float h, float s, float v) {
                float r, g, b;
                float h_scaled = h * 6.0;
                int i = (int)floor(h_scaled);
                float f = h_scaled - i;

                float p = v * (1.0 - s);
                float q = v * (1.0 - s * f);
                float t = v * (1.0 - s * (1.0 - f));

                switch(i % 6) {
                    case 0: r = v; g = t; b = p; break;
                    case 1: r = q; g = v; b = p; break;
                    case 2: r = p; g = v; b = t; break;
                    case 3: r = p; g = q; b = v; break;
                    case 4: r = t; g = p; b = v; break;
                    case 5: r = v; g = p; b = q; break;
                }

                return fixed4(r, g, b, 1.0);
            }

            // Sine function
            float CalSine(float x) {
                return _A * sin(_Omega * x + _Phi);
            }

            // Vertex shader
            v2f vert(float4 vertex : POSITION) {
                v2f o;
                float4 worldPos = mul(UNITY_MATRIX_M, vertex);  // Transform vertex to world space
                o.worldPos = worldPos;  // Store world position
                o.pos = UnityObjectToClipPos(vertex);  // Transform to clip space
                return o;
            }

            // Fragment shader
            fixed4 frag(v2f args) : SV_TARGET {
                float h = dot(args.worldPos, _Weights);
                h += CalSine(h);
                h = frac(h);
                fixed4 color = HSVtoRGB(h, 0.76, 0.86);
                
                const fixed4 white = fixed4(1, 1, 1, 1);
                const fixed4 black = fixed4(0, 0, 0, 0);

                if (color.r > 0.6) {
                    color = lerp(white, black, color.r - 0.1);
                }
                else {
                    
                }

                return color;
            }

            ENDCG
        }
    }
}
