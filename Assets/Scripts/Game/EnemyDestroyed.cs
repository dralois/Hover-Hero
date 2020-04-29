using UnityEngine;

public class EnemyDestroyed : MonoBehaviour
{
	public delegate void EnemyDestroyedDel();
	public event EnemyDestroyedDel EnemyDestroyedEvent;

	private void OnDestroy()
	{
		if (EnemyDestroyedEvent != null)
			EnemyDestroyedEvent();
	}
}
