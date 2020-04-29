using UnityEngine;

public class ProjectileController : MonoBehaviour
{
	private Vector3 moveDir;
	[HideInInspector]
	public string creator;
	[HideInInspector]
	public int damage;

	public Vector3 MoveDir
	{
		set { moveDir = value; }
	}

	private void OnBecameInvisible()
	{
		// simple destroy call
		Destroy(gameObject);
	}

	private void Update()
	{
		transform.Translate(moveDir * Time.deltaTime);
	}
}
