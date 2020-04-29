using UnityEngine;

public class GestureListener : MonoBehaviour, KinectGestures.GestureListenerInterface
{

	#region Declarations

	// player index of this listener
	private int playerIndex = -1;

	// gestures that are being tracked
	[SerializeField]
	private KinectGestures.Gestures[] detectedGestures = { };

	#endregion

	#region Events

	/// <summary>
	/// Called when a gesture was completed
	/// </summary>
	/// <param name="userId">ID of the user</param>
	/// <param name="gesture">Completed gesture</param>
	public delegate void GestureCompletedDel(KinectGestures.Gestures gesture, long userId);
	public event GestureCompletedDel GestureCompletedEvent;

	/// <summary>
	/// Called when a gesture is in progress
	/// </summary>
	/// <param name="gesture">ID of the user</param>
	/// <param name="userId">In progress gesture</param>
	public delegate void GestureInProgressDel(KinectGestures.Gestures gesture, float progress, long userId);
	public event GestureInProgressDel GestureInProgressEvent;

	/// <summary>
	/// Called when a gesture was cancelled
	/// </summary>
	/// <param name="userId">ID of the user</param>
	/// <param name="gesture">Cancelled gesture</param>
	public delegate void GestureCancelledDel(KinectGestures.Gestures gesture, long userId);
	public event GestureCancelledDel GestureCancelledEvent;

	/// <summary>
	/// Called when a player was detected
	/// </summary>
	/// <param name="userId">ID of the user</param>
	public delegate void PlayerDetectedDel(long userId);
	public event PlayerDetectedDel PlayerDetectedEvent;

	/// <summary>
	/// Called when a player was lost
	/// </summary>
	/// <param name="userId">ID of the user</param>
	public delegate void PlayerLostDel(long userId);
	public event PlayerLostDel PlayerLostEvent;

	#endregion

	#region Properties

	// Get player index of this listener
	public int PlayerIndex
	{
		get { return playerIndex; }
	}

	#endregion

	#region Functions

	#region Interface

	public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint)
	{
		if (userIndex != playerIndex)
			return false;

		// shield is continous -> check for cancelled, the end of the gesture
		if (gesture == KinectGestures.Gestures.ShieldBegin)
		{
			// save in manager
			PlayerManager.Instance.SetLastGesture(userIndex, KinectGestures.Gestures.ShieldEnd);
			// raise event shield end manually
			if (GestureCompletedEvent != null)
				GestureCompletedEvent(KinectGestures.Gestures.ShieldEnd, userId);

			Debug.Log(PlayerManager.Instance.GetLastGesture(userIndex) + "@" + Time.timeSinceLevelLoad);
		}
		else
		{
			// otherwise last gesture none
			PlayerManager.Instance.SetLastGesture(userIndex, KinectGestures.Gestures.None);
			// raise event
			if (GestureCancelledEvent != null)
				GestureCancelledEvent(gesture, userId);
		}

		return true;
	}

	public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint, Vector3 screenPos)
	{
		if (userIndex != playerIndex)
			return false;

		// save in manager
		PlayerManager.Instance.SetLastGesture(userIndex, gesture);

		Debug.Log(PlayerManager.Instance.GetLastGesture(userIndex) + "@" + Time.timeSinceLevelLoad);

		// raise event
		if (GestureCompletedEvent != null)
			GestureCompletedEvent(gesture, userId);

		return true;
	}

	public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture, float progress, KinectInterop.JointType joint, Vector3 screenPos)
	{
		if (userIndex != playerIndex)
			return;

		// shield and hadouken charge are continuous gestures -> check for begin
		if (gesture == KinectGestures.Gestures.ShieldBegin || gesture == KinectGestures.Gestures.HadoukenCharge)
		{
			// at start progress is 0.1
			if (progress <= 0.1f)
			{
				// save in manager
				PlayerManager.Instance.SetLastGesture(userIndex, gesture);

				Debug.Log(PlayerManager.Instance.GetLastGesture(userIndex) + "@"  + Time.timeSinceLevelLoad);

				// raise event
				if (GestureCompletedEvent != null)
					GestureCompletedEvent(gesture, userId);
			}
		}

		// raise event
		if (GestureInProgressEvent != null)
			GestureInProgressEvent(gesture, progress, userId);
	}

	public void UserDetected(long userId, int userIndex)
	{
		if (userIndex != playerIndex)
			return;

		// get manager
		KinectManager manager = KinectManager.Instance;
		// create body data
		PlayerManager.BodyData newData =
			new PlayerManager.BodyData(userId, PlayerManager.Instance.playerHandedness);
		// save in manager
		PlayerManager.Instance.AddPlayer(playerIndex, newData);

		// add all gestures that are being tracked
		foreach (KinectGestures.Gestures gesture in detectedGestures)
		{
			manager.DetectGesture(userId, gesture);
		}

		// fire event that a user was added
		if (PlayerDetectedEvent != null)
			PlayerDetectedEvent(userId);
	}

	public void UserLost(long userId, int userIndex)
	{
		if (userIndex != playerIndex)
			return;

        PlayerManager.Instance.RemovePlayer(userIndex);

		// fire event that a user was lost
		if (PlayerLostEvent != null)
			PlayerLostEvent(userId);
	}

	#endregion

	#region Unity

	private void Awake()
	{
		// get the mono scripts. avatar controllers and gesture listeners are among them
		MonoBehaviour[] monoScripts = gameObject.GetComponents<MonoBehaviour>();

		// locate the available avatar controllers
		foreach (MonoBehaviour monoScript in monoScripts)
		{
			if ((monoScript is AvatarController) && monoScript.enabled)
			{
				playerIndex = ((AvatarController)monoScript).playerIndex;
			}
			if ((monoScript is PointmanController) && monoScript.enabled)
			{
				playerIndex = ((PointmanController)monoScript).playerIndex;
			}
			if ((monoScript is CubemanController) && monoScript.enabled)
			{
				playerIndex = ((CubemanController)monoScript).playerIndex;
			}
		}

		// at least one controller must be in the scene
		if (playerIndex == -1)
			Debug.LogError("No valid character controller in scene!");
	}

	#endregion

	#endregion

}
