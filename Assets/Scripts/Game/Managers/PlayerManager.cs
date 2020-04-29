using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{

	#region Enums

	public enum Handedness
	{
		Right = 0,
		Left
	}

	#endregion

	#region Classes

	/// <summary>
	/// Player data (body data, gesture data)
	/// </summary>
	public class BodyData
	{

		public Dictionary<KinectGestures.Gestures, List<float>> CompletedGestures { get; private set; }
		public KinectGestures.Gestures LastGesture { get; private set; }
		public Handedness Handedness { get; private set; }
		public Vector3 HeadPos { get; private set; }
		public Vector3 HipPos { get; private set; }
		public Vector3 AnklesPos { get; private set; }
		public float ArmLength { get; private set; }
		public float Height { get; private set; }
		public float Width { get; private set; }

		// saves a completed gesture
		public void SaveGesture(KinectGestures.Gestures gesture)
		{
			// save as last gesture
			LastGesture = gesture;
			// save in dictionary with gesture time
			if(!CompletedGestures.ContainsKey(gesture))
			{
				CompletedGestures.Add(gesture, new List<float>());
			}
			CompletedGestures[gesture].Add(Time.timeSinceLevelLoad);
		}

		// output for debugging
		public override string ToString()
		{
			return "[ Head: " + HeadPos.ToString() + ", Hip: " + HipPos.ToString() +
						", Ankles: " + AnklesPos.ToString() + ", Arm Lenght: " + ArmLength.ToString() +
						", Height: " + Height.ToString() + ", Width: " + Width.ToString() +
						", Handedness: " + Handedness.ToString() + "]";
		}

		/// <summary>
		/// Create new player data
		/// </summary>
		/// <param name="pi_UserId">User Id of the player</param>
		/// <param name="pi_Handedness">Selected handedness</param>
		public BodyData(long pi_UserId, Handedness pi_Handedness)
		{
			// Fetch kinect manager
			KinectManager manager = KinectManager.Instance;
			// Manager is needed
			if (manager == null)
				return;
			// Init data
			Handedness = pi_Handedness;
			CompletedGestures = new Dictionary<KinectGestures.Gestures, List<float>>();
			// Calculate and fetch necessary data
			HeadPos = manager.GetJointPosition(pi_UserId, (int)KinectInterop.JointType.Head);
			HipPos = manager.GetJointPosition(pi_UserId, (int)KinectInterop.JointType.SpineBase);
			AnklesPos = (manager.GetJointPosition(pi_UserId, (int)KinectInterop.JointType.AnkleRight) +
								manager.GetJointPosition(pi_UserId, (int)KinectInterop.JointType.AnkleRight)) / 2.0f;
			Width = Vector3.Distance(manager.GetJointPosition(pi_UserId, (int)KinectInterop.JointType.ShoulderLeft),
															manager.GetJointPosition(pi_UserId, (int)KinectInterop.JointType.ShoulderRight));
			Height = Vector3.Distance(HeadPos, AnklesPos);
			ArmLength = (Vector3.Distance(manager.GetJointPosition(pi_UserId, (int)KinectInterop.JointType.ShoulderLeft),
																		manager.GetJointPosition(pi_UserId, (int)KinectInterop.JointType.HandLeft)) +
									Vector3.Distance(manager.GetJointPosition(pi_UserId, (int)KinectInterop.JointType.ShoulderRight),
																		manager.GetJointPosition(pi_UserId, (int)KinectInterop.JointType.HandRight))) / 2.0f;
		}

		/// <summary>
		/// Empty BodyData
		/// </summary>
		public BodyData()
		{
			CompletedGestures = new Dictionary<KinectGestures.Gestures, List<float>>();
			HeadPos = HipPos = AnklesPos = Vector3.zero;
			Width = Height = ArmLength = 0.0f;
		}
	}

	#endregion

	#region Fields

	// contains player indices / calibration positions
	Dictionary<int, BodyData> m_dicCalibrationPositions = new Dictionary<int, BodyData>();
	// normal handedness
	public Handedness playerHandedness = Handedness.Right;

	#endregion

	#region Properties

	// get player calibration position (if player exists)
	public BodyData GetPlayerCalibration(int playerIndex)
	{
		BodyData pos;
		if (m_dicCalibrationPositions.TryGetValue(playerIndex, out pos))
		{
			return pos;
		}
		else
		{
			return new BodyData();
		}
	}

	// gets last gesture of a player
	public KinectGestures.Gestures GetLastGesture(int playedIndex)
	{
		if (m_dicCalibrationPositions.ContainsKey(playedIndex))
		{
			return m_dicCalibrationPositions[playedIndex].LastGesture;
		}
		else
		{
			return KinectGestures.Gestures.None;
		}
	}

	// gets bool if a gesture was completed in a timeframe
	public bool WasGestureCompletedInTime(int playerIndex, KinectGestures.Gestures gesture, float timeframe)
	{
		if (m_dicCalibrationPositions.ContainsKey(playerIndex))
		{
			if (m_dicCalibrationPositions[playerIndex].CompletedGestures.ContainsKey(gesture))
			{
				List<float> completeTimes = m_dicCalibrationPositions[playerIndex].CompletedGestures[gesture];
				// Return if there was a gesture of type completed in timeframe
				return completeTimes.Exists(
					delegate (float completedTime)
					{
						return Time.timeSinceLevelLoad - completedTime <= timeframe;
					});
			}
		}
		// Player / Gesture not in dictionary
		return false;
	}

	// sets last gesture of a player
	public void SetLastGesture(int playerIndex, KinectGestures.Gestures gesture)
	{
		if (m_dicCalibrationPositions.ContainsKey(playerIndex))
		{
			m_dicCalibrationPositions[playerIndex].SaveGesture(gesture);
		}
	}

	#endregion

	#region Functions

	// add or update player
	public void AddPlayer(int playerIndex, BodyData calibrationPos)
	{
		if (m_dicCalibrationPositions.ContainsKey(playerIndex))
		{
			m_dicCalibrationPositions[playerIndex] = calibrationPos;
		}
		else
		{
			m_dicCalibrationPositions.Add(playerIndex, calibrationPos);
		}
		Debug.Log("Added " + m_dicCalibrationPositions[playerIndex].ToString());
	}

	// remove player
	public void RemovePlayer(int playerIndex)
	{
		Debug.Log("Removed " + m_dicCalibrationPositions[playerIndex].ToString());
		m_dicCalibrationPositions.Remove(playerIndex);
	}

	#endregion

}
