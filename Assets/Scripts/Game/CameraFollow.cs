using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{

	public enum UpdateType
	{
		_frame,
		_fixed,
		_late
	}

	public GameObject playerObject;
	public Vector3 additionalRotation = new Vector3(0, 0, 0);
	[SerializeField]
	public CamSettings camS;

	private GameObject cameraObject;
	private Quaternion originalRotation;

	[System.Serializable]
	public class CamSettings
	{
		public float back = 5;
		public float up = 3;
		public float dampening = 5;
		public bool lookAtPlayer = true;
		public bool lookX = false;
		public bool lookY = true;
		public bool lookZ = false;
		public UpdateType updateType = UpdateType._fixed;
	}

	void Start()
	{
		cameraObject = GetComponent<Camera>().gameObject;

		Debug.Log("camObject: " + cameraObject);
		originalRotation = cameraObject.transform.rotation;

	}

	private void Update()
	{
		if (camS.updateType == UpdateType._frame)
		{
			Move();
		}
	}

	void FixedUpdate()
	{
		if (camS.updateType == UpdateType._fixed)
		{
			Move();
		}
	}

	private void LateUpdate()
	{
		if (camS.updateType == UpdateType._late)
		{
			Move();
		}
	}

	private void Move()
	{
		Vector3 targetposition = playerObject.transform.position;
		targetposition += -playerObject.transform.forward * camS.back + playerObject.transform.up * camS.up;
		cameraObject.transform.position = Vector3.Lerp(cameraObject.transform.position, targetposition, camS.dampening * Time.deltaTime);
		if (camS.lookAtPlayer)
		{
			Transform t = cameraObject.transform;
			t.LookAt(playerObject.transform.position);
			Vector3 rot = t.rotation.eulerAngles;
			if (!camS.lookX)
			{
				rot.x = originalRotation.eulerAngles.x;
			}
			if (!camS.lookY)
			{
				rot.y = originalRotation.eulerAngles.y;
			}
			if (!camS.lookZ)
			{
				rot.z = originalRotation.eulerAngles.z;
			}
			rot += additionalRotation;
			cameraObject.transform.rotation = Quaternion.Euler(rot);

		}
		else
		{
			cameraObject.transform.rotation = originalRotation;
		}
	}
}
