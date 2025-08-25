using System;
using UnityEngine;

public class KartBasicController : MonoBehaviourEx
{
	private void Awake()
	{
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	protected virtual void FixedUpdate()
	{
		if (this.flipEffect_ != null && this.flipEffect_.active)
		{
			this.flipEffect_.transform.localPosition = this.kartBody_.transform.localPosition;
		}
		if (this.devilStartEffect_ != null && this.devilStartEffect_.active)
		{
			Vector3 localPosition = this.kartBody_.transform.localPosition;
			Vector3 vector = new Vector3(localPosition.x, localPosition.y + 3f, localPosition.z);
			this.devilStartEffect_.transform.localPosition = vector;
		}
		if (this.devilPlayEffect_ != null && this.devilPlayEffect_.active)
		{
			Vector3 localPosition2 = this.kartBody_.transform.localPosition;
			Vector3 vector2 = new Vector3(localPosition2.x, localPosition2.y + 3f, localPosition2.z);
			this.devilPlayEffect_.transform.localPosition = vector2;
		}
	}

	public GameObject KartBodyObject
	{
		get
		{
			return this.kartBody_;
		}
	}

	public Transform KartBodyTransform
	{
		get
		{
			return this.kartBody_.transform;
		}
	}

	public bool InViewFrustum
	{
		get
		{
			return !(this.bodyRenderer_ != null) || this.bodyRenderer_.isVisible;
		}
	}

	public void Initialize(byte kartBodyIdx, byte characterIdx, bool isNoAniCharacter)
	{
		GameObject gameObject = FiaUtil.GenerateKart(kartBodyIdx);
		string mainAsset = KartAssetDefinitionManager.Instance.GetMainAsset((int)kartBodyIdx);
		MeshRenderer[] components = gameObject.GetComponents<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in components)
		{
			if (meshRenderer.name == mainAsset)
			{
				this.bodyRenderer_ = meshRenderer;
			}
		}
		this.isNoAnimationCharacter_ = isNoAniCharacter;
		if (isNoAniCharacter)
		{
			this.character_ = FiaUtil.GenerateNoAniCharacter(characterIdx);
			this.characterFace_ = null;
		}
		else
		{
			this.character_ = FiaUtil.GenerateCharacter(characterIdx);
			this.characterFace_ = FiaUtil.GetChildGameObject(this.character_, "face");
			this.character_.animation.Play("idle");
		}
		Transform child = base.transform.GetChild(0);
		GameObject gameObject2 = child.gameObject;
		Transform transform = gameObject.transform;
		Transform transform2 = this.character_.transform;
		FiaUtil.AttachKartNCharacter(ref child, ref transform, ref transform2);
		global::UnityEngine.Object.DestroyImmediate(gameObject);
		this.kartBody_ = base.transform.GetChild(0).gameObject;
		if (this.shockWavePrefab != null)
		{
			GameObject gameObject3 = (GameObject)global::UnityEngine.Object.Instantiate(this.shockWavePrefab);
			FiaUtil.AttachChild(ref gameObject2, ref gameObject3);
		}
		if (this.crashEffectPrefab != null)
		{
			GameObject gameObject4 = (GameObject)global::UnityEngine.Object.Instantiate(this.crashEffectPrefab);
			FiaUtil.AttachChild(ref gameObject2, ref gameObject4);
		}
		if (this.boosterWavePrefab != null && this.boosterWave_ == null)
		{
			this.boosterWave_ = (GameObject)global::UnityEngine.Object.Instantiate(this.boosterWavePrefab);
			FiaUtil.AttachChild(ref gameObject2, ref this.boosterWave_);
			this.boosterWave_.SetActiveRecursively(false);
		}
		gameObject2 = base.gameObject;
		if (this.waterBombBubblePrefab_ != null)
		{
			this.waterBombBubble_ = (GameObject)global::UnityEngine.Object.Instantiate(this.waterBombBubblePrefab_);
			FiaUtil.AttachChild(ref gameObject2, ref this.waterBombBubble_);
			this.waterBombBubble_.SetActiveRecursively(false);
		}
		if (this.waterFlyBubblePrefab_ != null)
		{
			this.waterFlyBubble_ = (GameObject)global::UnityEngine.Object.Instantiate(this.waterFlyBubblePrefab_);
			FiaUtil.AttachChild(ref gameObject2, ref this.waterFlyBubble_);
			this.waterFlyBubble_.SetActiveRecursively(false);
		}
		if (this.shieldPrefab_ != null)
		{
			this.shield_ = (GameObject)global::UnityEngine.Object.Instantiate(this.shieldPrefab_);
			FiaUtil.AttachChild(ref gameObject2, ref this.shield_);
			this.shield_.SetActiveRecursively(false);
		}
		if (this.flipEffectPrefab_ != null)
		{
			this.flipEffect_ = (GameObject)global::UnityEngine.Object.Instantiate(this.flipEffectPrefab_);
			this.flipRenderer_ = this.flipEffect_.GetComponentsInChildren<Renderer>();
			FiaUtil.AttachChild(ref gameObject2, ref this.flipEffect_);
			this.flipEffect_.SetActiveRecursively(false);
		}
		if (this.devilStartEffectPrefab_ != null)
		{
			this.devilStartEffect_ = (GameObject)global::UnityEngine.Object.Instantiate(this.devilStartEffectPrefab_);
			this.devilStartRenderer_ = this.devilStartEffect_.GetComponentsInChildren<Renderer>();
			FiaUtil.AttachChild(ref gameObject2, ref this.devilStartEffect_);
			this.devilStartEffect_.SetActiveRecursively(false);
		}
		if (this.devilPlayEffectPrefab_ != null)
		{
			this.devilPlayEffect_ = (GameObject)global::UnityEngine.Object.Instantiate(this.devilPlayEffectPrefab_);
			this.devilPlayRenderer_ = this.devilPlayEffect_.GetComponentsInChildren<Renderer>();
			FiaUtil.AttachChild(ref gameObject2, ref this.devilPlayEffect_);
			this.devilPlayEffect_.SetActiveRecursively(false);
		}
		if (this.guardPrefab_ != null)
		{
			this.guard_ = (GameObject)global::UnityEngine.Object.Instantiate(this.guardPrefab_);
			FiaUtil.AttachChild(ref gameObject2, ref this.guard_);
			this.guard_.SetActiveRecursively(false);
		}
		this.booster_ = new GameObject[2];
		for (int j = 0; j < 2; j++)
		{
			this.booster_[j] = null;
		}
		this.ObjectSetting();
	}

