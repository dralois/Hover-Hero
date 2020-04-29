using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class SaveScore : MonoBehaviour
{
	List<KeyValuePair<string, float>> playerScore = new List<KeyValuePair<string, float>>();

	public Text score;  //score und points
	public Text highScore;

	public int coinPoints = 10;

	string playerName;

	float distance = 0;
	float bonusPoints = 0;

	public bool resetOnStart = false;

	public void Start()
	{
		if (resetOnStart)
		{
			PlayerPrefs.SetFloat("HighScore", 0);
			PlayerPrefs.DeleteAll();
		}
        float oldHighscore = PlayerPrefs.GetFloat("HighScore");
        highScore.text = "Highscore: " + oldHighscore.ToString();
	}

	public void Update()  //Wenn Score größer als HighScore ist, dann wird es zum neuen HighScore...
	{
		score.text = "Score: " + (distance + bonusPoints).ToString(); //current distance wird live gezeigt...
		//float oldHighscore = PlayerPrefs.GetFloat("HighScore");
		
        //if ((distance + bonusPoints) > oldHighscore)
        //{
        //    PlayerPrefs.SetFloat("HighScore", (distance + bonusPoints)); //Text wird geupdated und Wert wird gespeichert
        //    float testValue = PlayerPrefs.GetFloat("HighScore", (distance + bonusPoints));
        //    Debug.Log(testValue);
        //    highScore.text = "Highscore: " + (distance + bonusPoints).ToString();
        //    UpdateTopTen(distance + bonusPoints, PlayerPrefs.GetString("NewPlayer"));
        //    Debug.Log("updated!");
        //}
        //else
        //{
        //    highScore.text = "Highscore: " + oldHighscore.ToString();
        //}
	}

    public void SetHighScore()
    {
        float oldHighscore = PlayerPrefs.GetFloat("HighScore");
        if ((distance + bonusPoints) > oldHighscore)
        {
            PlayerPrefs.SetFloat("HighScore", (distance + bonusPoints)); //Text wird geupdated und Wert wird gespeichert
        }
		//float testValue = PlayerPrefs.GetFloat("HighScore", (distance + bonusPoints));
		//Debug.Log(testValue);
		//highScore.text = "Highscore: " + (distance + bonusPoints).ToString();
		UpdateTopTen(distance + bonusPoints, PlayerPrefs.GetString("NewPlayer"));
    }

	public void SetDistancePoints(int points)
	{
		distance = points;
	}

	public void AddPoints(int points)
	{
		bonusPoints += points;
	}

	public float GetPoints()
	{
		return distance + bonusPoints;
	}

	public void SaveArray(List<KeyValuePair<string, float>> topTen, string varName)
	{
		for (int i = 0; i < topTen.Count; i++)
		{
			PlayerPrefs.SetString(varName + "n" + i, topTen[i].Key);
			PlayerPrefs.SetFloat(varName + i, topTen[i].Value);
		}
	}

	public static List<KeyValuePair<string, float>> LoadArray(string varName)
	{//in einer neu erstellten Liste werden die gespeicherten Werte reingeladen
		List<KeyValuePair<string, float>> topTen = new List<KeyValuePair<string, float>>();
		for (int i = 0; i < 10; i++)
		{
			string testName = PlayerPrefs.GetString(varName + "n" + i);

			KeyValuePair<string, float> playerInfo = new KeyValuePair<string, float>(testName, PlayerPrefs.GetFloat(varName + i));
			topTen.Add(playerInfo);
		}
		return topTen;
	}

	public List<KeyValuePair<string, float>> UpdateTopTen(float newScore, string newName)    //greif auf den Spieler zu...
	{
		playerScore = LoadArray("NewHighScore"); //alte Liste wird geladen um überschrieben zu werden..
		KeyValuePair<string, float> playerInfo = new KeyValuePair<string, float>(newName, newScore);
		bool included = false;

		for (int i = 0; i < playerScore.Count; i++)
		{
			if (playerScore[i].Key == playerInfo.Key)
			{
                if (playerScore[i].Value < newScore)
                {
				    playerScore[i] = playerInfo;
                }
				included = true;
			}
		}

        playerScore.Sort((x, y) => (y.Value.CompareTo(x.Value)));
		
        if (!included)
		{
            playerScore.RemoveAt(playerScore.Count - 1);
			playerScore.Add(playerInfo);
		}
        
        playerScore.Sort((x, y) => (y.Value.CompareTo(x.Value)));
		
		SaveArray(playerScore, "NewHighScore");

		return playerScore;
	}
}
