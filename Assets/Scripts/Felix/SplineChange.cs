using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineChange : MonoBehaviour
{

    public GenManager genManager;
    public SplineFollow splineFollow;
    
    public float distanceRun = 0;
    private float distancePastElements = 0;


    // Use this for initialization
    void Start()
    {
        if (genManager == null)
            genManager = FindObjectOfType<GenManager>();
        if (splineFollow == null)
            splineFollow = FindObjectOfType<SplineFollow>();
    }

    // Update is called once per frame
    void Update()
    {
        if (splineFollow.done)
        {
            distancePastElements += splineFollow.customSpline.distance;
            Element entering = genManager.EnteringAuto();
            CustomSpline cS = entering.GetComponent<CustomSpline>();
            splineFollow.SetSpline(cS);
            splineFollow.ResetFollow();
            splineFollow.AdjustSpeed(cS.ElementDefaultSpeed);
            AvatarController ac = FindObjectOfType<AvatarController>();
            if (ac != null)
            {
                ac.moveRate = entering.Biome.moveRate;
            }
        }
        distanceRun = distancePastElements + splineFollow.trackPosition*splineFollow.customSpline.distance;
        if (GameManager.Instance != null && GameManager.Instance.ScoreManagerInstance != null)
            GameManager.Instance.ScoreManagerInstance.SetDistancePoints(Mathf.FloorToInt(distanceRun / 10));
    }
}
