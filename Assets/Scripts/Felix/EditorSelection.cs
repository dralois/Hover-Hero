using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;

public class EditorSelection : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(EditorSelection))]
    public class EditorSelectionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorSelection myTarget = (EditorSelection)target;
            base.OnInspectorGUI();
            if(GUILayout.Button("Select StoneStuff"))
            {
                Selection.objects = myTarget.GetComponentsInChildren<CustomStones>().Select(item => item.gameObject).ToArray();
            }
            if (GUILayout.Button("Select Stones"))
            {
                Transform[] stonesStuff = myTarget.GetComponentsInDirectChildren<CustomStones>().Select(item => item.transform).ToArray();
                List<Transform> stones = new List<Transform>();
                foreach (var item in stonesStuff)
                {
                    stones.Add(item.GetComponentInDirectChildren<Transform>());
                }
                Selection.objects = stones.ToArray().Select(item => item.gameObject).ToArray();
            }
        }
    }
#endif
}
