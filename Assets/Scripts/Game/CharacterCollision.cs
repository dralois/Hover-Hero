using UnityEngine;

public class CharacterCollision : MonoBehaviour
{

	#region Fields

	// Bodypart of this collision
	public BodyPart bodyPart;

	#endregion

	#region Enums

	public enum BodyPart
	{
		Hand,
		Foot,
		Torso
	}

	#endregion

	#region Functions

	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Pickup")
		{
			// Fetch item
			Item.ItemInstance item = collision.gameObject.GetComponent<Item>().myInstance;
			// Try add to player
			gameObject.GetComponentInParent<ItemController>().AddItem(item);
			// If fireball change projectile
			if (item.Type == Item.ItemType.FireBall)
			{
				Projectile proj = collision.gameObject.GetComponent<Projectile>();
				gameObject.GetComponentInParent<ItemController>().ChangeProjectile(proj);
			}
			// Play soundeffect
			AudioManager.Instance.Play("Pickup");
			// Delete item
			Destroy(collision.transform.parent.gameObject);
		}
		// Ignore own projectiles
		else if (collision.gameObject.tag == "Projectile")
		{
			GameObject projGo = collision.gameObject;
			ProjectileController proj = projGo.GetComponent<ProjectileController>();

			if (proj.creator == "Enemy")
			{
				AudioManager.Instance.Play("Explosion");
				GameManager.Instance.DamageControllerInstance.ChangeHealth(proj.damage, DamageController.DamageType.Enemy, "projectile");
			}
		}
		else if (bodyPart == BodyPart.Torso && !collision.gameObject.CompareTag("Sword"))
		{
			Obstacle obs = collision.gameObject.GetComponentInParent<Obstacle>();
			if (obs != null)
			{
				if (!obs.HasGesturePassed)
				{
					GameManager.Instance.DamageControllerInstance.ChangeHealth(10, DamageController.DamageType.Obstacle, "obstacle");
				}
				else
				{
					Debug.Log("No damage b/c gesture was passed!");
				}
			}
			else
			{
				Debug.Log("No damage b/c no obstacle attached!");
			}
		}
		else if (collision.gameObject.CompareTag("Coin"))
		{
			GameManager.Instance.ScoreManagerInstance.AddPoints(GameManager.Instance.ScoreManagerInstance.coinPoints);
			// Play soundeffect
			AudioManager.Instance.Play("Coin");
			// Delete item
			Destroy(collision.transform.parent.gameObject);
		}
	}

	#endregion

}
