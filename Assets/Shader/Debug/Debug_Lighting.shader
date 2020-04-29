//Debug shader to display lighting information
Shader "Debug/Lighting"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,0,0,1)
		_Ambient ("Ambient", Range (0, 1)) = 0.25
		//Specular color and exponent are added
		_SpecCol ("Specular Color", Color) = (1,1,1,1) 
		_SpecExp ("Specular Exponent", Float) = 10
	}
	SubShader
	{
		Tags { "LightMode" = "ForwardBase" }

		Pass
		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			//We need the world position of the vertex
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
				float4 vertexWorld : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float _Ambient;
			float _SpecExp;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//Vertex world position can be calculated by multiplying the object position with the ObjectToWorld matrix
				o.vertexWorld = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				//Normal, view direction and light direction vectors are needed for lighting calculations
				float3 normalDirection = normalize(i.worldNormal);
				float3 viewDirection = normalize(UnityWorldSpaceViewDir(i.vertexWorld));
				float3 lightDirection = normalize(UnityWorldSpaceLightDir(i.vertexWorld));

				//Same Lambert diffuse term as in the previous example
				float nl = max(_Ambient, dot(normalDirection, _WorldSpaceLightPos0.xyz));
				float4 diffuseTerm = nl * _LightColor0;

				//Phong specular lighting
				//Reflection direction can easily be calculated by using the reflect function, see the cg documentation for more details
				float3 reflectionDirection = -lightDirection - 2.0f * normalDirection * dot(normalDirection, -lightDirection);
				//Dot product needed for the final specular term
				float3 specularDot = max(0.0, dot(viewDirection, reflectionDirection));
				//Specular value is written into each channel (rgb). Exponent influences the size of the specular highlight
				float3 specular = pow(specularDot, _SpecExp); 
				//Final specular color
				float4 specularTerm = float4(specular, 1) * _LightColor0; 

				//Diffuse and specular lighting are additive. Texture base color is only applied to diffuse, not specular!
				return diffuseTerm + specularTerm;
			}

			ENDHLSL
		}
	}
	Fallback "Standard"
}
