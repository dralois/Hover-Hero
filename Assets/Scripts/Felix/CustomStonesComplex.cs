using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CustomStonesComplex : MonoBehaviour {

    public GameObject[] prefabs;
    public List<GameObject> parents = new List<GameObject>();

	// Use this for initialization
	void Awake () {

        if(prefabs==null || prefabs.Length <= 0)
        {
            Debug.Log("Specify Prefabs");
            return;
        }
        //DoRandom();
        
    }

    IEnumerator YieldingWork()
    {
        bool workDone = false;

        while (!workDone)
        {
            // Let the engine run for a frame.
            yield return null;

            // Do Work...
            foreach (var parent in parents)
            {
                GameObject pre = GetRandomPrefab();
                GameObject stone = Instantiate(pre, /*new Vector3(0,0,0),Quaternion.Euler(0,0,0) ,*/parent.transform, false);
                stone.transform.localPosition = Vector3.zero;
                Collider c = stone.GetComponent<Collider>();
                RandomizeOrientation(stone);
                parent.GetComponent<MeshRenderer>().enabled = false;
                if (c.enabled)
                {
                    c.enabled = false;
                    c.enabled = true;
                }
                //yield return new WaitForSecondsRealtime(0.0001f);
            }
            workDone = true;
        }
    }

    private GameObject GetRandomPrefab()
    {
        int r = Random.Range(0, prefabs.Length);
        return prefabs[r];
    }

    private void RandomizeOrientation(GameObject g)
    {
        Vector3 newOrientation = new Vector3();
        newOrientation.x = Random.Range(0f, 360f);
        newOrientation.y = Random.Range(0f, 360f);
        newOrientation.z = Random.Range(0f, 360f);

        g.transform.Rotate(newOrientation, Space.Self);
    }

    public void SetChildsAsStoneParents()
    {
        Transform[] t = this.gameObject.GetComponentsInChildren<Transform>();
        parents.Clear();
        foreach (var item in t)
        {
            parents.Add(item.gameObject);
        }
        parents.RemoveAt(0);
    }

    public void DoRandom()
    {
        StartCoroutine(YieldingWork());
    }

    public void OnEnable()
    {
        DoRandom();
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(CustomStonesComplex))]
    public class ComplexStonesEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            CustomStonesComplex myTarget = (CustomStonesComplex)target;
            base.DrawDefaultInspector();

            //myTarget.experience = EditorGUILayout.IntField("Experience", myTarget.experience);
            //EditorGUILayout.LabelField("Level", myTarget.Level.ToString());
            if (GUILayout.Button("SetChildsAsStoneParents"))
            {
                Undo.RecordObject(myTarget, "Set customStone parents");
                myTarget.SetChildsAsStoneParents();
            }
            //if (GUILayout.Button("Do Random"))
            //{
            //    Undo.RegisterCreatedObjectUndo(myTarget, "DoRandom");
            //    myTarget.DoRandom();
            //}
        }
    }
#endif
}