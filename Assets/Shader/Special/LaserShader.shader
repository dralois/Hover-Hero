// Laser shader for Halo sword
Shader "Custom/LaserShader"
{
	Properties
	{
		[Header(Basics)]
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Main Texture", 2D) = "white" {}

		[Header(Glow)]
		_GlowFactor ("Glow Amount", Float) = 1
		_GlowColor ("Glow Color", Color) = (1,1,1,1)

		[Header(Outline)]
		_OutlineAlpha("Outline Alpha Value", Float) = 0.75
		_OutlineThickness ("Outline Thickness", Float) = 0.005
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		// Base pass
		Pass
		{
			Tags { "LightMode" = "Always" }

			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"

			float4		_Color;
			sampler2D	_MainTex;
			float4		_MainTex_ST;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 norm : NORMAL0;
				float2 uv_main : TEXCOORD0;
				float3 worldPos : POSITION1;
			};

			// Simple vertex shader
			v2f vert(appdata_tan v)
			{
				v2f o;
				// Position and normal
				o.pos = UnityObjectToClipPos(v.vertex);
				o.norm = UnityObjectToWorldNormal(v.normal);
				// Texture coordinates
				o.uv_main = TRANSFORM_TEX(v.texcoord, _MainTex);
				// Also save world position
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			// Simple lambert light
			fixed4 frag(v2f i) : SV_Target
			{
				// Sample occlusion and main textures
				fixed4 finalCol = tex2D(_MainTex, i.uv_main);
				// Renormalize world normal
				float3 normWorld = normalize(i.norm);
				// Light direction
				float3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos.xyz));
				// Diffuse light (lambert)
				fixed4 diffCol = max(0.05, dot(normWorld, lightDir)) * _LightColor0;
				// Return final color
				return fixed4(finalCol.rgb * diffCol.rgb, 1);
			}

			ENDHLSL
		}
		// Glow pass
		Pass
		{
			// Always rendered, without lighting
			Tags { "LightMode" = "Always" }
			ZTest LEqual
			Blend SrcAlpha One

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			float4 _Color;
			float4 _GlowColor;
			float _GlowFactor;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 norm : NORMAL;
				float3 viewDir : TEXCOORD0;
			};

			// Simple vertex shader
			v2f vert(appdata v)
			{
				v2f o;
				// Save position, normal and view direction in world space
				o.pos = UnityObjectToClipPos(v.vertex);
				o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
				o.norm = UnityObjectToWorldNormal(v.normal);
				return o;
			}

			// Rim glow light fragment shader
			fixed4 frag(v2f i) : SV_Target
			{
				// Calculate rim (make it slightly stronger)
				half rim = (1.0 - saturate(dot(i.viewDir, i.norm))) * 2.0;
				// Lerp alpha for rim light
				return float4((_GlowColor * _Color).rgb, lerp(0, 1, pow(rim, _GlowFactor)));
			}

			ENDCG
		}
		// Outline pass
		Pass
		{
			// Always rendered, without lighting
			Tags { "LightMode" = "Always" }
			Cull Front
			ZTest LEqual
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			float4 _Color;
			float4 _GlowColor;
			float _OutlineAlpha;
			float _OutlineThickness;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
			};

			// Outline offset vertex shader
			v2f vert(appdata v)
			{
				v2f o;
				// Calculate offset
				float3 off = float3(v.normal.xy * _OutlineThickness, 0);
				// Position into clipspace
				o.pos = UnityObjectToClipPos(v.vertex + off);
				return o;
			}

			// Simple fragment shader
			fixed4 frag(v2f i) : SV_Target
			{
				// Returns outline color
				return fixed4((_GlowColor * _Color).rgb, _OutlineAlpha);
			}

			ENDCG
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

			#pragma shader_feature _SHADOWS_CAST_ON

			struct v2f
			{
				#if _SHADOWS_CAST_ON
				V2F_SHADOW_CASTER;
				#else
				float4 pos : SV_POSITION;
				#endif
			};

			// Vertex function for shadows
			v2f vert(appdata_base v)
			{
				v2f o;
				// If enabled cast shadow
				#if _SHADOWS_CAST_ON
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				// Otherwise lightweight function
				#else
				o.pos = v.vertex;
				#endif
				return o;
			}

			// Fragment function for shadows
			fixed4 frag(v2f i) : SV_Target
			{
				// Same as for vertex function
				#if _SHADOWS_CAST_ON
				SHADOW_CASTER_FRAGMENT(i)
				#else
				return fixed4(0,0,0,0);
				#endif
			}

			ENDHLSL
		}
	}
	FallBack "Diffuse"
}
