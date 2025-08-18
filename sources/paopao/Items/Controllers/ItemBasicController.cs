using System;
using UnityEngine;

public class ItemBasicController : MonoBehaviourEx
{
	private void Awake()
	{
		if (this.audioClip_.Length == this.audioLoopFlag_.Length)
		{
			this.audioSource_ = new AudioSourceEx[this.audioClip_.Length];
			for (int i = 0; i < this.audioClip_.Length; i++)
			{
				SoundController.CreateAudioSource(base.gameObject, new FxClipSetting(this.audioClip_[i], this.audioLoopFlag_[i]), out this.audioSource_[i]);
			}
		}
		this.RegistMonoBehaviour(40 + ItemBasicController.itemCounter_ % 64);
		ItemBasicController.itemCounter_++;
		this.ObjectSetting();
	}

	protected virtual void ObjectSetting()
	{
	}

	public virtual void Initialize(ItemParam param)
	{
		base.gameObject.SetActiveRecursively(true);
	}

	public virtual void OnUserDefinedTriggerEnter(Collider hit)
	{
	}

	protected void PlayFx(int idx)
	{
		if (MathHelper.IsBetweenIE(idx, 0, this.audioClip_.Length))
		{
			this.audioSource_[idx].Play();
		}
	}

	protected void PlayFx(int idx, bool condition)
	{
		if (MathHelper.IsBetweenIE(idx, 0, this.audioClip_.Length) && condition)
		{
			this.audioSource_[idx].Play();
		}
	}

	protected void StopFx(int idx, bool condition)
	{
		if (MathHelper.IsBetweenIE(idx, 0, this.audioClip_.Length) && condition)
		{
			this.audioSource_[idx].Stop();
		}
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		if ((msg.type_ == MonoBehaviourMessageType.PAUSE || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL2) && this.audioSource_ != null)
		{
			for (int i = 0; i < this.audioSource_.Length; i++)
			{
				if (this.audioSource_[i] != null)
				{
					this.audioSource_[i].Pause();
				}
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.RESUME && this.audioSource_ != null)
		{
			for (int j = 0; j < this.audioSource_.Length; j++)
			{
				if (this.audioSource_[j] != null)
				{
					this.audioSource_[j].Resume();
				}
			}
		}
	}

	public AudioClip[] audioClip_;

	public bool[] audioLoopFlag_;

	protected AudioSourceEx[] audioSource_;

	public static int itemCounter_;
}
