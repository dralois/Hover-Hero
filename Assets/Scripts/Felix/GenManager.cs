using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GenManager : MonoBehaviour
{


	public int maxSpawnedElements;
	//public GameObject[] allElementObjects;
	public GameObject[] preSpawned;

	public GameObject[] items;
	public GameObject itemAlternative;

	public Biome[] Biomes;
	public Biome StartBiome;
	[SerializeField] private Biome currentBiome;
	private Dictionary<Biome, float> BiomeChances = new Dictionary<Biome, float>();
	public bool MustUseTransition = false;

	[Range(0, 1)]
	public float ResetItemSpawnChance = 0.1f;

	public bool spawnItems = true;

	public Element currentElement;

	private List<GameObject> spawned = new List<GameObject>();
	/// <summary>
	/// string: element name
	/// int: times spawned
	/// </summary>
	private Dictionary<string, float> chances = new Dictionary<string, float>();
	private int countEverSpawnedElements = 0;
	//private List<Element> allElements = new List<Element>();

	private float tmpItemSpawnChance;

	public GameObject[] Enemies;
	[Range(0, 1)]
	public float ResetEnemySpawnChance = 0.1f;
	public bool spawnEnemies = true;
	private float tmpEnemySpawnChance;

    private List<GameObject> rangedEnemies = new List<GameObject>();
    private List<GameObject> closeEnemies = new List<GameObject>();

    public bool readSettings = true;

	public void Start()
	{
        //read Settings
        if (GameManager.Instance != null && readSettings)
        {
            GameSettings s = GameManager.Instance.gameSettings;
            spawnEnemies = s.SpawnEnemies;
            ResetEnemySpawnChance = s.EnemySpawnChance;
            StartBiome = Biomes[s.StartBiome];
        }

		spawned.AddRange(preSpawned);
		//foreach (var item in allElementObjects)
		//{
		//    Element e = item.GetComponent<Element>();
		//    allElements.Add(e);
		//    chances.Add(item.name, e.spawnChanceFaktor);
		//}

		foreach (var item in Biomes)
		{
			BiomeChances.Add(item, item.BiomeSpawnChance);
		}



		if (preSpawned != null && preSpawned.Length > 0)
		{
			currentElement = preSpawned[0].GetComponent<Element>();
		}
		else
		{
			Debug.LogWarning("no prespawned and no first spline");
		}

		if (StartBiome == null)
		{
			if (currentElement != null)
			{
				currentBiome = currentElement.Biome;
			}
			else
			{
				currentBiome = ChooseNextBiome();
			}
		}
		else
		{
			currentBiome = StartBiome;
		}


		foreach (var biome in Biomes)
		{
			biome.Init();
		}

		tmpItemSpawnChance = ResetItemSpawnChance;
		tmpEnemySpawnChance = ResetEnemySpawnChance;
        foreach (var enemy in Enemies)
        {
            if(enemy.GetComponent<EnemyBehaviour>().Type == EnemyBehaviour.EnemyType.Ranged)
            {
                rangedEnemies.Add(enemy);
            }
            else
            {
                closeEnemies.Add(enemy);
            }
        }

		StartSpawnFull();
    }



	private GameObject getNewest()
	{
		if (spawned != null && spawned.Count != 0)
		{
			int count = spawned.Count;
			//Debug.Log("got newest: "+ spawned[count - 1]);
			return spawned[count - 1];
		}

		return null;
	}

	private GameObject getOldest()
	{
		if (spawned != null && spawned.Count != 0)
		{
			//Debug.Log("got oldest: " + spawned[0]);
			GameObject g = spawned[0];
			spawned.RemoveAt(0);
			return g;
		}
		return null;
	}

	private Transform getEndpoint(GameObject go)
	{
		if (go != null)
		{
			Element e = go.GetComponent<Element>();
			if (e != null)
			{
				//Debug.Log("got endpoint: " + e.end);
				return e.end;
			}
		}
		return null;
	}

	private GameObject GetNextElementToSpawn()
	{
		if (currentBiome.elements != null && currentBiome.elements.Count != 0)
		{
			//Debug.Log("choosing elemnt to spawn: ");

			Element chosen = null;

			float sum = 0;
			float partSum = 0;
			foreach (var item in currentBiome.elements)
			{
				sum += currentBiome.chances[item.name];
			}
			float r = Random.Range(0, sum);
			foreach (var item in currentBiome.elements)
			{
				partSum += currentBiome.chances[item.name];
				if (partSum > r)
				{
					//got it
					chosen = item;
					break;
				}
			}
			if (chosen != null)
			{
				currentBiome.chances[chosen.name] *= chosen.spawnRepeatFaktor;
				//chosen.spawnChanceFaktor *= chosen.spawnRepeatFaktor;
				return chosen.gameObject;
			}

		}
		Debug.LogError("No Elements");
		return null;
	}

	private Element SpawnElement(Transform spawnpoint, GameObject element)
	{
		//spawn element
		Element elementBlueprint = element.GetComponent<Element>();
		//GameObject n = Instantiate(element, new Vector3(0,100,0), spawnpoint.rotation);
		GameObject n = Instantiate(element, new Vector3(0, 100, 0), Quaternion.identity);
		n.transform.rotation = spawnpoint.rotation;

		Element elementSpawned = n.GetComponent<Element>();

		//translate position
		Transform startOfSpawned = elementSpawned.start;
		Vector3 elementTranslation = n.transform.position - startOfSpawned.position;
		n.transform.position = spawnpoint.position + elementTranslation;


		SpawnItems(n.GetComponent<Element>().ItemSpawnPoints);

		SpawnEnemies(n.GetComponent<Element>().EnemySpawnPoints);
		//if (chances.ContainsKey(e.name))
		//{
		//    int i = chances[e.name];
		//    chances[e.name] = i + 1;
		//    Debug.Log("statistics " + e.name + " at "+ chances[e.name]);
		//}
		//else
		//{
		//    Debug.Log("adding " + e.name + " to statistics");
		//    chances.Add(e.name, 1);
		//}

		elementSpawned.AfterSpawning();

		spawned.Add(n);
		return elementSpawned;
	}

	private Biome ChooseNextBiome()
	{
		//biome where transition to exists
		if (MustUseTransition)
		{
			List<Biome> possibleBiomes = new List<Biome>();
			possibleBiomes = currentBiome.transitionsOut.Keys.ToList<Biome>();

			if (possibleBiomes != null && possibleBiomes.Count != 0)
			{
				//Debug.Log("choosing elemnt to spawn: ");

				Biome chosen = null;

				float sum = 0;
				float partSum = 0;
				foreach (var item in possibleBiomes)
				{
					sum += BiomeChances[item];
				}
				float r = Random.Range(0, sum);
				foreach (var item in possibleBiomes)
				{
					partSum += BiomeChances[item];
					if (partSum > r)
					{
						//got it
						chosen = item;
						break;
					}
				}
				if (chosen != null)
				{
					BiomeChances[chosen] *= chosen.BiomeRepeatChance;
					//chosen.spawnChanceFaktor *= chosen.spawnRepeatFaktor;
					return chosen;
				}

			}
			Debug.LogError("No Transitions -> no next Biomes");
			return null;
		}

		//any biome
		if (Biomes != null && Biomes.Length != 0)
		{
			//Debug.Log("choosing elemnt to spawn: ");

			Biome chosen = null;

			float sum = 0;
			float partSum = 0;
			foreach (var item in Biomes)
			{
				sum += BiomeChances[item];
			}
			float r = Random.Range(0, sum);
			foreach (var item in Biomes)
			{
				partSum += BiomeChances[item];
				if (partSum > r)
				{
					//got it
					chosen = item;
                    Debug.Log("Chose Biome " + chosen.BiomeName + " with chance: " + BiomeChances[item] + " of " + sum + " @ " + Time.timeSinceLevelLoad);
					break;
				}
			}
			if (chosen != null)
			{
				BiomeChances[chosen] *= chosen.BiomeRepeatChance;
				//chosen.spawnChanceFaktor *= chosen.spawnRepeatFaktor;
				return chosen;
			}

		}
		Debug.LogError("No Biomes");
		return null;
	}

	public bool CheckNewBiome()
	{
        Debug.Log("Check new Biome Count: " + currentBiome.ElementsSpawned +"/"+ currentBiome.MinSpawnedElements + " @ " + Time.timeSinceLevelLoad);
        int minBiomeLength = currentBiome.MinSpawnedElements;
        if (GameManager.Instance.gameSettings.BiomeLength > 0)
        {
            minBiomeLength = GameManager.Instance.gameSettings.BiomeLength;
        }
        if (currentBiome.ElementsSpawned < minBiomeLength)
		{
			//Debug.Log("no change because min");
			return false;
		}

		float r = Random.Range(0f, 1f);
        Debug.Log("Check new Biome Chance: " + r + "/" + currentBiome.StayChance + " @ " + Time.timeSinceLevelLoad);
        if (r <= currentBiome.StayChance)
		{
			//Debug.Log("no change because chance");
			return false;
		}
		return true;
	}

	public void Entering(Element element)
	{
		currentElement = element;
		GameObject nextSpawn;
		if (CheckNewBiome())
		{

			//TODO reset Biome
			currentBiome.ElementsSpawned = 0;
			Biome nextBiome = ChooseNextBiome();
			if (MustUseTransition)
			{
				nextSpawn = currentBiome.transitionsOut[nextBiome];
			}
			else
			{
				currentBiome = nextBiome;
				nextSpawn = GetNextElementToSpawn();
			}
			currentBiome = nextBiome;
			//Debug.Log("changing Biome to " + currentBiome.BiomeName + " @ " + Time.timeSinceLevelLoad);
			//TODO make shure transition will get spawned
		}
		else
		{
			nextSpawn = GetNextElementToSpawn();
		}


		if (nextSpawn != null)
		{
			//Debug.Log("spawning next");
			SpawnElement(getEndpoint(getNewest()), nextSpawn);
			currentBiome.ElementsSpawned++;
		}
		else
		{
			Debug.LogError("No next Element");
		}
	}

	public Element EnteringAuto()
	{
		int currentindex = spawned.FindIndex(e => e.GetComponent<Element>() == currentElement);
		Element entering = spawned[currentindex + 1].GetComponent<Element>();
		Entering(entering);
		Leaving();
		return entering;
	}

	public void Leaving()
	{
		if (spawned.Count > maxSpawnedElements)
		{
			Destroy(getOldest());
		}
	}

	public void StartSpawnFull(int less = 1)
	{
		int toSpawn = maxSpawnedElements - spawned.Count - less;
		for (int i = 0; i < toSpawn; i++)
		{
			SpawnElement(getEndpoint(getNewest()), GetNextElementToSpawn());
		}
	}

	//------------------------------------------------
	//                Spawnables
	//------------------------------------------------


	public void SpawnItems(List<GameObject> spawnPoints)
	{
		foreach (var item in spawnPoints)
		{
			float r = Random.Range(0f, 1f);
			if (r <= tmpItemSpawnChance)
			{
				if (spawnItems)
					Instantiate(items[Random.Range(0, items.Length)], item.transform);
				tmpItemSpawnChance = ResetItemSpawnChance;
			}
			else
			{
				Instantiate(itemAlternative, item.transform);
				tmpItemSpawnChance += tmpItemSpawnChance;//+tmpItemSpawnChance;//Increase SpawnChance exponantially?
			}
		}
	}

	public void SpawnEnemies(List<GameObject> spawnPoints)
	{
		foreach (var enemySpawnpoint in spawnPoints)
		{
			float r = Random.Range(0f, 1f);
			if (r <= tmpEnemySpawnChance)
			{
                if (spawnEnemies)
                {
                    if (enemySpawnpoint.GetComponent<EnemySpawn>().enemyType == EnemyBehaviour.EnemyType.Ranged)
                    {
                        Instantiate(rangedEnemies[Random.Range(0, rangedEnemies.Count)], enemySpawnpoint.transform);
                    }
                    else
                    {
                        Instantiate(closeEnemies[Random.Range(0, closeEnemies.Count)], enemySpawnpoint.transform);
                    }
                }
				
					
				tmpEnemySpawnChance = ResetEnemySpawnChance;
			}
			else
			{
				tmpEnemySpawnChance += tmpEnemySpawnChance;//+tmpItemSpawnChance;//Increase SpawnChance exponantially?
			}
		}
	}
}
