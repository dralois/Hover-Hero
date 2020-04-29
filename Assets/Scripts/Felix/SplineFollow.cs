using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class SplineFollow : MonoBehaviour
{

	public Text startCD;
	public Text endScreen;
	public GameObject ObjectFollowing;
	public float speed = 0.2f;
	public CustomSpline customSpline;
	public bool repeat = false;
	public bool done = false;
	public bool pause = false;
	public bool fixedUpdate = false;
	public bool useStartEnd = true;

	private LTSpline track;
	public float trackPosition = 0; // ratio 0,1 of the avatars position on the track
	[SerializeField] [HideInInspector] private float speedFactor = 1;
	[SerializeField] [HideInInspector] private float resultSpeed;

	void Start()
	{

		if (ObjectFollowing == null)
		{
			ObjectFollowing = gameObject;
		}

		if (customSpline == null || customSpline.track == null)
		{
			Debug.LogError("no spline set");
		}

		SetSpline(customSpline);
		AdjustSpeed(customSpline.ElementDefaultSpeed);

		GestureListener gl = FindObjectOfType<GestureListener>();

		if (KinectManager.Instance.GetUsersCount() > 0)
		{
			StartCoroutine(StartGame());
		}
		else
		{
			gl.PlayerDetectedEvent += gl_PlayerDetectedEvent;
		}

		gl.PlayerLostEvent += gl_PlayerLostEvent;
	}

	IEnumerator StartGame()
	{
		if (!useStartEnd)
		{
			AudioManager.Instance.Play("_Main");
			pause = false;
			yield break;
		}

		AudioManager.Instance.Play("Start");
		for (int i = 3; i > 0; i--)
		{
			startCD.text = "START IN " + i + "..";
			yield return new WaitForSecondsRealtime(1.0f);
		}
		pause = false;
		startCD.text = "GO!!";
		AudioManager.Instance.Play("_Main");
		yield return new WaitForSecondsRealtime(1.0f);
		startCD.text = "";
	}

	public void EndScreen()
	{
		if (!useStartEnd)
			return;
		pause = true;
		endScreen.enabled = true;
		AudioManager.Instance.Play("GameEnd");
		AudioManager.Instance.Stop("_Main");
        GameManager.Instance.ScoreManagerInstance.SetHighScore();
	}

	private void gl_PlayerDetectedEvent(long userId)
	{
		StartCoroutine(StartGame());
	}

	void gl_PlayerLostEvent(long userId)
	{
		pause = true;
	}

	void Update()
	{
		if (!fixedUpdate)
		{
			Move();
		}
	}

	private void FixedUpdate()
	{
		if (fixedUpdate)
		{
			Move();
		}
	}

	private void Move()
	{
		resultSpeed = speed * speedFactor;

		if (Input.GetKeyDown(KeyCode.P))
		{
			pause = !pause;
		}
		if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
		{
			speed += 0.2f;
		}
		if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
		{
			speed -= 0.2f;
		}

		if (track == null || pause)
			return;

		// Update avatar's position on correct track
		if (!done)
		{
			track.place(ObjectFollowing.transform, trackPosition);

			trackPosition += Time.deltaTime * resultSpeed;// * Input.GetAxis("Vertical"); // Uncomment to have the forward and backwards controlled by the directional arrows

		}
		if (repeat)
		{
			if (trackPosition < 0f) // We need to keep the ratio between 0-1 so after one we will loop back to the beginning of the track
				trackPosition = 1f;
			else if (trackPosition > 1f)
				trackPosition = 0f;
		}
		else
		{
			if (trackPosition < 0f)
				trackPosition = 0f;
			else if (trackPosition > 1f)
			{
				trackPosition = 1f;
				done = true;
			}
		}
	}

	public void SetSpline(CustomSpline spline)
	{
		if (spline == null)
		{
			Debug.LogError("Spline null");
		}
		customSpline = spline;
		track = customSpline.CalcSpline();
	}

	public void ResetFollow()
	{
		done = false;
		trackPosition = 0;
	}

	public void AdjustSpeed(float factor)
	{
		speedFactor = factor * GameManager.Instance.gameSettings.SpeedFactor;
	}
}
