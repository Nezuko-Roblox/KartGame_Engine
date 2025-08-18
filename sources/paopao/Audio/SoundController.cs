using System;
using UnityEngine;

public class SoundController : MonoBehaviourEx
{
	private void Start()
	{
		FxClipSetting[] array = new FxClipSetting[]
		{
			new FxClipSetting(this.motorClip_, true),
			new FxClipSetting(this.driftClip_, true),
			new FxClipSetting(this.crashClip_, false),
			new FxClipSetting(this.shockClip_, false),
			new FxClipSetting(this.boostNormalClip_, false),
			new FxClipSetting(this.boostDriftClip_, false),
			new FxClipSetting(this.getItemClip_, false),
			new FxClipSetting(this.flipClip_, false),
			new FxClipSetting(this.devilClip_, false),
			new FxClipSetting(this.trappedBananaClip_, false),
			new FxClipSetting(this.trappedWaterFlyClip_, false),
			new FxClipSetting(this.trappedWaterBombClip_, false),
			new FxClipSetting(this.shieldClip_, false)
		};
		for (int i = 0; i < 13; i++)
		{
			this.CreateAudioSource(array[i], out this.audioSource_[i]);
		}
		this.RegistMonoBehaviour(5);
		this.isUpdatePlayerKartSound_ = true;
		for (int j = 0; j < this.audioSource_.Length; j++)
		{
			if (this.audioSource_[j] != null)
			{
				this.audioSource_[j].Play();
				this.audioSource_[j].Stop();
			}
		}
		this.audioSource_[1].Play();
		this.audioSource_[1].Stop();
		this.audioSource_[2].Play();
		this.audioSource_[2].Stop();
		this.audioSource_[3].Play();
		this.audioSource_[3].Stop();
	}

	public static void CreateAudioSource(GameObject gameObject, FxClipSetting clipSetting, out AudioSourceEx audioSource)
	{
		SoundController.CreateAudioSource(gameObject, clipSetting, out audioSource, AudioSourceType.FX);
	}

	public static void CreateAudioSource(GameObject gameObject, FxClipSetting clipSetting, out AudioSourceEx audioSource, AudioSourceType audioSourceType)
	{
		if (clipSetting.clip_ == null)
		{
			audioSource = null;
			return;
		}
		audioSource = new AudioSourceEx(gameObject.AddComponent<AudioSource>(), audioSourceType);
		audioSource.loop = clipSetting.isLoop_;
		audioSource.playOnAwake = false;
		audioSource.clip = clipSetting.clip_;
	}

	private void CreateAudioSource(FxClipSetting clipSetting, out AudioSourceEx audioSource)
	{
		SoundController.CreateAudioSource(base.gameObject, clipSetting, out audioSource);
	}

	private void Update()
	{
		if (KartManager.Instance.goPlayKart_ == null)
		{
			return;
		}
		if (this.isPaused_ || !this.isUpdatePlayerKartSound_)
		{
			return;
		}
		GoPlayKart goPlayKart_ = KartManager.Instance.goPlayKart_;
		float magnitude = goPlayKart_.m_KartWLVel.magnitude;
		if (this.audioSource_[0] != null && (double)(Time.time - this.prevUpdateFx_) > 0.064)
		{
			if (!this.audioSource_[0].isPlaying)
			{
				this.audioSource_[0].Play();
			}
			this.prevUpdateFx_ = Time.time;
			this.audioSource_[0].pitch = ((magnitude >= 128f) ? 1.5f : (0.25f + magnitude * 0.01171875f));
			this.audioSource_[0].volume = ((magnitude >= 64f) ? 1f : (0.25f + magnitude * 0.01171875f));
		}
		if (goPlayKart_.m_isDrift)
		{
			if (!this.audioSource_[1].isPlaying)
			{
				this.audioSource_[1].Play();
			}
		}
		else
		{
			this.audioSource_[1].Stop();
		}
		if (goPlayKart_.isCrash_ && !this.audioSource_[2].isPlaying)
		{
			this.audioSource_[2].volume = Mathf.Clamp(goPlayKart_.crashVelocity_ * 0.1f, 0.1f, 1f);
			this.audioSource_[2].Play();
		}
		if (goPlayKart_.isShock_ && !this.audioSource_[3].isPlaying)
		{
			this.audioSource_[3].volume = Mathf.Clamp(goPlayKart_.shockVelocity_ * 0.04f, 0.1f, 1f);
			this.audioSource_[3].Play();
		}
		this.PlayBoost(this.audioSource_[4], goPlayKart_.isBoost(BoostKind.BoostNormal) || goPlayKart_.isBoost(BoostKind.BoostStart));
		this.PlayBoost(this.audioSource_[5], goPlayKart_.isBoost(BoostKind.BoostDrift));
	}

