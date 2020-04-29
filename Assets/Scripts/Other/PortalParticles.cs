using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PortalParticles : MonoBehaviour
{
	#region Structs

	private struct Particle
	{
		public Vector3 Position;
		public Vector3 ResetPos;
		public Vector3 Velocities;
		public Vector3 CurrDir;
		public Vector3 CrossDir;
		//float2x2 RotMatrix;
		public float TimeToLive;
		public float ResetTTL;
		public float RandZ;
		public float RandCol;
	};

	#endregion

	#region Fields

	[SerializeField] private int m_maxParticleCount;
	[SerializeField] private float m_outSpeed;
	[SerializeField] private float m_angSpeed;
	[SerializeField] private float m_randSpeed;
	[SerializeField] private float m_radius;
	[SerializeField] private float m_maxTTL;
	[SerializeField] private Vector3 m_rotationAxis;
	[SerializeField] private Material m_particleMaterial;
	[SerializeField] private ComputeShader m_computeShader;

	private Particle[] m_particles;
	private ComputeBuffer m_buffer;

	private int m_kernelId;
	private static readonly int ParticleBuffer = Shader.PropertyToID(ParticleBufferName);
	private const string ParticleBufferName = "particleBuffer";


	#endregion

	#region Functions

	public void OpenPortal()
	{
		// Play sound
		//AudioManager.Instance.Play("Portal");
		// Reset buffer
		if (m_buffer != null && m_buffer.count != m_maxParticleCount)
		{
			m_buffer.Release();
			m_particles = new Particle[m_maxParticleCount];
			m_buffer = new ComputeBuffer(m_maxParticleCount, 76);
		}

		// Fill array
		for (int i = 0; i < m_maxParticleCount; i++)
		{
			Particle p;
			// Random position on a circle
			float curr = (i * 1.0f / m_maxParticleCount);
			float randPos = curr * 2.0f * Mathf.PI;
			p.Position = new Vector3(Mathf.Sin(randPos) * m_radius * curr * 0.5f, Mathf.Cos(randPos) * m_radius * curr * 0.5f, 0.0f);
			p.Position = transform.TransformPoint(p.Position);
			p.ResetPos = new Vector3(Mathf.Sin(randPos) * m_radius, Mathf.Cos(randPos) * m_radius, 0.0f);
			p.ResetPos = transform.TransformPoint(p.ResetPos);
			// Save velocities
			p.Velocities = new Vector3(m_angSpeed, m_outSpeed, m_randSpeed) * Random.Range(0.3f, 1.0f);
			// Save time to live
			p.TimeToLive = m_maxTTL * curr + m_maxTTL;
			p.ResetTTL = Random.Range(0.0f, 1.0f) * m_maxTTL;
			// Other stuff
			p.CurrDir = Vector3.zero;
			p.CrossDir = Vector3.zero;
			p.RandCol = Random.value;
			p.RandZ = Random.value;
			m_particles[i] = p;
		}
		// Set data
		m_buffer.SetData(m_particles);
		// Set buffer
		m_computeShader.SetBuffer(m_kernelId, ParticleBufferName, m_buffer);
		m_particleMaterial.SetBuffer(ParticleBuffer, m_buffer);
	}

	#region Unity

	private void Start()
	{
		// Find the ID of the kernel
		m_kernelId = m_computeShader.FindKernel("CSMain");
		m_particles = new Particle[m_maxParticleCount];
		m_buffer = new ComputeBuffer(m_maxParticleCount, 76);
		OpenPortal();
	}

	private void Update()
	{
		m_computeShader.SetFloat("decayRate", 0.1f);
		m_computeShader.SetFloat("deltaTime", Time.deltaTime);
		Vector3 rotationAxisTrans = transform.TransformDirection(m_rotationAxis);
		m_computeShader.SetFloats("rotationAxis", rotationAxisTrans.x, rotationAxisTrans.y, rotationAxisTrans.z);
		m_computeShader.SetFloats("position", transform.position.x, transform.position.y, transform.position.z);
		m_computeShader.Dispatch(m_kernelId, Mathf.CeilToInt(m_maxParticleCount / 256.0f), 1, 1);
	}

	private void OnRenderObject()
	{
		//Draw the particles as points
		m_particleMaterial.SetPass(0);
		Graphics.DrawProcedural(MeshTopology.Points, 1, m_maxParticleCount);
	}

	private void OnApplicationQuit()
	{
		if (m_buffer != null)
			m_buffer.Release();
	}

	private void OnDestroy()
	{
		if (m_buffer != null)
			m_buffer.Release();
	}

	private void OnTriggerExit(Collider other)
	{
		// Destroy portal after player leaves the collider
		if (other.CompareTag("ObstacleTrigger"))
		{
			if (m_buffer != null)
				m_buffer.Release();
			Destroy(gameObject);
		}
	}

	#endregion

	#endregion

#if UNITY_EDITOR
	[CustomEditor(typeof(PortalParticles))]
	public class ObjectBuilderEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			PortalParticles myScript = (PortalParticles)target;
			if (GUILayout.Button("Open Portal"))
			{
				myScript.OpenPortal();
			}
		}
	}
#endif
}
