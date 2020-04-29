using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoverboard : MonoBehaviour
{
    public GameObject _Hoverboard;
    public GameObject FrontPosition;
    public GameObject BackPosition;

    public GameObject FrontFeet;
    public GameObject BackFeet;

    private Vector3 FrontOffset;
    private Vector3 BackOffset;
    private Vector3 MidOffset;
    private Quaternion initRotHoverboard;

    // Start is called before the first frame update
    void Start()
    {
        FrontOffset = FrontPosition.transform.position - _Hoverboard.transform.position;
        BackOffset = BackPosition.transform.position - _Hoverboard.transform.position;
        MidOffset = FrontOffset+ (BackOffset - FrontOffset) / 2;
        initRotHoverboard = _Hoverboard.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 MidFeet = BackFeet.transform.position + (FrontFeet.transform.position - BackFeet.transform.position) / 2;
        _Hoverboard.transform.position = MidFeet - MidOffset;
        Vector3 FeetVector = (FrontFeet.transform.position - BackFeet.transform.position).normalized;
        _Hoverboard.transform.rotation = Quaternion.LookRotation(FeetVector, Vector3.up) * initRotHoverboard;


    }
}
