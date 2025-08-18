using System;
using UnityEngine;

public class AudioSourceEx
{
	public AudioSourceEx(AudioSource audioSource)
	{
		this.audioSource_ = audioSource;
		this.isPaused_ = false;
		this.type_ = AudioSourceType.FX;
	}

	public AudioSourceEx(AudioSource audioSource, AudioSourceType audioSourceType)
	{
		this.audioSource_ = audioSource;
		this.isPaused_ = false;
		this.type_ = audioSourceType;
	}

	public AudioClip clip
	{
		get
		{
			return this.audioSource_.clip;
		}
		set
		{
			this.audioSource_.clip = value;
		}
	}

	public bool ignoreListenerVolume
	{
		get
		{
			return this.audioSource_.ignoreListenerVolume;
		}
		set
		{
			this.audioSource_.ignoreListenerVolume = value;
		}
	}

	public bool isPlaying
	{
		get
		{
			return this.audioSource_.isPlaying;
		}
	}

	public bool loop
	{
		get
		{
			return this.audioSource_.loop;
		}
		set
		{
			this.audioSource_.loop = value;
		}
	}

	public float pitch
	{
		get
		{
			return this.audioSource_.pitch;
		}
		set
		{
			this.audioSource_.pitch = value;
		}
	}

	public bool playOnAwake
	{
		get
		{
			return this.audioSource_.playOnAwake;
		}
		set
		{
			this.audioSource_.playOnAwake = value;
		}
	}

	public float time
	{
		get
		{
			return this.audioSource_.time;
		}
		set
		{
			this.audioSource_.time = value;
		}
	}

	public int timeSamples
	{
		get
		{
			return this.audioSource_.timeSamples;
		}
		set
		{
			this.audioSource_.timeSamples = value;
		}
	}

	public AudioVelocityUpdateMode velocityUpdateMode
	{
		get
		{
			return this.audioSource_.velocityUpdateMode;
		}
		set
		{
			this.audioSource_.velocityUpdateMode = value;
		}
	}

	public float volume
	{
		get
		{
			return this.audioSource_.volume;
		}
		set
		{
			this.audioSource_.volume = value;
		}
	}

	public void Play()
	{
		if (this.IsPlayable())
		{
			this.audioSource_.Play();
		}
	}

	public static void PlayClipAtPoint(AudioClip clip, Vector3 position)
	{
		AudioSource.PlayClipAtPoint(clip, position);
	}

	public static void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume)
	{
		AudioSource.PlayClipAtPoint(clip, position, volume);
	}

	public void PlayOneShot(AudioClip clip)
	{
		if (this.IsPlayable())
		{
			this.audioSource_.PlayOneShot(clip);
		}
	}

	public void PlayOneShot(AudioClip clip, float volumeScale)
	{
		if (this.IsPlayable())
		{
			this.audioSource_.PlayOneShot(clip, volumeScale);
		}
	}

	public void Stop()
	{
		this.audioSource_.Stop();
	}

	public bool IsPaused()
	{
		return this.isPaused_;
	}

	public void Pause()
	{
		if (this.audioSource_.isPlaying)
		{
			this.isPaused_ = true;
			this.audioSource_.Pause();
		}
	}

	public void Resume()
	{
		if (this.isPaused_)
		{
			this.isPaused_ = false;
			this.audioSource_.Play();
		}
	}

	private bool IsPlayable()
	{
		return (this.type_ == AudioSourceType.BGM && KartOptions.Instance.Bgm) || (this.type_ == AudioSourceType.FX && KartOptions.Instance.Fx);
	}

	public AudioSource GetAudioSource()
	{
		return this.audioSource_;
	}

	private bool isPaused_;

	private AudioSource audioSource_;

	private AudioSourceType type_ = AudioSourceType.FX;
}
