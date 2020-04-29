using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BloodExplosion : MonoBehaviour
{
	#region Structs

	private struct Particle
	{
		public Vector3 Position;
		public Vector3 Velocity;
		public float TimeToLive;
	}

	#endregion

	#region Fields

	[SerializeField] private int m_maxParticleCount;
	[SerializeField] private float m_Speed;
	[SerializeField] private float m_Duration;
	[SerializeField] private float m_Radius;
	[SerializeField] private Vector3 m_Offset;
	[SerializeField] private Material m_particleMaterial;
	[SerializeField] private ComputeShader m_computeShader;

	private bool isInitialized = false;
	private Particle[] m_particles;
	private ComputeBuffer m_buffer;

	private int m_kernelId;
	private const string ParticleBufferName = "particleBuffer";
	private static readonly int ParticleBuffer = Shader.PropertyToID(ParticleBufferName);

	#endregion

	#region Properties

	public float bloodDuration
	{
		get { return m_Duration; }
	}

	#endregion

	#region Functions

	public void DeathExplode()
	{
		// Play sound
		//AudioManager.Instance.Play("Blood");
		// Reset buffer
		if (m_buffer != null && m_buffer.count != m_maxParticleCount)
		{
			m_buffer.Release();
			m_particles = new Particle[m_maxParticleCount];
			m_buffer = new ComputeBuffer(m_maxParticleCount, 28);
		}
		// Setup particle array
		for (int i = 0; i < m_maxParticleCount; i++)
		{
			Particle p;
			// Random position on a sphere
			p.Position = Vector3.one - 2.0f * new Vector3(Random.value, Random.value, Random.value);
			p.Position.Normalize();
			p.Position *= m_Radius;
			p.Position = transform.TransformPoint(p.Position);
			// Random circular velocity
			p.Velocity = (p.Position - transform.position).normalized * m_Speed * Random.Range(0.2f, 1.0f);
			p.Velocity = transform.TransformVector(p.Velocity);
			// Add offet
			p.Position += m_Offset;
			// Save TTL and particle
			p.TimeToLive = Random.Range(0.5f, 1.0f) * m_Duration;
			m_particles[i] = p;
		}
		// Set data
		m_buffer.SetData(m_particles);
		// Set buffer
		m_computeShader.SetBuffer(m_kernelId, ParticleBufferName, m_buffer);
		m_particleMaterial.SetBuffer(ParticleBuffer, m_buffer);
		// Init complete
		isInitialized = true;
	}

	#region Unity

	private void Start()
	{
		// Find the ID of the kernel
		m_kernelId = m_computeShader.FindKernel("CSMain");
		m_particles = new Particle[m_maxParticleCount];
		m_buffer = new ComputeBuffer(m_maxParticleCount, 28);
	}

	private void Update()
	{
		// Only if buffer filled
		if(isInitialized)
		{
			m_computeShader.SetFloat("deltaTime", Time.deltaTime);
			m_computeShader.Dispatch(m_kernelId, Mathf.CeilToInt(m_maxParticleCount / 256.0f), 1, 1);
		}
	}

	private void OnRenderObject()
	{
		//Draw the particles as points
		m_particleMaterial.SetPass(0);
		Graphics.DrawProcedural(MeshTopology.Points, 1, m_maxParticleCount);
	}

	private void OnApplicationQuit()
	{
		if(m_buffer != null)
			m_buffer.Release();
	}

	private void OnDestroy()
	{
		if (m_buffer != null)
			m_buffer.Release();
	}

	#endregion

	#endregion

#if UNITY_EDITOR
	[CustomEditor(typeof(BloodExplosion))]
	public class ObjectBuilderEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			BloodExplosion myScript = (BloodExplosion)target;
			if (GUILayout.Button("Death Explosion"))
			{
				myScript.DeathExplode();
			}
		}
	}
#endif
}