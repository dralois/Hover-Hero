using UnityEngine;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Effects
{
	public class FireLight : MonoBehaviour
	{
		public float m_Intensity = 2.0f;
		public bool m_Move = true;

		private float m_Rnd;
		private bool m_Burning = true;
		private Light m_Light;

		private void Start()
		{
			m_Rnd = Random.value * 100;
			m_Light = GetComponent<Light>();
		}

		private void Update()
		{
			if (m_Burning)
			{
				m_Light.intensity = m_Intensity * Mathf.PerlinNoise(m_Rnd + Time.time, m_Rnd + 1 + Time.time * 1);
				float x = Mathf.PerlinNoise(m_Rnd + 0 + Time.time * 2, m_Rnd + 1 + Time.time * 2) - 0.5f;
				float y = Mathf.PerlinNoise(m_Rnd + 2 + Time.time * 2, m_Rnd + 3 + Time.time * 2) - 0.5f;
				float z = Mathf.PerlinNoise(m_Rnd + 4 + Time.time * 2, m_Rnd + 5 + Time.time * 2) - 0.5f;
				if(m_Move)
					transform.localPosition = Vector3.up + new Vector3(x, y, z) * 1;
			}
		}

		public void Extinguish()
		{
			m_Burning = false;
			m_Light.enabled = false;
		}
	}
}