	public void ObjectSetting()
	{
		Transform transform = this.kartBody_.transform;
		foreach (object obj in transform)
		{
			Transform transform2 = (Transform)obj;
			if (transform2.name == "tire0")
			{
				this.wheels_[0] = transform2;
			}
			else if (transform2.name == "tire1")
			{
				this.wheels_[1] = transform2;
			}
			else if (transform2.name == "tire2")
			{
				this.wheels_[2] = transform2;
			}
			else if (transform2.name == "tire3")
			{
				this.wheels_[3] = transform2;
			}
			else if (transform2.name.Contains("port"))
			{
				int num = int.Parse(transform2.name.Substring(transform2.name.Length - 1));
				Transform transform3 = transform2.Find("booster");
				if (MathHelper.IsBetweenIE(num, 0, 2) && transform3 != null)
				{
					this.booster_[num] = transform3.gameObject;
					this.booster_[num].SetActiveRecursively(false);
				}
			}
		}
	}

	public void ResetDebugText()
	{
		if (this.debugText_ == null)
		{
			return;
		}
		this.debugText_.text = string.Empty;
	}

	public void AddDebugText(string t, bool isNewline)
	{
		if (this.debugText_ == null)
		{
			return;
		}
		if (isNewline)
		{
			GUIText guitext = this.debugText_;
			guitext.text += "\r\n";
		}
		GUIText guitext2 = this.debugText_;
		guitext2.text += t;
	}

	public virtual GoKart GetGoKart()
	{
		return null;
	}

	public virtual bool SetKartBodyAnimation(KartBodyAnimation anim)
	{
		return this.SetKartBodyAnimation(anim, false);
	}

	public virtual bool SetKartBodyAnimation(KartBodyAnimation anim, bool isForced)
	{
		GoKart goKart = this.GetGoKart();
		if (goKart != null)
		{
			if (goKart.KartBodyAnim >= anim && !isForced)
			{
				return false;
			}
		}
		goKart.KartBodyAnim = anim;
		if (anim == KartBodyAnimation.IDLE)
		{
			this.playMode_ = KartBasicController.PlayMode.NORMAL;
			this.kartBody_.animation.Stop();
			this.kartBody_.transform.localPosition = Vector3.zero;
			this.kartBody_.transform.localRotation = Quaternion.identity;
		}
		else
		{
			this.playMode_ = KartBasicController.PlayMode.ANIMATION_PLAYING;
			string text = KartDefine.KartAnimationString[(int)anim];
			this.kartBody_.animation.Play(text);
		}
		this.OnChangePlayMode();
		return true;
	}

