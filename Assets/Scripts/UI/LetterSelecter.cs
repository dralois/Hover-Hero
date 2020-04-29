using UnityEngine.UI;
using UnityEngine;

public class LetterSelecter : MonoBehaviour
{
	Text myLetter;
    public int letterNr;
	private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	private int stepper = 0;
	private int letterSelect = 0;

	void Start()
	{
		myLetter = GetComponent<Text>();
        myLetter.text = PlayerPrefs.GetString("Letter" + letterNr, "A");
        stepper = PlayerPrefs.GetInt("Number" + letterNr, 0);
	}

	public void NextLetter()
	{
		if(stepper < alphabet.Length - 1)
		{
			stepper++;
			myLetter.text = alphabet[stepper].ToString();
            PlayerPrefs.SetString("Letter" + letterNr, myLetter.text);
            PlayerPrefs.SetInt("Number" + letterNr, stepper);
		}
        else
        {
            stepper = 0;
            myLetter.text = alphabet[stepper].ToString();
            PlayerPrefs.SetString("Letter" + letterNr, myLetter.text);
            PlayerPrefs.SetInt("Number" + letterNr, stepper);
        }
	}

	public void PrevLetter()
	{
		if(stepper > 0)
		{
			stepper--;
            myLetter.text = alphabet[stepper].ToString();
            PlayerPrefs.SetString("Letter" + letterNr, myLetter.text);
            PlayerPrefs.SetInt("Number" + letterNr, stepper);
		}
        else
        {
            stepper = alphabet.Length - 1;
            myLetter.text = alphabet[stepper].ToString();
            PlayerPrefs.SetString("Letter" + letterNr, myLetter.text);
            PlayerPrefs.SetInt("Number" + letterNr, stepper);
        }
            
	}
}
