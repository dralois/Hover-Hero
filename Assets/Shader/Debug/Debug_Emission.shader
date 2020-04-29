//Debug shader to display the emission texture
Shader "Debug/Emission"
{
	Properties
	{
		_EmissionMap ("Main Texture", 2D) = "White" {}
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

			sampler2D _EmissionMap;
			float4 _EmissionMap_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _EmissionMap);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 tex = tex2D(_EmissionMap, i.uv);
				return tex;
			}

			ENDHLSL
		}
	}
	Fallback "Standard"
}
