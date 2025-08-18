using System;
using System.Collections.Generic;
using UnityEngine;

public class AIController : KartBasicController
{
	private void Awake()
	{
		base.gameObject.name = string.Format("ai_kart_{0}", this.kartIndex_);
		this.goAIKart_ = (GoAIKart)KartManager.Instance.SetKart(this.kartIndex_, new GoAIKartBuilder(this.kartIndex_, this.recordName_, this.isRecordInAssetBundle_), this, null);
		KartManager.Instance.goCourse_.SetKart(this.kartIndex_, this.goAIKart_);
		KartParameter kartParameter = KartManager.Instance.parameter_.kart_[this.kartIndex_];
		if (this.boosterWavePrefabLow_ != null)
		{
			GameObject gameObject = base.transform.GetChild(0).gameObject;
			if (this.boosterWavePrefabLow_ != null)
			{
				this.boosterWave_ = (GameObject)global::UnityEngine.Object.Instantiate(this.boosterWavePrefabLow_);
				FiaUtil.AttachChild(ref gameObject, ref this.boosterWave_);
				this.boosterWave_.SetActiveRecursively(false);
			}
		}
		base.Initialize(kartParameter.body_, kartParameter.character_, true);
		this.renderers_ = base.GetComponentsInChildren<Renderer>();
		this.RegistMonoBehaviour(24 + this.kartIndex_);
		this.updateTick_ = 0;
	}

	public void SetAiControllerType(AIControllerType aiType, float targetTime, bool useStartBoost, MedalType medalType)
	{
		this.aiType_ = aiType;
		this.useStartBoost_ = useStartBoost;
		if (targetTime <= 0f)
		{
			this.DEFAULT_DELTA_TICK = Time.fixedDeltaTime;
		}
		else if (this.useStartBoost_)
		{
			float num = this.START_BOOSTER_VELOCITY_FACTOR;
			if (medalType == MedalType.BRONZE)
			{
				num = 1.7f;
			}
			else if (medalType == MedalType.SILVER)
			{
				num = 1.5f;
			}
			else if (medalType == MedalType.GOLD)
			{
				num = 1.3f;
			}
			float num2 = 1f;
			float num3 = 3f;
			float num4 = num2 + num3;
			float num5 = (num * num2 + (num + 1f) * num3 / 2f) / num4;
			float num6 = num4 * num5 - num4;
			this.DEFAULT_DELTA_TICK = this.goAIKart_.TotalTime / (targetTime + num6) * Time.fixedDeltaTime;
		}
		else
		{
			this.DEFAULT_DELTA_TICK = this.goAIKart_.TotalTime * Time.fixedDeltaTime / targetTime;
		}
		this.defaultSpeedController_ = new DefaultSpeedController(this.kartIndex_, this.aiType_, this.DEFAULT_DELTA_TICK);
		this.speedController_ = this.defaultSpeedController_;
	}

