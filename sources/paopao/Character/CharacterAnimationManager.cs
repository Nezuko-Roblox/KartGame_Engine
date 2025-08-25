using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationManager
{
	public CharacterAnimationManager()
	{
		this.characterAnimation_ = new CharacterAnimationElem[]
		{
			new CharacterAnimationElem("idle", "f00"),
			new CharacterAnimationElem("turn_left", "f00"),
			new CharacterAnimationElem("turn_right", "f00"),
			new CharacterAnimationElem("turn_back_left", "f00"),
			new CharacterAnimationElem("turn_back_right", "f00"),
			new CharacterAnimationElem("look_back_left", "f00"),
			new CharacterAnimationElem("look_back_right", "f00"),
			new CharacterAnimationElem("boost", "f02"),
			new CharacterAnimationElem("small_accident", "f00"),
			new CharacterAnimationElem("big_accident", "f00"),
			new CharacterAnimationElem("attack", "f00"),
			new CharacterAnimationElem("item_success", "f00"),
			new CharacterAnimationElem("captured_bubble", "f00"),
			new CharacterAnimationElem("win_game", "f01"),
			new CharacterAnimationElem("lose_game", "f03"),
			new CharacterAnimationElem("win_game", "f01")
		};
	}

	public void Release()
	{
		this.characterAnimation_ = null;
		this.materials_.Clear();
	}

	public void Initialize(byte characterIdx)
	{
		this.character_ = characterIdx;
		string modelingName = FiaUtil.GetModelingName(this.character_);
		for (int i = 0; i < this.characterAnimation_.Length; i++)
		{
			if (!this.materials_.ContainsKey(this.characterAnimation_[i].faceAnim_))
			{
				this.materials_[this.characterAnimation_[i].faceAnim_] = (Material)ResourceLoader.Instance.GetAsset(modelingName, this.characterAnimation_[i].faceAnim_, typeof(Material));
			}
		}
	}

	public Material GetFaceMaterial(CharacterAnimation anim)
	{
		return this.materials_[this.characterAnimation_[(int)anim].faceAnim_];
	}

	public string GetAnimationString(CharacterAnimation anim)
	{
		return this.characterAnimation_[(int)anim].anim_;
	}

	private CharacterAnimationElem[] characterAnimation_;

	private byte character_;

	private Dictionary<string, Material> materials_ = new Dictionary<string, Material>();
}
