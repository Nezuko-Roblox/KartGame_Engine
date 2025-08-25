using System;
using UnityEngine;

public class GUIWrongWay : MonoBehaviourEx
{
	private void Awake()
	{
		base.guiTexture.enabled = false;
		this.RegistMonoBehaviour(12);
	}

	private void Start()
	{
		SoundController.CreateAudioSource(base.gameObject, new FxClipSetting(this.wrongWayClip_, false), out this.wrongWayFx_);
	}

	private void Update()
	{
		if (KartManager.Instance.goCourse_.IsWrongWay())
		{
			this.wrongWayTime_ += Time.deltaTime;
			if (this.wrongWayTime_ >= 0f)
			{
				bool flag = (int)(this.wrongWayTime_ / 0.5f) % 2 == 0;
				if (this.wrongWayFx_ != null && !base.guiTexture.enabled && flag)
				{
					this.wrongWayFx_.Play();
				}
				base.guiTexture.enabled = flag;
			}
		}
		else
		{
			this.wrongWayTime_ = -1f;
			base.guiTexture.enabled = false;
			this.wrongWayFx_.Stop();
		}
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		if ((msg.type_ == MonoBehaviourMessageType.PAUSE || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL2) && this.wrongWayFx_ != null)
		{
			this.wrongWayFx_.Pause();
		}
		else if (msg.type_ == MonoBehaviourMessageType.RESUME && this.wrongWayFx_ != null)
		{
			this.wrongWayFx_.Resume();
		}
	}

	private const float guiTextureFLICKER_INTERVAL = 0.5f;

	private const float WRONG_WAY_START_TIME = 1f;

	public AudioClip wrongWayClip_;

	private AudioSourceEx wrongWayFx_;

	private float wrongWayTime_ = -1f;
}
