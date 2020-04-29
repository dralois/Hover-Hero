using System.Collections;
using UnityEngine;

public class StopPoint : MonoBehaviour
{

	#region Enums

	public enum ContinueType
	{
		Gesture,
		Proximity,
		Enemy,
		Shield,
		Instant,
		Final,
		Invalid = -1
	}

	#endregion

	#region Fields

	private SplineFollow SplineFollow;
	private ItemController ShieldCheck;

	public ContinueType continueType = ContinueType.Invalid;
	public KinectGestures.Gestures ContinueGesture = KinectGestures.Gestures.None;
	public Transform ProximityPoint;
	public float ProximityDistance = 1;
	public GameObject enemyToKill;
	public string HelpText = "";

	private KinectGestures.Gestures LastGesture = KinectGestures.Gestures.None;
	private Transform[] playerFeet = new Transform[2];
	private int playerIndex;
	private bool isCompleted = false;
	private bool hasStopped = false;

	#endregion

	#region Functions

	public static Vector3 CalcCenter(Transform[] transforms)
	{
		Vector3 center = new Vector3(0, 0, 0);
		int childCount = 0;
		foreach (Transform tfs in transforms)
		{
			center += tfs.position;
			childCount++;
		}
		center /= childCount;
		return center;
	}

	private bool CheckContinueCondition()
	{
		switch (continueType)
		{
			case ContinueType.Gesture:
				return PlayerManager.Instance.WasGestureCompletedInTime(playerIndex, ContinueGesture, 1.0f);
			case ContinueType.Proximity:
				return Vector3.Distance(CalcCenter(playerFeet), ProximityPoint.transform.position) <= ProximityDistance;
			case ContinueType.Enemy:
				return false;
			case ContinueType.Shield:
				return ShieldCheck.shieldHit;
			default:
				Debug.LogError("Unhandled continue type");
				break;
		}
		return false;
	}

	private IEnumerator LoadMainGame()
	{
		yield return new WaitForSecondsRealtime(2.0f);
		GameManager.Instance.LoadScene("MainGame");
	}

	#region Unity

	void Start()
	{

		GameObject playerChar = FindObjectOfType<SplineFollow>().gameObject;
		SplineFollow = playerChar.GetComponent<SplineFollow>();
		ShieldCheck = playerChar.GetComponentInChildren<ItemController>();
		AvatarController playerCon = playerChar.GetComponentInChildren<AvatarController>();

		if (playerCon != null)
		{
			playerFeet[0] = playerCon.GetBoneTransform(playerCon.GetBoneIndexByJoint(KinectInterop.JointType.AnkleLeft, false));
			playerFeet[1] = playerCon.GetBoneTransform(playerCon.GetBoneIndexByJoint(KinectInterop.JointType.AnkleRight, false));
			playerIndex = playerCon.playerIndex;
		}

		if (ProximityPoint == null)
			ProximityPoint = gameObject.transform;

		if (enemyToKill != null)
			enemyToKill.GetComponent<EnemyDestroyed>().EnemyDestroyedEvent += EnemyKilled;

		Debug.Assert(SplineFollow != null && ShieldCheck != null && playerCon != null,
								"A necessary component (Spline Follow, Item Controller, Avatar Controller) is null!");
	}

	private void EnemyKilled()
	{
		TutorialHelper helper = FindObjectOfType<TutorialHelper>();
        if (helper != null)
        {
            helper.UpdateHelperText("");
		    SplineFollow.pause = false;
		    isCompleted = true;
        }
	}

	private void Update()
	{
		if (continueType == ContinueType.Instant || continueType == ContinueType.Final)
			return;

		if (!isCompleted)
		{
			if (hasStopped)
			{
				if (CheckContinueCondition())
				{
					FindObjectOfType<TutorialHelper>().UpdateHelperText("");
					SplineFollow.pause = false;
					isCompleted = true;
				}
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("ObstacleTrigger") && !hasStopped && !isCompleted)
		{
			FindObjectOfType<TutorialHelper>().UpdateHelperText(HelpText);
			if (continueType != ContinueType.Instant)
				SplineFollow.pause = true;
			hasStopped = true;
			// Load main game if final stop
			if (continueType == ContinueType.Final)
			{
                Debug.Log("Final point enter");
				ItemController con = FindObjectOfType<ItemController>();
				con.RemoveAllItems();
				StartCoroutine(LoadMainGame());
			}
		}
	}

	#endregion

	#endregion

}
