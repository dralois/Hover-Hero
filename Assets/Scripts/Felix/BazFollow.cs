using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezFollow : MonoBehaviour {

    public GameObject ObjectFollowing;
    public Transform[] trackPoints;
    public GameObject trackTrailRenderers;
    public float speed = 0.03f;

    private LTBezierPath track;
    private float trackPosition; // ratio 0,1 of the avatars position on the track

    // Use this for initialization
    void Start () {
        Vector3[] points = new Vector3[trackPoints.Length];
        for(int i=0; i<points.Length;i++)
        {
            points[i] = trackPoints[i].position;
        }
        track = new LTBezierPath(points);

        // Optional technique to show the trails in game
		//LeanTween.moveSpline( trackTrailRenderers, track, 2f ).setOrientToPath(true).setRepeat(-1);
	}
	
	// Update is called once per frame
	void Update () {
        // Update avatar's position on correct track
        track.place(ObjectFollowing.transform, trackPosition);

        trackPosition += Time.deltaTime * speed;// * Input.GetAxis("Vertical"); // Uncomment to have the forward and backwards controlled by the directional arrows

        if (trackPosition < 0f) // We need to keep the ratio between 0-1 so after one we will loop back to the beginning of the track
            trackPosition = 1f;
        else if (trackPosition > 1f)
            trackPosition = 0f;
    }

    void OnDrawGizmos()
    {
        LTSpline.drawGizmo(trackPoints, Color.red);
    }
}
