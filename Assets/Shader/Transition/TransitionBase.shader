Shader "Hidden/TransitionBase"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
		_LastTex ("Last Texture", 2D) = "black" {}
		[NoScaleOffset] _NoiseTex ("Noise Texture", 2D) = "white"{}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma enable_d3d11_debug_symbols

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
			sampler2D _LastTex;
			sampler2D _NoiseTex;
			float percentage; //0 full Main, 1 full Last
	

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 resultColor;
                fixed4 colorMain = tex2D(_MainTex, i.uv);
				fixed4 colorLast = tex2D(_LastTex, i.uv);
				float4 colorNoise = tex2D(_NoiseTex, i.uv);

				float noise = colorNoise.r;

				if(noise < percentage){
					resultColor = colorLast;
				}
				else{
					resultColor = colorMain;
				}
                return resultColor;
            }
            ENDCG
        }
    }
}
