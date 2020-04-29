using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Element : MonoBehaviour, IEditorReady {

    public Biome Biome;

    public Transform start;
    public Transform end;

    public List<GameObject> ItemSpawnPoints;
    public GameObject terrain;
    public TerrainData terrainDataDuplicate;
    public GameObject OrigianlTerrainCorner;
    public GameObject terrainCenter;
    public Vector3 CornerToCenter;

    [Range(0,1)]
    public float spawnChanceFaktor = 1;
    [Range(0,1)]
    public float spawnRepeatFaktor = 1;


    [SerializeField] public DecoSpawn[] decoSpawns;
    [Range(0, 1)]
    public float decoChance = 0.5f;

    public List<GameObject> EnemySpawnPoints;

	// Use this for initialization
	void Start () {
        if (Biome == null)
        {
            Debug.LogWarning("Element " + this.name + " without Biome");
        }

        foreach (var item in ItemSpawnPoints)
        {
            item.GetComponent<MeshRenderer>().enabled = false;
            item.GetComponent<Collider>().enabled = false;
        }

        foreach (var sp in EnemySpawnPoints)
        {
            sp.GetComponent<MeshRenderer>().enabled = false;
            sp.GetComponent<Collider>().enabled = false;
        }

        ActivateRandom(this);
        //if (EnemySpawnPoints == null||EnemySpawnPoints.Count == 0) EnemySpawnPoints = ItemSpawnPoints; //this is tmp TODO add own Enemy Spawnpoints
	}

    public void AfterSpawning()
    {
        if (terrain == null)
            return;
        //Rotate terrain back
        terrain.transform.rotation = Quaternion.Euler(0, 0, 0);
       
        //generate a new rotated one
        Vector3 elementRotation = OrigianlTerrainCorner.transform.rotation.eulerAngles;
        Debug.Log("element rotation: " + elementRotation);
        float angle = 360-elementRotation.y;
        Debug.Log("rotating degree: " + angle);
        RuntimeTerrainRotator rttr = new RuntimeTerrainRotator();
        GameObject nGo = new GameObject();
        nGo.layer = terrain.layer;
        nGo.transform.parent = terrain.transform.parent;
        nGo.name = "Generated Terrain";
        rttr.RotateTerrain(terrain, nGo, angle, terrainDataDuplicate);
        //reset position of terrain
        Vector3 newCornerToCenter = Quaternion.Euler(0, elementRotation.y, 0) * CornerToCenter;
        //GameObject n = new GameObject(); n.transform.position = OrigianlTerrainCorner.transform.position + CornerToCenter; n.name = "Original Corner + Corner To Center";
        //GameObject nn = new GameObject(); nn.transform.position = OrigianlTerrainCorner.transform.position + newCornerToCenter; nn.name = "Original Corner + new To Center";
        //GameObject nnn = new GameObject(); nnn.transform.position = terrainCenter.transform.position + CornerToCenter; nnn.name = "Center + Corner To Center";
        //GameObject nnnn = new GameObject(); nnnn.transform.position = terrainCenter.transform.position + newCornerToCenter; nnnn.name = "Center + new CornerToCenter";
        
        nGo.transform.position = OrigianlTerrainCorner.transform.position + newCornerToCenter + -CornerToCenter;
    }


    public string CheckReady()
    {
        string ret = "";
        if (Biome == null)
            ret += "Element: Biome missing\n";
        if (start == null)
            ret += "Element: Start missing\n";
        if (end == null)
            ret += "Element: End missing\n";
        if(terrain==null && GetComponentInChildren<Terrain>() != null)
        {
            ret += "Element: Add TerrainObject\n";
        }else if (terrain != null)
        {
            if (terrainDataDuplicate == null)
                ret += "Element: Add TerrainData DUPLICATE\n";
            if (OrigianlTerrainCorner == null)
                ret += "Element: Add Terrain Center Object\n";
        }
        return ret;
    }

    public void SetMarkersActive(bool active, bool recalc)
    {
        
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(Element))]
    public class ElementEditor : Editor
    {
        [SerializeField] bool editMode = true;
        [SerializeField] string releaseFacts = "";
        [SerializeField] bool markersVisible = false;
        [SerializeField] bool markersVisibleLast = true;
        List<IEditorReady> eR = new List<IEditorReady>();

        public override void OnInspectorGUI()
        {
            Element myTarget = (Element)target;
            base.DrawDefaultInspector();


            GUIStyle redLabel = new GUIStyle(EditorStyles.label);
            redLabel.normal.textColor = Color.red;
            GUIStyle greenLabel = new GUIStyle(EditorStyles.label);
            greenLabel.normal.textColor = Color.green;


            GUILayout.Space(10);
            DrawUILine(Color.gray);
            editMode = GUILayout.Toggle(editMode, "Edit Mode");

            if (editMode)
            {
                GUILayout.Space(5);
                EditButtons(myTarget);
            }

            GUILayout.Space(10);
            //if (GUILayout.Button("Check ready manually"))
            //{
            //    releaseFacts = CheckReleaseElement(myTarget);
            //}
            releaseFacts = CheckReleaseElement(myTarget);
            if(releaseFacts != "")
            {
                GUILayout.Label(releaseFacts,redLabel);
            }
            else
            {
                GUILayout.Label("prefab is ready", greenLabel);
            }
            
        }


        //Editor Methods

        public void EditButtons(Element myTarget)
        {
            if (GUILayout.Button("Find Deco Spawns"))
            {
                Undo.RecordObject(myTarget, "Find deco Spawns");
                myTarget.decoSpawns = myTarget.GetComponentsInChildren<DecoSpawn>(true);
                PrefabUtility.RecordPrefabInstancePropertyModifications(myTarget);
            }
            if (GUILayout.Button("Detect in Deco Spawns"))
            {
                Undo.RecordObjects(myTarget.decoSpawns, "Detect in Deco Spawns");
                foreach (var item in myTarget.decoSpawns)
                {
                    
                    //Undo.RecordObject(item, "Detect in Deco Spawns");
                    item.GetChildren();
                }
            }
            if (GUILayout.Button("Random deco test"))
            {
                ActivateRandom(myTarget);
            }
            if (GUILayout.Button("Activate all deco"))
            {
                foreach (var decoSpawn in myTarget.decoSpawns)
                {
                    foreach (var obj in decoSpawn.objects)
                    {
                        obj.SetActive(true);
                    }
                }
            }
            if (GUILayout.Button("Deactivate all deco"))
            {
                foreach (var decoSpawn in myTarget.decoSpawns)
                {
                    foreach (var obj in decoSpawn.objects)
                    {
                        obj.SetActive(false);
                    }
                }
            }
            GUILayout.Space(10);
            if(GUILayout.Button("Calc vector"))
            {
                Undo.RecordObject(myTarget, "Calc Vector");
                myTarget.CornerToCenter = myTarget.terrainCenter.transform.position - myTarget.OrigianlTerrainCorner.transform.position;
            }
            GUILayout.Space(10);
            markersVisible = GUILayout.Toggle(markersVisible, "Markers visible");
            if (markersVisible && !markersVisibleLast)
            {
                markersVisibleLast = markersVisible;
                foreach (var e in eR)
                {
                    e.SetMarkersActive(true, true);
                    Debug.Log("Markers On");
                }
            }
            else if (!markersVisible && markersVisibleLast)
            {
                foreach (var e in eR)
                {
                    e.SetMarkersActive(false, true);
                    Debug.Log("Markers Off");
                }
                markersVisibleLast = markersVisible;
            }
        }

        public string CheckReleaseElement(Element myTarget)
        {
            string ret = "";
            eR.Clear();
            eR.Add(myTarget);
            var cs = myTarget.GetComponent<CustomSpline>();
            if (cs == null)
                ret += "missing CustomSpline \n";
            else
            {
                eR.Add(cs);
            }

            foreach (var e in eR)
            {
                ret += e.CheckReady();
            }

            bool hasFloor = false;
            foreach (var item in myTarget.GetComponentsInChildren<Collider>())
            {
                if (item.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    hasFloor = true;
                }
            }
            if (!hasFloor)
            {
                ret += "Element: No collider with ground layer\n";
            }
            if (markersVisible)
            {
                ret += "Markers still on";
            }
            return ret;
        }

        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

    }
#endif

    //Editor & Class Methods

    public static void ActivateRandom(Element myTarget)
    {
        foreach (var decoSpawn in myTarget.decoSpawns)
        {
            if (decoSpawn.self)
            {
                if (Random.Range(0f, 1f) <= myTarget.decoChance * decoSpawn.chance)
                {
                    decoSpawn.gameObject.SetActive(true);
                }
                else
                {
                    decoSpawn.gameObject.SetActive(false);
                }
            }
            else
            {
                foreach (var obj in decoSpawn.objects)
                {
                    obj.SetActive(false);
                }

                if (decoSpawn.mustChoose)
                {
                    decoSpawn.objects[Random.Range(0, decoSpawn.objects.Length)].SetActive(true);
                }
                else
                {
                    if (Random.Range(0f, 1f) <= myTarget.decoChance * decoSpawn.chance)
                    {
                        decoSpawn.objects[Random.Range(0, decoSpawn.objects.Length)].SetActive(true);
                    }
                }
            }
        }
    }
}

public interface IEditorReady
{
    string CheckReady();
    void SetMarkersActive(bool active, bool recalc);
}
