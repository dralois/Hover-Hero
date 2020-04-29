using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class StrangeParticles : MonoBehaviour
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
	[SerializeField] private float m_innerTTL;
	[SerializeField] private float m_speed;
	[SerializeField] private float m_burstTTL;
	[SerializeField] private float m_burstRadius;
	[SerializeField] private Material m_particleMaterial;
	[SerializeField] private ComputeShader m_computeShader;

	private Particle[] m_particles;
	private ComputeBuffer m_buffer;

	private int m_kernelId;
	private static readonly int ParticleBuffer = Shader.PropertyToID(ParticleBufferName);
	private const string ParticleBufferName = "particleBuffer";

	#endregion

	#region Functions

	public void Burst()
	{
		Vector3 position = transform.position;
		// Setup particle array
		for (int i = 0; i < m_maxParticleCount; i++)
		{
			Particle p;
			// Random position on a circle
			float randPos = Random.Range(0, 2.0f * Mathf.PI);
			p.Position = new Vector3(Mathf.Sin(randPos) * m_burstRadius, Mathf.Cos(randPos) * m_burstRadius, 0.0f);
			p.Position = transform.TransformPoint(p.Position);
			// Random circular velocity
			float randVel = Random.Range(0, 2.0f * Mathf.PI);
			p.Velocity = new Vector3(Mathf.Sin(randVel), Mathf.Cos(randVel), Random.Range(-1.0f, 1.0f)) * m_speed;
			p.Velocity = transform.TransformVector(p.Velocity);
			p.TimeToLive = Random.Range(0.5f, 1.0f) * m_burstTTL;
			m_particles[i] = p;
		}

		m_buffer.SetData(m_particles);
	}

	#region Unity

	private void Start()
	{
		m_particles = new Particle[m_maxParticleCount];
		m_buffer = new ComputeBuffer(m_maxParticleCount, 28);
		
		// Setup particle array
		for (int i = 0; i < m_maxParticleCount; i++)
		{
			Particle p;
			p.Position = new Vector3(0,0,0);
			p.Velocity = new Vector3(0,0,0);
			p.TimeToLive = Random.Range(0.0f, 10.0f);
			m_particles[i] = p;
		}

		m_buffer.SetData(m_particles);

		// Find the ID of the kernel
		m_kernelId = m_computeShader.FindKernel("CSMain");

		// Set the particle buffer in the compute and particle shader
		m_computeShader.SetBuffer(m_kernelId, ParticleBufferName, m_buffer);
		m_particleMaterial.SetBuffer(ParticleBuffer, m_buffer);
	}

	private void Update()
	{
		Vector3 position = transform.position;
		m_computeShader.SetFloats("resetPosition", position.x + Random.Range(-1.0f, 1.0f), position.y + Random.Range(-1.0f, 1.0f), position.z);
		m_computeShader.SetFloats("resetVelocity", Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f), 0);
		m_computeShader.SetFloat("resetTimeToLive", Random.Range(0.5f, 1.0f) * m_innerTTL);
		m_computeShader.SetFloat("deltaTime", Time.deltaTime);
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
	[CustomEditor(typeof(StrangeParticles))]
	public class ObjectBuilderEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			StrangeParticles myScript = (StrangeParticles)target;
			if (GUILayout.Button("Burst Particles"))
			{
				myScript.Burst();
			}
		}
	}
#endif
}
