Shader "Bodhi/PostFX/Blur"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Direction("Direction",Vector)=(1,0,0,0)
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
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
			float4 _MainTex_TexelSize, _Direction;
			float4 frag (v2f i) : SV_Target
			{
				float4 c = 0;
				c += tex2D(_MainTex, i.uv + (_MainTex_TexelSize.xy*_Direction*.75))*126;
				c += tex2D(_MainTex, i.uv + (_MainTex_TexelSize.xy*_Direction*2.25))*84;
				c += tex2D(_MainTex, i.uv + (_MainTex_TexelSize.xy*_Direction*3.75))*36;
				c += tex2D(_MainTex, i.uv + (_MainTex_TexelSize.xy*_Direction*5.25))*9;
				c += tex2D(_MainTex, i.uv + (_MainTex_TexelSize.xy*_Direction*6.75))*1;
				c += tex2D(_MainTex, i.uv - (_MainTex_TexelSize.xy*_Direction*.75))*126;
				c += tex2D(_MainTex, i.uv - (_MainTex_TexelSize.xy*_Direction*2.25))*84;
				c += tex2D(_MainTex, i.uv - (_MainTex_TexelSize.xy*_Direction*3.75))*36;
				c += tex2D(_MainTex, i.uv - (_MainTex_TexelSize.xy*_Direction*5.25))*9;
				c += tex2D(_MainTex, i.uv - (_MainTex_TexelSize.xy*_Direction*6.75))*1;
				return c/512;
			}
			ENDCG
		}
	}
}
