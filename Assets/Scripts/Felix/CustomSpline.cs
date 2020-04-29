using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomSpline : MonoBehaviour, IEditorReady {

    //public GameObject ObjectFollowing;
    public List<Transform> trackPoints;
    //public GameObject trackTrailRenderers;
    //public float speed = 0.03f;
    public float cornerstrenght = 1;

    public LTSpline track;
    private float trackPosition; // ratio 0,1 of the avatars position on the track
    [SerializeField] List<GameObject> markers = new List<GameObject>();

    public float distance;
    public float ElementDefaultSpeed;
    public float ManualSpeedFaktor = 1;
    // Use this for initialization
    void Awake () {
        //Vector3[] points = new Vector3[trackPoints.Count+2];
        
        // Optional technique to show the trails in game
		//LeanTween.moveSpline( trackTrailRenderers, track, 2f ).setOrientToPath(true).setRepeat(-1);
	}

    void Start()
    {
        
    }

    private void RecalcMarkers()
    {
        markers.Clear();
        //markers.AddRange(trackPoints.Select(item => item.gameObject));
        markers.AddRange(trackPoints.SelectMany(item => item.GetComponentsInChildren<Transform>()).Select(item => item.gameObject));
    }

    private void SetMarkers(bool b)
    {
        //markers.ForEach(SetAct(b));
        foreach (var item in markers)
        {
            item.GetComponent<MeshRenderer>().enabled = b;
            item.GetComponent<Collider>().enabled = b;
        }
    }

    private System.Action<GameObject> SetAct(bool b)
    {
        return delegate
        {
            GetComponent<MeshRenderer>().enabled = b;
            GetComponent<Collider>().enabled = b;
        };
    }

    public LTSpline CalcSpline()
    {
        if (trackPoints==null || trackPoints.Count < 2)
        {
            Debug.LogError("not enough track points");
        }
        Vector3[] points = getPoints();
        track = new LTSpline(points, true);
        distance = track.distance;
        ElementDefaultSpeed = (1f / distance)*ManualSpeedFaktor;
        return track;
    }

	// Update is called once per frame
	void Update () {
        //// Update avatar's position on correct track
        //track.place(ObjectFollowing.transform, trackPosition);

        //trackPosition += Time.deltaTime * speed;// * Input.GetAxis("Vertical"); // Uncomment to have the forward and backwards controlled by the directional arrows

        //if (trackPosition < 0f) // We need to keep the ratio between 0-1 so after one we will loop back to the beginning of the track
        //    trackPosition = 1f;
        //else if (trackPosition > 1f)
        //    trackPosition = 0f;
    }

    private Vector3[] getPoints()
    {
        Vector3[] points = new Vector3[trackPoints.Count + 2];
        //set first direction
        points[0] = trackPoints[0].position - trackPoints[0].forward*cornerstrenght;
        //set last direction
        points[points.Length-1] = trackPoints[trackPoints.Count-1].position + trackPoints[trackPoints.Count-1].forward*cornerstrenght;
        
        //set points
        for (int i = 0; i < trackPoints.Count; i++)
        {
            points[i + 1] = trackPoints[i].position;
        }
        return points;
    }

    public string CheckReady()
    {
        string ret = "";
        if(trackPoints == null || trackPoints.Count < 2)
        {
            ret += "CustomSpline: missing track Points\n";
        }
        return ret;
    }

    public void SetMarkersActive(bool active, bool recalc)
    {
        if (recalc)
        {
            RecalcMarkers();
        }
        //SetAct(active);
        SetMarkers(active);
    }

    void OnDrawGizmos()
    {
        if (CheckReady() != "")
            return;
        LTSpline.drawGizmo(getPoints(), Color.red);
    }
}
