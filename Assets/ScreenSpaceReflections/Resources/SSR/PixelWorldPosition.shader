Shader "Bodhi/PostFX/PixelWorldPosition"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D_float _CameraDepthTexture;
			uniform float4x4 _CAMERA_XYZMATRIX;
			float4 xyzMatrix;
			float4 GetWorldPositionFromDepth(float2 uv_depth)
			{
				return (xyzMatrix = mul(_CAMERA_XYZMATRIX, float4(uv_depth*2.0 - 1.0, SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv_depth), 1.0))) / xyzMatrix.w;
			}
			float3 XYZ;
			float4 frag(v2f i) : SV_Target
			{
				return float4(XYZ = GetWorldPositionFromDepth(i.uv.xy).xyz,distance(XYZ, _WorldSpaceCameraPos));
			}
			ENDCG
		}
	}
}
