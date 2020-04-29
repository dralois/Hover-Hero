Shader "Custom/ParticleShader"
{
	Properties
	{
		_Color ("Particle color", Color) = (1.0,0,0,0)
		_MainTex ("Particle texture", 2D) = "white" {}
		_AlphaOffset ("Alpha offset", Float) = 0.5
		_ParticleSize ("Size", Float) = 1.0
	}
	SubShader
	{
		Tags {"RenderType"="Transparent" "Queue"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		Zwrite Off
		Cull Off

		// Billboard particle pass
		Pass
		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct Particle
			{
				float3 Position;
				float3 Velocity;
				float TimeToLive;
			};

			//Buffer containing all the particles
			StructuredBuffer<Particle> particleBuffer;

			float4 _Color;
			sampler2D _MainTex;
			float _AlphaOffset;
			float _ParticleSize;

			struct v2g
			{
				float4 pos : POSITION;
				float alpha : TEXCOORD1;
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float alpha : TEXCOORD1;
			};

			v2g vert(uint instance_id : SV_InstanceID)
			{
				v2g o;
				// Position to world space
				o.pos =  mul(unity_ObjectToWorld, float4(particleBuffer[instance_id].Position, 1.0f));
				// Save TTL of particle
				o.alpha = particleBuffer[instance_id].TimeToLive;
				return o;
			}

			// Billboard geometry shader
			[maxvertexcount(4)]
			void geom(point v2g IN[1], inout TriangleStream<g2f> tristream)
			{
				// Calculate look and up direction
				float3 up = float3(0, 1, 0);
				float3 look = float3(0,0,0);
				look.xz = normalize((_WorldSpaceCameraPos - IN[0].pos).xz);
				// Right is given by cross product
				float3 right = cross(up, look);
				// Calculate size of extrusion
				float halfS = 0.5f * _ParticleSize;
				// Calculate the four billboard vertices
				float4 v[4];
				v[0] = float4(IN[0].pos + halfS * right - halfS * up, 1.0f);
				v[1] = float4(IN[0].pos + halfS * right + halfS * up, 1.0f);
				v[2] = float4(IN[0].pos - halfS * right - halfS * up, 1.0f);
				v[3] = float4(IN[0].pos - halfS * right + halfS * up, 1.0f);
				// View projection is needed since position is already in world space
				float4x4 vp = UNITY_MATRIX_VP;
				// Save the vertices
				g2f pIn;
				pIn.alpha = IN[0].alpha;
				pIn.pos = mul(vp, v[0]);
				pIn.uv = float2(1.0f, 0.0f);
				tristream.Append(pIn);
				pIn.pos =  mul(vp, v[1]);
				pIn.uv = float2(1.0f, 1.0f);
				tristream.Append(pIn);
				pIn.pos =  mul(vp, v[2]);
				pIn.uv = float2(0.0f, 0.0f);
				tristream.Append(pIn);
				pIn.pos =  mul(vp, v[3]);
				pIn.uv = float2(0.0f, 1.0f);
				tristream.Append(pIn);
			}

			fixed4 frag(g2f i) : SV_Target
			{
				fixed4 tex = _Color * tex2D(_MainTex, i.uv);
				// Set TTL as alpha
				tex.a = _AlphaOffset * i.alpha * tex2D(_MainTex, i.uv).a;
				return tex;
			}

			ENDHLSL
		}
	}
}
