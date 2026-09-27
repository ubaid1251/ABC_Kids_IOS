// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/BufferLayerShader" {
	Properties{
		_MainTex("Base (Alpha8)", 2D) = "red" {}
		_FillTex("Filling gradient", 2D) = "white" {}
		_FillValue("fill value", Range(0,1)) = 1
		_Mouse("mouse", Vector) = (.1, .5, .5, 1)
	}

		SubShader{
			Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			Pass {
				 CGPROGRAM //Shader Start, Vertex Shader named vert, Fragment shader named frag
				 #pragma vertex vert
				 #pragma fragment frag
				 #include "UnityCG.cginc"
				//Link properties to the shader			
				sampler2D _MainTex;
				sampler2D _FillTex;
				float     _FillValue;
				float4     _Mouse;

				struct appdata
				{

					float4 vertex : POSITION;
					float4 texcoord : TEXCOORD0;
					float4 texcoord1 : TEXCOORD1;
				};

				struct v2f
				{
					 float4  pos : SV_POSITION;
					 float2  uv : TEXCOORD0;
					 float2  uv2: TEXCOORD1;
				};

				v2f vert(appdata v)
				{
					 v2f o;
					 o.pos = UnityObjectToClipPos(v.vertex);
					 o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
					 o.uv2 = MultiplyUV(UNITY_MATRIX_TEXTURE1, v.texcoord1);
					 return o;
				}


				half4 frag(v2f i) : COLOR
				{
					float4 texcol = tex2D(_MainTex, i.uv);
					//float4 gradientValue = tex2D (_FillTex, i.uv2);			
				//	float4 alfa = gradientValue[3] + _FillValue < 1 ? 0 : 1;
				//  float4 alfa = clamp( (_FillValue - gradientValue[3])*100  , 0 , 1);
				//	float4 result =texcol * alfa;
					float mixValue = distance(i.uv2, _Mouse.xy);
					//	_Mouse = float4(0.8, 0.8,0.1,0.1);
						if (mixValue < _FillValue) {
							return float4(texcol.r, texcol.g, texcol.b, texcol.a);
						}
						else {
							return float4(texcol.r, texcol.g, texcol.b, 0);
						}
					}

					 ENDCG //Shader End
				}
		}
}