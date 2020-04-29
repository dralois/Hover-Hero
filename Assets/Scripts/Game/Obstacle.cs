using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Obstacle : MonoBehaviour
{

	public float maxTimeSinceGesture = 2.0f;
	public KinectGestures.Gestures[] expectedGestures = { };

	private bool _hasGesturePassed = false;
	public bool HasGesturePassed { get { return _hasGesturePassed; } }

	private void OnTriggerEnter(Collider other)
	{
		// Only if collision with player
		if (other.CompareTag("ObstacleTrigger"))
		{
			int playerIndex = other.gameObject.GetComponentInChildren<AvatarController>().playerIndex;
			// Check if one of the expected gesture was completed in expected timeframe
			foreach(KinectGestures.Gestures gesture in expectedGestures)
			{
				if(PlayerManager.Instance.WasGestureCompletedInTime(playerIndex, gesture, maxTimeSinceGesture))
				{
					// If so, make save ignore collision
					_hasGesturePassed = true;
				}
			}
		}
	}

}
