using UnityEngine;

public class Item : MonoBehaviour
{
	#region Classes

	/// <summary>
	/// Instance of an item
	/// </summary>
	public class ItemInstance
	{
		public Item.ItemType Type { get; private set; }
		public Item.ItemVersion Version { get; private set; }
		public int MaxUses { get; private set; }
		public int RemainingUses { get; set; }

		public ItemInstance(Item.ItemType pi_Type, Item.ItemVersion pi_Version, int pi_iMaxUses)
		{
			Type = pi_Type;
			Version = pi_Version;
			MaxUses = pi_iMaxUses;
			RemainingUses = pi_iMaxUses;
		}
	}

	#endregion

	#region Enums

	// all item types
	public enum ItemType
	{
		Invalid = -1,
		Sword = 0,
		Shield,
		FireBall
	}

	// all item version
	public enum ItemVersion
	{
		Invalid = -1,
		MasterSword = 0,
		HaloSword,
		MinecraftSword,
		HylianShield,
		CAShield,
		Hadouken,
		FireFlower
	}

	#endregion

	#region Fields

	[SerializeField]
	private ItemType type;
	[SerializeField]
	private ItemVersion version;
	[SerializeField]
	private int maxUses;
	// Instance of the item, generated at create time
	[HideInInspector]
	public ItemInstance myInstance;
	[SerializeField]
	private Vector3 spinAround = Vector3.up;

	#endregion

	#region Functions

	private void Start()
	{
		// Save an instance in the gameobject
		myInstance = new ItemInstance(type, version, maxUses);
	}

	private void Update()
	{
		// Slowly rotates item (about 2s to complete)
		transform.parent.Rotate(spinAround, 180.0f* Time.deltaTime);
	}

	#endregion
}
