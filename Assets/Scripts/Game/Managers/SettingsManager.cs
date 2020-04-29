using UnityEngine.UI;
using UnityEngine;
using System.IO;

public class SettingsManager : MonoBehaviour
{
	public Toggle MusicToggle;
	public Slider MusicSlider;
	public Toggle SoundToggle;
	public Slider SoundSlider;
	public Toggle LeftHandToggle;
	public Toggle TutorialToggle;

	public Toggle EnemyToggle;
	public Slider EnemySlider;
	public Slider BiomeLengthSlider;
	public Dropdown StartbiomeDropdown;
	private int StartbiomeDropdownChildren;
	public Slider SpeedSlider;

	public Button ApplyButton;
	public Button BackButton;
	public GameSettings gameSettings;

	public string MainMenuName = "MainMenu";

	private void Start()
	{

		if (GameManager.Instance != null)
		{
			gameSettings = GameManager.Instance.gameSettings;
		}
		else
		{
			Debug.Log("no gameManager Instance found -> loading new settings");
			gameSettings = new GameSettings();
		}

		MusicToggle.onValueChanged.AddListener(delegate { OnMusicToggle(); });
		MusicSlider.onValueChanged.AddListener(delegate { OnMusicSlider(); });
		SoundToggle.onValueChanged.AddListener(delegate { OnSoundToggle(); });
		SoundSlider.onValueChanged.AddListener(delegate { OnSoundSlider(); });
		LeftHandToggle.onValueChanged.AddListener(delegate { OnLeftHandToggle(); });
		TutorialToggle.onValueChanged.AddListener(delegate { OnTutorialToggle(); });

		EnemyToggle.onValueChanged.AddListener(delegate { OnEnemyToggle(); });
		EnemySlider.onValueChanged.AddListener(delegate { OnEnemySlider(); });
		BiomeLengthSlider.onValueChanged.AddListener(delegate { OnBiomeLengthSlider(); });
		StartbiomeDropdown.onValueChanged.AddListener(delegate { OnStartBiomeDropdown(); });
		StartbiomeDropdownChildren = StartbiomeDropdown.transform.childCount;
		SpeedSlider.onValueChanged.AddListener(delegate { OnSpeedSlider(); });

		ApplyButton.onClick.AddListener(delegate { OnButtonApply(); });
		BackButton.onClick.AddListener(delegate { OnBackButton(); });

		LoadSettings();
	}

	private void Update()
	{
		if (StartbiomeDropdown.transform.childCount != StartbiomeDropdownChildren)
		{
			AudioManager.Instance.Play("Click");
		}
		StartbiomeDropdownChildren = StartbiomeDropdown.transform.childCount;
	}

	public void OnMusicToggle()
	{
		gameSettings.Music = MusicToggle.isOn;
		AudioManager.Instance.Play("Switch");
	}

	public void OnMusicSlider()
	{
		gameSettings.MusicValue = MusicSlider.value;
		AudioManager.Instance.InitVolume();
		AudioManager.Instance.Play("Click");
	}

	public void OnSoundToggle()
	{
		gameSettings.Sounds = SoundToggle.isOn;
		AudioManager.Instance.Play("Switch");
	}

	public void OnSoundSlider()
	{
		gameSettings.SoundsValue = SoundSlider.value;
		AudioManager.Instance.InitVolume();
		AudioManager.Instance.Play("Click");
	}

	public void OnLeftHandToggle()
	{
		gameSettings.LeftHand = LeftHandToggle.isOn;
		AudioManager.Instance.Play("Switch");
	}

	public void OnTutorialToggle()
	{
		gameSettings.Tutorial = TutorialToggle.isOn;
		AudioManager.Instance.Play("Switch");
	}

	public void OnEnemyToggle()
	{
		gameSettings.SpawnEnemies = EnemyToggle.isOn;
		AudioManager.Instance.Play("Switch");
	}

	public void OnEnemySlider()
	{
		gameSettings.EnemySpawnChance = EnemySlider.value;
		AudioManager.Instance.Play("Click");
	}
	public void OnBiomeLengthSlider()
	{
		gameSettings.BiomeLength = (int)BiomeLengthSlider.value;
		AudioManager.Instance.Play("Click");
	}
	public void OnStartBiomeDropdown()
	{
		gameSettings.StartBiome = StartbiomeDropdown.value;
	}

	public void OnSpeedSlider()
	{
		gameSettings.SpeedFactor = SpeedSlider.value;
		AudioManager.Instance.Play("Click");
	}

	public void OnButtonApply()
	{
		SaveSettings();
		AudioManager.Instance.Play("Click");
	}

	public void SaveSettings()
	{
		string jsonData = JsonUtility.ToJson(gameSettings, true);
		File.WriteAllText(Application.persistentDataPath + "/gamesettings.json", jsonData);
	}

	public void LoadSettings()
	{
		MusicToggle.isOn = gameSettings.Music;
		MusicSlider.value = gameSettings.MusicValue;
		SoundToggle.isOn = gameSettings.Sounds;
		SoundSlider.value = gameSettings.SoundsValue;
		LeftHandToggle.isOn = gameSettings.LeftHand;
		TutorialToggle.isOn = gameSettings.Tutorial;

		EnemyToggle.isOn = gameSettings.SpawnEnemies;
		EnemySlider.value = gameSettings.EnemySpawnChance;
		BiomeLengthSlider.value = gameSettings.BiomeLength;
		StartbiomeDropdown.value = gameSettings.StartBiome;
		SpeedSlider.value = gameSettings.SpeedFactor;
	}

	public void OnBackButton()
	{
		AudioManager.Instance.Play("Click");
		GameManager.Instance.LoadScene(MainMenuName);
	}
}
