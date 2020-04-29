using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;
using System.IO;

public class GesturePlayer : MonoBehaviour
{

	#region Classes

	[System.Serializable]
	public class MoveDic : SerializableDictionaryBase<AvailableMoves, string>{}

	#endregion

	#region Declarations

	[SerializeField]
	private MoveDic m_FilePaths;
	private string currFilePath = "";
	private AvailableMoves currLane = AvailableMoves.Idle;

	// whether it is playing saved data at the moment
	private bool isPlaying = false;

	// reference to the KM
	private KinectManager manager = null;

	// time variables used for recording and playing
	private long liRelTime = 0;
	private float fStartTime = 0f;
	private float fCurrentTime = 0f;
	private int fCurrentFrame = 0;

	// player variables
	private StreamReader fileReader = null;
	private float fPlayTime = 0f;
	private string sPlayLine = string.Empty;

	#endregion

	#region Enums

	/// <summary>
	/// All available captured moves
	/// </summary>
	public enum AvailableMoves
	{
		Jump,
		Idle,
		Crouch,
		Fire,
		FireLeft,
		FireRight,
		IdleLeft,
		WalkLeft,
		WalkLeftBack,
		IdleRight,
		WalkRight,
		WalkRightBack,
		Shield1,
		Shield2,
		Sword1,
		Sword2,
		Grab1,
		Grab2
	}

	#endregion

	#region Properties

	// returns if file-play is in progress at the moment
	public bool IsPlaying()
	{
		return isPlaying;
	}

	#endregion

	#region Functions

	#region Public

	// switch current lane one to the right
	public void MoveRight()
	{
		switch (currLane)
		{
			case AvailableMoves.Idle:
				{
					currLane = AvailableMoves.IdleRight;
					PlayFile(AvailableMoves.WalkRight);
				} break;
			case AvailableMoves.IdleLeft:
				{
					currLane = AvailableMoves.Idle;
					PlayFile(AvailableMoves.WalkLeftBack);
				} break;
			case AvailableMoves.IdleRight:
				break;
			default:
				break;
		}
	}

	// switch current lane one to the left
	public void MoveLeft()
	{
		switch (currLane)
		{
			case AvailableMoves.Idle:
				{
					currLane = AvailableMoves.IdleLeft;
					PlayFile(AvailableMoves.WalkLeft);
				}
				break;
			case AvailableMoves.IdleLeft:
				break;
			case AvailableMoves.IdleRight:
				{
					currLane = AvailableMoves.Idle;
					PlayFile(AvailableMoves.WalkRightBack);
				}
				break;
			default:
				break;
		}
	}

	// selects and plays a move
	public void PlayFile(AvailableMoves move)
	{
		// if file set, play file
		if(m_FilePaths.TryGetValue(move, out currFilePath))
		{
			// stop previous file, start new file
			StopPlaying();
			StartPlaying();
		}
	}

	// starts playing
	public bool StartPlaying()
	{
		if (isPlaying)
			return false;

		isPlaying = true;

		// stop playing if there is no file name specified
		if (currFilePath.Length == 0 || !File.Exists(currFilePath))
		{
			isPlaying = false;
		}

		if (isPlaying)
		{
			// initialize times
			fStartTime = fCurrentTime = Time.time;
			fCurrentFrame = -1;

			// open the file and read a line
#if !UNITY_WSA
			fileReader = new StreamReader(currFilePath);
#endif
			ReadLineFromFile();

			// enable the play mode
			if (manager)
			{
				manager.EnablePlayMode(true);
			}
		}

		return isPlaying;
	}

	// stops recording or playing
	public void StopPlaying()
	{
		if (isPlaying)
		{
			// close the file, if it is playing
			CloseFile();
			isPlaying = false;
		}
	}

	#endregion

	#region Private