	public virtual void OnChangePlayMode()
	{
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		GoKart goKart = this.GetGoKart();
		if (goKart == null)
		{
			return;
		}
		if (msg.type_ == MonoBehaviourMessageType.CHANGE_KART_ANIMATION)
		{
			MonoBehaviourMessage2Param<KartBodyAnimation, KartAnimationOption> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<KartBodyAnimation, KartAnimationOption>)msg;
			if (monoBehaviourMessage2Param != null)
			{
				this.SetKartBodyAnimation(monoBehaviourMessage2Param.lparam_);
				if (monoBehaviourMessage2Param.rparam_ == KartAnimationOption.FORCING)
				{
					this.kartBody_.transform.localPosition = Vector3.zero;
					this.kartBody_.transform.localRotation = Quaternion.identity;
					base.rigidbody.AddForce(-base.rigidbody.velocity, ForceMode.VelocityChange);
					goKart.Forcing = true;
				}
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.RACE_OVER)
		{
			base.CancelInvoke();
			this.RestoreFlip();
			this.RestoreDevilEffect();
			this.DisableShield();
			this.FinishGuard();
		}
	}

	public virtual void SetCharacterAnimation(CharacterAnimation anim, WrapMode wrapMode)
	{
	}

	public virtual bool UseItem(GameItem toUse)
	{
		if (toUse == GameItem.NONE)
		{
			return false;
		}
		this.SetCharacterAnimation(CharacterAnimation.ATTACK, WrapMode.Default);
		if (toUse == GameItem.BANANA)
		{
			ItemBananaParam itemBananaParam = new ItemBananaParam(this.kartIndex_, this.GetGoKart().m_kart.transform.position, global::UnityEngine.Random.value);
			base.SendMessage(1, ((MonoBehaviourMessage2Param<GameItem, ItemParam>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.ITEM)).Initialize(GameItem.BANANA, itemBananaParam));
		}
		else if (toUse == GameItem.UFO)
		{
			int num = KartManager.Instance.goCourse_.Get1stKartIndexInRacing();
			if (!KartManager.Instance.IsValidKart(num) || num == this.kartIndex_)
			{
				num = -1;
			}
			base.SendMessage(1, ((MonoBehaviourMessage2Param<GameItem, ItemParam>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.ITEM)).Initialize(GameItem.UFO, new ItemUFOParam(this.kartIndex_, num, true)));
		}
		else if (toUse == GameItem.WATER_MISSILE)
		{
			int rank = KartManager.Instance.goCourse_.GetRank(this.kartIndex_);
			int num2 = -1;
			if (rank > 0)
			{
				num2 = KartManager.Instance.goCourse_.GetKartIndexByRank(rank - 1);
				if (!KartManager.Instance.IsValidKart(num2) || KartManager.Instance.goCourse_.IsKartGoalIn(num2))
				{
					num2 = -1;
				}
			}
			base.SendMessage(1, ((MonoBehaviourMessage2Param<GameItem, ItemParam>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.ITEM)).Initialize(GameItem.WATER_MISSILE, new ItemWaterMissileParam(this.kartIndex_, num2)));
		}
		else if (toUse == GameItem.WATER_FLY)
		{
			int rank2 = KartManager.Instance.goCourse_.GetRank(this.kartIndex_);
			if (rank2 > 0)
			{
				int kartIndexByRank = KartManager.Instance.goCourse_.GetKartIndexByRank(rank2 - 1);
				if (KartManager.Instance.IsValidKart(kartIndexByRank) && !KartManager.Instance.goCourse_.IsKartGoalIn(kartIndexByRank))
				{
					base.SendMessage(1, ((MonoBehaviourMessage2Param<GameItem, ItemParam>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.ITEM)).Initialize(GameItem.WATER_FLY, new ItemWaterFlyParam(this.kartIndex_, kartIndexByRank)));
				}
			}
		}
		else if (toUse == GameItem.WATER_BOMB)
		{
			base.SendMessage(1, ((MonoBehaviourMessage2Param<GameItem, ItemParam>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.ITEM)).Initialize(GameItem.WATER_BOMB, new ItemWaterBombParam(this.kartIndex_)));
		}
		else if (toUse == GameItem.FLIP)
		{
			bool[] array = new bool[6];
			int rank3 = KartManager.Instance.goCourse_.GetRank(this.kartIndex_);
			for (int i = 0; i < 6; i++)
			{
				array[i] = KartManager.Instance.goCourse_.GetRank(i) < rank3;
			}
			base.SendMessage(1, ((MonoBehaviourMessage2Param<GameItem, ItemParam>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.ITEM)).Initialize(GameItem.FLIP, new ItemFlipParam(this.kartIndex_, array, true)));
		}
		else if (toUse == GameItem.DEVIL)
		{
			bool[] array2 = new bool[6];
			int rank4 = KartManager.Instance.goCourse_.GetRank(this.kartIndex_);
			for (int j = 0; j < 6; j++)
			{
				array2[j] = KartManager.Instance.goCourse_.GetRank(j) < rank4;
			}
			base.SendMessage(1, ((MonoBehaviourMessage2Param<GameItem, ItemParam>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.ITEM)).Initialize(GameItem.DEVIL, new ItemDevilParam(this.kartIndex_, array2, true)));
		}
		else if (toUse == GameItem.GUARD && !this.waterBombBubble_.active && !this.waterFlyBubble_.active && !this.guard_.active)
		{
			if (KartManager.Instance.parameter_.Stage == StageType.GAME_WIFI && KartManager.PLAYER_KART_IDX == this.kartIndex_)
			{
				NetworkManager.Inst.SendPacketToAll(new GuardEffectPacket(this.kartIndex_), SendDataMode.RELIABLE);
			}
			this.guard_.SetActiveRecursively(true);
			base.Invoke("FinishGuard", 3f);
		}
		if (this.kartIndex_ == KartManager.PLAYER_KART_IDX)
		{
			InGameStatistics.Instance.TotalItemUsage.IncreaseStat(toUse);
		}
		return true;
	}

