using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{

	public Sound[] sounds;

	private void Awake()
	{
		foreach (var s in sounds)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;
			s.source.pitch = s.pitch;
			s.source.loop = s.loop;
		}
		InitVolume();
	}

	public void InitVolume()
	{
		foreach (var s in sounds)
		{

			if (s.name.StartsWith("_"))//music must be marked with _ as first Character
				s.source.volume = s.volume * GameManager.Instance.gameSettings.MusicValue;
			else
				s.source.volume = s.volume * GameManager.Instance.gameSettings.SoundsValue;
		}
	}


	public void Play(string name)
	{
		if (name.StartsWith("_"))
		{
			//return if music is off
			if (!GameManager.Instance.gameSettings.Music)
				return;
		}
		else
		{
			//return if sounds are off
			if (!GameManager.Instance.gameSettings.Sounds)
				return;
		}

		Sound s = Array.Find(sounds, sound => sound.name == name);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}
		////Temporary adjusting SoundVolume
		//float vTmp = s.source.volume;
		//s.source.volume *= GameManager.Instance.gameSettings.SoundsValue;

		//play sound
		s.source.Play();

		//revert volumechange
		//s.source.volume = vTmp;
	}

	public void PlayRandom(string[] names)
	{
		if (names.Length == 0)
		{
			Debug.LogWarning("No sounds to choose random");
		}
		string chosen = names[UnityEngine.Random.Range(0, names.Length)];
		Play(chosen);
	}

	public void Stop(string name)
	{
		Sound s = Array.Find(sounds, sound => sound.name == name);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}
		s.source.Stop();
	}
}
