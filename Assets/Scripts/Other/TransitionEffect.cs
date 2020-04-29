using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionEffect : MonoBehaviour
{
	//Simple Stuff
	[SerializeField] private bool DoOverride = false;
	[SerializeField] private RenderTexture RenderTextureLastScene;
	public RenderTexture NowTextureScene;
	public Material transitionMaterial;
	public float transitionTime;
	public TransitionManager transitionManager;
	public TransitionManager.TransitionMode transitionMode = TransitionManager.TransitionMode.Dissolve;

	//Paricle Stuff
	public Material ParticleMaterial;
	public ComputeShader ComputeShader;
	ComputeBuffer buffer;
	private int particleCount = 1920 * 1080;
	public float direction = 1;
	public float timeToCicle = 2.5f;
	public float timeInCirlce = 3.0f;
	public float timeToEndPos = 2.0f;

	private float StartTime = 0;
	private int state = 0;

	public void SetLastSceneRenderTexture(RenderTexture renderTexture)
	{
		RenderTextureLastScene = renderTexture;
	}

	public void Capture(ref RenderTexture rendertexture, Camera cam)
	{
		cam.targetTexture = rendertexture;
		cam.Render();
		cam.targetTexture = null;
	}

	public void CaptureNow(Camera cam)
	{
		cam.targetTexture = NowTextureScene;
		cam.Render();
		cam.targetTexture = null;
	}

	public void StartOverride()
	{
		DoOverride = true;
	}
	public void EndOverride()
	{
		DoOverride = false;
	}

	private void OnStart()
	{
		//Setup();
	}

	private void OnDisable()
	{
		if (buffer != null)
			buffer.Release();
	}

	private void Update()
	{
		if (DoOverride)
		{
			if (transitionMode == TransitionManager.TransitionMode.Particle)
			{
				float timePassed = Time.timeSinceLevelLoad - StartTime;
				ComputeShader.SetFloat("deltaTime", Time.deltaTime);
				ComputeShader.SetFloat("direction", direction);
				ComputeShader.SetFloat("timePassedSinceStart", timePassed);
				if (state == 0)
				{
					ComputeShader.Dispatch(ComputeShader.FindKernel("CSMoveParticles1"), particleCount / 256, 1, 1);
					if (true)
					{
						state = 1;
					}
				}
				else if (state == 1)
				{
					ComputeShader.Dispatch(ComputeShader.FindKernel("CSMoveParticles2"), particleCount / 256, 1, 1);
					if (timePassed >= timeToCicle)
					{
						ComputeShader.SetFloat("inStart", timeToCicle);
						ComputeShader.SetFloat("inEnd", timeToCicle + timeInCirlce);
						state = 2;
					}
				}
				else if (state == 2)
				{
					ComputeShader.Dispatch(ComputeShader.FindKernel("CSMoveParticles3"), particleCount / 256, 1, 1);
					if (timePassed >= timeToCicle + timeInCirlce)
					{
						ComputeShader.SetFloat("backStartTime", timeToCicle + timeInCirlce);
						ComputeShader.SetFloat("backEndTime", timeToCicle + timeInCirlce + timeToEndPos);
						ComputeShader.Dispatch(ComputeShader.FindKernel("CSMoveParticlesSetOrigin"), particleCount / 256, 1, 1);
						state = 3;

					}
				}
				else if (state == 3)
				{
					ComputeShader.Dispatch(ComputeShader.FindKernel("CSMoveParticles4"), particleCount / 256, 1, 1);
					if (timePassed >= timeToCicle + timeInCirlce + timeToEndPos)
					{
						transitionManager.EndPhaseParticle();

					}
				}

				//ComputeShader.Dispatch(ComputeShader.FindKernel("CSMoveParticles1"), particleCount / 256, 1, 1);
			}
		}
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (DoOverride)
		{
			if (transitionMode == TransitionManager.TransitionMode.Particle)
			{

				//ParticleMaterial.SetPass(0);
				//Graphics.DrawProcedural(MeshTopology.Points, 1, particleCount);
				Graphics.Blit(source, destination);
			}
			else if (transitionMode == TransitionManager.TransitionMode.Dissolve)
			{
				transitionMaterial.SetTexture("_LastTex", RenderTextureLastScene);
				Graphics.Blit(source, destination, transitionMaterial);
			}
		}
		else
		{
			Graphics.Blit(source, destination);
		}
	}

	void OnRenderObject()
	{
		if (DoOverride)
		{
			if (transitionMode == TransitionManager.TransitionMode.Particle)
			{

				ParticleMaterial.SetPass(0);
				Graphics.DrawProcedural(MeshTopology.Points, 1, particleCount);
			}
			else
			{

				//ParticleMaterial.SetPass(0);
				//Graphics.DrawProcedural(MeshTopology.Points, 1, particleCount);
			}
		}
		else
		{

		}
	}

	//particle stuff
	private struct Particle
	{
		public Vector3 position;
		public Vector3 velocity;
		public Vector3 destination;
		public Vector3 origin;
		public Color sourceColor;
		public Color destColor;
		public Color currentColor;
	}

	public void SetupParticles()
	{
		Particle[] particles = new Particle[particleCount];

		buffer = new ComputeBuffer(particleCount, 96);
		buffer.SetData(particles);
		ComputeShader.SetBuffer(ComputeShader.FindKernel("CSMoveParticles1"), "particleBuffer", buffer);
		ComputeShader.SetBuffer(ComputeShader.FindKernel("CSMoveParticles2"), "particleBuffer", buffer);
		ComputeShader.SetBuffer(ComputeShader.FindKernel("CSMoveParticles3"), "particleBuffer", buffer);
		ComputeShader.SetBuffer(ComputeShader.FindKernel("CSMoveParticlesSetOrigin"), "particleBuffer", buffer);
		ComputeShader.SetBuffer(ComputeShader.FindKernel("CSMoveParticles4"), "particleBuffer", buffer);
		ComputeShader.SetBuffer(ComputeShader.FindKernel("CSSetTexture"), "particleBuffer", buffer);

		ComputeShader.SetFloat("transitionTime", timeToCicle + timeInCirlce + timeToEndPos);
		StartTime = Time.timeSinceLevelLoad;

		ParticleMaterial.SetBuffer("particleBuffer", buffer);
	}

	public void SetGPU(RenderTexture textureLast, RenderTexture textureNow, Camera cam)
	{
		ComputeShader.SetMatrix("UNITY_MATRIX_MVP", cam.previousViewProjectionMatrix);
		ComputeShader.SetTexture(ComputeShader.FindKernel("CSSetTexture"), "TextureOriginal", textureLast);
		ComputeShader.SetTexture(ComputeShader.FindKernel("CSSetTexture"), "TextureDestination", textureNow);
		//ComputeShader.SetVector("cameraPos", cam.transform.position);
		//ComputeShader.SetVector("viewDirection", cam.transform.forward);

		float plane = 200;
		Vector3 leftbottom = cam.ScreenToWorldPoint(new Vector3(0, 0, plane));
		Vector3 rightbottom = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, 0, plane));
		Vector3 leftTop = cam.ScreenToWorldPoint(new Vector3(0, cam.pixelHeight, plane));
		Vector3 rightTop = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, plane));
		ComputeShader.SetMatrix("invVPM", cam.previousViewProjectionMatrix.inverse);
		ComputeShader.SetVector("leftbottom", leftbottom);
		ComputeShader.SetVector("rightbottom", rightbottom);
		ComputeShader.SetVector("leftTop", leftTop);
		ComputeShader.SetVector("rightTop", rightTop);
		ComputeShader.SetVector("center", leftbottom + (rightTop - leftbottom) / 2.0f);
		ComputeShader.SetVector("forward", cam.transform.forward);

		ComputeShader.Dispatch(ComputeShader.FindKernel("CSSetTexture"), textureLast.width / 32, textureLast.height / 32, 1);
	}
}
