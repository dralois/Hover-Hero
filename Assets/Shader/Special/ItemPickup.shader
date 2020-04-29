Shader "Custom/ItemPickup"
{
	Properties
	{
		[Header(Basics)]
		_Color ("Color", Color) = (1,1,1,1)
		_Alpha("Alpha value", float) = 0.5
		_MainTex("Main waterlike texture", 2D) = "white"{}
		[Header(Noise)]
		_NoiseTex("Noise Texture", 2D) = "white" {}
		_NoiseTex2("Second noise texture", 2D) = "white" {}
		_NoiseFactor("Noise color multiplier", float) = 2
		[Header(Movement)]
		_ShiftSpeedTex1("Shift Speed of first noise Texture", float) = 0.5
		_ShiftSpeedTex2("Shift Speed of second noise Texture", float) = 2
		_MainTexRotationSpeed("Main texture rotation speed", float) = 0.5
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		Blend SrcAlpha One

		Pass
		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;
			sampler2D _NoiseTex2;
			float4 _NoiseTex2_ST;
			fixed4 _Color;
			float _Alpha;
			float _NoiseFactor;
			float _ShiftSpeedTex1;
			float _ShiftSpeedTex2;
			float _MainTexRotationSpeed;

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uvNoise1 : TEXCOORD0;
				float2 uvNoise2 : TEXCOORD1;
				float2 uv : TEXCOORD2;
			};

			v2f vert (appdata_base v)
			{
				v2f o;
				_NoiseTex_ST.z = _Time * _ShiftSpeedTex1;
				_NoiseTex2_ST.w = _Time * _ShiftSpeedTex2;
				_MainTex_ST.zw = _Time * _MainTexRotationSpeed;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uvNoise1 = TRANSFORM_TEX(v.texcoord, _NoiseTex);
				o.uvNoise2 = TRANSFORM_TEX(v.texcoord, _NoiseTex2);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 noise1 = tex2D(_NoiseTex, i.uvNoise1);
				fixed4 noise2 = tex2D(_NoiseTex2, i.uvNoise2);
				fixed4 waterlike = tex2D(_MainTex, i.uv);
				fixed4 color = _Color;

				color *= waterlike;
				if(noise1.r > 0.3 && noise2.r > 0.3){
					color *= _NoiseFactor;
				}
				color.a = _Alpha;

				return color;
			}

			ENDHLSL
		}
	}
}