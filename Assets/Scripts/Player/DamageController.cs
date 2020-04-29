using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class DamageController : MonoBehaviour
{

	#region Enums

	// All damage types
	public enum DamageType
	{
		Enemy = 0,
		Obstacle,
	}

	#endregion

	#region Fields

	[SerializeField] private bool isInvurnable = false;
	[SerializeField] private float startHealth = 100;
	[SerializeField] private float obstacleDamage = 10;
	[SerializeField] private Image healthBar;
	[SerializeField] private float DamageCooldown = 2.0f;

	private float health;
	private bool shieldUp;
	private bool isDead;
	private float cooldown;

	#endregion

	#region Properties

	public bool IsDead
	{
		get { return isDead; }
	}
	public float GetHealth
	{
		get { return health; }
	}

	public bool IsShieldUp
	{
		get { return shieldUp; }
		set { shieldUp = value; }
	}

	#endregion

	#region Functions

	void Start()
	{
		health = startHealth;
		isDead = false;
	}

	public bool ChangeHealth(float amount, DamageType type, string reason)
	{
		// Change damage amount if obstacle was hit
		if (type == DamageType.Obstacle && cooldown <= 0.0f)
		{
			amount = obstacleDamage;
			cooldown = DamageCooldown;
		}
		else if (type != DamageType.Enemy)
		{
			Debug.Log("Still on damage cooldown!");
			return false;
		}

		if (shieldUp && type == DamageType.Enemy)
		{
			FindObjectOfType<ItemController>().TryUseItem(Item.ItemType.Shield);
			StrangeParticles shield = FindObjectOfType<StrangeParticles>();
			if (shield != null)
			{
				AudioManager.Instance.Play("ShieldHit");
				shield.Burst();
			}
			Debug.Log("No damage b/c shield up!");
			return false;
		}

		// Invurnable
		if (isInvurnable)
			return isDead = false;

		Debug.Log("Reducing by " + amount + " because of " + reason);

		// Adjust damage / visuals
		health -= amount;
		if(healthBar != null)
			healthBar.fillAmount = health / startHealth;

		// Play sound effect
		AudioManager.Instance.PlayRandom(new string[] { "DeathRoblox", "HurtMinecraft" });
        
        // Blood effect
        FindObjectOfType<ItemController>().GetComponentInChildren<BloodExplosion>().DeathExplode();

		if (health <= 0)
			return isDead = true;
		else
			return isDead = false;
	}

	private void Update()
	{
		if (cooldown >= 0)
		{
			cooldown -= Time.deltaTime;
		}
	}

	#endregion

}
