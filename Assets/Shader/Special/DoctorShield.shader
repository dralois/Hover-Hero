Shader "Custom/DoctorShield"
{
	Properties
	{
		[Header(Basics)]
		_Color("Color", Color) = (0.7,0.3,0,1)
		_Alpha("Brightness", float) = 0.5
		_MainTex ("Main Texture", 2D) = "white" {}
		[NoScaleOffset] _NoiseTex("Noise Texture", 2D) = "white" {}
		[Header(Glow)]
		_AlphaOffset("The alpha offset of glow", float) = 0.5
		_AlphaFactor("The alpha factor of glow", float) = 0.5
		[Header(Rotation)]
		_BlueRotationSpeed ("Blue Rotation Speed", float) = 2.0
		_GreenRotationSpeed ("Green Rotation Speed", float) = 2.0
		_GreenGlow("Green color gets multiplied by this", float) = 2.0
		[Header(Center Bloom)]
		_CenterHighlightTex("Center highlight texture", 2D) = "white"{}
		_CenterHighlightFactor("Center highlight factor", float) = 2.0
	}
	SubShader
	{
		Tags { "Rendertype"="Transparent" "Queue"="Transparent" }
		Blend SrcAlpha One
		Cull Off
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 blueRotatingUV : TEXCOORD1;
				float2 greenRotatingUV : TEXCOORD2;
			};

			sampler2D _MainTex;
			sampler2D _NoiseTex;
			sampler2D _CenterHighlightTex;
			float4 _MainTex_ST;
			float4 _Color;
			float _Alpha;
			float _AlphaOffset;
			float _AlphaFactor;
			float _BlueRotationSpeed;
			float _GreenRotationSpeed;
			float _GreenGlow;
			float _CenterHighlightFactor;

			v2f vert(appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				// Pivot
				float2 pivot = float2(0.5, 0.5);
				// Rotate blue uv
				float cosAngle = cos(_Time * _BlueRotationSpeed);
				float sinAngle = sin(_Time * _BlueRotationSpeed);
				float2x2 rot = float2x2(cosAngle, -sinAngle, sinAngle, cosAngle);
				// Rotation considering pivot
				float2 uv = v.texcoord - pivot;
				o.blueRotatingUV = mul(rot, uv);
				o.blueRotatingUV += pivot;
				// Rotation green uv
				cosAngle = cos(_Time * _GreenRotationSpeed);
				sinAngle = sin(_Time * _GreenRotationSpeed);
				rot = float2x2(cosAngle, -sinAngle, sinAngle, cosAngle);
				// Rotation considering pivot
				uv = v.texcoord - pivot;
				o.greenRotatingUV = mul(rot, uv);
				o.greenRotatingUV += pivot;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 texCol = tex2D(_MainTex, i.uv);
				fixed4 blueRotatingCol = tex2D(_MainTex,i.blueRotatingUV);
				fixed4 greenRotatingCol = tex2D(_MainTex, i.greenRotatingUV);
				fixed4 noiseTexCol = tex2D(_NoiseTex, i.uv);
				fixed4 highlightNoise = tex2D(_CenterHighlightTex, i.uv);

				fixed4 color = _Color;
				if(texCol.r >= 0.2)
				{
						//Red
						color.a = sin(_Time * 40 + 5) * _AlphaFactor + _AlphaOffset;
						color.a *= noiseTexCol;
						color *= _Alpha;
				}
				else if(greenRotatingCol.g >= 0.2)
				{
						//Green
						color.a = sin(_Time * 13 + 2) * _AlphaFactor + _AlphaOffset;
						color *= _GreenGlow;
				}
				else if(blueRotatingCol.b >= 0.2)
				{
						//Blue
						color.a = sin(_Time * 35 + 15) * _AlphaFactor + _AlphaOffset;
						color.a *= noiseTexCol;
						color *= _Alpha;
				}
				else
				{
						color = fixed4(0.0,0.0,0.0,0.0);
				}

				if(highlightNoise.r > 0.1f)
				{
					color *= _CenterHighlightFactor;
				}

				return color;
			}
			ENDCG
		}
	}
}
