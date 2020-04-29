Shader "Custom/FireBallShader"
{
	Properties
	{
		[Header(Basics)]
		_MainTex ("Texture", 2D) = "white" {}
		[Toggle(_HADOUKEN_MODE_ON)] _HadoukenMode("Hadouken Mode", Float) = 0.0
		[Header(Noise)]
		_ConvolutionScale ("Time Scale", Range(0, 1)) = 0.5
		_TurbulenceOctaves ("Octave Count", Int) = 10
		_TurbulenceLevel ("Turbulence", Float) = 10.0
		_TurbulenceScale ("Spike Scale", Range(0.0, 1.0)) = 0.65
		_PulseScale ("Pulse Scale", Range(0.0, 1.0)) = 0.65
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		// Fireball pass
		Pass
		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#pragma shader_feature _HADOUKEN_MODE_ON

			#include "UnityCG.cginc"
			#include "NoisePerlin.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _ConvolutionScale;
			int _TurbulenceOctaves;
			float _TurbulenceLevel;
			float _TurbulenceScale;
			float _PulseScale;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			// Turbulence function
			float turbulence(float3 pos)
			{
				// Guarantees that noise is always positive
				float turbulence = -0.5;
				float maxOct = _TurbulenceOctaves * 1.0;
				// Add octaves of perlin noise
				for(float octave = 1.0; octave <= maxOct; octave++)
				{
					// Calculate octave power of 2
					float pow2 = pow(2.0, octave);
					// Add turbulence level
					turbulence += abs(pnoise(float3(pow2 * pos), float3(maxOct, maxOct, maxOct)) / pow2);
				}
				// Return final noise
				return turbulence;
			}

			// Extruding vertex shader
			v2f vert (appdata_base v)
			{
				v2f o;
				// Calculate noise using normal and time
				float noise = -1.0 * turbulence(_TurbulenceScale * v.normal + _Time.y * _ConvolutionScale);
				// Use scaled time for pulse
				float pulse = _PulseScale * pnoise(float3(_Time.y, _Time.y, _Time.y), float3(_TurbulenceLevel, _TurbulenceLevel, _TurbulenceLevel));
				// Extrude along normal
				float3 newPos = v.vertex.xyz + v.normal * (noise + pulse);
				// Save clip position and texcoords
				o.pos = UnityObjectToClipPos(float4(newPos,1.0));
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			// Animated texture fragment shader
			fixed4 frag (v2f i) : SV_Target
			{
				// Sample texture using sintime and costime for animation
				fixed4 tex = tex2D(_MainTex, (i.uv + float2(_SinTime.w * _ConvolutionScale, _CosTime.w * _ConvolutionScale)));
				// Return fireball or hadouken color scheme
				#if _HADOUKEN_MODE_ON
				return fixed4(fixed3(1,1,1) - tex.rgb, 1);
				#else
				return fixed4(tex.rgb, 1);
				#endif
			}

			ENDHLSL
		}
	}
}
