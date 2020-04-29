using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeTest : MonoBehaviour
{
    public TerrainData terrainDataDuplicate;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Presed R");
            GameObject t = FindObjectOfType<Terrain>().gameObject;
            RuntimeTerrainRotator rttr = new RuntimeTerrainRotator();
            GameObject nGo = new GameObject();
            rttr.RotateTerrain(t, nGo, 45, terrainDataDuplicate);
        }
    }
}
