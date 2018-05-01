using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class ScreenSpaceReflections : MonoBehaviour
{
	[Range(1,8)]
	public int downSample = 2;
	[Range(0, 8)]
	public int blurAmount = 1;
	public bool showReflectionsOnly = false;
	private Camera cam;

	//doesn't change, being static avoids doubles in memory with multiple cameras
	private Material ssrMaterial, pixelWorldPosMaterial, combineMaterial, blurMaterial;
	private static Texture2D ssrMask, noise; 

	private void OnEnable()
	{
		if (ssrMaterial == null) ssrMaterial = new Material(Resources.Load<Shader>("SSR/ScreenSpaceReflections"));
		if (pixelWorldPosMaterial == null) pixelWorldPosMaterial = new Material(Resources.Load<Shader>("SSR/PixelWorldPosition"));
		if (combineMaterial == null) combineMaterial = new Material(Resources.Load<Shader>("SSR/Combine"));
		if (blurMaterial==null)blurMaterial = new Material(Resources.Load<Shader>("SSR/Blur"));
		if (ssrMask == null) Shader.SetGlobalTexture("_ssrMask", ssrMask = Resources.Load<Texture2D>("SSR/ssrMaskSoft"));
		if (noise == null) Shader.SetGlobalTexture("_Noise", ssrMask = Resources.Load<Texture2D>("SSR/Noise"));
		if (cam==null) cam = GetComponent<Camera>();
	}
	public void OnRenderImage(RenderTexture src, RenderTexture des)
	{
		RenderTextureFormat format = cam.allowHDR ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
		if (cam.actualRenderingPath != RenderingPath.DeferredShading)
		{
			Debug.LogWarning("SSR: Camera must be in deferred rendering mode");
			Graphics.Blit(src, des);
			return;
		}

		//Create temporary buffers
		downSample = Mathf.Clamp(downSample, 1, 8);
		RenderTexture tempWorldPos = RenderTexture.GetTemporary(src.width, src.height , 0, RenderTextureFormat.ARGBFloat);
		RenderTexture tempA = RenderTexture.GetTemporary(src.width / downSample, src.height / downSample, 0, format);
		RenderTexture tempB = RenderTexture.GetTemporary(src.width / downSample, src.height / downSample, 0, format);

		//Calculate per-pixel world position
		pixelWorldPosMaterial.SetMatrix("_CAMERA_XYZMATRIX", (GL.GetGPUProjectionMatrix(cam.projectionMatrix, false) * cam.worldToCameraMatrix).inverse);
		Graphics.Blit(null, tempWorldPos, pixelWorldPosMaterial);
		ssrMaterial.SetTexture("_WPOS", tempWorldPos);

		//Raytrace
		Shader.SetGlobalMatrix("_View2Screen", (cam.cameraToWorldMatrix * cam.projectionMatrix.inverse).inverse);
		ssrMaterial.SetInt("_Downscale", downSample);
		Graphics.Blit(src, tempA, ssrMaterial);

		//blur SSR result
		for (int i = 0; i < blurAmount; ++i)
		{
			blurMaterial.SetVector("_Direction", new Vector4(1, 0, 0, 0));
			Graphics.Blit(tempA, tempB, blurMaterial);
			blurMaterial.SetVector("_Direction", new Vector4(0,1, 0, 0));
			Graphics.Blit(tempB, tempA, blurMaterial);
		}

		combineMaterial.SetTexture("_SSR", tempA);

		//show reflections?
		if (showReflectionsOnly)
		{
			Graphics.Blit(tempA, des);
		}
		else
		{
			//Final combine
			Graphics.Blit(src, des,combineMaterial);
		}
		//Release temporary buffers
		RenderTexture.ReleaseTemporary(tempA);
		RenderTexture.ReleaseTemporary(tempB);
		RenderTexture.ReleaseTemporary(tempWorldPos);
	}
	public void CheckSampleSwitch()
	{
		//int setting = 64;
		//if (samplesCurrent != (int)setting)
		//{
		//	samplesCurrent = (int)setting;
		//	ssrMaterial.DisableKeyword("S64");
		//	ssrMaterial.DisableKeyword("S48");
		//	ssrMaterial.DisableKeyword("S32");
		//	ssrMaterial.DisableKeyword("S16");
		//	switch (samplesCurrent)
		//	{
		//		case 64:
		//			ssrMaterial.EnableKeyword("S64");
		//			return;
		//		case 48:
		//			ssrMaterial.EnableKeyword("S48");
		//			return;
		//		case 32:
		//			ssrMaterial.EnableKeyword("S32");
		//			return;
		//		default:
		//			ssrMaterial.EnableKeyword("S16");
		//			return;
		//	}
		//}
	}
}
