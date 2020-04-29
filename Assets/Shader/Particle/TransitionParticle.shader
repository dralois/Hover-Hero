Shader "Unlit/TransitionParticle"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
c1("",Color) = (0,0,1,1)
c2("",Color) = (1,0,0,1)

    }
    SubShader
    {
        //Tags { "RenderType"="Opaque" }
        //LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma enable_d3d11_debug_symbols

            #include "UnityCG.cginc"

			struct Particle
			{
				float3 position;
				float3 velocity;
				float3 destination;
				float3 origin;
				float4 sourceColor;
				float4 destColor;
				float4 currentColor;
			};

			StructuredBuffer<Particle> particleBuffer;

            struct v2f
            {
                float4 position : SV_POSITION;
				float4 color : COLOR;
            };

            float4 c1;
			float4 c2;

            v2f vert (uint instance_id : SV_InstanceID)
            {
                v2f o;
                o.position = UnityObjectToClipPos(float4(particleBuffer[instance_id].position,1));
				o.color = particleBuffer[instance_id].currentColor;
				//o.color = float4(1,0,1,1);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
               return i.color;
            }
            ENDHLSL
        }
    }
}
