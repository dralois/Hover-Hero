using UnityEngine.UI;
using UnityEngine;

public class MainMenuButtons : MonoBehaviour
{
	public Button HighScoreButton;
	public Button SettingsButton;
	public Button CreditsButton;
	public Button ExitButton;

	void Start()
	{
		HighScoreButton.onClick.AddListener(delegate { OnHighScoreButton(); });
		SettingsButton.onClick.AddListener(delegate { OnSettingsButton(); });
		CreditsButton.onClick.AddListener(delegate { OnCreditsButton(); });
		ExitButton.onClick.AddListener(delegate { OnExitButton(); });
	}

	void OnHighScoreButton()
	{
		AudioManager.Instance.Play("Click");
		GameManager.Instance.LoadScene("HighScore");
	}

	void OnSettingsButton()
	{
		AudioManager.Instance.Play("Click");
		GameManager.Instance.LoadScene("Settings");
	}

	void OnCreditsButton()
	{
		AudioManager.Instance.Play("Click");
		GameManager.Instance.LoadScene("Credits");
	}

	void OnExitButton()
	{
		AudioManager.Instance.Play("Click");
		Application.Quit();
	}
}
