// Variant of standart shader, dissolves based on depth
Shader "Custom/DissolveShader"
{
	Properties
	{
		[Header(Dissolve)]
		_DissolveFrom ("Dissolve Begin", Float) = 10.0
		_DissolveOffset ("Dissolve Offset", Float) = -2.0

		[Header(Basics)]
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Main Texure", 2D) = "white" {}
		[NoScaleOffset] _Occlusion ("Occlusion Map", 2D) = "white" {}
		[NoScaleOffset] [Normal] _NormalMap ("Normal Map", 2D) = "bump" {}
		[Toggle(_ADDITIONAL_LIGHTS_ON)] _AdditionalLights("Enable Additional Lights", Float) = 0.0

		[Header(General Light)]
		_AmbientInt ("Ambient Intensity", Range (0, 1)) = 0.25
		_DiffuseInt ("Diffuse Intensity", Range (0, 1)) = 1.0

		[Header(Specular Light)]
		[Toggle] _Specular("Specular Enabled", Float) = 0.0
		_SpecularPow ("Specular Exponent", Float) = 1.0
		_SpecularCol ("Specular Color", Color) = (1,1,1,1)
		[NoScaleOffset] _SpecularMap ("Specular Map", 2D) = "white" {}

		[Header(Emission)]
		_EmissionInt ("Emission Intensity", Range (0, 1)) = 0.0
		_EmissionCol ("Emission Color", Color) = (1,1,1,1)
		[NoScaleOffset] _EmissionMap ("Emission Map", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Back

		// Base pass for first light
		Pass
		{
			Name "Base"
			Tags { "LightMode" = "ForwardBase" }

			HLSLPROGRAM

			#pragma multi_compile_fwdbase

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "UnityLightingCommon.cginc"

			#pragma shader_feature _SPECULAR_ON

			float			_DissolveFrom;
			float			_DissolveOffset;

			float4		_Color;
			sampler2D	_MainTex;
			float4		_MainTex_ST;
			sampler2D	_Occlusion;
			sampler2D	_NormalMap;

			float			_AmbientInt;
			float4		_AmbientCol;

			float			_DiffuseInt;
			float4		_DiffuseCol;

			float			_SpecularPow;
			float4		_SpecularCol;
			sampler2D _SpecularMap;

			float			_EmissionInt;
			float4		_EmissionCol;
			sampler2D _EmissionMap;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 norm : NORMAL0;
				float3 tan : TANGENT0;
				float3 binorm : BINORMAL0;
				float2 uv : TEXCOORD0;
				float3 worldPos : POSITION1;
				LIGHTING_COORDS(1, 2)
			};

			// Simple vertex shader
			v2f vert(appdata_tan v)
			{
				v2f o;
				// Position and normal
				o.pos = UnityObjectToClipPos(v.vertex);
				o.norm = UnityObjectToWorldNormal(v.normal);
				// Save tangent in world space
				o.tan = UnityObjectToWorldDir(v.tangent);
				// Calculate and save binormal
				float sign = v.tangent.w * unity_WorldTransformParams.w;
				o.binorm = cross(o.norm, o.tan) * sign;
				// Scale and offset uv
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				// Also save world position
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				// Transfer light information
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				return o;
			}

			// Blinn phong lighting
			// Supports normal, specular, emission and occlusion maps
			fixed4 frag(v2f i) : SV_Target
			{
				// Sample occlusion and main textures
				fixed4 finalCol = tex2D(_MainTex, i.uv);
				fixed4 aoAmount = tex2D(_Occlusion, i.uv);
				// Apply occlusion
				finalCol *= aoAmount;

				// Sample normal map
				float3 normMap = UnpackNormal(tex2D(_NormalMap, i.uv));
				// Calculate world normal (using the normal map if one is given)
				float3 normWorld = normalize((normMap.x * i.tan) + (normMap.y * i.binorm) + (normMap.z * i.norm));

				// Light, view and reflection direction
				float3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos.xyz));
				float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos.xyz));
				float3 refDir = reflect(-lightDir, normWorld);

				// Ambient light
				fixed4 ambCol = _AmbientInt * _Color;
				// Diffuse light (lambert)
				fixed4 diffCol = max(0.0, dot(normWorld, lightDir)) * _DiffuseInt * _Color * _LightColor0;
				// Add both to light color
				fixed4 lightCol = ambCol + diffCol;

				// If we want specular light (if enabled on material)
				#if _SPECULAR_ON
				// Specular light + amount from texture
				fixed4 specAmount = tex2D(_SpecularMap, i.uv);
				fixed4 specCol = pow(max(0.0, dot(refDir, viewDir)), _SpecularPow) * specAmount * _SpecularCol * _LightColor0;
				// Add to light color
				lightCol += specCol;
				#endif

				// Determine light attenuation
				float atten = LIGHT_ATTENUATION(i);
				// Multiply light onto current final color
				finalCol.rgb *= lightCol.rgb * atten;

				// Calculate emission
				fixed4 emCol = tex2D(_EmissionMap, i.uv) * _EmissionCol * _EmissionInt;
				// Calculate camera distance and alpha
				float dist = distance(i.worldPos.xz, _WorldSpaceCameraPos.xz + (viewDir.xz * _DissolveOffset));
				float alpha = (dist < _DissolveFrom ? saturate(lerp(0, 1, dist / _DissolveFrom)) : 1) * finalCol.a;
				// If close enough slowly fade to transparent
				return fixed4(finalCol.rgb + emCol.rgb, alpha);
			}

			ENDHLSL
		}
		// Pass for additional lights
		Pass
		{
			Name "Add"
			Tags { "LightMode" = "ForwardAdd" }
			Blend One One
			ZWrite Off

			HLSLPROGRAM

			#pragma multi_compile_fwdadd

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "UnityLightingCommon.cginc"

			#pragma shader_feature _SPECULAR_ON
			#pragma shader_feature _ADDITIONAL_LIGHTS_ON

			float			_DissolveFrom;
			float			_DissolveOffset;

			float4		_Color;
			sampler2D	_MainTex;
			float4		_MainTex_ST;
			sampler2D	_Occlusion;
			sampler2D	_NormalMap;

			float			_AmbientInt;
			float4		_AmbientCol;

			float			_DiffuseInt;
			float4		_DiffuseCol;

			float			_SpecularPow;
			float4		_SpecularCol;
			sampler2D _SpecularMap;

			float			_EmissionInt;
			float4		_EmissionCol;
			sampler2D _EmissionMap;

			struct v2f
			{
				float4 pos : SV_POSITION;
				#if _ADDITIONAL_LIGHTS_ON
				float3 norm : NORMAL0;
				float3 tan : TANGENT0;
				float3 binorm : BINORMAL0;
				float2 uv : TEXCOORD0;
				float3 worldPos : POSITION1;
				LIGHTING_COORDS(1, 2)
				#endif
			};

			// Simple vertex shader
			v2f vert(appdata_tan v)
			{
				v2f o;
				// If additional lights are enabled
				#if _ADDITIONAL_LIGHTS_ON
				// Position and normal
				o.pos = UnityObjectToClipPos(v.vertex);
				o.norm = UnityObjectToWorldNormal(v.normal);
				// Save tangent in world space
				o.tan = UnityObjectToWorldDir(v.tangent);
				// Calculate and save binormal
				float sign = v.tangent.w * unity_WorldTransformParams.w;
				o.binorm = cross(o.norm, o.tan) * sign;
				// Texture coordinates
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				// Also save world position
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				// Transfer light information
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				// Lightweight alternative
				#else
				o.pos = v.vertex;
				#endif
				return o;
			}

			// Blinn phong lighting
			// Supports normal, specular, emission and occlusion maps
			fixed4 frag(v2f i) : SV_Target
			{
				// Same as for vertex function
				#if _ADDITIONAL_LIGHTS_ON
				// Sample occlusion and main textures
				fixed4 finalCol = tex2D(_MainTex, i.uv);
				fixed4 aoAmount = tex2D(_Occlusion, i.uv);
				// Apply occlusion
				finalCol *= aoAmount;

				// Sample normal map
				float3 normMap = UnpackNormal(tex2D(_NormalMap, i.uv));
				// Calculate world normal (using the normal map if one is given)
				float3 normWorld = normalize((normMap.x * i.tan) + (normMap.y * i.binorm) + (normMap.z * i.norm));

				// Light, view and reflection direction
				float3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos.xyz));
				float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos.xyz));

				// Ambient light
				fixed4 ambCol = _AmbientInt * _Color;
				// Diffuse light (lambert)
				fixed4 diffCol = max(0.0, dot(normWorld, lightDir)) * _DiffuseInt * _Color * _LightColor0;
				// Add both to light color
				fixed4 lightCol = ambCol + diffCol;

				// If we want specular light (if enabled on material)
				#if _SPECULAR_ON
				float3 halfDir = normalize(lightDir + viewDir);
				// Specular light + amount from texture
				fixed4 specAmount = tex2D(_SpecularMap, i.uv);
				fixed4 specCol = pow(max(0.0, dot(normWorld, halfDir)), _SpecularPow) * specAmount * _SpecularCol * _LightColor0;
				// Add to light color
				lightCol += specCol;
				#endif

				// Determine light attenuation
				float atten = LIGHT_ATTENUATION(i);
				// Multiply light onto current final color
				finalCol.rgb *= lightCol.rgb * atten;

				// Add emission and return color
				fixed4 emCol = tex2D(_EmissionMap, i.uv) * _EmissionCol * _EmissionInt;
				// Calculate camera distance and alpha
				float dist = distance(i.worldPos.xz, _WorldSpaceCameraPos.xz + (viewDir.xz * _DissolveOffset));
				float alpha = (dist < _DissolveFrom ? saturate(lerp(0, 1, dist / _DissolveFrom)) : 1) * finalCol.a;
				// If close enough slowly fade to transparent
				return fixed4(finalCol.rgb + emCol.rgb, alpha);
				#else
				return fixed4(0,0,0,0);
				#endif
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
