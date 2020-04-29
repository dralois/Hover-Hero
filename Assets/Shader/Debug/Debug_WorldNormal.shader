//Debug shader to display world normals
Shader "Debug/WorldNormal"
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
				float3 norm : NORMAL;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 norm : NORMAL0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.norm = (UnityObjectToWorldNormal(v.norm) + 1.0f) * 0.5f;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float3 normal = normalize(i.norm);
				return fixed4(normal, 1.0f);
			}

			ENDHLSL
		}
	}
	Fallback "Standard"
}
