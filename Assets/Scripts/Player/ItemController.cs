using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GestureListener))]
public class ItemController : MonoBehaviour
{

	#region Classes

	[System.Serializable]
	public class HandednessGO : SerializableDictionaryBase<PlayerManager.Handedness, GameObject> { }
	[System.Serializable]
	public class ItemActivation : SerializableDictionaryBase<Item.ItemVersion, HandednessGO> { }

	#endregion

	#region Fields

	// Player ID
	private int playerIndex;

	// currently owned items
	private Dictionary<Item.ItemType, Item.ItemInstance> ownedItems = new Dictionary<Item.ItemType, Item.ItemInstance>();
	private Projectile currProjectile;

	// Projectile info
	private Vector3 projectileStart = Vector3.zero;
	private Vector3 hadoukenMaxSize = Vector3.zero;
	private float hadoukenCharge = 0.0f;
	private int hadoukenCount = 0;
	private Vector3[] hadoukenStarts = new Vector3[10];

	// Reference to the top most transform and item gameobjects
	[SerializeField] private Transform projectileRef;
	[SerializeField] private GameObject shieldActive;
	[HideInInspector] public bool shieldHit;
	private float shieldDeactivateRemaining = 0.0f;
	private float shieldDeactivateDelay = 1.0f;
	public ItemActivation itemObjects;

	// Transforms of model hands
	[SerializeField] private Transform leftHand;
	[SerializeField] private Transform rightHand;

	// Indices of body parts
	private int shoulderLeftIndex;
	private int shoulderRightIndex;
	private int handLeftIndex;
	private int handRightIndex;

	// Enemy stuff
	[HideInInspector]
	public List<EnemyBehaviour> enemies;
	public float autoAimAngle = 20f;

	#endregion

	#region Functions

	#region Unity

	private void Start()
	{
		// add event handler on awake
		GestureListener listener = GetComponent<GestureListener>();
		listener.GestureCompletedEvent += ItemGestureComplete;
		listener.GestureInProgressEvent += ItemGestureInProgress;
		listener.GestureCancelledEvent += AbortItemGesture;

		// Manager is needed
		if (KinectManager.Instance == null)
			return;

		// Save player index
		playerIndex = gameObject.GetComponent<AvatarController>().playerIndex;

		// Find indices of body parts
		shoulderLeftIndex = KinectManager.Instance.GetJointIndex(KinectInterop.JointType.ShoulderLeft);
		shoulderRightIndex = KinectManager.Instance.GetJointIndex(KinectInterop.JointType.ShoulderRight);
		handLeftIndex = KinectManager.Instance.GetJointIndex(KinectInterop.JointType.HandLeft);
		handRightIndex = KinectManager.Instance.GetJointIndex(KinectInterop.JointType.HandRight);

		// Prefill dictionary
		foreach (Item.ItemType type in System.Enum.GetValues(typeof(Item.ItemType)))
		{
			ownedItems.Add(type, new Item.ItemInstance(type, Item.ItemVersion.Invalid, -1));
		}
	}

	private void Update()
	{
		// If hadouken is charging
		if (hadoukenCharge > 0)
		{
			GameObject itemObject;
			HandednessGO handDic;
			// Find hadouken gameobject
			if (itemObjects.TryGetValue(Item.ItemVersion.Hadouken, out handDic))
			{
				if (handDic.TryGetValue(PlayerManager.Instance.GetPlayerCalibration(playerIndex).Handedness, out itemObject))
				{
					// Adjust position
					itemObject.transform.position = (leftHand.position + rightHand.position) / 2.0f;
					// Slowly adjust scale
					itemObject.transform.localScale =
						Vector3.Lerp(itemObject.transform.localScale, hadoukenCharge * hadoukenMaxSize, Time.deltaTime);
				}
			}
		}
		// Shield duration update
		if (shieldDeactivateRemaining > 0.0f)
		{
			shieldDeactivateRemaining -= Time.deltaTime;
			// Remove shield on zero
			if (shieldDeactivateRemaining <= 0.0f)
			{
				GameManager.Instance.DamageControllerInstance.IsShieldUp = false;
				AudioManager.Instance.Stop("ShieldLoop");
				shieldActive.SetActive(false);
			}
		}
	}

	#endregion

	#region Item Handling

