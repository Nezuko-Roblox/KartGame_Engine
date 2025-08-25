using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyFPSWalker : KartBasicController
{
	public bool UsedNormalBooster
	{
		get
		{
			return this.usedNormalBooster_;
		}
	}

	private void Awake()
	{
		base.gameObject.name = "player_kart";
		base.rigidbody.freezeRotation = false;
		base.rigidbody.useGravity = false;
		base.rigidbody.isKinematic = true;
		this.kartIndex_ = KartManager.PLAYER_KART_IDX;
		BoxCollider boxCollider = base.gameObject.GetComponentInChildren(typeof(BoxCollider)) as BoxCollider;
		if (boxCollider != null)
		{
			BoxCollider boxCollider2 = base.gameObject.AddComponent(typeof(BoxCollider)) as BoxCollider;
			boxCollider2.center = boxCollider.center;
			boxCollider2.size = boxCollider.size;
			boxCollider2.isTrigger = true;
			this.thisBoxCollider_ = boxCollider2;
			this.childBoxCollider_ = boxCollider;
		}
		KartParameter kartParameter = KartManager.Instance.parameter_.kart_[KartManager.PLAYER_KART_IDX];
		base.Initialize(kartParameter.body_, kartParameter.character_, false);
		this.animationManager_ = new CharacterAnimationManager();
		this.animationManager_.Initialize(kartParameter.character_);
		this.renderers_ = base.GetComponentsInChildren<Renderer>();
		this.goPlayKart_ = (GoPlayKart)KartManager.Instance.SetKart(KartManager.PLAYER_KART_IDX, new GoPlayKartBuilder(), this, this.wheels_);
		KartManager.Instance.goCourse_.SetKart(KartManager.PLAYER_KART_IDX, this.goPlayKart_);
		TextAsset textAsset = (TextAsset)Resources.Load(KartManager.Instance.parameter_.level_);
		if (textAsset == null)
		{
		}
		string mainAsset = KartAssetDefinitionManager.Instance.GetMainAsset((int)kartParameter.body_);
		TextAsset textAsset2 = (TextAsset)ResourceLoader.Instance.GetAsset(mainAsset, "param");
		this.goPlayKart_.UpdateKartSpec(textAsset.text, (!(textAsset2 == null)) ? textAsset2.text : string.Empty);
		this.RegistMonoBehaviour(MonoBehaiourExConst.PLAYER_KART);
		this.toSoundController_ = (MonoBehaviourMessage1Param<SoundController.FxType>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.PLAY_SOUND);
	}

	private void DumpInit()
	{
		if (Debug.isDebugBuild)
		{
		}
	}

	private void Start()
	{
		this.skidmark = (Skidmarks)global::UnityEngine.Object.FindObjectOfType(typeof(Skidmarks));
		if (this.skidmark == null)
		{
		}
		this.skidmarkTime = new float[4];
		for (int i = 0; i < 4; i++)
		{
			this.skidmarkTime[i] = 0f;
		}
		base.rigidbody.mass = this.goPlayKart_.m_spec.mass;
		base.rigidbody.centerOfMass = new Vector3(0f, 0f, 0f);
		RegenInfo startInfo = KartManager.Instance.goCourse_.GetStartInfo(KartManager.PLAYER_KART_IDX);
		base.transform.localPosition = startInfo.position_;
		base.transform.localRotation = Quaternion.LookRotation(startInfo.direction_, Vector3.up);
		this.returnToFrontTime = 0f;
	}

	private void UpdateDaoAnimation()
	{
		if (this.character_ == null)
		{
			return;
		}
		if (this.kartBody_.animation.isPlaying)
		{
			return;
		}
		CharacterAnimation characterAnimation = CharacterAnimation.IDLE;
		WrapMode wrapMode = WrapMode.Loop;
		for (int i = 0; i < this.HI_PRIORITY_ANIMATION.Length; i++)
		{
			if (this.character_.animation.IsPlaying(this.animationManager_.GetAnimationString(this.HI_PRIORITY_ANIMATION[i])))
			{
				return;
			}
		}
		if (this.goPlayKart_.isCrash_ && this.goPlayKart_.crashVelocity_ >= this.SMALL_CRASH_VELOCITY)
		{
			if (this.goPlayKart_.crashVelocity_ >= this.BIG_CRASH_VELOCITY)
			{
				characterAnimation = CharacterAnimation.BIG_ACCIDENT;
			}
			else
			{
				characterAnimation = CharacterAnimation.SMALL_ACCIDENT;
			}
			this.goPlayKart_.CharacterAnim = characterAnimation;
			this.character_.animation.wrapMode = WrapMode.Default;
			this.character_.animation.Play(this.animationManager_.GetAnimationString(this.goPlayKart_.CharacterAnim));
			this.characterFace_.renderer.material = this.animationManager_.GetFaceMaterial(this.goPlayKart_.CharacterAnim);
			return;
		}
		if (this.goPlayKart_.isRealBoost())
		{
			characterAnimation = CharacterAnimation.BOOST;
			wrapMode = WrapMode.Loop;
		}
		else if (this.goPlayKart_.getRealBrake() != 0f && !this.goPlayKart_.Stuck)
		{
			if (Vector3.Dot(base.transform.forward, this.goPlayKart_.m_KartWLVel) < 0f)
			{
				wrapMode = WrapMode.ClampForever;
				if (this.goPlayKart_.getRealSteer() < 0f)
				{
					characterAnimation = CharacterAnimation.TURN_BACK_RIGHT;
				}
				else
				{
					characterAnimation = CharacterAnimation.TURN_BACK_LEFT;
				}
			}
		}
		else if (this.goPlayKart_.getRealSteer() != 0f)
		{
			wrapMode = WrapMode.ClampForever;
			if (this.goPlayKart_.getRealSteer() > 0f)
			{
				characterAnimation = CharacterAnimation.TURN_RIGHT;
			}
			else if (this.goPlayKart_.getRealSteer() < 0f)
			{
				characterAnimation = CharacterAnimation.TURN_LEFT;
			}
		}
		if ((this.goPlayKart_.CharacterAnim == CharacterAnimation.TURN_BACK_RIGHT && characterAnimation == CharacterAnimation.TURN_BACK_LEFT) || (this.goPlayKart_.CharacterAnim == CharacterAnimation.TURN_BACK_LEFT && characterAnimation == CharacterAnimation.TURN_BACK_RIGHT))
		{
			this.returnToFrontTime = 0.33f;
			characterAnimation = CharacterAnimation.IDLE;
			wrapMode = WrapMode.Loop;
		}
		if (this.returnToFrontTime > 0f)
		{
			this.returnToFrontTime -= Time.deltaTime;
			characterAnimation = CharacterAnimation.IDLE;
			wrapMode = WrapMode.Loop;
		}
		if (this.goPlayKart_.CharacterAnim != characterAnimation)
		{
			this.character_.animation.wrapMode = wrapMode;
			this.character_.animation.CrossFade(this.animationManager_.GetAnimationString(characterAnimation));
			this.characterFace_.renderer.material = this.animationManager_.GetFaceMaterial(characterAnimation);
			this.goPlayKart_.CharacterAnim = characterAnimation;
		}
	}

	private void Update()
	{
		this.InputUpdate();
		this.UpdateDaoAnimation();
		this.jobManager_.Update();
	}

	private void LateUpdate()
	{
		this.UpdateCollider();
	}

	private void UpdateCollider()
	{
		if (this.thisBoxCollider_ != null && this.childBoxCollider_ != null)
		{
			this.thisBoxCollider_.center = this.childBoxCollider_.center;
			Vector3 localPosition = this.childBoxCollider_.transform.localPosition;
			this.thisBoxCollider_.center += localPosition;
		}
	}

	private void InputUpdate()
	{
		TouchController instance = iOSController.Instance;
		instance.InputUpdate();
		if (!this.kartBody_.animation.isPlaying)
		{
			this.goPlayKart_.setAccel(instance.GetAccel());
			this.goPlayKart_.setBrake(instance.GetBreak());
			this.goPlayKart_.Wheel = instance.GetDirection() * ((!instance.analogSteer_ || !ScreenController.Instance.IsDisplayingLandscapeRight()) ? 1f : (-1f));
			if (instance.GetDrift() && this.goPlayKart_.Wheel != 0f)
			{
				this.goPlayKart_.setDrift(true);
			}
			else
			{
				this.goPlayKart_.setDrift(false);
			}
			float driveStartTime = KartManager.Instance.DriveStartTime;
			if (this.goPlayKart_.IsAccel())
			{
				if (driveStartTime >= 0f && MathHelper.IsBetweenII(Time.time, driveStartTime - 0.1f, driveStartTime + 0.1f))
				{
					if (Debug.isDebugBuild)
					{
						Debug.Log("OH!!! START BOOSTER!!!!!");
					}
					this.goPlayKart_.setBoost(1000, BoostKind.BoostStart);
				}
				else if (this.goPlayKart_.Stuck && instance.GetDrift())
				{
					this.goPlayKart_.setBoost(1000, BoostKind.BoostPlay);
				}
			}
		}
		else
		{
			this.goPlayKart_.setAccel(false);
			this.goPlayKart_.setBrake(false);
			this.goPlayKart_.setDrift(false);
			this.goPlayKart_.Wheel = 0f;
		}
		if (instance.GetItem() && !KartManager.Instance.goCourse_.IsKartGoalIn(KartManager.PLAYER_KART_IDX))
		{
			GameItem firstItemSlot = (GameItem)KartManager.Instance.gameInterface_.GetFirstItemSlot();
			if (firstItemSlot != GameItem.NONE)
			{
				if (firstItemSlot == GameItem.BOOSTER)
				{
					if (!this.goPlayKart_.isRealBoost() && !this.goPlayKart_.isZoneBoost() && this.goPlayKart_.IsAccel())
					{
						KartManager.Instance.gameInterface_.UseItemSlotItem();
						this.goPlayKart_.setBoost(this.goPlayKart_.GetNormalBoosterTime(), BoostKind.BoostNormal);
						this.usedNormalBooster_ = true;
						InGameStatistics.Instance.TotalItemUsage.IncreaseStat(GameItem.BOOSTER);
						InGameStatistics.Instance.EffectiveItemUsage.IncreaseStat(GameItem.BOOSTER);
					}
				}
				else if (firstItemSlot == GameItem.GUARD)
				{
					if (this.guard_ != null && !this.guard_.active)
					{
						KartManager.Instance.gameInterface_.UseItemSlotItem();
						base.UseItem(firstItemSlot);
					}
				}
				else
				{
					KartManager.Instance.gameInterface_.UseItemSlotItem();
					base.UseItem(firstItemSlot);
				}
			}
		}
	}

	private void UpdateWheelGraphic()
	{
		if (this.goPlayKart_ == null || this.skidmark == null)
		{
			return;
		}
		Vector3 vector = base.transform.right * 0.2f;
		for (int i = 2; i < 4; i++)
		{
			if ((double)this.skidmarkTime[i] < 0.02)
			{
				this.skidmarkTime[i] += Time.deltaTime;
			}
			else
			{
				this.skidmarkTime[i] = 0f;
				if (this.goPlayKart_.m_isDrift && this.goPlayKart_.m_sus.wheelContact[i])
				{
					Vector3 vector2 = this.wheels_[i].position + vector * ((i % 2 != 0) ? 1f : (-1f));
					this.lastSkidMark[i] = this.skidmark.AddSkidMark(vector2, base.transform.up, 1f, this.lastSkidMark[i]);
				}
				else
				{
					this.lastSkidMark[i] = -1;
				}
			}
		}
	}

	private void UpdateWheels()
	{
		this.wheelRotation = Mathf.Repeat(this.wheelRotation + this.goPlayKart_.GetKartRealSpeed() * Time.deltaTime * 180f / 3.14159274f, 360f);
		Quaternion quaternion = Quaternion.Euler(this.wheelRotation, this.goPlayKart_.getSteerAngle() * 3f, 0f);
		Quaternion quaternion2 = Quaternion.Euler(this.wheelRotation, 0f, 0f);
		this.wheels_[1].localRotation = quaternion;
		this.wheels_[0].localRotation = quaternion;
		this.wheels_[3].localRotation = quaternion2;
		this.wheels_[2].localRotation = quaternion2;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		this.fixedUpdateCount_++;
		this.goPlayKart_.BackupVelocity = base.rigidbody.velocity;
		this.goPlayKart_.basicAction(Time.time);
		this.UpdateWheels();
		this.UpdateWheelGraphic();
		this.processLeadCompensation();
		KartManager.Instance.goCourse_.Update();
		bool flag = this.goPlayKart_.isRealBoost() && this.playMode_ == KartBasicController.PlayMode.NORMAL;
		if (this.isBooster_ != flag)
		{
			this.isBooster_ = this.goPlayKart_.isRealBoost();
			base.SetEnableBooster(this.isBooster_);
		}
		if (!this.goPlayKart_.Forcing && !this.goPlayKart_.IsInResetState)
		{
			Vector3 targetUpVector = this._targetUpVector;
			Vector3 right = base.transform.right;
			Vector3 vector = base.transform.forward;
			vector = Vector3.Cross(right, targetUpVector);
			float num = ((!this.grounded) ? 1f : 10f);
			Quaternion quaternion = Quaternion.LookRotation(vector, targetUpVector);
			Quaternion quaternion2 = Quaternion.Slerp(base.transform.localRotation, quaternion, Time.deltaTime * num);
			base.transform.localRotation = quaternion2;
			this._targetUpVector = Vector3.up;
			if (this.grounded)
			{
				this.goPlayKart_.m_KartLAVel.x = 0f;
				this.goPlayKart_.m_KartLAVel.z = 0f;
				Quaternion localRotation = base.transform.localRotation;
				Quaternion quaternion3 = MathHelper.CreateQuaternion(0f, this.goPlayKart_.m_KartLAVel);
				Quaternion quaternion4 = localRotation * quaternion3;
				MathHelper.QuaMulScala(ref quaternion4, 0.5f * Time.deltaTime);
				MathHelper.QuaAdd(ref localRotation, quaternion4);
				MathHelper.QuaNormalize(ref localRotation);
				base.transform.localRotation = localRotation;
				this.prevState_.rotate_ = localRotation;
			}
			Vector3 velocity_ = this.prevState_.velocity_;
			Vector3 vector2 = this.goPlayKart_.m_KartWLVel - velocity_;
			vector2.x = Mathf.Clamp(vector2.x, -100f, 100f);
			vector2.z = Mathf.Clamp(vector2.z, -100f, 100f);
			vector2.y = 0f;
			Vector3 kartWLVel = this.goPlayKart_.m_KartWLVel;
			Vector3 vector3 = kartWLVel * Time.deltaTime;
			Vector3 position = base.transform.position;
			Vector3 vector4 = base.transform.position + vector3;
			Vector3 vector5 = vector4 - position;
			Bounds bounds = base.rigidbody.collider.bounds;
			float num2 = Mathf.Min(new float[]
			{
				bounds.size.x,
				bounds.size.y,
				bounds.size.z
			});
			if (vector5.magnitude >= num2 / 2f)
			{
				Vector3 center = bounds.center;
				Vector3 vector6 = bounds.center + vector3;
				vector6 = this.GetCollisionCheckedPosition(center, vector6);
				vector4 = base.transform.position + (vector6 - center);
			}
			base.transform.position = vector4;
		}
		this.UpdatePlayMode();
		Vector3 vector7 = base.rigidbody.position - this.prevState_.position_;
		this.goPlayKart_.m_KartRealVelocity = vector7 / Time.deltaTime;
		this.prevState_.position_ = base.rigidbody.position;
		this.prevState_.velocity_ = this.goPlayKart_.m_KartWLVel;
		this.prevState_.angular_ = base.rigidbody.angularVelocity;
		this.prevState_.forward_ = base.transform.forward;
		this.prevState_.isGrounded_ = this.grounded;
		this.grounded = false;
		if (this.isSuddenChange)
		{
			this.isSuddenChange = false;
		}
		this.goPlayKart_.ResetCrash();
		this.goPlayKart_.ResetShock();
		this.UpdateFlickering();
	}

	private Vector3 GetCollisionCheckedPosition(Vector3 fromPosition, Vector3 toPosition)
	{
		Vector3 vector = toPosition - fromPosition;
		Vector3 normalized = vector.normalized;
		float magnitude = vector.magnitude;
		RaycastHit[] array = Physics.RaycastAll(fromPosition, vector, vector.magnitude, 8413440);
		if (array != null && array.Length > 0)
		{
			float num = magnitude;
			foreach (RaycastHit raycastHit in array)
			{
				if (!raycastHit.collider.gameObject.Equals(base.gameObject) && raycastHit.distance < num)
				{
					toPosition = fromPosition + normalized * (raycastHit.distance - 0.1f);
					num = raycastHit.distance;
				}
			}
		}
		return toPosition;
	}

	private void UpdatePlayMode()
	{
		if (this.playMode_ == KartBasicController.PlayMode.ANIMATION_PLAYING && !this.kartBody_.animation.isPlaying)
		{
			this.goPlayKart_.Forcing = false;
			this.SetCharacterAnimation(CharacterAnimation.IDLE, WrapMode.Loop);
			Transform kartBodyTransform = base.KartBodyTransform;
			kartBodyTransform.localPosition = Vector3.zero;
			kartBodyTransform.localRotation = Quaternion.identity;
			this.playMode_ = KartBasicController.PlayMode.NORMAL;
			this.goPlayKart_.KartBodyAnim = KartBodyAnimation.IDLE;
			this.OnChangePlayMode();
			iOSController.Instance.CheckShake = false;
			if (iOSController.Instance.GetShakeCount() > 1)
			{
				this.goPlayKart_.setBoostForce(1000, BoostKind.BoostStart);
				InGameStatistics.Instance.Others.ShakeBooster++;
				if (Debug.isDebugBuild)
				{
					Debug.Log("Boost Start");
				}
			}
			iOSController.Instance.ResetShaking();
		}
	}

	public override void SetCharacterAnimation(CharacterAnimation anim, WrapMode wrapMode)
	{
		this.goPlayKart_.CharacterAnim = anim;
		this.character_.animation.wrapMode = wrapMode;
		this.character_.animation.Play(this.animationManager_.GetAnimationString(anim));
		this.characterFace_.renderer.material = this.animationManager_.GetFaceMaterial(anim);
	}

	private void OutputDebugString(string str)
	{
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
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.WARP)
		{
			WarpMessage warpMessage = (WarpMessage)msg;
			if (warpMessage != null)
			{
				this.goPlayKart_.Warp(warpMessage.pos_, warpMessage.rot_, warpMessage.isFlush_, warpMessage.isResetVel_);
				this.goPlayKart_.ResetDriftGauge();
				this.SetCharacterAnimation(CharacterAnimation.IDLE, WrapMode.Loop);
				this.SetKartBodyAnimation(KartBodyAnimation.IDLE);
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.CHANGE_CHARACTER_ANIMATION)
		{
			MonoBehaviourMessage2Param<CharacterAnimation, WrapMode> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<CharacterAnimation, WrapMode>)msg;
			if (monoBehaviourMessage2Param != null)
			{
				this.SetCharacterAnimation(monoBehaviourMessage2Param.lparam_, monoBehaviourMessage2Param.rparam_);
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.APPLY_ITEM && !KartManager.Instance.goCourse_.IsKartGoalIn(KartManager.PLAYER_KART_IDX))
		{
			MonoBehaviourMessage2Param<GameItem, ApplyItemParam> monoBehaviourMessage2Param2 = (MonoBehaviourMessage2Param<GameItem, ApplyItemParam>)msg;
			GameItem[] array = new GameItem[]
			{
				GameItem.BANANA,
				GameItem.WATER_BOMB,
				GameItem.WATER_FLY,
				GameItem.WATER_MISSILE
			};
			KartBodyAnimation[] array2 = new KartBodyAnimation[]
			{
				KartBodyAnimation.ROLLING,
				KartBodyAnimation.LEVITATION,
				KartBodyAnimation.LEVITATION,
				KartBodyAnimation.LEVITATION
			};
			if (monoBehaviourMessage2Param2 != null)
			{
				GameItem lparam_ = monoBehaviourMessage2Param2.lparam_;
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
					this.jobManager_.AddJob(new JobApplyUFO());
				}
				else if (lparam_ == GameItem.FLIP)
				{
					if (!KartManager.Instance.goPlayKart_.IsStatusOn(GoKartStatus.FLIP))
					{
						this.goPlayKart_.SetStatus(GoKartStatus.FLIP, true);
						base.SendMessage(5, this.toSoundController_.Initialize(SoundController.FxType.FLIP));
						this.flipEffect_.SetActiveRecursively(true);
						base.InvokeRepeating("ToggleFlipVisible", 0f, 0.1f);
						base.Invoke("ApplyFlip", 1f);
						base.Invoke("RestoreFlip", 7f);
					}
				}
				else if (lparam_ == GameItem.DEVIL)
				{
					if (!KartManager.Instance.goPlayKart_.IsStatusOn(GoKartStatus.DEVIL))
					{
						this.goPlayKart_.SetStatus(GoKartStatus.DEVIL, true);
						base.SendMessage(5, this.toSoundController_.Initialize(SoundController.FxType.DEVIL));
						this.ApplyDevil();
					}
				}
				else if (lparam_ == GameItem.SHIELD && !this.waterBombBubble_.active && !this.waterFlyBubble_.active)
				{
					if (this.shield_ != null)
					{
						if (KartManager.Instance.parameter_.Stage == StageType.GAME_WIFI && KartManager.PLAYER_KART_IDX == this.kartIndex_)
						{
							NetworkManager.Inst.SendPacketToAll(new ShieldEffectPacket(this.kartIndex_), SendDataMode.RELIABLE);
						}
						base.SendMessage(5, this.toSoundController_.Initialize(SoundController.FxType.SHIELD));
						this.shield_.SetActiveRecursively(true);
						base.Invoke("DisableShield", 1f);
					}
				}
				else if (this.guard_ != null && this.guard_.active)
				{
					if (this.shield_ != null && !this.shield_.active)
					{
						if (KartManager.Instance.parameter_.Stage == StageType.GAME_WIFI && KartManager.PLAYER_KART_IDX == this.kartIndex_)
						{
							NetworkManager.Inst.SendPacketToAll(new ShieldEffectPacket(this.kartIndex_), SendDataMode.RELIABLE);
						}
						base.SendMessage(5, this.toSoundController_.Initialize(SoundController.FxType.SHIELD));
						this.shield_.SetActiveRecursively(true);
						base.Invoke("DisableShield", 1f);
					}
					InGameStatistics.Instance.EffectiveItemUsage.IncreaseStat(GameItem.GUARD);
				}
				else if (lparam_ == GameItem.WATER_BOMB || lparam_ == GameItem.WATER_FLY || lparam_ == GameItem.WATER_MISSILE)
				{
					this.kartBody_.transform.localPosition = Vector3.zero;
					this.kartBody_.transform.localRotation = Quaternion.identity;
					base.rigidbody.AddForce(-base.rigidbody.velocity, ForceMode.VelocityChange);
					this.goPlayKart_.Forcing = true;
					this.SetCharacterAnimation(CharacterAnimation.CAPTURED_BUBBLE, WrapMode.Default);
					if (lparam_ == GameItem.WATER_BOMB)
					{
						this.waterBombBubble_.SetActiveRecursively(true);
						base.SendMessage(5, this.toSoundController_.Initialize(SoundController.FxType.TRAPPED_WATERBOMB));
					}
					else if (lparam_ == GameItem.WATER_FLY || lparam_ == GameItem.WATER_MISSILE)
					{
						this.waterFlyBubble_.SetActiveRecursively(true);
						base.SendMessage(5, this.toSoundController_.Initialize(SoundController.FxType.TRAPPED_WATERFLY));
					}
					this.DisableShield();
					flag2 = true;
					iOSController.Instance.CheckShake = true;
				}
				else if (lparam_ == GameItem.BANANA && flag)
				{
					base.SendMessage(5, this.toSoundController_.Initialize(SoundController.FxType.TRAPPED_BANANA));
					flag2 = true;
				}
				if (flag2 && KartManager.Instance.parameter_.Stage == StageType.GAME_WIFI && KartManager.PLAYER_KART_IDX == this.kartIndex_)
				{
					int senderId_ = monoBehaviourMessage2Param2.rparam_.senderId_;
					if (senderId_ != this.kartIndex_)
					{
						string id_ = KartManager.Instance.parameter_.kart_[senderId_].id_;
						if (id_ != null)
						{
							NetworkManager.Inst.SendPacket(new ItemSuccessPacket(), id_, SendDataMode.RELIABLE);
						}
					}
				}
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.GET_ITEM)
		{
			MonoBehaviourMessage1Param<GameItem> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<GameItem>)msg;
			if (monoBehaviourMessage1Param != null)
			{
				base.SendMessage(5, this.toSoundController_.Initialize(SoundController.FxType.GET_ITEM));
				if (KartManager.Instance.gameInterface_.AddItemSlotItem((int)monoBehaviourMessage1Param.param_))
				{
					base.SendMessage(3, monoBehaviourMessage1Param);
				}
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.RACE_OVER)
		{
			base.SendMessage(5, this.toSoundController_.Initialize(SoundController.FxType.DISABLE_UPDATE_PLAYER_SOUND));
		}
	}

	private void UpdateFlickering()
	{
		if (this.goPlayKart_.Valid && !this.isVisible_)
		{
			this.isVisible_ = true;
			this.EnableMeshRenderers(true);
		}
		else if (!this.goPlayKart_.Valid)
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
		if (1 << collider.gameObject.layer == 65536)
		{
			int num;
			int num2;
			if (collider.name.Contains("s_"))
			{
				num = int.Parse(collider.name.Substring(2));
				num2 = 1;
			}
			else
			{
				num = int.Parse(collider.name);
				num2 = 0;
			}
			MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.ENTER_USER_SECTION);
			monoBehaviourMessage2Param.Initialize(num2, num);
			base.SendMessage(1, monoBehaviourMessage2Param);
		}
		this.UpdateTriggerPosition(collider, true);
	}

	private void OnTriggerStay(Collider collider)
	{
		this.UpdateTriggerPosition(collider, false);
	}

	private void UpdateTriggerPosition(Collider collider, bool firstTrigger)
	{
		float num = 0.65f;
		if (1 << collider.gameObject.layer == 256)
		{
			Vector3 position = base.transform.position;
			Vector3 vector = -base.transform.up;
			Vector3 vector2 = position - vector;
			float magnitude = vector.magnitude;
			Ray ray = new Ray(vector2, vector);
			RaycastHit[] array = Physics.RaycastAll(ray, magnitude * 2f, 8413440);
			if (array != null && array.Length > 0)
			{
				bool flag = false;
				float num2 = 0f;
				Vector3 vector3 = Vector3.zero;
				int num3 = 0;
				int num4 = 0;
				foreach (RaycastHit raycastHit in array)
				{
					if (raycastHit.normal.y > 0f && (!flag || raycastHit.distance < num2))
					{
						num2 = raycastHit.distance;
						vector3 = raycastHit.normal;
						num4 = raycastHit.collider.gameObject.layer;
						flag = true;
					}
					num3++;
				}
				if (flag)
				{
					bool flag2 = false;
					if (magnitude - num2 > 0f)
					{
						this._targetUpVector = vector3;
						flag2 = true;
					}
					if (this.goPlayKart_.m_Contact)
					{
						vector3 = this.goPlayKart_.m_sus.contactN;
						this._targetUpVector = vector3;
						flag2 = true;
					}
					if (flag2)
					{
						float num5 = (float)(1 << num4);
						if (magnitude - num2 > 0f && num5 != 16384f)
						{
							Vector3 vector4 = -vector * (magnitude - num2);
							base.transform.position += vector4;
						}
						float num6 = Vector3.Dot(this.goPlayKart_.m_KartWLVel, vector3);
						Vector3 vector5 = vector3 * num6;
						Vector3 vector6 = this.goPlayKart_.m_KartWLVel - vector5;
						if (vector3.y > num)
						{
							float num7 = Mathf.Abs(Vector3.Dot(this.goPlayKart_.m_first.front, vector3)) * 0.7f;
							this.goPlayKart_.m_KartWLVel = vector5 * -num7 + vector6;
							this.goPlayKart_.shockVelocity_ = vector5.magnitude;
							this.grounded = true;
							if (!this.prevState_.isGrounded_)
							{
								this.goPlayKart_.SetShock(Mathf.Abs(num6));
							}
						}
						else if (this.goPlayKart_.isCrash_)
						{
							float num8 = Mathf.Abs(num6);
							this.goPlayKart_.SetCrash(num8);
							if (num8 >= this.SMALL_CRASH_VELOCITY)
							{
								this.goPlayKart_.ResetDriftGauge();
							}
						}
						float num9 = Vector3.Dot(Vector3.Cross(vector3, Vector3.up), this.goPlayKart_.m_first.front);
						base.transform.RotateAroundLocal(base.transform.up, num9 * 0.3f * Time.deltaTime);
					}
				}
			}
		}
		if (this.goPlayKart_.m_Contact)
		{
		}
		if (!this.goPlayKart_.Forcing && !this.goPlayKart_.IsInResetState)
		{
			Vector3 center = base.collider.bounds.center;
			Vector3 size = base.collider.bounds.size;
			this.HandleCollision(base.transform.right, center, size.x / 2f, true, num);
			this.HandleCollision(-base.transform.right, center, size.x / 2f, true, num);
			this.HandleCollision(base.transform.forward, center, size.z * 0.75f, true, num);
			this.HandleCollision(-base.transform.forward, center, size.z / 2f, true, num);
		}
	}

	private bool HandleCollision(Vector3 localRayDirection, Vector3 rayOrigin, float rayLength, bool handleCrash, float gndLimit)
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(rayOrigin, localRayDirection, out raycastHit, rayLength, 8413440))
		{
			Vector3 vector = raycastHit.normal;
			if (raycastHit.distance < rayLength && vector.y < gndLimit)
			{
				float num = Vector3.Dot(vector, localRayDirection);
				if (num > 0f)
				{
					vector = -vector;
				}
				float num2 = Mathf.Abs(num);
				float num3 = (float)(1 << raycastHit.collider.gameObject.layer);
				if (num3 != 16384f)
				{
					Vector3 vector2 = -vector * (raycastHit.distance - rayLength) * num2;
					base.transform.position += vector2;
				}
				if (handleCrash && !this.goPlayKart_.isCrash_)
				{
					float num4 = Vector3.Dot(vector, this.prevState_.velocity_);
					if (num4 < 0f)
					{
						float num5 = Mathf.Abs(num4);
						this.goPlayKart_.SetCrash(num5);
						if (1 << base.collider.gameObject.layer == 8388608 && num5 >= this.SMALL_CRASH_VELOCITY)
						{
							this.goPlayKart_.ResetDriftGauge();
						}
						this.goPlayKart_.m_ctrl.oldSteerAngle = 0f;
						float num6 = 0.618f;
						Vector3 vector3 = Vector3.zero;
						if (raycastHit.collider.gameObject.transform.parent != null)
						{
							RigidbodyFPSWalker rigidbodyFPSWalker = raycastHit.collider.gameObject.transform.parent.GetComponent(typeof(RigidbodyFPSWalker)) as RigidbodyFPSWalker;
							if (rigidbodyFPSWalker != null)
							{
								vector3 = rigidbodyFPSWalker.goPlayKart_.m_KartWLVel;
							}
							else
							{
								AIController aicontroller = raycastHit.collider.gameObject.transform.parent.GetComponent(typeof(AIController)) as AIController;
								if (aicontroller != null)
								{
									vector3 = aicontroller.rigidbody.velocity;
								}
							}
						}
						Vector3 vector4 = this.goPlayKart_.m_KartWLVel - vector3 * num6;
						vector4.x = Mathf.Clamp(vector4.x, -20f, 20f);
						vector4.y = Mathf.Clamp(vector4.y, -5f, 5f);
						vector4.z = Mathf.Clamp(vector4.z, -20f, 20f);
						Vector3 vector5 = vector * Vector3.Dot(vector4, vector);
						Vector3 vector6 = this.goPlayKart_.m_KartWLVel - vector5;
						if (vector.y > gndLimit)
						{
							float num7 = Mathf.Abs(Vector3.Dot(this.goPlayKart_.m_first.front, vector)) * 0.7f;
							this.goPlayKart_.m_KartWLVel = vector5 * -num7 + vector6;
							this.goPlayKart_.m_cState.shockVel = vector5.magnitude;
						}
						else
						{
							this.goPlayKart_.m_adBoost.validTrigger = false;
							this.goPlayKart_.m_driftGauge.progressOn = false;
							this.goPlayKart_.m_driftGauge.progressTime = 0f;
							this.goPlayKart_.m_driftGauge.progress = 0f;
							float magnitude = vector6.magnitude;
							Vector3 vector7 = ((magnitude <= 0f) ? Vector3.zero : vector6.normalized);
							Vector3 vector8 = Vector3.zero;
							vector8 = vector5 * -1.2f - vector7 * Mathf.Min(vector5.magnitude * 1.2f, magnitude * 0.02f);
							if (!this.goPlayKart_.m_drift.slipMode && this.goPlayKart_.m_Contact)
							{
								bool flag = false;
								float num8 = 0f;
								float num9 = 0f;
								if (Mathf.Abs(Vector3.Dot(vector, base.transform.forward)) > 0.5f && Mathf.Approximately(this.goPlayKart_.m_ctrl.getRealAccel(), 1f))
								{
									if (Mathf.Approximately(this.goPlayKart_.m_ctrl.steer, 1f))
									{
										flag = true;
										num8 = ((!this.goPlayKart_.m_ctrl.wheelFlip && !this.goPlayKart_.m_ctrl.wheelDevil) ? 1f : (-1f));
										num9 = 1f;
									}
									else if (Mathf.Approximately(this.goPlayKart_.m_ctrl.steer, -1f))
									{
										flag = true;
										num8 = ((!this.goPlayKart_.m_ctrl.wheelFlip && !this.goPlayKart_.m_ctrl.wheelDevil) ? (-1f) : 1f);
										num9 = -1f;
									}
								}
								if (flag)
								{
									Vector3 vector9 = vector;
									Vector3 vector10 = -this.goPlayKart_.m_first.front;
									vector10.Normalize();
									float num10 = Vector3.Dot(vector9, vector10);
									float num11;
									if (Vector3.Dot(vector9, this.goPlayKart_.m_first.left * num9) < 0f)
									{
										num10 = 2f - num10;
										num11 = Mathf.Max(num10, 1.5f);
									}
									else
									{
										num11 = num10 * num10 * num10;
									}
									vector8 -= vector5.normalized * (3f * num10 + 1f);
									vector8 += this.goPlayKart_.m_first.left * (3f * num10 + 1f) * num8;
									float num12 = (6f * num11 + 5f) * num8 * Time.deltaTime;
									base.transform.RotateAroundLocal(Vector3.up, num12);
								}
							}
							this.goPlayKart_.m_KartWLVel += vector8;
							float num13 = Vector3.Dot(vector, this.goPlayKart_.m_first.front);
							float num14 = Vector3.Dot(vector, this.goPlayKart_.m_first.left);
							float num15 = 3f;
							if (Mathf.Abs(num13 * 0.8f) > Mathf.Abs(num14))
							{
								Vector3 zero = Vector3.zero;
								zero.z = num14 * ((num13 <= 0f) ? (-1f) : 1f) * Mathf.Max(1f, Mathf.Min(30f, Mathf.Abs(Vector3.Dot(vector, this.goPlayKart_.m_KartWLVel) * 0.5f)));
								if (Vector3.Dot(zero, this.goPlayKart_.m_KartLAVel) <= 1f)
								{
									float num16 = zero.z * Time.deltaTime * num15;
									base.transform.RotateAroundLocal(Vector3.up, num16);
								}
							}
							else
							{
								Vector3 zero2 = Vector3.zero;
								zero2.z = num13 * ((num14 <= 0f) ? 1f : (-1f)) * Mathf.Max(1f, Mathf.Min(30f, Mathf.Abs(Vector3.Dot(vector, this.goPlayKart_.m_KartWLVel) * 0.5f)));
								if (Vector3.Dot(zero2, this.goPlayKart_.m_KartLAVel) <= 1f)
								{
									float num17 = zero2.z * Time.deltaTime * num15;
									base.transform.RotateAroundLocal(Vector3.up, num17);
								}
							}
						}
					}
				}
				return true;
			}
		}
		return false;
	}

	public override GoKart GetGoKart()
	{
		return this.goPlayKart_;
	}

	public void UpdateTest()
	{
		this.generateItemEvent_.Update();
		if (this.generateItemEvent_.IsEventOccurred())
		{
			int num = global::UnityEngine.Random.Range(0, 10);
			KartManager.Instance.gameInterface_.AddItemSlotItem(num);
		}
	}

	public override void OnChangePlayMode()
	{
		if (this.playMode_ == KartBasicController.PlayMode.NORMAL)
		{
			this.waterFlyBubble_.SetActiveRecursively(false);
			this.waterBombBubble_.SetActiveRecursively(false);
		}
	}

	private void SetFlipVisible(bool visible)
	{
		if (this.flipRenderer_ != null)
		{
			foreach (Renderer renderer in this.flipRenderer_)
			{
				renderer.enabled = visible;
			}
		}
	}

	protected override void RestoreFlip()
	{
		this.SetFlipVisible(true);
		base.RestoreFlip();
		base.CancelInvoke("ToggleFlipVisible");
		this.goPlayKart_.SetStatus(GoKartStatus.FLIP, false);
	}

	protected void RestoreDevil()
	{
		this.goPlayKart_.m_ctrl.wheelDevil = false;
		this.RestoreDevilEffect();
		this.goPlayKart_.SetStatus(GoKartStatus.DEVIL, false);
	}

	private void ToggleFlipVisible()
	{
		if (this.flipRenderer_ != null)
		{
			foreach (Renderer renderer in this.flipRenderer_)
			{
				renderer.enabled = !renderer.enabled;
			}
		}
	}

	private void ApplyFlip()
	{
		if (!StageController.IsInstantiated())
		{
			return;
		}
		this.SetFlipVisible(true);
		base.CancelInvoke("ToggleFlipVisible");
		base.InvokeRepeating("ToggleFlipVisible", 5f, 0.1f);
	}

	private void ApplyDevil()
	{
		if (!StageController.IsInstantiated())
		{
			return;
		}
		this.goPlayKart_.m_ctrl.wheelDevil = true;
		this.StartDevilEffect();
		base.CancelInvoke("RestoreDevil");
		base.Invoke("RestoreDevil", 6f);
	}

	private void processLeadCompensation()
	{
		int num = KartManager.Instance.goCourse_.Get1stKartIndexInRacing();
		if (KartManager.Instance.IsValidKart(num))
		{
			if (num == this.kartIndex_)
			{
				KartManager.Instance.goPlayKart_.CompensationDragFactor = 1f;
			}
			else
			{
				float passPlaneTrackDistanceByKartIndex = KartManager.Instance.goCourse_.GetPassPlaneTrackDistanceByKartIndex(num);
				float passPlaneTrackDistanceByKartIndex2 = KartManager.Instance.goCourse_.GetPassPlaneTrackDistanceByKartIndex(this.kartIndex_);
				float num2 = passPlaneTrackDistanceByKartIndex - passPlaneTrackDistanceByKartIndex2;
				if (num2 < 0f)
				{
					num2 = 0f;
				}
				KartManager.Instance.goPlayKart_.CompensationDragFactor = 1f - Mathf.Min(0.1f, num2 / 500f);
			}
		}
	}

	private const float MAX_VELOCITY_CHANGE = 100f;

	private bool grounded;

	private RigidbodyFPSWalker.PrevState prevState_ = new RigidbodyFPSWalker.PrevState();

	public GoPlayKart goPlayKart_;

	private Skidmarks skidmark;

	private float[] skidmarkTime;

	private bool isBooster_;

	private bool usedNormalBooster_;

	private CharacterAnimationManager animationManager_;

	private bool isVisible_ = true;

	private Renderer[] renderers_;

	public PlayerJobManager jobManager_ = new PlayerJobManager();

	protected MonoBehaviourMessage1Param<SoundController.FxType> toSoundController_;

	private BoxCollider thisBoxCollider_;

	private BoxCollider childBoxCollider_;

	private Vector3 _targetUpVector = Vector3.up;

	private float SMALL_CRASH_VELOCITY = 15f;

	private float BIG_CRASH_VELOCITY = 30f;

	private CharacterAnimation[] HI_PRIORITY_ANIMATION = new CharacterAnimation[]
	{
		CharacterAnimation.SMALL_ACCIDENT,
		CharacterAnimation.BIG_ACCIDENT,
		CharacterAnimation.WIN_GAME,
		CharacterAnimation.LOSE_GAME
	};

	private float returnToFrontTime;

	private int[] lastSkidMark = new int[4];

	private float wheelRotation;

	private bool isSuddenChange;

	private int fixedUpdateCount_;

	private TickEvent generateItemEvent_ = new TickEvent(150);

	public class PrevState
	{
		public Vector3 position_ = Vector3.zero;

		public Vector3 force_ = Vector3.zero;

		public Vector3 velocity_ = Vector3.zero;

		public Vector3 angular_ = Vector3.zero;

		public Vector3 forward_ = Vector3.zero;

		public Quaternion rotate_;

		public bool isGrounded_;
	}
}
