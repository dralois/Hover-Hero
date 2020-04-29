using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Jump : MonoBehaviour
{

	public bool shouldJump = false;
	public float distanceToGround = 1;
	public LayerMask ground;
	public float jumpVelocity = 100;

	RaycastHit rayhitDown;

	private bool isJumping = false;
	private Rigidbody playerRigidbody;

	// Use this for initialization
	void Start()
	{
		if (playerRigidbody == null)
		{
			playerRigidbody = GetComponent<Rigidbody>();
		}

		GestureListener gl = FindObjectOfType<GestureListener>();
		gl.PlayerDetectedEvent += gl_PlayerDetectedEvent;
		gl.PlayerLostEvent += gl_PlayerLostEvent;

		gl.GestureCompletedEvent += gl_GestureCompletedEvent;
	}

	private void gl_GestureCompletedEvent(KinectGestures.Gestures gesture, long userId)
	{
		if (gesture == KinectGestures.Gestures.ImprovedJump)
		{
			DoJump();
		}
	}

	private void gl_PlayerLostEvent(long userId)
	{
		//transform.position = new Vector3(0, 0, 0);
	}

	private void gl_PlayerDetectedEvent(long userId)
	{
		//throw new System.NotImplementedException();
	}

	// Update is called once per frame
	void Update()
	{

		if (isJumping)
		{
			if (Grounded())
			{
				isJumping = false;
			}
		}
		else if (shouldJump)
		{
			//Debug.Log("Trying to jump "+isJumping+", "+Grounded()+"@" + Time.timeSinceLevelLoad);
			DoJumpStuff();
		}

	}

	public void DoJump()
	{
		shouldJump = true;
	}

	private void DoJumpStuff()
	{
		if (isJumping)
			return;

		if (shouldJump && Grounded())
		{
			isJumping = true;
			shouldJump = false;
			//playerRigidbody.constraints = RigidbodyConstraints.None;
			//playerRigidbody.freezeRotation = true;

			playerRigidbody.AddForce(0, jumpVelocity, 0);
			//Debug.Log("Jump force @" + Time.timeSinceLevelLoad);
			//StartCoroutine(groundedCo());
		}
	}

	bool Grounded()
	{
		bool a = Physics.Raycast(transform.position, Vector3.down, out rayhitDown, distanceToGround, ground);
		return a;
	}


}