	// reads a line from the file
	private bool ReadLineFromFile()
	{
		if (fileReader == null)
			return false;

		// read a line
		sPlayLine = fileReader.ReadLine();
		if (sPlayLine == null)
			return false;

		// extract the unity time and the body frame
		char[] delimiters = { '|' };
		string[] sLineParts = sPlayLine.Split(delimiters);

		if (sLineParts.Length >= 2)
		{
			float.TryParse(sLineParts[0], out fPlayTime);
			sPlayLine = sLineParts[1];
			fCurrentFrame++;

			return true;
		}

		return false;
	}

	// close the file and disable the play mode
	private void CloseFile()
	{
		// close the file
		if (fileReader != null)
		{
			fileReader.Dispose();
			fileReader = null;
		}

		// disable the play mode
		if (manager)
		{
			manager.EnablePlayMode(false);
		}
	}

	#endregion

	#region Unity

	void Start()
	{
		manager = KinectManager.Instance;
		// Start playing idle
		if (manager != null)
		{
			PlayFile(currLane);
		}
		else
		{
			Debug.Log("KinectManager not found, probably not initialized.");
		}
	}

	void Update()
	{
		// save key down
		bool leftPressed = Input.GetAxis("Horizontal") < 0;
		bool rightPressed = Input.GetAxis("Horizontal") > 0;
		bool downPressed = Input.GetAxis("Vertical") < 0;
		bool upPressed = Input.GetAxis("Vertical") > 0;
		bool firePressed = Input.GetKeyDown(KeyCode.Space);
		bool grabPressed = Input.GetKeyDown(KeyCode.X);
		bool shieldPressed = Input.GetKeyDown(KeyCode.C);
		bool swordPressed = Input.GetKeyDown(KeyCode.V);

		// apply lane switch
		if ((leftPressed || rightPressed) && !(leftPressed && rightPressed) && Input.anyKeyDown)
		{
			if (leftPressed)
			{;
				MoveLeft();
			}
			else
			{
				MoveRight();
			}
		}

		// jump and crouch
		if((downPressed || upPressed) && !(downPressed && upPressed) && Input.anyKeyDown)
		{
			if (upPressed)
			{
				currLane = AvailableMoves.Idle;
				PlayFile(AvailableMoves.Jump);
			}
			if (downPressed)
			{
				currLane = AvailableMoves.Idle;
				PlayFile(AvailableMoves.Crouch);
			}
		}

		// fire depending on lane
		if (firePressed)
		{
			switch (currLane)
			{
				case AvailableMoves.Idle:
					PlayFile(AvailableMoves.Fire);
					break;
				case AvailableMoves.IdleRight:
					PlayFile(AvailableMoves.FireLeft);
					break;
				case AvailableMoves.IdleLeft:
					PlayFile(AvailableMoves.FireRight);
					break;
				default:
					break;
			}
		}

		// random shield / sword gesture
		if (swordPressed || shieldPressed || grabPressed && !(swordPressed && shieldPressed && grabPressed) &&
			!(swordPressed && shieldPressed) && !(swordPressed && grabPressed) && !(grabPressed && shieldPressed))
		{
			if (Random.value > 0.5)
				if (swordPressed)
					PlayFile(AvailableMoves.Sword1);
				else if (grabPressed)
					PlayFile(AvailableMoves.Grab1);
				else
					PlayFile(AvailableMoves.Shield1);
			else
				if (swordPressed)
					PlayFile(AvailableMoves.Sword2);
				else if (grabPressed)
					PlayFile(AvailableMoves.Grab2);
				else
					PlayFile(AvailableMoves.Shield2);
		}

		if (isPlaying)
		{
			// wait for the right time
			fCurrentTime = Time.time;
			float fRelTime = fCurrentTime - fStartTime;

			if (sPlayLine != null && fRelTime >= fPlayTime)
			{
				// then play the line
				if (manager && sPlayLine.Length > 0)
				{
					manager.SetBodyFrameData(sPlayLine);
				}

				// and read the next line
				ReadLineFromFile();
			}

			// play current idle at the end
			if (sPlayLine == null)
			{
				PlayFile(currLane);
			}
		}
	}

	void OnDestroy()
	{
		CloseFile();
		isPlaying = false;
	}

	#endregion

	#endregion

}
