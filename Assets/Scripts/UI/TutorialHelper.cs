using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Text))]
public class TutorialHelper : MonoBehaviour
{

	private Text helperText;

	private void Start()
	{
		helperText = GetComponent<Text>();
	}

	public void UpdateHelperText(string newText)
	{
		helperText.text = newText;
		if (newText.Length > 0)
			AudioManager.Instance.Play("Alert");
		else
			AudioManager.Instance.Stop("Alert");
	}
}