	// Trys to give the player the item
	public void AddItem(Item.ItemInstance item)
	{
		GameObject itemObject;
		HandednessGO handDic;

		// Activate gestures
		long userId = KinectManager.Instance.GetUserIdByIndex(playerIndex);
		switch (item.Type)
		{
			case Item.ItemType.FireBall:
				{
					if (item.Version == Item.ItemVersion.FireFlower)
						KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.FireFlower);
					else
					{
						KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.HadoukenCharge);
						KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.HadoukenFire);
					}
					// Remove sword
					RemoveItem(Item.ItemType.Sword);
					break;
				}
			case Item.ItemType.Shield:
				{
					KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.ShieldBegin);
					break;
				}
			case Item.ItemType.Sword:
				{
					KinectManager.Instance.DetectGesture(userId, KinectGestures.Gestures.Sword);
					// Remove fireball
					RemoveItem(Item.ItemType.FireBall);
					break;
				}
			default:
				{
					Debug.LogError("Invalid item type " + item.Type);
					break;
				}
		}

		// If the player doesnt have the item type already
		if (ownedItems[item.Type].Version == Item.ItemVersion.Invalid)
		{
			// Save the version
			ownedItems[item.Type] = item;
		}
		// Otherwise exchange
		else
		{
			// Disable old gameobject
			if (itemObjects.TryGetValue(ownedItems[item.Type].Version, out handDic))
			{
				if (handDic.TryGetValue(PlayerManager.Instance.GetPlayerCalibration(playerIndex).Handedness, out itemObject))
				{
					if (itemObject != null)
						itemObject.SetActive(false);
				}
			}
			// Save new version
			ownedItems[item.Type] = item;
		}

		// Enable corresponding gameobject
		if (itemObjects.TryGetValue(ownedItems[item.Type].Version, out handDic))
		{
			if (handDic.TryGetValue(PlayerManager.Instance.GetPlayerCalibration(playerIndex).Handedness, out itemObject))
			{
				// Hadouken is not activated here
				if (itemObject != null && item.Version != Item.ItemVersion.Hadouken)
					itemObject.SetActive(true);
			}
		}
	}

	// Removes all items the player has
	public void RemoveAllItems()
	{
		foreach (Item.ItemType type in System.Enum.GetValues(typeof(Item.ItemType)))
		{
			if(type != Item.ItemType.Invalid)
				RemoveItem(type);
		}
	}

	// Removes an item
	private void RemoveItem(Item.ItemType type)
	{
		GameObject itemObject;
		HandednessGO handDic;
		Item.ItemInstance item;

		// Deactivate gesture
		long userId = KinectManager.Instance.GetUserIdByIndex(playerIndex);
		if (ownedItems.TryGetValue(type, out item))
		{
			switch (type)
			{
				case Item.ItemType.FireBall:
					{
						if (ownedItems[type].Version == Item.ItemVersion.FireFlower)
							KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.FireFlower);
						else
						{
							KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.HadoukenCharge);
							KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.HadoukenFire);
						}
						break;
					}
				case Item.ItemType.Shield:
					{
						KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.ShieldBegin);
						shieldDeactivateRemaining = shieldDeactivateDelay;
						shieldHit = true;
						break;
					}
				case Item.ItemType.Sword:
					{
						KinectManager.Instance.DeleteGesture(userId, KinectGestures.Gestures.Sword);
						break;
					}
				default:
					{
						break;
					}
			}
		}

		// Disable gameobject
		if (itemObjects.TryGetValue(ownedItems[type].Version, out handDic))
		{
			if (handDic.TryGetValue(PlayerManager.Instance.GetPlayerCalibration(playerIndex).Handedness, out itemObject))
			{
				if (itemObject != null)
					itemObject.SetActive(false);
			}
		}
	}

	// Removes one use from the item type
	public bool TryUseItem(Item.ItemType type)
	{
		if (ownedItems[type].RemainingUses > 0)
		{
			// Remove one use
			ownedItems[type].RemainingUses--;
			// Disable item if uses reached 0
			if (ownedItems[type].RemainingUses == 0)
			{
				RemoveItem(type);
			}
			return true;
		}
		return false;
	}

	// Change current projectile type
	public void ChangeProjectile(Projectile proj)
	{
		// Just override it
		currProjectile = proj;
	}

	#endregion

	#region Gesture Handling

	// Gets called by gesture cancelled event
	private void AbortItemGesture(KinectGestures.Gestures gesture, long userId)
	{
		// Abort Hadouken if charge / fire was interrupted
		if ((gesture == KinectGestures.Gestures.HadoukenCharge && hadoukenCharge > 1.0f) ||
			gesture == KinectGestures.Gestures.HadoukenFire)
		{
			GameObject itemObject;
			HandednessGO handDic;
			// Make Hadouken in player hands invisible
			if (itemObjects.TryGetValue(Item.ItemVersion.Hadouken, out handDic))
			{
				if (handDic.TryGetValue(PlayerManager.Instance.GetPlayerCalibration(playerIndex).Handedness, out itemObject))
				{
					if (itemObject != null)
						itemObject.SetActive(false);
				}
			}
			// Reset charge
			projectileStart = Vector3.zero;
			hadoukenCharge = 0.0f;
		}
	}

	// Gets called by gesture in progress event
	private void ItemGestureInProgress(KinectGestures.Gestures gesture, float progress, long userId)
	{
		// Charge Hadouken
		if (gesture == KinectGestures.Gestures.HadoukenCharge)
		{
			// Max charge is 1
			hadoukenCharge = Mathf.Clamp01(progress * 2.0f);
			// Save last few gesture positions
			hadoukenStarts[hadoukenCount] = (KinectManager.Instance.GetJointPosition(userId, handLeftIndex) +
																			KinectManager.Instance.GetJointPosition(userId, handRightIndex)) / 2.0f;
			hadoukenCount++;
			hadoukenCount = hadoukenCount % hadoukenStarts.Length;
			// Invalidate after 10s
			if (progress > 9.9f)
				hadoukenCharge = 1.1f;
		}
		// Save start position for fire flower
		else if (gesture == KinectGestures.Gestures.FireFlower)
		{
			// Only if start is zero
			if (projectileStart.magnitude == 0)
				projectileStart = KinectManager.Instance.GetJointPosition(userId, handRightIndex);
		}
	}

	// Gets called by gesture completed event
	private void ItemGestureComplete(KinectGestures.Gestures gesture, long userId)
	{
		// Shield activation
		if (gesture == KinectGestures.Gestures.ShieldBegin)
		{
			if (shieldActive != null)
			{
				shieldActive.SetActive(true);
				GameManager.Instance.DamageControllerInstance.IsShieldUp = true;
				AudioManager.Instance.Play("ShieldStart");
				AudioManager.Instance.Play("ShieldLoop");
			}
		}
		// Shield deactivation
		else if (gesture == KinectGestures.Gestures.ShieldEnd)
		{
			shieldDeactivateRemaining = shieldDeactivateDelay;
		}
		// If Hadouken is charging (first call)
		else if (gesture == KinectGestures.Gestures.HadoukenCharge)
		{
			//set startvalues toa average
			for (int i = 0; i < hadoukenStarts.Length; i++)
			{
				hadoukenStarts[i] = (KinectManager.Instance.GetJointPosition(userId, handLeftIndex) +
														KinectManager.Instance.GetJointPosition(userId, handRightIndex)) / 2.0f;
			}
			GameObject itemObject;
			HandednessGO handDic;
			// Find hadouken gameobject
			if (itemObjects.TryGetValue(Item.ItemVersion.Hadouken, out handDic))
			{
				if (handDic.TryGetValue(PlayerManager.Instance.GetPlayerCalibration(playerIndex).Handedness, out itemObject))
				{
					if (itemObject != null)
					{
						// Set active
						itemObject.SetActive(true);
						// Adjust position
						itemObject.transform.position = (leftHand.position + rightHand.position) / 2.0f;
						// Save scale the first time
						if (hadoukenMaxSize.magnitude == 0)
							hadoukenMaxSize = itemObject.transform.localScale;
						// Make size zero
						itemObject.transform.localScale = Vector3.zero;
					}
				}
			}
		}
		// Shoot Hadouken
		else if (gesture == KinectGestures.Gestures.HadoukenFire)
		{
			// Use a charge
			if (TryUseItem(Item.ItemType.FireBall))
			{
				GameObject itemObject;
				HandednessGO handDic;
				// Calculate hand pos of character
				Vector3 charHandPos = (leftHand.position + rightHand.position) / 2.0f;
				// Calculate gesture end position
				Vector3 projectileEnd =
					(KinectManager.Instance.GetJointPosition(userId, handLeftIndex) +
					KinectManager.Instance.GetJointPosition(userId, handRightIndex)) / 2.0f;
				// Calculate Hadouken direction
				Vector3 projectileStartTmp = Vector3.zero;
				foreach (var startSample in hadoukenStarts)
				{
					projectileStartTmp += startSample;
				}
				Vector3 hadoukenDir = projectileEnd - (projectileStartTmp / hadoukenStarts.Length);
				hadoukenDir.Scale(new Vector3(1, 1, -1));
				hadoukenDir = Vector3.Normalize(projectileRef.TransformDirection(hadoukenDir));
				// Adjust direction
				hadoukenDir = AutoAim(hadoukenDir, charHandPos, autoAimAngle);
				// If invalid direction / no charge -> no projectile is spawned
				if (hadoukenDir.magnitude > 0.0f && hadoukenCharge > 0.0f)
				{
					// Spawn projectile
					GameObject newProj = Instantiate(currProjectile.ProjectilePrefab, charHandPos, Quaternion.identity);
					newProj.transform.localScale = hadoukenCharge * newProj.transform.localScale;
					// Add velocity in direction etc.
					ProjectileController projCon = newProj.GetComponent<ProjectileController>();
					projCon.MoveDir = hadoukenDir * currProjectile.Speed;
					projCon.damage = (int)(currProjectile.Damage * hadoukenCharge);
					projCon.creator = "player";
					// Play soundeffect
					AudioManager.Instance.Play("Hadouken");
				}
				// Make Hadouken in player hands invisible
				if (itemObjects.TryGetValue(Item.ItemVersion.Hadouken, out handDic))
				{
					if (handDic.TryGetValue(PlayerManager.Instance.GetPlayerCalibration(playerIndex).Handedness, out itemObject))
					{
						itemObject.SetActive(false);
					}
				}
				// Reset charge
				projectileStart = Vector3.zero;
				hadoukenCharge = 0.0f;
			}
		}
		// Shoot fireball
		else if (gesture == KinectGestures.Gestures.FireFlower)
		{
			// Uses up a charge
			if (TryUseItem(Item.ItemType.FireBall))
			{
				// Fetch handedness
				PlayerManager.Handedness handedness =
					PlayerManager.Instance.GetPlayerCalibration(KinectManager.Instance.GetBodyIndexByUserId(userId)).Handedness;
				// Fetch hand pos of character
				Vector3 charHandPos = handedness == PlayerManager.Handedness.Right ? rightHand.position : leftHand.position;
				// Calculate gesture end position
				Vector3 projectileEnd = handedness == PlayerManager.Handedness.Right ?
						KinectManager.Instance.GetJointPosition(userId, handRightIndex) :
						KinectManager.Instance.GetJointPosition(userId, handLeftIndex);
				// Calculate fireball direction
				Vector3 throwDir = projectileEnd - projectileStart;
				throwDir.Scale(new Vector3(1, 1, -1));
				throwDir = Vector3.Normalize(projectileRef.TransformDirection(throwDir));
				// Spawn projectile
				GameObject newProj = Instantiate(currProjectile.ProjectilePrefab, charHandPos, Quaternion.identity);
				// Adjust direction
				throwDir = AutoAim(throwDir, charHandPos, autoAimAngle);
				// Add velocity in direction etc.
				ProjectileController projCon = newProj.GetComponent<ProjectileController>();
				projCon.MoveDir = throwDir * currProjectile.Speed;
				projCon.damage = currProjectile.Damage;
				projCon.creator = "player";
				// Play soundeffect
				AudioManager.Instance.Play("Fireball");
				// Reset
				projectileStart = Vector3.zero;
				hadoukenCharge = 0.0f;
			}
		}
	}

	// Checks and corrects angle if possible
	private Vector3 AutoAim(Vector3 direction, Vector3 handPos, float angle = 15)
	{
		foreach (var enemy in enemies)
		{
			Vector3 dirEnemy = enemy.transform.position + enemy.selfTargetOffset - handPos;
			float currentAngle = Vector3.Angle(direction, dirEnemy);
			if (currentAngle < 0)
				currentAngle *= -1;
			if (currentAngle < angle)
				return dirEnemy.normalized;
		}
		return direction;
	}

	#endregion

	#endregion

}
