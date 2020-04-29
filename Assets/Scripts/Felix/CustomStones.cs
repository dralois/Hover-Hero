using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Linq;

public class CustomStones : MonoBehaviour {

    public GameObject[] prefabs;
    private GameObject stone;
    public GameObject parent;

    public GameObject SpawnFromPrefab()
    {
        if(stone!=null)
            DestroyImmediate(stone);
        stone = Instantiate(prefabs[UnityEngine.Random.Range(0,prefabs.Length)], /*new Vector3(0,0,0),Quaternion.Euler(0,0,0) ,*/this.gameObject.transform,false);
        stone.transform.localPosition = Vector3.zero;
        return stone;
    }

    public void RandomizeOrientation()
    {
        Vector3 newOrientation = new Vector3();
        newOrientation.x = UnityEngine.Random.Range(0f, 360f);
        newOrientation.y = UnityEngine.Random.Range(0f, 360f);
        newOrientation.z = UnityEngine.Random.Range(0f, 360f);

        stone.transform.Rotate(newOrientation,Space.Self);
    }

    public static void RandomizeOrientation(GameObject o)
    {
        Vector3 newOrientation = new Vector3();
        newOrientation.x = UnityEngine.Random.Range(0f, 360f);
        newOrientation.y = UnityEngine.Random.Range(0f, 360f);
        newOrientation.z = UnityEngine.Random.Range(0f, 360f);

        o.transform.Rotate(newOrientation, Space.Self);
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(CustomStones))]
[CanEditMultipleObjects]
public class LevelScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //CustomStones myTarget = (CustomStones)target;
        base.DrawDefaultInspector();

        //myTarget.experience = EditorGUILayout.IntField("Experience", myTarget.experience);
        //EditorGUILayout.LabelField("Level", myTarget.Level.ToString());
        if (GUILayout.Button("Spawn from prefab"))
        {
            DoForAllSpawn();
        }
        if (GUILayout.Button("Random Orientation"))
        {
            DoForAllRandom();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("EnableChildren"))
        {
            EnableChildren();
        }
        if (GUILayout.Button("DisableChildren"))
        {
            DisableChildren();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("EnableMesh"))
        {
            EnableMesh();
        }
        if (GUILayout.Button("DisableMesh"))
        {
            DisableMesh();
        }
        GUILayout.EndHorizontal();
    }

    void DoForAllSpawn()
    {
        foreach (CustomStones target in targets)
        {
            foreach (var child in target.GetComponentsInDirectChildren<Transform>(true).Select(item => item.gameObject).ToArray())
            {
                Undo.DestroyObjectImmediate(child);
            }
            GameObject o = target.SpawnFromPrefab();
            Undo.RegisterCreatedObjectUndo(o, "spawned Stone");
        }
    }

    void DoForAllRandom()
    {
        foreach (CustomStones target in targets)
        {
            Undo.RecordObject(target, "random Orientation");
            target.RandomizeOrientation();
        }
    }

    void DisableChildren()
    {
        foreach (CustomStones target in targets)
        {
            foreach (var child in target.GetComponentsInDirectChildren<Transform>(true).Select(item => item.gameObject).ToArray())
            {
                Undo.RecordObject(child, "disable children");
                child.SetActive(false);
            }
        }
    }
    void EnableChildren()
    {
        foreach (CustomStones target in targets)
        {
            foreach (var child in target.GetComponentsInDirectChildren<Transform>(true).Select(item => item.gameObject).ToArray())
            {
                Undo.RecordObject(child, "enable children");
                child.SetActive(true);
            }
        }
    }

    void DisableMesh ()
    {
        foreach (CustomStones target in targets)
        {
            Undo.RecordObject(target, "disable mesh");
            target.GetComponent<MeshRenderer>().enabled = false;
        }
    }
    void EnableMesh()
    {
        foreach (CustomStones target in targets)
        {
            Undo.RecordObject(target, "enable mesh");
            target.GetComponent<MeshRenderer>().enabled = true;
        }
    }
}
#endif