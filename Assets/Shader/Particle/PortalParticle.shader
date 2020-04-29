Shader "Custom/PortalParticle"
{
	Properties
	{
		_ColorOne ("Particle color 1", Color) = (1.0,0,0,0)
		_ColorTwo ("Particle color 2", Color) = (1.0,0,0,0)
		_MainTex ("Particle texture", 2D) = "white" {}
		_AlphaOffset ("Alpha offset", Float) = 0.5
		_ParticleSize ("Size", Float) = 1.0
	}
	SubShader
	{
		Tags {"RenderType"="Transparent" "Queue"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Cull Off

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#include "UnityCG.cginc"

			float4 _ColorOne;
			float4 _ColorTwo;
			sampler2D _MainTex;
			float _AlphaOffset;
			float _ParticleSize;

			struct Particle
			{
				float3 Position;
				float3 ResetPos;
				float3 Velocities;
				float3 CurrDir;
				float3 CrossDir;
				float TimeToLive;
				float ResetTTL;
				float RandCol;
				float RandZ;
			};

			//Buffer containing all the particles
			StructuredBuffer<Particle> particleBuffer;

			struct v2g
			{
				float4 pos : POSITION;
				float3 lenDir : NORMAL1;
				float3 outDir : NORMAL2;
				float alpha : TEXCOORD1;
				float rand : TEXCOORD2;
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float alpha : TEXCOORD1;
				float rand : TEXCOORD2;
			};

			v2g vert(uint instance_id : SV_InstanceID)
			{
				v2g o;
				// Position to world space
				o.pos =  mul(unity_ObjectToWorld, float4(particleBuffer[instance_id].Position, 1.0f));
				// Save current directions (gets longer the further out)
				o.lenDir = particleBuffer[instance_id].CurrDir * (particleBuffer[instance_id].ResetTTL - particleBuffer[instance_id].TimeToLive);
				o.outDir = particleBuffer[instance_id].CrossDir * _ParticleSize;
				// Save TTL as alpha and the random value
				o.alpha = particleBuffer[instance_id].TimeToLive;
				o.rand = particleBuffer[instance_id].RandCol;
				return o;
			}

			// Billboard geometry shader
			[maxvertexcount(4)]
				void geom(point v2g IN[1], inout TriangleStream<g2f> tristream)
			{
				float4 v[4];
				// Add length and outwards to create a billboard
				v[0] = float4(IN[0].pos + IN[0].lenDir + IN[0].outDir, 1.0f);
				v[1] = float4(IN[0].pos - IN[0].lenDir + IN[0].outDir, 1.0f);
				v[2] = float4(IN[0].pos + IN[0].lenDir - IN[0].outDir, 1.0f);
				v[3] = float4(IN[0].pos - IN[0].lenDir - IN[0].outDir, 1.0f);
				// Fetch view projection matrix
				float4x4 vp = UNITY_MATRIX_VP;
				// Create vertices
				g2f pIn;
				pIn.alpha = IN[0].alpha;
				pIn.rand = IN[0].rand;
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

			float4 frag(g2f i) : COLOR
			{
				// Lerp between colors
				float4 color = lerp(_ColorOne, _ColorTwo, i.rand) * tex2D(_MainTex, i.uv);
				// Set TTL as alpha
				color.a = _AlphaOffset * i.alpha * tex2D(_MainTex, i.uv).a;
				return color;
			}

			ENDCG
		}
	}
}
