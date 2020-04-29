using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyBehaviour : MonoBehaviour
{

	#region Enums

	public enum EnemyType
	{
		Neutral,
		Meele,
		Ranged
	}

	#endregion

	#region Fields

	public EnemyType Type = EnemyType.Neutral;
	public bool invurnable = false;
	public int meeleDamage = 10;
	public int rangeDamage = 25;
	public GameObject Projectile;
	public float ProjectileSpeed = 10;
	public float range = 0;
	public float cooldown = 5;
	public int rewardPoints = 20;

	private GameObject Player;
	private GameObject currentProjectile;
	private float timeToFire = 0;
	private float distanceToPlayer = 0;
	public Transform target;

	private Animator animator;
	private Quaternion originalRotation;

	public float distanceToGround = 0;
	public LayerMask ground;

	private bool pauseLookAtPlayer = false;
	private float timeToDealDamage = 0;
	private bool shouldDoDamage = false;
	private float timeToContinueLook = 0;

	public Vector3 playerTargetOffset = new Vector3(0, 1, 0);
	public Vector3 selfTargetOffset = new Vector3(0, 0, 0);

	#endregion

	#region Functions

	void AwardPoints()
	{
		if (GameManager.Instance.ScoreManagerInstance != null)
			GameManager.Instance.ScoreManagerInstance.AddPoints(rewardPoints);
	}

	void FireProjectile()
	{
		Debug.Log("enemy fired projectile");
		currentProjectile = Instantiate(Projectile, this.transform.position, Quaternion.identity);
		ProjectileController p = currentProjectile.GetComponent<ProjectileController>();
		p.creator = "Enemy";
		p.MoveDir = (target.position + playerTargetOffset - this.transform.position).normalized * ProjectileSpeed;
		p.damage = rangeDamage;
	}

	void MeeleAttack()
	{
		shouldDoDamage = true;
		if (HasParameter("ToAttack", animator))
		{
			timeToDealDamage = 0.3f; //Time in seconds until damage is dealt
			animator.SetTrigger("ToAttack");
		}
		else if (HasParameter("ToAttack1", animator) && HasParameter("ToAttack2", animator))
		{
			timeToDealDamage = 0.6f; //Time in seconds until damage is dealt
			if (Random.Range(0f, 1f) > 0.5f)
			{
				animator.SetTrigger("ToAttack1");
			}
			else
			{
				animator.SetTrigger("ToAttack2");
			}
		}
	}

	public static bool HasParameter(string paramName, Animator animator)
	{
		foreach (AnimatorControllerParameter param in animator.parameters)
		{
			if (param.name == paramName)
				return true;
		}
		return false;
	}

	void LookAtPlayer()
	{
		if (pauseLookAtPlayer)
			return;
		Transform t = transform;
		t.LookAt(target.transform.position + playerTargetOffset);
		Vector3 rot = t.rotation.eulerAngles;
		//rot.y = originalRotation.eulerAngles.y;
		rot.x = originalRotation.eulerAngles.x;
		rot.z = originalRotation.eulerAngles.z;

		this.transform.rotation = Quaternion.Euler(rot);
	}

	void SetHeight()
	{
		RaycastHit rayhitDown;
		if (Physics.Raycast(transform.position, Vector3.down, out rayhitDown, 10, ground))
		{
			Vector3 ground = transform.position - new Vector3(0, rayhitDown.distance, 0);
			transform.position = ground + new Vector3(0, distanceToGround, 0);
		}
		else
		{
			Debug.LogWarning("Enemy didn't find ground: " + this.name);
		}
	}

	void Die()
	{
		float minTime = 0.0f;
		if (animator != null)
		{
			if (HasParameter("ToDie", animator))
			{
				animator.SetTrigger("ToDie");
				minTime = 1.5f;
			}
		}
		// Fetch blood script and play explosion
		BloodExplosion blood = gameObject.GetComponent<BloodExplosion>();
        if (blood != null)
        {
            blood.DeathExplode();
		// Delay destroy until done
		    StartCoroutine(DelayDestroy(Mathf.Max(minTime, blood.bloodDuration)));
		    StartCoroutine(DestroyBlood(blood.bloodDuration));
        }
        else
        {
            StartCoroutine(DelayDestroy(minTime));
        }

	}

	private IEnumerator DestroyBlood(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		Destroy(gameObject.GetComponent<BloodExplosion>());
	}

	private IEnumerator DelayDestroy(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		Destroy(this.gameObject);
	}

	#region Unity

	void Start()
	{
		Player = FindObjectOfType<AvatarController>().gameObject;
		if (Player != null)
			Player.GetComponent<ItemController>().enemies.Add(this);

		CharacterCollision[] collidersInChildren = Player.GetComponentsInChildren<CharacterCollision>();
		foreach (var cc in collidersInChildren)
		{
			if (cc.bodyPart == CharacterCollision.BodyPart.Torso)
			{
				target = cc.transform;
				break;
			}
		}

		animator = this.GetComponent<Animator>();
		originalRotation = transform.rotation;
		SetHeight();
	}

	void Update()
	{
		LookAtPlayer();

		if (Type == EnemyType.Ranged)
		{
			distanceToPlayer = (target.position - this.transform.position).magnitude;
			if (distanceToPlayer <= range && timeToFire <= 0)
			{
				FireProjectile();
				timeToFire = cooldown;
			}
			else
			{
				timeToFire -= Time.deltaTime;
			}
		}
		if (Type == EnemyType.Meele)
		{
			distanceToPlayer = (target.position - this.transform.position).magnitude;
			if (distanceToPlayer <= range && timeToFire <= 0)
			{
				MeeleAttack();
				timeToFire = cooldown;
			}
			else
			{
				timeToFire -= Time.deltaTime;
				// Attack after animation has played
				if (timeToDealDamage < 0.0f && distanceToPlayer <= range && shouldDoDamage)
				{
					GameManager.Instance.DamageControllerInstance.ChangeHealth(meeleDamage, DamageController.DamageType.Enemy, "enemy attack");
					timeToDealDamage = 0.0f;
					shouldDoDamage = false;
				}
				else if (timeToDealDamage > 0.0f)
				{
					timeToDealDamage -= Time.deltaTime;
				}
			}
		}

		if (timeToContinueLook <= 0)
		{
			pauseLookAtPlayer = false;
		}
		else
		{
			timeToContinueLook -= Time.deltaTime;
			pauseLookAtPlayer = true;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (invurnable)
			return;

		if (collision.transform.tag == "Projectile")
		{
			ProjectileController proj = collision.gameObject.GetComponent<ProjectileController>();
			if (proj.creator == "player")
			{
				AudioManager.Instance.Play("Explosion");
				AwardPoints();
				Die();
			}
		}
		else if (collision.transform.CompareTag("Sword"))
		{
			AudioManager.Instance.Play("Swing");
			AwardPoints();
			Die();
		}
	}

	private void OnDestroy()
	{
		if (Player != null)
			Player.GetComponent<ItemController>().enemies.Remove(this);
	}

	#endregion

	#endregion

}
