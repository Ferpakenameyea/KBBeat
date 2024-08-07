Shader "Custom/OUTLINE"
{
    Properties
    {
        _MainColor("主颜色",Color)=(0,0,0,1)

        _OutlineColor("描边颜色",Color)=(1,1,1,1)
        _OutlineArea("描边区域",Range(0,0.2))=2
        _OutlineStrength("描边强度",Range(0,1))=1

    }
    SubShader
    {
        Tags{"Queue"="Overlay"}
        //模型扩充渲染
        Pass
        {
            ZWrite off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float4 _MainColor;
            fixed4 _OutlineColor;
            fixed _OutlineArea;
            fixed _OutlineStrength;

            struct a2v
            {
                float4 vertex:POSITION;
                float3 normal:NORMAL;
            };

            struct v2f
            {
                float4 pos:SV_POSITION;
            };

      
            v2f vert(a2v v)
            {
                v2f o;
                v.vertex.xyz+=v.vertex.xyz*_OutlineArea;
                o.pos = UnityObjectToClipPos(v.vertex); 
                return o;
            }
            
            fixed4 frag():SV_Target
            {                                
                return _OutlineColor*_OutlineStrength;
            }
            ENDCG
        }
        //正常漫反射光照
        Pass
        {
            Tags
            {
                "LightMode"="ForwardBase"
            }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            fixed4 _MainColor;

            struct a2v
            {
                float4 vertex:POSITION;
                float3 normal:NORMAL;
            };

            struct v2f
            {
                float4 pos:SV_POSITION;
                float3 worldNormal :COLOR;
            };

            v2f vert(a2v v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);

                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                return o;
            }

            fixed4 frag(v2f i):SV_Target
            {
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb;

                fixed3 worldNormal = normalize(i.worldNormal);

                fixed3 worldLight = normalize(_WorldSpaceLightPos0.xyz);

                fixed3 diffuse = _LightColor0.rgb * _MainColor.rgb *
                    (dot(worldLight, worldNormal) * 0.5 + 0.5);
                return fixed4(ambient + diffuse, 1);
            }
            ENDCG
        }

    }
}