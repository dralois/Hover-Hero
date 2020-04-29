Shader "Custom/LavaStandartShader"
{
	Properties
	{
		[Header(Basics)]
		_MainTex ("Texture", 2D) = "white" {}

		[Header(Flow)]
		[NoScaleOffset] _FlowMap ("Flow Map", 2D) = "grey" {}
		_Speed ("Speed", Range(-1, 1)) = 0.2
		_RippleDir ("Ripple Direction", Vector) = (0,1,0,0)
		_WaveStrength ("Wave Strength", Float) = 1.0
		_WavePeriod ("Wave Amount", Float) = 1.0
		_Seed ("Time Offset", Float) = 0.0
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		Cull Off

		// Flow base pass
		Pass
		{
			Tags { "LightMode" = "Always" }

			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D	_MainTex;
			float4		_MainTex_ST;

			sampler2D	_FlowMap;
			fixed			_Speed;
			float4		_RippleDir;
			float			_WaveStrength;
			float			_WavePeriod;
			float			_Seed;

			struct v2f
			{
					float4 pos : SV_POSITION;
					fixed2 uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				// Determine wave strength for vertex
				float time = _Time.y + _Seed;
				float waveAddX = cos((v.vertex.y - time) * _WavePeriod) * _WaveStrength * _RippleDir.x;
				float waveAddY = cos((v.vertex.x - time) * _WavePeriod) * _WaveStrength * _RippleDir.y;
				float waveAddZ = cos((v.vertex.y - time) * _WavePeriod) * _WaveStrength * _RippleDir.z;
				// Calculate wave displaced position
				float4 wave = float4(v.vertex.x + waveAddX, v.vertex.y + waveAddY, v.vertex.z + waveAddZ, v.vertex.w);
				// Position into clip space and calculate scaled uv
				o.pos = UnityObjectToClipPos(wave);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			fixed4 frag(v2f v) : SV_Target
			{
				// Sample flowmap, convert back into range (-1,1)
				float2 flow = tex2D(_FlowMap, v.uv).rg * 2.0f - 1.0f;
				// Calculate phases (normal + offset)
				float phase0 = frac(_Time.y * _Speed + 0.5f);
				float phase1 = frac(_Time.y * _Speed);
				// Sample texture twice with flow added
				fixed4 col1 = tex2D(_MainTex, v.uv + flow * phase0);
				fixed4 col2 = tex2D(_MainTex, v.uv + flow * phase1);
				// Return interpolated color with alpha
				return fixed4(lerp(col1, col2, abs(0.5 - phase0) / 0.5).rgb, 1);
			}

			ENDHLSL
		}
		// Shadow caster pass
		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			HLSLPROGRAM

			#pragma multi_compile_shadowcaster

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f
			{
				V2F_SHADOW_CASTER;
			};

			// Vertex function for shadows
			v2f vert(appdata_base v)
			{
				v2f o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}

			// Fragment function for shadows
			fixed4 frag(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}

			ENDHLSL
		}
	}
	FallBack "Diffuse"
}
