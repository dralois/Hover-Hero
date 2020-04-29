//Debug shader to display vertex color
Shader "Debug/VertexColor"
{
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
				float4 col : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 col : COLOR0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.col = v.col;
				return o;
			}

			float4 frag (v2f i) : SV_Target
			{
				return i.col;
			}

			ENDHLSL
		}
	}
	Fallback "Standard"
}
