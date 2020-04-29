using UnityEngine;

public abstract class Singleton<T> : Singleton where T : MonoBehaviour
{
	#region  Fields

	private static T _instance;

	private static readonly object Lock = new object();

	[SerializeField]
	private bool _persistent = true;
	
	#endregion

	#region  Properties

	public static T Instance
	{
		get
		{
			if (Quitting)
			{
				Debug.LogWarning(typeof(T).ToString() + " singleton instance will not be returned because the application is quitting.");
				return null;
			}
			lock (Lock)
			{
				if (_instance != null)
					return _instance;
				var instances = FindObjectsOfType<T>();
				var count = instances.Length;
				if (count > 0)
				{
					if (count == 1)
						return _instance = instances[0];
					Debug.LogWarning("There should never be more than one " + typeof(T).ToString() + " singleton in the scene, but " +
						count.ToString() + " were found. The first instance found will be used, and all others will be destroyed.");
					for (var i = 1; i < instances.Length; i++)
						Destroy(instances[i]);
					return _instance = instances[0];
				}

				Debug.Log("An instance is needed in the scene and no existing instances were found, so a new instance will be created.");
				return _instance = new GameObject(typeof(T).ToString() + "_Singleton").AddComponent<T>();
			}
		}
	}

	#endregion

	#region  Methods

	private void Awake()
	{
		if (_persistent)
			DontDestroyOnLoad(gameObject);
		OnAwake();
	}

	protected virtual void OnAwake() { }

	#endregion
}

public abstract class Singleton : MonoBehaviour
{
	#region  Properties

	public static bool Quitting { get; private set; }

	#endregion

	#region  Methods

	private void OnApplicationQuit()
	{
		Quitting = true;
	}
	
	#endregion
}