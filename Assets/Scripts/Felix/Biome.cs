using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Biome")]
public class Biome : ScriptableObject {

    public string BiomeName = "";

    public List<GameObject> TransitionsOutPrefabs;
    public BiomeDictionary transitionsOut;
    public List<Element> elements;
    [Range(0,1)]
    public float BiomeSpawnChance = 1;
    [Range(0, 1)]
    public float BiomeRepeatChance = 1;
    public int MinSpawnedElements = 10;
    [Range(0,1)]
    public float StayChance = 0.5f;

    [HideInInspector]
    public int ElementsSpawned = 0;
    private int r_ElementSpawned;

    public float moveRate = 7;

    [HideInInspector]
    /// <summary>
    /// string: element name
    /// int: times spawned
    /// </summary>
    public Dictionary<string, float> chances = new Dictionary<string, float>();

    public void Init()
    {
        chances.Clear();
        foreach (var element in elements)
        {
            if(element.Biome != this)
            {
                Debug.LogWarning("Biome on Element: " + element.name + " messed up");
            }
            chances.Add(element.name, element.spawnChanceFaktor);
        }

        transitionsOut.Clear();
        foreach (var gO in TransitionsOutPrefabs)
        {
            Element e = gO.GetComponent<Element>();
            transitionsOut.Add(e.Biome, gO);
        }
    }

    public void SetResetValues()
    {
        r_ElementSpawned = ElementsSpawned;
    }

    public void DoReset()
    {
        ElementsSpawned = r_ElementSpawned;
    }

}

[System.Serializable]
public class BiomeDictionary : RotaryHeart.Lib.SerializableDictionary.SerializableDictionaryBase<Biome, GameObject>
{

}
