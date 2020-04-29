//original Terrain Rotate tool by UnityCoder.com
//https://assetstore.unity.com/packages/tools/terrain/terrain-rotator-21303


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RuntimeTerrainRotator
{
    //private float angle = 0; // rotation angle
    private float oldAngle = -1; // previous angle
    private bool isRotating = false; // are we currently rotating

    private float[,] origHeightMap; // original heightmap, unrotated
    private int[][,] origDetailLayer; // original detail layer, unrotated
    private float[,,] origAlphaMap; // original alphamap, unrotated
    private TreeInstance[] origTrees; // original trees, unrotated

    private bool grabOriginal = false; // have we grabbed original data
    private Terrain terrainOriginal;
    private Terrain terrainTmp;
    //private Terrain terrainNew;

    public void RotateTerrain(GameObject original, GameObject newGameObject, float angle, TerrainData terrainDataDuplicate)
    {
        terrainOriginal = original.GetComponent<Terrain>();
        terrainTmp = terrainOriginal;
        Terrain newT = newGameObject.AddComponent<Terrain>();
        TerrainCollider tc = newGameObject.AddComponent<TerrainCollider>(terrainTmp.GetComponent<TerrainCollider>());
        //newT.terrainData = (TerrainData)Object.Instantiate(terrainTmp.terrainData);
        newT.terrainData = terrainDataDuplicate;
        tc.terrainData = newT.terrainData;
        //tc.terrainData.

        ReadData();
        DoRotate(angle, newT);
        original.SetActive(false);
    }

    private void ReadData(/*Terrain t*/)
    {
        //terrainOriginal = t;
        origHeightMap = terrainTmp.terrainData.GetHeights(0, 0, terrainTmp.terrainData.heightmapWidth, terrainTmp.terrainData.heightmapHeight);
        origDetailLayer = new int[terrainTmp.terrainData.detailPrototypes.Length][,];
        for (int n = 0; n < terrainTmp.terrainData.detailPrototypes.Length; n++)
        {
            origDetailLayer[n] = terrainTmp.terrainData.GetDetailLayer(0, 0, terrainTmp.terrainData.detailWidth, terrainTmp.terrainData.detailHeight, n);
        }
        origAlphaMap = terrainTmp.terrainData.GetAlphamaps(0, 0, terrainTmp.terrainData.alphamapWidth, terrainTmp.terrainData.alphamapHeight);
        origTrees = terrainTmp.terrainData.treeInstances;
        //angle = 0;
        oldAngle = 0;
        grabOriginal = true;
    }

    private void DoRotate(float angle, Terrain newTerrain)
    {
        if (terrainTmp ==null||origHeightMap == null)
        {
            grabOriginal = false;
            Debug.LogWarning("No terrain to rotate");
            return;
        }

        isRotating = true;

        //Terrain terrain = o.GetComponent<Terrain>();

        int nx, ny;
        float cs, sn;

        // heightmap rotation
        int tw = terrainTmp.terrainData.heightmapWidth;
        int th = terrainTmp.terrainData.heightmapHeight;
        float[,] newHeightMap = new float[tw, th];
        float angleRad = angle * Mathf.Deg2Rad;
        float heightMiddle = (terrainTmp.terrainData.heightmapResolution) / 2.0f; // pivot at middle

        for (int y = 0; y < th; y++)
        {
            for (int x = 0; x < tw; x++)
            {
                cs = Mathf.Cos(angleRad);
                sn = Mathf.Sin(angleRad);

                nx = (int)((x - heightMiddle) * cs - (y - heightMiddle) * sn + heightMiddle);
                ny = (int)((x - heightMiddle) * sn + (y - heightMiddle) * cs + heightMiddle);

                if (nx < 0) nx = 0;
                if (nx > tw - 1) nx = tw - 1;
                if (ny < 0) ny = 0;
                if (ny > th - 1) ny = th - 1;

                newHeightMap[x, y] = origHeightMap[nx, ny];
            } // for x
        } // for y



        // detail layer (grass, meshes)
        int dw = terrainTmp.terrainData.detailWidth;
        int dh = terrainTmp.terrainData.detailHeight;
        float detailMiddle = (terrainTmp.terrainData.detailResolution) / 2.0f; // pivot at middle
        int numDetails = terrainTmp.terrainData.detailPrototypes.Length;
        int[][,] newDetailLayer = new int[numDetails][,];

        // build new layer arrays
        for (int n = 0; n < numDetails; n++)
        {
            newDetailLayer[n] = new int[dw, dh];
        }

        for (int z = 0; z < numDetails; z++)
        {
            for (int y = 0; y < dh; y++)
            {
                for (int x = 0; x < dw; x++)
                {
                    cs = Mathf.Cos(angleRad);
                    sn = Mathf.Sin(angleRad);

                    nx = (int)((x - detailMiddle) * cs - (y - detailMiddle) * sn + detailMiddle);
                    ny = (int)((x - detailMiddle) * sn + (y - detailMiddle) * cs + detailMiddle);


                    if (nx < 0) nx = 0;
                    if (nx > dw - 1) nx = dw - 1;
                    if (ny < 0) ny = 0;
                    if (ny > dh - 1) ny = dh - 1;

                    newDetailLayer[z][x, y] = origDetailLayer[z][nx, ny];
                } // for x
            } // for y
        } // for z


        // alpha layer (texture splatmap) rotation
        dw = terrainTmp.terrainData.alphamapWidth;
        dh = terrainTmp.terrainData.alphamapHeight;
        int dz = terrainTmp.terrainData.alphamapLayers;
        float alphaMiddle = (terrainTmp.terrainData.alphamapResolution) / 2.0f; // pivot at middle
        float[,,] newAlphaMap = new float[dw, dh, dz];
        float[,,] origAlphaMapCopy;
        origAlphaMapCopy = origAlphaMap.Clone() as float[,,];

        for (int z = 0; z < dz; z++)
        {
            for (int y = 0; y < dh; y++)
            {
                for (int x = 0; x < dw; x++)
                {
                    cs = Mathf.Cos(angleRad);
                    sn = Mathf.Sin(angleRad);

                    nx = (int)((x - alphaMiddle) * cs - (y - alphaMiddle) * sn + alphaMiddle);
                    ny = (int)((x - alphaMiddle) * sn + (y - alphaMiddle) * cs + alphaMiddle);

                    if (nx < 0) nx = 0;
                    if (nx > dw - 1) nx = dw - 1;
                    if (ny < 0) ny = 0;
                    if (ny > dh - 1) ny = dh - 1;

                    newAlphaMap[x, y, z] = origAlphaMapCopy[nx, ny, z];
                } // for x
            } // for y
        } // for z



        // trees rotation, one by one..
        // TODO: use list instead, then can remove trees outside the terrain
        int treeCount = terrainTmp.terrainData.treeInstances.Length;
        TreeInstance[] newTrees = new TreeInstance[treeCount];
        Vector3 newTreePos = Vector3.zero;
        float tx, tz;

        for (int n = 0; n < treeCount; n++)
        {

            cs = Mathf.Cos(angleRad);
            sn = Mathf.Sin(angleRad);

            tx = origTrees[n].position.x - 0.5f;
            tz = origTrees[n].position.z - 0.5f;

            newTrees[n] = origTrees[n];

            newTreePos.x = (cs * tx) - (sn * tz) + 0.5f;
            newTreePos.y = origTrees[n].position.y;
            newTreePos.z = (cs * tz) + (sn * tx) + 0.5f;

            newTrees[n].position = newTreePos;
        } // for treeCount

        // this is too slow in unity..
        //Undo.RecordObject(terrain.terrainData,"Rotate terrain ("+angle+")");

        // Apply new data to terrain
        //newTerrain.terrainData.treeInstances = newTrees;
        newTerrain.terrainData.treeInstances = new TreeInstance[] { /*newTrees[0]*/}; //Just a test to delete tree colliders
        newTerrain.terrainData.SetHeightsDelayLOD(0, 0, newHeightMap); // splitting up SetHeights part1
        newTerrain.ApplyDelayedHeightmapModification(); //part2
        //newTerrain.terrainData.treeInstances = newTrees;
        newTerrain.terrainData.SetAlphamaps(0, 0, newAlphaMap);
        for (int n = 0; n < terrainTmp.terrainData.detailPrototypes.Length; n++)
        {
            newTerrain.terrainData.SetDetailLayer(0, 0, n, newDetailLayer[n]);
        }

        // we are done..
        isRotating = false;

    } //TerrainRotate
}
