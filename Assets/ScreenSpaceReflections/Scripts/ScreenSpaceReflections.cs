using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenSpaceReflections : MonoBehaviour
{
	[Range(0,8)]
	public int downSample = 2;
	public bool hdr = false;
	private Camera cam;

	//doesn't change, being static avoids doubles in memory with multiple cameras
	private Material ssrMaterial, pixelWorldPosMaterial;
	private static Texture2D ssrMask; 

	private void OnEnable()
	{
		if (ssrMaterial == null) ssrMaterial = new Material(Resources.Load<Shader>("SSR/ScreenSpaceReflections"));
		if (pixelWorldPosMaterial == null) pixelWorldPosMaterial = new Material(Resources.Load<Shader>("SSR/PixelWorldPosition"));
		if (ssrMask == null) Shader.SetGlobalTexture("_ssrMask", ssrMask = Resources.Load<Texture2D>("SSR/ssrMaskSoft"));
		if(cam==null) cam = GetComponent<Camera>();
	}
	public void OnRenderImage(RenderTexture src, RenderTexture des)
	{
		RenderTextureFormat format = hdr ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;

		//Create temporary buffers
		downSample = Mathf.Clamp(downSample, 1, 8);
		RenderTexture tempWorldPos = RenderTexture.GetTemporary(src.width, src.height , 0, RenderTextureFormat.ARGBFloat);
		RenderTexture tempA = RenderTexture.GetTemporary(src.width / downSample, src.height / downSample, 0, format);
		RenderTexture tempB = RenderTexture.GetTemporary(src.width / downSample, src.height / downSample, 0, format);

		//Calculate per-pixel world position
		pixelWorldPosMaterial.SetMatrix("_CAMERA_XYZMATRIX", (GL.GetGPUProjectionMatrix(cam.projectionMatrix, false) * cam.worldToCameraMatrix).inverse);
		Graphics.Blit(null, tempWorldPos, pixelWorldPosMaterial);

		//Raytrace
		Shader.SetGlobalMatrix("_View2Screen", (cam.cameraToWorldMatrix * cam.projectionMatrix.inverse).inverse);
		ssrMaterial.SetInt("_Downscale", downSample);

		//Final combine
		Graphics.Blit(tempWorldPos, des);

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
