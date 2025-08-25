using System;
using UnityEngine;

public class GhostController : KartBasicController
{
	private void Awake()
	{
		base.gameObject.name = "ghost_kart";
		this.goGhostKart_ = (GoGhostKart)KartManager.Instance.SetKart(this.kartIndex_, new GoGhostKartBuilder(this.recordName_, this.isResourceDirectory_), this, null);
		KartManager.Instance.goCourse_.SetKart(this.kartIndex_, this.goGhostKart_);
		KartParameter kartParameter = KartManager.Instance.parameter_.kart_[this.kartIndex_];
		KartRecordHeader recordHeader = this.goGhostKart_.GetRecordHeader();
		if (kartParameter.body_ == 255)
		{
			kartParameter.body_ = recordHeader.kart_;
		}
		if (kartParameter.character_ == 255)
		{
			kartParameter.character_ = recordHeader.character_;
		}
		if (kartParameter.name_ == string.Empty)
		{
			kartParameter.name_ = recordHeader.playername_;
		}
		GameObject gameObject = base.transform.GetChild(0).gameObject;
		if (this.boosterWavePrefabLow_ != null)
		{
			this.boosterWave_ = (GameObject)global::UnityEngine.Object.Instantiate(this.boosterWavePrefabLow_);
			FiaUtil.AttachChild(ref gameObject, ref this.boosterWave_);
			this.boosterWave_.SetActiveRecursively(false);
		}
		base.Initialize(kartParameter.body_, kartParameter.character_, true);
	}

	private void Start()
	{
		RegenInfo startInfo = KartManager.Instance.goCourse_.GetStartInfo(this.kartIndex_);
		base.transform.localPosition = startInfo.position_;
		base.transform.localRotation = Quaternion.LookRotation(startInfo.direction_, Vector3.up);
	}

	protected override void FixedUpdate()
	{
		this.goGhostKart_.basicAction(Time.time);
		base.transform.position = this.goGhostKart_.position_;
		base.transform.localRotation = this.goGhostKart_.rotation_;
		CharacterAnimation characterAnim = this.goGhostKart_.CharacterAnim;
		if (this.characterAnimation_ != characterAnim)
		{
			if (!this.isNoAnimationCharacter_)
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
			}
			this.characterAnimation_ = characterAnim;
		}
		bool flag = this.characterAnimation_ == CharacterAnimation.BOOST;
		if (this.isBooster_ != flag)
		{
			this.isBooster_ = flag;
			base.SetEnableBooster(this.isBooster_);
		}
		if (KartManager.Instance.goCourse_.IsKartGoalIn(this.kartIndex_))
		{
			MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.GOAL_IN);
			monoBehaviourMessage1Param.Initialize(this.kartIndex_);
			MonoBehaviourExCenter.Instance.SendMessage(0, 1, monoBehaviourMessage1Param);
		}
	}

	private GoGhostKart goGhostKart_;

	public string recordName_;

	public bool isResourceDirectory_;

	private CharacterAnimation characterAnimation_;

	private bool isBooster_;

	public GameObject boosterWavePrefabLow_;
}
