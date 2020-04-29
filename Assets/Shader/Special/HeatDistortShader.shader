Shader "Custom/HeatDistort"
{
	Properties
	{
		_NoiseFilter("Noise + Filter", 2D) = "white" {}
		_Strength("Distort Strength", float) = 1.0
		_Speed("Distort Speed", float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue" = "Transparent+2" }
		ZWrite Off
		// Grab rendered stuff into texture
		GrabPass
		{
			"_BackgroundTexture"
		}
		// Distortion pass
		Pass
		{
			ZTest Always
			Cull Off

			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			// Noise and filter have to be combined into one texture
			sampler2D	_BackgroundTexture;
			sampler2D	_NoiseFilter;
			float			_Strength;
			float			_Speed;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 grabPos : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				// Position into clip space
				o.pos = UnityObjectToClipPos(v.vertex);
				// Compute grab position
				o.grabPos = ComputeGrabScreenPos(o.pos);
				// Read noise + filter map
				float2 noisefilter = tex2Dlod(_NoiseFilter, v.texcoord).rg;
				// Vary garb position randomly
				o.grabPos.x += cos(noisefilter.r * _Time.y * _Speed) * noisefilter.g * _Strength;
				o.grabPos.y += sin(noisefilter.r * _Time.y * _Speed) * noisefilter.g * _Strength;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// Output distorted picture
				return tex2Dproj(_BackgroundTexture, i.grabPos);
			}

			ENDHLSL
		}
	}
}
