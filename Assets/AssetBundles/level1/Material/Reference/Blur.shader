Shader "lvl1 assets/CRLuo/CRLuo_CFX_Blur"
{
    Properties
    {
		_MainTex("渲染图像", 2D) = "white" {}
		_BlurRadius("模糊强度",float) = 0.5

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
			float _BlurRadius;
			sampler2D  _CameraDepthTexture;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            //9层模糊（图像纹理，图像UV，模糊半径）
            float4 BlurLayer9(sampler2D ColorTex,float2 UV,float Radius)
            {
                //原始数据
                float4 outTex = 0;

                //中心点
                outTex +=tex2D(ColorTex, UV+float2(0,0)*Radius)*0.2;
    
                //右、左、上、下
                outTex +=tex2D(ColorTex, UV+float2(1,0)*Radius)*0.1;
                outTex +=tex2D(ColorTex, UV+float2(-1,0)*Radius)*0.1;
                outTex +=tex2D(ColorTex, UV+float2(0,1)*Radius)*0.1;
                outTex +=tex2D(ColorTex, UV+float2(0,-1)*Radius)*0.1;

                //右上、左下、左上、右下
                outTex +=tex2D(ColorTex, UV+float2(0.7071,0.7071)*Radius)*0.1;
                outTex +=tex2D(ColorTex, UV+float2(-0.7071,-0.7071)*Radius)*0.1;
                outTex +=tex2D(ColorTex, UV+float2(-0.7071,0.7071)*Radius)*0.1;
                outTex +=tex2D(ColorTex, UV+float2(0.7071,-0.7071)*Radius)*0.1;

                return outTex;
            }

            //5层模糊（图像纹理，图像UV，模糊半径）
            float4 BlurLayer5(sampler2D ColorTex,float2 UV,float Radius)
			{
			  float4 outTex = 0;
              //中心图像
				outTex +=tex2D(ColorTex, UV+float2(0,0)*Radius)*0.2;

                //右、左、上、下
                outTex +=tex2D(ColorTex, UV+float2(1,0)*Radius)*0.2;
                outTex +=tex2D(ColorTex, UV+float2(-1,0)*Radius)*0.2;
                outTex +=tex2D(ColorTex, UV+float2(0,1)*Radius)*0.2;
                outTex +=tex2D(ColorTex, UV+float2(0,-1)*Radius)*0.2;

			 return outTex;
			}


            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = 0;
				col = BlurLayer5(_MainTex,i.uv,_BlurRadius*0.1);
                return col;
            }
            ENDCG
        }
    }
}