	private void Start()
	{
		RegenInfo startInfo = KartManager.Instance.goCourse_.GetStartInfo(this.kartIndex_);
		base.transform.localPosition = startInfo.position_;
		base.transform.localRotation = Quaternion.LookRotation(startInfo.direction_, Vector3.up);
		this.prevState_.Reset();
		this.prevState_.srcPosition_ = base.transform.localPosition;
		this.resetInfo_.Reset();
		base.rigidbody.mass = 100f;
		base.rigidbody.centerOfMass = new Vector3(0f, 0f, 0f);
		this.goAIKart_.InitFirstRecord();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!this.CheckKartReset() && !this.CheckStuckedReset(Time.time) && !this.goAIKart_.Forcing)
		{
			int latestPassingPlane = KartManager.Instance.goCourse_.GetLatestPassingPlane(this.kartIndex_);
			if (this.resetInfo_.latestPassingPlane_ != latestPassingPlane)
			{
				this.resetInfo_.tick_ = this.tick_;
				this.resetInfo_.latestPassingPlane_ = latestPassingPlane;
				this.resetInfo_.passCorrect_ = KartManager.Instance.goCourse_.sectionInfo_[this.kartIndex_].passCorrect_;
				this.resetInfo_.prevState_ = this.prevState_;
			}
			if (KartManager.Instance.goCourse_.IsKartGoalIn(this.kartIndex_))
			{
				Vector3 velocity = base.rigidbody.velocity;
				float magnitude = velocity.magnitude;
				base.rigidbody.angularVelocity = Vector3.zero;
				if (magnitude <= 0.5f)
				{
					base.rigidbody.AddForce(-velocity, ForceMode.VelocityChange);
				}
				else
				{
					base.rigidbody.AddForce(-velocity * (0.03f + 18f / magnitude) * Time.fixedDeltaTime, ForceMode.VelocityChange);
				}
			}
			else
			{
				if (KartManager.Instance.DriveStartTime > 0f && Time.time >= KartManager.Instance.DriveStartTime)
				{
					if (this.isFirstDrive_)
					{
						this.isFirstDrive_ = false;
						if (this.useStartBoost_)
						{
							this.SetSpeedController(new BoostSpeedController(this.deltaTick_ * this.START_BOOSTER_VELOCITY_FACTOR, 1f, this.DEFAULT_DELTA_TICK)
							{
								nextSpeedController_ = new LerpSpeedController(this.deltaTick_ * this.START_BOOSTER_VELOCITY_FACTOR, this.deltaTick_, 3f)
							});
						}
					}
					bool flag = Vector3Helper.IsZero(this.prevState_.targetDirection_) || this.prevState_.magnitude_ == 0f;
					if (!flag)
					{
						Vector3 vector = (base.transform.localPosition - this.prevState_.srcPosition_) / this.prevState_.magnitude_;
						vector.y = 0f;
						float num = Vector3.Dot(vector, this.prevState_.targetDirection_);
						Vector3 velocity2 = base.rigidbody.velocity;
						velocity2.y = 0f;
						float num2 = velocity2.magnitude / this.prevState_.magnitude_;
						flag = num2 + num >= 1f;
					}
					if (flag)
					{
						this.prevState_.srcPosition_ = this.goAIKart_.position_;
						this.tick_ += this.deltaTick_;
						this.goAIKart_.basicAction(KartManager.Instance.DriveStartTime + this.tick_);
						Vector3 vector2 = this.goAIKart_.position_ - this.prevState_.srcPosition_;
						vector2.y = 0f;
						this.prevState_.magnitude_ = vector2.magnitude;
						this.prevState_.targetDirection_ = vector2.normalized;
					}
				}
				else
				{
					this.goAIKart_.basicAction(0f);
				}
				if (this.grounded_)
				{
					Vector3 vector3 = (this.goAIKart_.position_ - base.transform.position) / Time.deltaTime;
					vector3.y = 0f;
					Vector3 vector4 = vector3 - base.rigidbody.velocity;
					vector4.y = 0f;
					base.rigidbody.AddForce(vector4, ForceMode.VelocityChange);
					base.rigidbody.angularVelocity = Vector3.zero;
					base.transform.localRotation = this.goAIKart_.rotation_;
					if (this.itemList_.Count > 0)
					{
						GameItemElem gameItemElem = this.itemList_[0];
						if (gameItemElem.useTime_ <= Time.time)
						{
							if (gameItemElem.itemType_ == GameItem.BOOSTER)
							{
								this.SetSpeedController(new BoostSpeedController(this.deltaTick_ * 1.2f, 3f, this.DEFAULT_DELTA_TICK));
								this.SetCharacterAnimation(CharacterAnimation.BOOST, WrapMode.Default);
								this.animationLock_ = true;
							}
							else
							{
								base.UseItem(gameItemElem.itemType_);
							}
							this.itemList_.RemoveAt(0);
						}
					}
				}
				else
				{
					base.transform.localRotation = this.goAIKart_.rotation_;
				}
			}
			base.rigidbody.AddForce(new Vector3(0f, -49f * base.rigidbody.mass, 0f));
		}
		else
		{
			base.rigidbody.angularVelocity = Vector3.zero;
			base.rigidbody.AddForce(-base.rigidbody.velocity, ForceMode.VelocityChange);
		}
		this.UpdatePlayMode();
		if (!this.isNoAnimationCharacter_ && this.playMode_ != KartBasicController.PlayMode.ANIMATION_PLAYING)
		{
			if (this.animationLock_)
			{
				if (this.characterAnimation_ == CharacterAnimation.ATTACK)
				{
					this.animationLock_ = this.character_.animation.isPlaying;
				}
				else if (this.characterAnimation_ == CharacterAnimation.BOOST)
				{
					this.animationLock_ = this.speedController_.GetSpeedControllerType() == SpeedControllerType.BOOST_SPEED;
				}
				else if (this.characterAnimation_ == CharacterAnimation.WIN_GAME || this.characterAnimation_ == CharacterAnimation.LOSE_GAME)
				{
				}
			}
			if (!this.animationLock_)
			{
				CharacterAnimation characterAnim = this.goAIKart_.CharacterAnim;
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
			}
		}
		bool flag2 = this.speedController_.GetSpeedControllerType() == SpeedControllerType.BOOST_SPEED && this.playMode_ == KartBasicController.PlayMode.NORMAL;
		if (this.isBooster_ != flag2)
		{
			this.isBooster_ = flag2;
			base.SetEnableBooster(this.isBooster_);
		}
		this.grounded_ = false;
		this.UpdateFlickering();
	}

	private void UpdatePlayMode()
	{
		if (this.playMode_ == KartBasicController.PlayMode.ANIMATION_PLAYING && !this.kartBody_.animation.isPlaying)
		{
			this.playMode_ = KartBasicController.PlayMode.NORMAL;
			this.goAIKart_.Forcing = false;
			Transform kartBodyTransform = base.KartBodyTransform;
			kartBodyTransform.localPosition = Vector3.zero;
			kartBodyTransform.localRotation = Quaternion.identity;
			this.goAIKart_.KartBodyAnim = KartBodyAnimation.IDLE;
			this.OnChangePlayMode();
			this.SetSpeedController(new LerpSpeedController(0f, this.DEFAULT_DELTA_TICK, 3f));
		}
	}

	private void OnCollisionStay(Collision collisionInfo)
	{
		this.OnCollisionDetection(collisionInfo);
	}

	private void OnCollisionEnter(Collision collisionInfo)
	{
		this.OnCollisionDetection(collisionInfo);
	}

	private void OnCollisionDetection(Collision collisionInfo)
	{
		this.grounded_ = true;
		if (collisionInfo.gameObject.rigidbody != null)
		{
			KartBasicController kartBasicController = collisionInfo.gameObject.GetComponent(typeof(KartBasicController)) as KartBasicController;
			if (kartBasicController != null)
			{
				int rank = KartManager.Instance.goCourse_.GetRank(this.kartIndex_);
				int rank2 = KartManager.Instance.goCourse_.GetRank(kartBasicController.kartIndex_);
				if (rank > rank2)
				{
					this.SetSpeedController(new LerpSpeedController(0f, this.DEFAULT_DELTA_TICK, global::UnityEngine.Random.Range(3f, 5f)));
				}
			}
		}
	}

	private bool CheckKartReset()
	{
		if (this.goAIKart_ == null)
		{
			return false;
		}
		if (this.goAIKart_.m_kart.transform.position.y < -5f)
		{
			this.StartResetProcess();
			return true;
		}
		if (KartManager.Instance.goCourse_.IsKartNeedReset(this.kartIndex_))
		{
			this.StartResetProcess();
			return true;
		}
		return false;
	}

	private void StartResetProcess()
	{
		if (this.kartResetState_ == 0)
		{
			this.kartResetState_ = 1;
			this.kartResetTick_ = Time.time;
			this.goAIKart_.Valid = false;
			this.goAIKart_.Forcing = true;
			base.rigidbody.AddForce(-base.rigidbody.velocity, ForceMode.VelocityChange);
		}
	}

	private void ResetKart(bool changeDefaultPath)
	{
		if (this.goAIKart_ == null)
		{
			return;
		}
		if (KartManager.Instance.DriveStartTime <= 0f || Time.time < KartManager.Instance.DriveStartTime)
		{
			return;
		}
		if (changeDefaultPath)
		{
			int latestPassingPlane_ = this.resetInfo_.latestPassingPlane_;
			this.tick_ = this.goAIKart_.ResetKartWithDefaultRecord(latestPassingPlane_, 0f);
			KartManager.Instance.goCourse_.ResetAIKart(this.kartIndex_, latestPassingPlane_, true);
			this.prevState_.Reset();
			this.resetInfo_.Reset();
			this.resetInfo_.tick_ = this.tick_;
			this.resetInfo_.latestPassingPlane_ = latestPassingPlane_;
		}
		else
		{
			this.tick_ = this.resetInfo_.tick_;
			this.prevState_ = this.resetInfo_.prevState_;
			this.goAIKart_.ResetKart(KartManager.Instance.DriveStartTime + this.resetInfo_.tick_);
			KartManager.Instance.goCourse_.ResetAIKart(this.kartIndex_, this.resetInfo_.latestPassingPlane_, this.resetInfo_.passCorrect_);
		}
		Vector3 position_ = this.goAIKart_.position_;
		position_.y += 0.1f;
		RaycastHit raycastHit;
		if (Physics.Raycast(position_, -Vector3.up, out raycastHit, 500f, 256))
		{
			base.transform.localPosition = raycastHit.point;
			base.transform.localRotation = this.goAIKart_.rotation_;
		}
		else
		{
			KartManager.Instance.goCourse_.ResetKart(this.kartIndex_);
		}
		base.rigidbody.AddForce(-base.rigidbody.velocity, ForceMode.VelocityChange);
		base.rigidbody.angularVelocity = Vector3.zero;
		base.rigidbody.AddForce(new Vector3(0f, -49f * base.rigidbody.mass, 0f));
		this.SetKartBodyAnimation(KartBodyAnimation.IDLE);
		this.SetSpeedController(new StaticSpeedController(this.deltaTick_ * 1.5f, 3f, this.DEFAULT_DELTA_TICK));
	}

	public override bool SetKartBodyAnimation(KartBodyAnimation anim, bool isForced)
	{
		if (base.SetKartBodyAnimation(anim, isForced))
		{
			this.animationLock_ = false;
			return true;
		}
		return false;
	}

	private bool CheckStuckedReset(float tick)
	{
		if (KartManager.Instance.DriveStartTime <= 0f || Time.time < KartManager.Instance.DriveStartTime)
		{
			this.stuckedTime_ = 0f;
			return false;
		}
		if (KartManager.Instance.goCourse_.IsKartGoalIn(this.kartIndex_))
		{
			this.stuckedTime_ = 0f;
			return false;
		}
		if (this.stuckedTime_ == 0f)
		{
			this.stuckedTime_ = tick;
			this.backupProcessTick_ = this.tick_;
		}
		if (this.stuckedTime_ != 0f)
		{
			if (!this.goAIKart_.Valid || this.goAIKart_.Forcing || base.rigidbody.velocity.sqrMagnitude >= 1f)
			{
				this.stuckedTime_ = 0f;
				this.backupProcessTick_ = this.tick_;
			}
			if (this.tick_ != this.backupProcessTick_)
			{
				this.stuckedTime_ = 0f;
				this.backupProcessTick_ = this.tick_;
			}
		}
		if (this.stuckedTime_ != 0f && this.stuckedTime_ + 2f < tick)
		{
			this.stuckedTime_ = 0f;
			this.StartResetProcess();
			return true;
		}
		return false;
	}

	public bool IsInRandomRange(float probability)
	{
		return global::UnityEngine.Random.Range(0f, 1f) <= probability;
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.RESET)
		{
			this.StartResetProcess();
		}
		else if (msg.type_ == MonoBehaviourMessageType.WARP)
		{
			WarpMessage warpMessage = (WarpMessage)msg;
			if (warpMessage != null)
			{
				this.goAIKart_.Warp(warpMessage.pos_, warpMessage.rot_, warpMessage.isFlush_, warpMessage.isResetVel_);
				this.SetKartBodyAnimation(KartBodyAnimation.IDLE);
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.APPLY_ITEM && !KartManager.Instance.goCourse_.IsKartGoalIn(this.kartIndex_))
		{
			MonoBehaviourMessage2Param<GameItem, ApplyItemParam> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<GameItem, ApplyItemParam>)msg;
			GameItem[] array = new GameItem[]
			{
				GameItem.BANANA,
				GameItem.WATER_BOMB,
				GameItem.WATER_FLY
			};
			KartBodyAnimation[] array2 = new KartBodyAnimation[]
			{
				KartBodyAnimation.ROLLING,
				KartBodyAnimation.LEVITATION,
				KartBodyAnimation.LEVITATION
			};
			if (monoBehaviourMessage2Param != null)
			{
				GameItem lparam_ = monoBehaviourMessage2Param.lparam_;
				bool flag = false;
				bool flag2 = false;
				if (this.guard_ == null || !this.guard_.active)
				{
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i] == lparam_)
						{
							flag = this.SetKartBodyAnimation(array2[i]);
							break;
						}
					}
				}
				if (lparam_ == GameItem.UFO)
				{
					this.SetSpeedController(new StaticSpeedController(this.deltaTick_ * 0.5f, 3f, this.DEFAULT_DELTA_TICK));
					flag2 = true;
				}
				else if (lparam_ == GameItem.FLIP)
				{
					this.SetSpeedController(new StaticSpeedController(this.deltaTick_ * 0.5f, 2.5f, this.DEFAULT_DELTA_TICK));
					this.flipEffect_.SetActiveRecursively(true);
					flag2 = true;
					base.Invoke("RestoreFlip", 7f);
				}
				else if (lparam_ == GameItem.DEVIL)
				{
					this.SetSpeedController(new StaticSpeedController(this.deltaTick_ * 0.5f, 2.5f, this.DEFAULT_DELTA_TICK));
					this.StartDevilEffect();
					flag2 = true;
				}
				else if (this.guard_ != null && this.guard_.active)
				{
					if (this.shield_ != null && !this.shield_.active)
					{
						this.shield_.SetActiveRecursively(true);
						base.Invoke("DisableShield", 1f);
					}
				}
				else if (lparam_ == GameItem.WATER_MISSILE)
				{
					if (this.IsInRandomRange(0.5f))
					{
						this.kartBody_.transform.localPosition = Vector3.zero;
						this.kartBody_.transform.localRotation = Quaternion.identity;
						base.rigidbody.AddForce(-base.rigidbody.velocity, ForceMode.VelocityChange);
						this.goAIKart_.Forcing = true;
						this.SetKartBodyAnimation(KartBodyAnimation.LEVITATION);
						this.SetCharacterAnimation(CharacterAnimation.CAPTURED_BUBBLE, WrapMode.Default);
						this.waterFlyBubble_.SetActiveRecursively(true);
						flag = true;
					}
					else
					{
						if (!this.waterBombBubble_.active && !this.waterFlyBubble_.active && this.shield_ != null)
						{
							this.shield_.SetActiveRecursively(true);
							base.Invoke("DisableShield", 1f);
						}
						this.SetSpeedController(new StaticSpeedController(this.deltaTick_ * 0.75f, 3f, this.DEFAULT_DELTA_TICK));
					}
				}
				else if (lparam_ == GameItem.WATER_BOMB || lparam_ == GameItem.WATER_FLY)
				{
					this.kartBody_.transform.localPosition = Vector3.zero;
					this.kartBody_.transform.localRotation = Quaternion.identity;
					base.rigidbody.AddForce(-base.rigidbody.velocity, ForceMode.VelocityChange);
					this.goAIKart_.Forcing = true;
					this.SetCharacterAnimation(CharacterAnimation.CAPTURED_BUBBLE, WrapMode.Default);
					if (lparam_ == GameItem.WATER_BOMB)
					{
						this.waterBombBubble_.SetActiveRecursively(true);
					}
					else if (lparam_ == GameItem.WATER_FLY)
					{
						this.waterFlyBubble_.SetActiveRecursively(true);
					}
					this.DisableShield();
				}
				else if (lparam_ == GameItem.BANANA && flag)
				{
					this.SetSpeedController(new LerpSpeedController(this.deltaTick_, 0f, 1.2f));
					this.SetCharacterAnimation(CharacterAnimation.IDLE, WrapMode.Loop);
				}
				if (flag || flag2)
				{
					ApplyItemParam rparam_ = monoBehaviourMessage2Param.rparam_;
					if (rparam_ != null && rparam_.senderId_ == KartManager.PLAYER_KART_IDX)
					{
						MonoBehaviourMessage2Param<CharacterAnimation, WrapMode> monoBehaviourMessage2Param2 = (MonoBehaviourMessage2Param<CharacterAnimation, WrapMode>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.CHANGE_CHARACTER_ANIMATION);
						monoBehaviourMessage2Param2.Initialize(CharacterAnimation.ATTACK_SUCCESS, WrapMode.Default);
						base.SendMessage(MonoBehaiourExConst.PLAYER_KART, monoBehaviourMessage2Param2);
					}
				}
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.GET_ITEM)
		{
			MonoBehaviourMessage1Param<GameItem> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<GameItem>)msg;
			if (monoBehaviourMessage1Param != null && !KartManager.Instance.goCourse_.IsKartGoalIn(this.kartIndex_))
			{
				GameItemElem gameItemElem = default(GameItemElem);
				gameItemElem.itemType_ = monoBehaviourMessage1Param.param_;
				if (this.itemList_.Count > 0)
				{
					gameItemElem.useTime_ = this.itemList_[this.itemList_.Count - 1].useTime_ + global::UnityEngine.Random.Range(1f, 3f);
				}
				else
				{
					gameItemElem.useTime_ = Time.time + global::UnityEngine.Random.Range(0f, 3f);
				}
				this.itemList_.Add(gameItemElem);
			}
		}
	}

	private void Update()
	{
		this.updateTick_++;
		this.InputUpdate();
		if (this.kartResetState_ != 0)
		{
			float time = Time.time;
			if (this.kartResetState_ == 1 && time > this.kartResetTick_ + 0.5f)
			{
				this.ResetKart(this.isResetDefaultPath_);
				this.isResetDefaultPath_ = false;
				this.kartResetState_ = 2;
			}
			if (this.kartResetState_ == 2 && time > this.kartResetTick_ + 1f)
			{
				this.goAIKart_.Forcing = false;
				this.kartResetState_ = 3;
			}
			if (this.kartResetState_ == 3 && time > this.kartResetTick_ + 3f)
			{
				this.goAIKart_.Valid = true;
				this.kartResetState_ = 0;
				this.kartResetTick_ = 0f;
			}
		}
		if (this.speedController_ != null)
		{
			this.speedController_.Update();
			this.deltaTick_ = this.speedController_.GetDeltaTick();
			if (this.speedController_.IsFinish())
			{
				if (this.speedController_.nextSpeedController_ != null)
				{
					this.SetSpeedController(this.speedController_.nextSpeedController_);
				}
				else
				{
					this.defaultSpeedController_.Reset(this.deltaTick_);
					this.SetSpeedController(this.defaultSpeedController_);
				}
			}
		}
		if (!this.isFinishSettingCompleted_)
		{
			if (KartManager.Instance.goCourse_.IsKartGoalIn(this.kartIndex_))
			{
				this.isFinishSettingCompleted_ = true;
				this.SetCharacterAnimation(CharacterAnimation.WIN_GAME, WrapMode.Loop);
				this.animationLock_ = true;
				MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.GOAL_IN);
				monoBehaviourMessage1Param.Initialize(this.kartIndex_);
				MonoBehaviourExCenter.Instance.SendMessage(0, 1, monoBehaviourMessage1Param);
			}
			else if (KartManager.Instance.DriveEndTime > 0f && Time.time >= KartManager.Instance.DriveEndTime)
			{
				this.SetCharacterAnimation(CharacterAnimation.LOSE_GAME, WrapMode.Loop);
				this.animationLock_ = true;
				this.goAIKart_.Forcing = true;
				base.rigidbody.AddForce(-base.rigidbody.velocity, ForceMode.VelocityChange);
				this.isFinishSettingCompleted_ = true;
			}
		}
	}

	private bool IsDebuggingKart()
	{
		return this.kartIndex_ == 2;
	}

	private void InputUpdate()
	{
		if (this.IsDebuggingKart())
		{
		}
	}

	private void UpdateFlickering()
	{
		if (this.goAIKart_.Valid && !this.isVisible_)
		{
			this.isVisible_ = true;
			this.EnableMeshRenderers(true);
		}
		else if (!this.goAIKart_.Valid)
		{
			bool flag = (int)(Time.time * 1000f) % 200 >= 100;
			if (this.isVisible_ != flag)
			{
				this.EnableMeshRenderers(flag);
				this.isVisible_ = flag;
			}
		}
	}

	private void EnableMeshRenderers(bool isVisible)
	{
		foreach (Renderer renderer in this.renderers_)
		{
			renderer.enabled = isVisible;
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		int num = 1 << collider.gameObject.layer;
		if (num == 32768)
		{
			this.isResetDefaultPath_ = false;
			this.StartResetProcess();
		}
		else if (num == 65536 && !collider.name.Contains("s_"))
		{
			int num2 = int.Parse(collider.name);
			this.isResetDefaultPath_ = !this.goAIKart_.IsDefaultPath && !this.goAIKart_.IsPassThoughUserSection(num2);
			if (this.isResetDefaultPath_)
			{
				this.StartResetProcess();
			}
		}
	}

	public override GoKart GetGoKart()
	{
		return this.goAIKart_;
	}

	public override void SetCharacterAnimation(CharacterAnimation anim, WrapMode wrapMode)
	{
		if (this.isNoAnimationCharacter_)
		{
			return;
		}
		this.characterAnimation_ = anim;
		this.character_.animation.wrapMode = wrapMode;
		this.character_.animation.Play(KartDefine.CharacterAnimationString[(int)anim]);
	}

	public override void OnChangePlayMode()
	{
		if (this.playMode_ == KartBasicController.PlayMode.NORMAL)
		{
			this.waterFlyBubble_.SetActiveRecursively(false);
			this.waterBombBubble_.SetActiveRecursively(false);
		}
	}

	public void SetSpeedController(SpeedController controller)
	{
		this.speedController_ = controller;
		this.OnChangeSpeedController();
	}

	public void OnChangeSpeedController()
	{
	}

	public float GetCurrentDeltaTick()
	{
		if (this.speedController_ != null)
		{
			return this.speedController_.GetDeltaTick();
		}
		return this.defaultSpeedController_.GetDeltaTick();
	}

	private new void StartDevilEffect()
	{
		if (this.devilStartEffect_ != null)
		{
			this.devilStartEffect_.SetActiveRecursively(true);
			base.CancelInvoke("PlayDevilEffect");
			base.Invoke("PlayDevilEffect", 1f);
		}
	}

	private new void PlayDevilEffect()
	{
		if (this.devilStartEffect_ != null)
		{
			this.devilStartEffect_.SetActiveRecursively(false);
			if (this.devilPlayEffect_ != null)
			{
				this.devilPlayEffect_.SetActiveRecursively(true);
				base.CancelInvoke("RestoreDevilEffect");
				base.Invoke("RestoreDevilEffect", 6f);
			}
		}
	}

	private bool grounded_;

	public float maxAccelation_ = 10f;

	public float maxVelocity_ = 60f;

	public GoAIKart goAIKart_;

	public bool isRecordInAssetBundle_ = true;

	public string[] recordName_;

	private bool animationLock_;

	private CharacterAnimation characterAnimation_;

	private bool isBooster_;

	private bool isVisible_ = true;

	private Renderer[] renderers_;

	private float tick_;

	private int updateTick_;

	private SpeedController speedController_;

	private DefaultSpeedController defaultSpeedController_;

	private float DEFAULT_DELTA_TICK;

	private float deltaTick_;

	private bool isFinishSettingCompleted_;

	public GameObject boosterWavePrefabLow_;

	private bool isFirstDrive_ = true;

	private float START_BOOSTER_VELOCITY_FACTOR = 1.3f;

	private List<GameItemElem> itemList_ = new List<GameItemElem>();

	private AIController.PreviousState prevState_ = default(AIController.PreviousState);

	private AIController.ResetInfo resetInfo_ = default(AIController.ResetInfo);

	private float stuckedTime_;

	private int kartResetState_;

	private float kartResetTick_;

	private AIControllerType aiType_;

	private bool useStartBoost_;

	private float backupProcessTick_;

	private bool isResetDefaultPath_;

	private struct PreviousState
	{
		public void Reset()
		{
			this.srcPosition_ = Vector3.zero;
			this.targetDirection_ = Vector3.zero;
			this.magnitude_ = 0f;
		}

		public Vector3 srcPosition_;

		public Vector3 targetDirection_;

		public float magnitude_;
	}

	private struct ResetInfo
	{
		public void Reset()
		{
			this.tick_ = 0f;
			this.latestPassingPlane_ = -1;
			this.passCorrect_ = true;
			this.prevState_.Reset();
		}

		public float tick_;

		public int latestPassingPlane_;

		public bool passCorrect_;

		public AIController.PreviousState prevState_;
	}
}
