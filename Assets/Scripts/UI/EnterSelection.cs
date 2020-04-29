using UnityEngine.UI;
using UnityEngine;

public class EnterSelection : MonoBehaviour
{
	[SerializeField]
	private Text letter1;
	[SerializeField]
	private Text letter2;
	[SerializeField]
	private Text letter3;

	public string playerName;

	public void ConfirmInput()
	{
		PlayerPrefs.SetString("NewPlayer", letter1.text + letter2.text + letter3.text);
		Debug.Log(PlayerPrefs.GetString("NewPlayer"));
	}
}
