using System;
using UnityEngine;

public struct FxClipSetting
{
	public FxClipSetting(AudioClip clip, bool isLoop)
	{
		this.clip_ = clip;
		this.isLoop_ = isLoop;
	}

	public AudioClip clip_;

	public bool isLoop_;
}
