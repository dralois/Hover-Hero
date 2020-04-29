using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : Singleton<TransitionManager>
{

	public enum TransitionMode
	{
		Dissolve,
		Particle
	}

	public Camera mainCam;
	public RenderTexture LastSceneRenderTexture;
	public RenderTexture NowSceneRenderTexture;
	public Material renderMaterial;
	//public Texture2D testTexture;
	//public bool DoOverride = false;
	private bool isPhasing = false;

	public Material ParticelMaterial;
	public ComputeShader ComputeShader;
	//public bool useParticles = true;
	public TransitionMode transitionMode = TransitionMode.Dissolve;
	public LayerMask particleLayer;
	private int layerDefault;

	TransitionEffect transitionEffect;
	public float transitionTime = 0;
	float lerpT0 = 0;
	float lerpTEnd = 0;
	float lerpCurrent = 0;

	int delayedCampturingFramesToWait = -1;

	// Start is called before the first frame update
	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
		//SceneManager.sceneUnloaded += OnSceneUnloaded;

		GameManager.Instance.SceneUnloadingEvent += OnSceneUnloading;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		//SceneManager.sceneUnloaded -= OnSceneUnloaded;
		if (GameManager.Instance != null)
			GameManager.Instance.SceneUnloadingEvent -= OnSceneUnloading;
	}

	// Update is called once per frame
	void Update()
	{
		//if (Input.GetKeyDown(KeyCode.I))
		//{
		//    Debug.Log("Started Capturing render @" + Time.timeSinceLevelLoad);
		//    //testRenderTexture = RenderTexture.GetTemporary(testTexture.width, testTexture.height, 24);
		//    mainCam = Camera.main;
		//    //Capture(ref LastRenderTexture, mainCam);
		//    Debug.Log("Ended Capturing render @" + Time.timeSinceLevelLoad);
		//}

		//if (Input.GetKeyDown(KeyCode.O))
		//{
		//    //transitionEffect.DoOverride = !transitionEffect.DoOverride;
		//    StartPhaseDissolve();
		//}


		if (isPhasing)
		{
			if (transitionMode == TransitionMode.Dissolve)
			{
				Phasing();
			}
			else if (transitionMode == TransitionMode.Particle)
			{
				PhasingParticle();
			}
		}

		//delayed capturing
		if (delayedCampturingFramesToWait == 0)
		{
			mainCam.cullingMask = layerDefault;
			transitionEffect.Capture(ref NowSceneRenderTexture, mainCam);
			transitionEffect.SetGPU(LastSceneRenderTexture, NowSceneRenderTexture, mainCam);
			mainCam.cullingMask = particleLayer;
			Start2PhaseParticle();
		}
		if (delayedCampturingFramesToWait >= 0)
		{
			delayedCampturingFramesToWait--;
		}

	}

	void StartPhaseDissolve()
	{
		transitionEffect.StartOverride();
		renderMaterial.SetFloat("percentage", 1);
		lerpT0 = 0;
		lerpTEnd = lerpT0 + transitionTime;
		lerpCurrent = 0;
		isPhasing = true;
	}

	void Phasing()
	{
		float percentage = (lerpCurrent - lerpT0) / (lerpTEnd - lerpT0);
		renderMaterial.SetFloat("percentage", 1 - percentage);
		lerpCurrent += Time.deltaTime;
		if (lerpCurrent >= lerpTEnd)
		{
			transitionEffect.EndOverride();
			isPhasing = false;
		}
	}

	//void DisplayRenderTexture(ref RenderTexture rt)
	//{
	//    //Debug.Log("Blitting rendertexture");
	//    RenderTexture tmp = RenderTexture.GetTemporary(LastSceneRenderTexture.descriptor);
	//    RenderTexture.active = null;
	//    GL.Clear(true, true, Color.black);
	//    //Graphics.Blit(this.texture, this.renderTexture);

	//    //Graphics.Blit(testRenderTexture, tmp, renderMaterial);
	//    Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), tmp);
	//    RenderTexture.ReleaseTemporary(tmp);
	//}

	void InitTransitionEffect()
	{

		InitTransitionComponent();

		//Both
		transitionEffect.SetLastSceneRenderTexture(LastSceneRenderTexture);
		transitionEffect.transitionMode = transitionMode;
		transitionEffect.transitionTime = transitionTime;
		transitionEffect.transitionManager = this;

		if (transitionMode == TransitionMode.Dissolve)
		{
			transitionEffect.transitionMaterial = renderMaterial;
		}
		else if (transitionMode == TransitionMode.Particle)
		{
			transitionEffect.NowTextureScene = NowSceneRenderTexture;
			transitionEffect.ComputeShader = ComputeShader;
			transitionEffect.ParticleMaterial = ParticelMaterial;
			transitionEffect.SetupParticles();
		}

	}

	void InitTransitionComponent()
	{
		if (Camera.main != null)
		{
			mainCam = Camera.main;
		}

		transitionEffect = Camera.main.gameObject.GetComponent<TransitionEffect>();
		if (transitionEffect == null)
		{
			Debug.LogWarning("No Transition effect on Main Camera: " + Camera.main.name + ". Adding one");
			transitionEffect = Camera.main.gameObject.AddComponent<TransitionEffect>();
		}
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{

		InitTransitionEffect();
		if (transitionMode == TransitionMode.Dissolve)
		{

			StartPhaseDissolve();
		}
		else if (transitionMode == TransitionMode.Particle)
		{
			//StartCoroutine(DelayedOnSceneLoadedParticles(scene));
			Start1PhaseParticle();
			delayedCampturingFramesToWait = 1;
		}
	}

	//IEnumerator DelayedOnSceneLoadedParticles(Scene scene)
	//{
	//    //yield return new WaitForEndOfFrame();
	//    yield return null;
	//    yield return new WaitForEndOfFrame();


	//}

	void OnSceneUnloading(Scene scene, string nextScene)
	{
		if (nextScene == "Credits")
		{
			transitionMode = TransitionMode.Particle;
		}
		else if (scene.name == "MainGame")
		{
			return;
		}
		else
		{
			transitionMode = TransitionMode.Dissolve;
		}
		if (transitionEffect == null)
		{
			InitTransitionComponent();
		}

		//Workaround capturing UI - Not Working
		//Canvas canvas = FindObjectOfType<Canvas>();
		//if (canvas != null)
		//{
		//    RenderMode pre = canvas.renderMode;
		//    if(pre != RenderMode.ScreenSpaceCamera)
		//    {
		//        canvas.renderMode = RenderMode.ScreenSpaceCamera;
		//        canvas.planeDistance = 0.7f;

		//    }
		//}

		transitionEffect.Capture(ref LastSceneRenderTexture, mainCam);
	}

	public void ManualBeforeSceneUnloading()
	{
		transitionMode = TransitionMode.Particle;
		if (transitionEffect == null)
		{
			InitTransitionComponent();
		}

		transitionEffect.Capture(ref LastSceneRenderTexture, mainCam);
	}

	private void Start1PhaseParticle()
	{
		layerDefault = mainCam.cullingMask;
		mainCam.cullingMask = particleLayer;
	}

	void Start2PhaseParticle()
	{
		//if (isPhasing)
		//    return;
		//layerDefault = mainCam.cullingMask;
		//mainCam.cullingMask = particleLayer;

		lerpT0 = 0;
		lerpTEnd = lerpT0 + transitionTime;
		lerpCurrent = 0;

		transitionEffect.StartOverride();
		isPhasing = true;
	}

	void PhasingParticle()
	{
		//float percentage = (lerpCurrent - lerpT0) / (lerpTEnd - lerpT0);
		//renderMaterial.SetFloat("percentage", 1 - percentage);
		//Debug.Log("1-percentage: " + (1 - percentage));
		lerpCurrent += Time.deltaTime;
		if (lerpCurrent >= lerpTEnd)
		{
			//transitionEffect.EndOverride();
			//EndPhasePaticle(); //Called from TransitionEffect
		}
	}

	public void EndPhaseParticle()
	{
		mainCam.cullingMask = layerDefault;
		transitionEffect.EndOverride();
		isPhasing = false;
	}
}
