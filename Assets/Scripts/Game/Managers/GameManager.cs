using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{

	public enum GameState
	{
		menu,
		loading,
		inGame,
		end,
		error
	}

	public GameState CurrentGameState = GameState.loading;

	public DamageController DamageControllerInstance;
	public SaveScore ScoreManagerInstance;
	public TransitionManager TransitionManager;

	public GameSettings gameSettings;
	public bool PlayAfterDeath = false;

	private float delayGameEnd = 0;

	void OnEnable()
	{
		//Load Settings or Save them with default values
		if (File.Exists(Application.persistentDataPath + "/gamesettings.json"))
			gameSettings = JsonUtility.FromJson<GameSettings>(File.ReadAllText(Application.persistentDataPath + "/gamesettings.json"));
		else
		{
			gameSettings = new GameSettings();
			File.WriteAllText(Application.persistentDataPath + "/gamesettings.json", JsonUtility.ToJson(gameSettings, true));
		}

		SceneManager.sceneLoaded += OnSceneLoaded;
		SceneManager.sceneUnloaded += OnSceneUnloaded;
		//TransitionManager = TransitionManager.Instance;
	}

	private void Update()
	{
		UpdateIfState();
	}

	void OnDisable()
	{
		//Debug.Log("OnDisable");
		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.sceneUnloaded -= OnSceneUnloaded;
	}

	private void gl_PlayerDetectedEvent(long userId)
	{
		CurrentGameState = GameState.inGame;
	}

	private void gl_PlayerLostEvent(long userId)
	{
		//CurrentGameState = GameState.error;
		//ChangeState(GameState.error);
		Debug.Log("Lost Player (in GameManager)");
		if (KinectManager.Instance != null)
		{
			KinectManager.Instance.playerCalibrationPose = KinectGestures.Gestures.None;
		}
		if (CurrentGameState == GameState.inGame)
		{
			LoadScene("MainMenu");
		}
	}

	//Once on StateChange
	public void ChangeState(GameState newState)
	{
		if (GameStateChangedEvent != null)
			GameStateChangedEvent(newState);

		switch (newState)
		{
			case GameState.menu:
				//TransitionManager.Instance.transitionMode = TransitionManager.TransitionMode.Dissolve;
				break;
			case GameState.loading:
				break;
			case GameState.inGame:
				GestureListener gl = FindObjectOfType<GestureListener>();
				gl.PlayerDetectedEvent += gl_PlayerDetectedEvent;
				gl.PlayerLostEvent += gl_PlayerLostEvent;
				DamageControllerInstance = FindObjectOfType<DamageController>();
				ScoreManagerInstance = FindObjectOfType<SaveScore>();
				//TransitionManager.Instance.transitionMode = TransitionManager.TransitionMode.Particle;
				break;
			case GameState.end:
				Canvas canvas = FindObjectOfType<Canvas>();
				if (canvas != null)
				{
					RenderMode pre = canvas.renderMode;
					if (pre != RenderMode.ScreenSpaceCamera)
					{
						canvas.renderMode = RenderMode.ScreenSpaceCamera;
						canvas.planeDistance = 0.7f;
					}
				}
				break;
			case GameState.error:
				FromGameToMainMenu();
				break;
			default:
				break;
		}
		CurrentGameState = newState;
		Debug.Log("GameState changed to: " + CurrentGameState);
	}

	//every frame
	public void UpdateIfState()
	{
		switch (CurrentGameState)
		{
			case GameState.menu:
				ReturnToMainMenuOnEscape();
				break;
			case GameState.loading:
				ReturnToMainMenuOnEscape();
				break;
			case GameState.inGame:
				ReturnToMainMenuOnEscape();
				if (DamageControllerInstance != null && DamageControllerInstance.IsDead)
				{
					RequestGameEnd();
				}
				break;
			case GameState.end:
				ReturnToMainMenuOnEscape();
				EndGame();
				break;
			case GameState.error:
				ReturnToMainMenuOnEscape();
				break;
			default:
				break;
		}
	}

	public void RequestGameEnd()
	{
		ChangeState(GameState.end);
		FindObjectOfType<SplineFollow>().EndScreen();
		delayGameEnd = 5.0f;
	}

	private void EndGame()
	{
		if (delayGameEnd < 0)
		{
			if (!PlayAfterDeath)
			{
				TransitionManager.Instance.ManualBeforeSceneUnloading();
				KinectManager.Instance.playerCalibrationPose = KinectGestures.Gestures.None;
				KinectManager.Instance.ClearKinectUsers();
				LoadScene("MainMenu");
			}
			delayGameEnd = 0;
		}
		else if (delayGameEnd > 0)
		{
			delayGameEnd -= Time.deltaTime;
		}
	}

	public void LoadScene(int index)
	{
		if (SceneUnloadingEvent != null)
			SceneUnloadingEvent(SceneManager.GetActiveScene(), SceneManager.GetSceneByBuildIndex(index).name);
		SceneManager.LoadScene(index);
	}

	public void LoadScene(string name)
	{
		if (SceneUnloadingEvent != null)
			SceneUnloadingEvent(SceneManager.GetActiveScene(), name);
		SceneManager.LoadScene(name);
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == "MainGame" || scene.name == "Tutorial")
		{
			ChangeState(GameState.inGame);
		}
		else if (scene.name == "MainMenu")
		{
			ChangeState(GameState.menu);
		}
	}

	void OnSceneUnloaded(Scene current)
	{

	}

	void ReturnToMainMenuOnEscape()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			FromGameToMainMenu();
		}
	}

	void FromGameToMainMenu()
	{
		if (KinectManager.Instance != null)
		{
			KinectManager.Instance.playerCalibrationPose = KinectGestures.Gestures.None;
			KinectManager.Instance.ClearKinectUsers();
		}
		LoadScene("MainMenu");
	}

	//Events

	/// <summary>
	/// Called when an Game State Changed
	/// </summary>
	/// <param name="gameState">new GameState</param>
	public delegate void GameStateChangedDel(GameState gameState);
	public event GameStateChangedDel GameStateChangedEvent;

	/// <summary>
	/// Called when an Scene is getting unloaded
	/// 
	/// </summary>
	/// <param name="scene">Scene getting unloaded</param>
	public delegate void UnloadingSceneDel(Scene scene, string nextScene);
	public event UnloadingSceneDel SceneUnloadingEvent;
}