	private void PlayBoost(AudioSourceEx audioSource, bool isPlay)
	{
		if (audioSource == null)
		{
			return;
		}
		if (isPlay)
		{
			if (!audioSource.isPlaying)
			{
				audioSource.Play();
			}
		}
		else
		{
			audioSource.Stop();
		}
	}

	private void PlaySound(SoundController.FxType fxType)
	{
		if (this.audioSource_[(int)fxType] != null && !this.audioSource_[(int)fxType].isPlaying)
		{
			this.audioSource_[(int)fxType].Play();
		}
	}

	public static bool IsPlayingSound(AudioSourceEx audioSource)
	{
		return audioSource != null && audioSource.isPlaying;
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		if (msg.type_ == MonoBehaviourMessageType.PAUSE || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL2)
		{
			for (int i = 0; i < this.audioSource_.Length; i++)
			{
				if (this.audioSource_[i] != null)
				{
					this.audioSource_[i].Pause();
				}
			}
			this.isPaused_ = true;
		}
		else if (msg.type_ == MonoBehaviourMessageType.RESUME)
		{
			for (int j = 0; j < this.audioSource_.Length; j++)
			{
				if (this.audioSource_[j] != null)
				{
					this.audioSource_[j].Resume();
				}
			}
			this.isPaused_ = false;
		}
		else if (msg.type_ == MonoBehaviourMessageType.PLAY_SOUND)
		{
			MonoBehaviourMessage1Param<SoundController.FxType> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<SoundController.FxType>)msg;
			if (monoBehaviourMessage1Param != null)
			{
				if (monoBehaviourMessage1Param.param_ == SoundController.FxType.DISABLE_UPDATE_PLAYER_SOUND)
				{
					this.isUpdatePlayerKartSound_ = false;
					for (int k = 0; k < this.audioSource_.Length; k++)
					{
						if (this.audioSource_[k] != null && this.audioSource_[k].loop)
						{
							this.audioSource_[k].Stop();
						}
					}
				}
				else
				{
					this.PlaySound(monoBehaviourMessage1Param.param_);
				}
			}
		}
	}

	private bool isPaused_;

	private bool isUpdatePlayerKartSound_ = true;

	public AudioClip motorClip_;

	public AudioClip driftClip_;

	public AudioClip crashClip_;

	public AudioClip shockClip_;

	public AudioClip boostNormalClip_;

	public AudioClip boostDriftClip_;

	public AudioClip getItemClip_;

	public AudioClip flipClip_;

	public AudioClip devilClip_;

	public AudioClip trappedBananaClip_;

	public AudioClip trappedWaterFlyClip_;

	public AudioClip trappedWaterBombClip_;

	public AudioClip shieldClip_;

	private AudioSourceEx[] audioSource_ = new AudioSourceEx[13];

	private float prevUpdateFx_;

	public enum FxType
	{
		MOTOR,
		DRIFT,
		CRASH,
		SHOCK,
		BOOST_NORMAL,
		BOOST_DRIFT,
		GET_ITEM,
		FLIP,
		DEVIL,
		TRAPPED_BANANA,
		TRAPPED_WATERFLY,
		TRAPPED_WATERBOMB,
		SHIELD,
		MAX_SIZE,
		DISABLE_UPDATE_PLAYER_SOUND
	}
}
