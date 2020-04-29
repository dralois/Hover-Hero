using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DisplayScore : MonoBehaviour
{  //for the Scene HighScore

	public Text highScore;
	List<KeyValuePair<string, float>> topTen = new List<KeyValuePair<string, float>>();

	// Start is called before the first frame update
	void Start()
	{
		//highScore.text = PlayerPrefs.GetFloat("HighScore").ToString(); //gets the score that has been saved in SaveScore
		DontDestroyOnLoad(highScore);
		topTen = SaveScore.LoadArray("NewHighScore");

		highScore.text = "";

		for (int i = 0; i < 10; i++)
			highScore.text += (i+1).ToString() + ") " + topTen[i].Key + " : " + topTen[i].Value + "\n";
	}

	//wir laden die zehn besten aus unserem Spiel raus
}
