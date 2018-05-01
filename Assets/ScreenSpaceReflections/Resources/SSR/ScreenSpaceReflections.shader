﻿Shader "Hidden/Bodhi Donselaar/ScreenSpaceReflections"
{
	Properties
	{
		_MainTex("Camera",2D)= "white" {}
		_Downscale("Downscale",int)=1
	}
	CGINCLUDE
		#pragma multi_compile S64 S48 S32 S16   
		#include "UnityCG.cginc"
		#if S16
		#define NUM_SAMPLES (8)
		#define NUM_SAMPLES_HALF (4)
		#endif
		#if S32
		#define NUM_SAMPLES (16)
		#define NUM_SAMPLES_HALF (8)
		#endif
		#if S48
		#define NUM_SAMPLES (24)
		#define NUM_SAMPLES_HALF (12)
		#endif
		#if S64
		#define NUM_SAMPLES (32)
		#define NUM_SAMPLES_HALF (16)
		#endif
		struct VertexDataCalculate
		{
			float2 uv : TEXCOORD0;
			float4 viewUV:TEXCOORD1;
			float4 noiseUV:TEXCOORD2;
			float4 vertex : SV_POSITION;
		};
		uniform sampler2D _Noise;
		float4 _Noise_TexelSize, _MainTex_TexelSize;
		int _Downscale;
		VertexDataCalculate VertexCalculate (float4 vertex : POSITION, float2 uv : TEXCOORD0)
		{
			VertexDataCalculate o;
			o.vertex = UnityObjectToClipPos(vertex);
			o.uv = uv;
			o.viewUV = float4(uv * 2 - 1,1,1);
			o.noiseUV = float4(_MainTex_TexelSize.zw*_Noise_TexelSize.xy*uv/_Downscale, 0, 0);
			return o;
		}
		uniform float4x4 _View2Screen;
		float4 screenuv;
		float2 world2screen(float3 wp)
		{
			return ((screenuv= mul(_View2Screen, wp - _WorldSpaceCameraPos)).xy / screenuv.w)*.5 + .5;
		}
		uniform sampler2D _WPOS;
		float2 rayuv;
		float3 ray(float3 wp)
		{
			return float3(rayuv = world2screen(wp), tex2Dlod(_WPOS, float4(rayuv,0,0)).a - distance(wp, _WorldSpaceCameraPos));
		}
		uniform sampler2D _MainTex, _CameraGBufferTexture2, _ssrMask;
		float3 result, dir, pos;
		uint j;
		float4 FragmentCalculate (VertexDataCalculate i) : SV_Target
		{
			float4 SSRValue = 0;
			float3 worldPosition=tex2D(_WPOS, i.uv).rgb;
			float distanceToCamera=distance(worldPosition,_WorldSpaceCameraPos);

			//todo: remove this if statement, this prevents skybox reflection
			if (distanceToCamera > 100)return 0;

			float sampleDistance=distanceToCamera/NUM_SAMPLES_HALF*1.5;
			float3 viewDir=normalize(worldPosition-_WorldSpaceCameraPos);
			dir =reflect(viewDir, tex2D(_CameraGBufferTexture2, i.uv).rgb * 2.0 - 1.0)*sampleDistance;//sample dis
			pos = dir*tex2Dlod(_Noise, i.noiseUV).r*4+worldPosition;
			float collision;
			for (j = 0; j < NUM_SAMPLES; ++j) //sample count
			{
				collision = 0;
				result = ray(pos);
				if (any(floor(result.xy)!=float2(0,0)))
				{
					collision = 0;
					SSRValue=float4(0,0,0,0);
					break; //out of screen 
				}
				if(result.z<0&&result.z>-(sampleDistance*2))
				{ 
					collision = tex2Dlod(_ssrMask, float4(result.xy, 0, 0)).r;
					SSRValue= float4(tex2Dlod(_MainTex, float4(result.xy,0,0)).rgb*1,1);
					break;
				}
				pos += dir;
			}
			return saturate( float4(SSRValue.rgb,collision));
		}
	ENDCG
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex VertexCalculate
			#pragma fragment FragmentCalculate
			#pragma target 3.0
			ENDCG
		}
	}
}