	public virtual void ResetForRestarting()
	{
	}

	protected void SetEnableBooster(bool enable)
	{
		if (this.boosterWave_ != null)
		{
			this.boosterWave_.SetActiveRecursively(enable);
		}
		for (int i = 0; i < 2; i++)
		{
			if (this.booster_[i] != null)
			{
				this.booster_[i].SetActiveRecursively(enable);
			}
		}
	}

	protected virtual void DisableShield()
	{
		if (this.shield_ != null)
		{
			this.shield_.SetActiveRecursively(false);
		}
	}

	protected virtual void RestoreFlip()
	{
		if (this.flipEffect_ != null)
		{
			this.flipEffect_.SetActiveRecursively(false);
		}
	}

	protected virtual void RestoreDevilEffect()
	{
		if (this.devilStartEffect_ != null)
		{
			this.devilStartEffect_.SetActiveRecursively(false);
		}
		if (this.devilPlayEffect_ != null)
		{
			this.devilPlayEffect_.SetActiveRecursively(false);
		}
	}

	protected virtual void StartDevilEffect()
	{
		if (this.devilStartEffect_ != null)
		{
			this.devilStartEffect_.SetActiveRecursively(true);
			base.CancelInvoke("PlayDevilEffect");
			base.Invoke("PlayDevilEffect", 1f);
		}
	}

	protected virtual void PlayDevilEffect()
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

	protected virtual void FinishGuard()
	{
		this.guard_.SetActiveRecursively(false);
	}

	public const int MAX_BOOSTER = 2;

	protected GameObject boosterWave_;

	protected GameObject[] booster_;

	protected GameObject character_;

	protected GameObject characterFace_;

	protected GameObject kartBody_;

	protected GameObject waterBombBubble_;

	protected GameObject waterFlyBubble_;

	protected GameObject shield_;

	protected GameObject guard_;

	protected GameObject flipEffect_;

	protected GameObject devilStartEffect_;

	protected GameObject devilPlayEffect_;

	protected bool isNoAnimationCharacter_;

	public GameObject shockWavePrefab;

	public GameObject crashEffectPrefab;

	public GameObject boosterWavePrefab;

	public GameObject waterBombBubblePrefab_;

	public GameObject waterFlyBubblePrefab_;

	public GameObject shieldPrefab_;

	public GameObject guardPrefab_;

	public GameObject flipEffectPrefab_;

	public GameObject devilStartEffectPrefab_;

	public GameObject devilPlayEffectPrefab_;

	protected Transform[] wheels_ = new Transform[4];

	public int kartIndex_;

	protected GUIText debugText_;

	protected Renderer bodyRenderer_;

	protected Renderer[] flipRenderer_;

	protected Renderer[] devilStartRenderer_;

	protected Renderer[] devilPlayRenderer_;

	protected KartBasicController.PlayMode playMode_;

	public enum PlayMode
	{
		NORMAL,
		ANIMATION_PLAYING
	}
}
