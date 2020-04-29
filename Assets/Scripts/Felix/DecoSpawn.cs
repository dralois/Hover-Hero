using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class DecoSpawn : MonoBehaviour {

    public bool self = true;
    public bool mustChoose = true;
    public bool autoTakeChildren = true;
    [SerializeField] public GameObject[] objects;
    [Range(0,2)]
    public float chance = 1f;

    public void GetChildren()
    {
        if (self)
        {
            objects = new GameObject[] { this.gameObject };
        }
        else if(autoTakeChildren)
        {
            objects = transform.GetComponentsInDirectChildren<Transform>(true).Select(item=>item.gameObject).ToArray();

            if (objects == null || objects.Length == 0)
            {
                Debug.LogWarning("No Children in Deco Spawn of " + this.gameObject.name);
            }
        }
        else
        {
            Debug.Log("Manual deco on: " + this.gameObject.name);
        }
        
    }


    //Editor Stuff
#if UNITY_EDITOR
    [CustomEditor(typeof(DecoSpawn))]
    public class DecoSpawnEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            DecoSpawn myTarget = (DecoSpawn)target;
            base.DrawDefaultInspector();

            if (GUILayout.Button("Detect in Deco Spawns"))
            {
                Undo.RecordObject(myTarget, "Detect in Deco Spawns");
                myTarget.GetChildren();
            }
        }
    }


    public void RecordChange()
    {
        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }
#endif
}
