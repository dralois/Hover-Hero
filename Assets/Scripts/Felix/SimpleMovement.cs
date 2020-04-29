using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovement : MonoBehaviour {

    public float speed;
    Rigidbody rb;
	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (Input.GetKey(KeyCode.W))
        {
            //this.transform.Translate(Vector3.forward*speed);
            if(rb.velocity.magnitude<=5)
            rb.AddForce(Vector3.forward*speed, ForceMode.Force);
        }
	}
}
