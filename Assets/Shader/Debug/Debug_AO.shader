//Debug shader to display the ambient occlusion texture
Shader "Debug/AO"
{
	Properties
	{
		_OcclusionMap ("Main Texture", 2D) = "White" {}
	}
	SubShader
	{
		Pass
		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _OcclusionMap;
			float4 _OcclusionMap_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _OcclusionMap);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 tex = tex2D(_OcclusionMap, i.uv);
				return tex;
			}

			ENDHLSL
		}
	}
	Fallback "Standard"
}
