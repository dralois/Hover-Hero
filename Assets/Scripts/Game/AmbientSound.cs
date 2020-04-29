using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AmbientSound : MonoBehaviour
{
	[SerializeField]
	private string[] SoundsToPlay = { };

	private void OnTriggerEnter(Collider other)
	{
		// Only if collision with player
		if (other.CompareTag("ObstacleTrigger"))
		{
			AudioManager.Instance.PlayRandom(SoundsToPlay);
		}
	}
}
