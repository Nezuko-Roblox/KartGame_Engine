using System;
using UnityEngine;

public class NetController : KartBasicController
{
	private void Awake()
	{
		base.gameObject.name = "net_kart";
		this.goNetKart_ = (GoNetKart)KartManager.Instance.SetKart(this.kartIndex_, new GoNetKartBuilder(), this, null);
		KartManager.Instance.goCourse_.SetKart(this.kartIndex_, this.goNetKart_);
		KartParameter kartParameter = KartManager.Instance.parameter_.kart_[this.kartIndex_];
		this.RegistMonoBehaviour(24 + this.kartIndex_);
		base.Initialize(kartParameter.body_, kartParameter.character_, false);
	}

	private void Start()
	{
		RegenInfo startInfo = KartManager.Instance.goCourse_.GetStartInfo(this.kartIndex_);
		base.transform.localPosition = startInfo.position_;
		base.transform.localRotation = Quaternion.LookRotation(startInfo.direction_, Vector3.up);
		base.rigidbody.mass = 100f;
		base.rigidbody.centerOfMass = new Vector3(0f, 0f, 0f);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (this.sync_ == null)
		{
			this.sync_ = NetworkManager.Inst.sync_;
			if (this.sync_ == null)
			{
				return;
			}
		}
		this.goNetKart_.basicAction(this.sync_.MakeLocalT2ServerT(Time.time));
		if (this.goNetKart_.apply_)
		{
			if (this.goNetKart_.position_ != Vector3.zero)
			{
			}
			this.goNetKart_.apply_ = false;
			base.transform.rotation = this.goNetKart_.rotation_;
			KartManager.Instance.goCourse_.sectionInfo_[this.kartIndex_].networkRankValue = this.goNetKart_.rankValue_;
			CharacterAnimation characterAnim = this.goNetKart_.CharacterAnim;
			if (this.characterAnimation_ != characterAnim)
			{
				if (characterAnim == CharacterAnimation.BIG_ACCIDENT || characterAnim == CharacterAnimation.SMALL_ACCIDENT)
				{
					this.character_.animation.wrapMode = WrapMode.Default;
					this.character_.animation.Play(KartDefine.CharacterAnimationString[(int)characterAnim]);
				}
				else
				{
					this.character_.animation.wrapMode = WrapMode.Loop;
					this.character_.animation.CrossFade(KartDefine.CharacterAnimationString[(int)characterAnim]);
				}
				this.characterAnimation_ = characterAnim;
			}
			if (this.kartBodyAnimation_ != this.goNetKart_.KartBodyAnim)
			{
				this.ApplyItemEffect(this.goNetKart_.KartBodyAnim);
				this.kartBodyAnimation_ = this.goNetKart_.KartBodyAnim;
			}
			bool flag = this.characterAnimation_ == CharacterAnimation.BOOST;
			if (this.isBooster_ != flag)
			{
				this.isBooster_ = flag;
				base.SetEnableBooster(this.isBooster_);
			}
		}
		if (this.goNetKart_.shielded_ && !this.appliedShieldEffect_)
		{
			this.shield_.SetActiveRecursively(true);
			base.Invoke("DisableShield", 1f);
			this.appliedShieldEffect_ = true;
		}
	}

	private void ApplyItemEffect(KartBodyAnimation anim)
	{
		if (this.kartBodyAnimation_ == KartBodyAnimation.LEVITATION)
		{
			this.waterFlyBubble_.SetActiveRecursively(false);
		}
		switch (anim)
		{
		case KartBodyAnimation.IDLE:
			this.SetKartBodyAnimation(anim, true);
			break;
		case KartBodyAnimation.ROLLING:
			this.SetKartBodyAnimation(anim, true);
			break;
		case KartBodyAnimation.JUMP_ROLLING:
			this.SetKartBodyAnimation(anim, true);
			break;
		case KartBodyAnimation.LEVITATION:
			this.DisableShield();
			this.waterFlyBubble_.SetActiveRecursively(true);
			this.SetKartBodyAnimation(anim, true);
			break;
		default:
			this.SetKartBodyAnimation(anim, true);
			break;
		}
	}

	protected override void DisableShield()
	{
		if (Debug.isDebugBuild)
		{
			base.DisableShield();
		}
		this.goNetKart_.shielded_ = false;
		this.appliedShieldEffect_ = false;
	}

	public override GoKart GetGoKart()
	{
		return this.goNetKart_;
	}

	public override bool UseItem(GameItem toUse)
	{
		if (toUse == GameItem.SHIELD)
		{
			if (this.shield_ != null)
			{
				this.shield_.SetActiveRecursively(true);
				base.Invoke("DisableShield", 1f);
			}
			return true;
		}
		return base.UseItem(toUse);
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.APPLY_ITEM && !KartManager.Instance.goCourse_.IsKartGoalIn(this.kartIndex_))
		{
			MonoBehaviourMessage2Param<GameItem, ApplyItemParam> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<GameItem, ApplyItemParam>)msg;
			if (monoBehaviourMessage2Param != null)
			{
				GameItem lparam_ = monoBehaviourMessage2Param.lparam_;
				if (lparam_ == GameItem.FLIP)
				{
					this.flipEffect_.SetActiveRecursively(true);
					base.Invoke("RestoreFlip", 7f);
				}
				else if (lparam_ == GameItem.DEVIL)
				{
					this.StartDevilEffect();
				}
			}
		}
	}

	private GoNetKart goNetKart_;

	private CharacterAnimation characterAnimation_;

	private KartBodyAnimation kartBodyAnimation_;

	private bool isBooster_;

	private TimeSync sync_;

	private bool appliedShieldEffect_;
}
