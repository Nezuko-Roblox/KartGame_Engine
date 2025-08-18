using System;
using UnityEngine;

public class GoItemWaterMissile : ItemBasicController
{
	public override void Initialize(ItemParam param)
	{
		ItemWaterMissileParam itemWaterMissileParam = (ItemWaterMissileParam)param;
		if (itemWaterMissileParam != null)
		{
			base.Initialize(param);
			if (GUIBase.GetGUIType() == GUIType.IPHONE)
			{
				this.MISSILE_START_POSITION = this.MISSILE_START_POSITION_IPHONE;
				this.MISSILE_END_POSITION = this.MISSILE_END_POSITION_IPHONE;
			}
			else
			{
				this.MISSILE_START_POSITION = this.MISSILE_START_POSITION_IPAD;
				this.MISSILE_END_POSITION = this.MISSILE_END_POSITION_IPAD;
			}
			this.srcKartIndex_ = itemWaterMissileParam.srcKartIndex_;
			this.targetKartIndex_ = itemWaterMissileParam.dstKartIndex_;
			this.srcKartTrans_ = KartManager.Instance.goKart_[this.srcKartIndex_].m_kart.transform;
			if (KartManager.Instance.IsValidKart(this.targetKartIndex_))
			{
				this.dstKartTrans_ = KartManager.Instance.goKart_[this.targetKartIndex_].m_kart.transform;
			}
			for (int i = 0; i < 6; i++)
			{
				this.SetEnableItemObject((GoItemWaterMissile.ItemObject)i, false);
			}
			base.transform.position = this.srcKartTrans_.position;
			base.transform.localRotation = this.srcKartTrans_.rotation;
			this.SetEnableItemObject(GoItemWaterMissile.ItemObject.FIRE, true);
			base.PlayFx(0, this.srcKartIndex_ == KartManager.PLAYER_KART_IDX);
			this.state_ = GoItemWaterMissile.State.FIRE;
		}
		else
		{
			this.state_ = GoItemWaterMissile.State.DESTROY;
		}
	}

	protected override void ObjectSetting()
	{
		string[] array = new string[] { "fire", "approach_1", "approach_2", "approach_3", "explore", "explore_bubble" };
		foreach (object obj in base.transform)
		{
			Transform transform = (Transform)obj;
			int i;
			for (i = 0; i < 6; i++)
			{
				if (array[i] == transform.name)
				{
					this.itemObjects_[i] = transform.gameObject;
					break;
				}
			}
			if (i >= 6)
			{
				Debug.LogError("Invalid gameobject name in ITEM " + FiaUtil.AddSquareBracket(transform.name));
			}
		}
		this.applyItemParam_ = new ApplyItemParam(0);
		this.toPlayer_ = (MonoBehaviourMessage2Param<GameItem, ApplyItemParam>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.APPLY_ITEM);
	}

	private void SetEnableItemObject(GoItemWaterMissile.ItemObject obj, bool isEnable)
	{
		this.itemObjects_[(int)obj].SetActiveRecursively(isEnable);
		if (isEnable && this.itemObjects_[(int)obj].animation != null)
		{
			this.itemObjects_[(int)obj].animation.Play();
		}
	}

	private void FixedUpdate()
	{
		if (this.state_ == GoItemWaterMissile.State.FIRE && this.srcKartTrans_ != null)
		{
			base.transform.position = this.srcKartTrans_.position;
			base.transform.localRotation = this.srcKartTrans_.rotation;
		}
		else if (this.state_ == GoItemWaterMissile.State.APPROACH)
		{
			base.transform.position = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
			for (int i = 0; i < 3; i++)
			{
				GameObject gameObject = this.itemObjects_[1 + i];
				ApproachObject.ApproachState approachState = this.approachObjects_[i].GetApproachState();
				if (approachState == ApproachObject.ApproachState.ACTIVE)
				{
					this.missilePath_[i].Update(Time.deltaTime);
					Vector3 path = this.missilePath_[i].GetPath();
					path.z += CameraManager.Instance.guiCam_.nearClipPlane + 1f;
					gameObject.transform.position = CameraManager.Instance.guiCam_.ScreenToWorldPoint(path);
				}
			}
		}
	}

	private void UpdateTouch()
	{
		if (KartManager.Instance.isPaused_)
		{
			return;
		}
		float num = ((GUIBase.GetGUIType() != GUIType.IPAD) ? 3600f : 8100f);
		if (Input.touchCount > 0)
		{
			foreach (Touch touch in Input.touches)
			{
				if (touch.phase == TouchPhase.Began)
				{
					for (int j = 0; j < 3; j++)
					{
						if (this.approachObjects_[j].GetApproachState() == ApproachObject.ApproachState.ACTIVE)
						{
							Vector2 vector = this.missilePath_[j].GetPath();
							if ((vector - touch.position).sqrMagnitude < num)
							{
								this.approachObjects_[j].SetApproachState(ApproachObject.ApproachState.EXPLORE);
								base.PlayFx(2, true);
							}
						}
					}
					break;
				}
			}
		}
	}

	private void ApplyToAIKart()
	{
		this.applyItemParam_.senderId_ = this.srcKartIndex_;
		this.toPlayer_.Initialize(GameItem.WATER_MISSILE, this.applyItemParam_);
		MonoBehaviourExCenter.Instance.SendMessage(0, 24 + this.targetKartIndex_, this.toPlayer_);
		this.state_ = GoItemWaterMissile.State.DESTROY;
	}

	private void Update()
	{
		switch (this.state_)
		{
		case GoItemWaterMissile.State.FIRE:
			if (!this.itemObjects_[0].animation.isPlaying)
			{
				this.SetEnableItemObject(GoItemWaterMissile.ItemObject.FIRE, false);
				if (KartManager.Instance.IsValidKart(this.targetKartIndex_) && !KartManager.Instance.goCourse_.IsKartGoalIn(this.targetKartIndex_))
				{
					if (this.targetKartIndex_ == KartManager.PLAYER_KART_IDX)
					{
						this.state_ = GoItemWaterMissile.State.APPROACH;
						this.missilePath_ = new WaterMissilePath[3];
						this.approachObjects_ = new ApproachObject[3];
						for (int i = 0; i < 3; i++)
						{
							GameObject gameObject = this.itemObjects_[1 + i];
							this.approachObjects_[i] = new ApproachObject(gameObject);
							this.approachObjects_[i].SetApproachState(ApproachObject.ApproachState.ACTIVE);
							int num = global::UnityEngine.Random.Range(3 * i, 3 * (i + 1));
							int num2 = i;
							Vector3 vector = new Vector3(this.MISSILE_START_POSITION[num][0], (float)Screen.height - this.MISSILE_START_POSITION[num][1], 0f);
							Vector3 vector2 = new Vector3(this.MISSILE_END_POSITION[num2][0], (float)Screen.height - this.MISSILE_END_POSITION[num2][1], 0f);
							this.missilePath_[i] = new WaterMissilePath(vector, vector2, 2f);
							vector.z += CameraManager.Instance.guiCam_.nearClipPlane + 1f;
							gameObject.transform.position = CameraManager.Instance.guiCam_.ScreenToWorldPoint(vector);
						}
						base.PlayFx(1, true);
					}
					else
					{
						base.Invoke("ApplyToAIKart", global::UnityEngine.Random.Range(1f, 2f));
						this.state_ = GoItemWaterMissile.State.WAIT;
					}
				}
				else
				{
					this.state_ = GoItemWaterMissile.State.DESTROY;
				}
			}
			break;
		case GoItemWaterMissile.State.APPROACH:
		{
			this.UpdateTouch();
			bool flag = true;
			for (int j = 0; j < 3; j++)
			{
				ApproachObject.ApproachState approachState = this.approachObjects_[j].GetApproachState();
				if (approachState == ApproachObject.ApproachState.EXPLORED)
				{
					this.approachObjects_[j].SetApproachState(ApproachObject.ApproachState.DESTROYED);
				}
				if (approachState != ApproachObject.ApproachState.DESTROYED)
				{
					flag = false;
				}
			}
			if (flag)
			{
				this.toPlayer_.Initialize(GameItem.SHIELD, null);
				MonoBehaviourExCenter.Instance.SendMessage(0, 24 + this.targetKartIndex_, this.toPlayer_);
				this.state_ = GoItemWaterMissile.State.DESTROY;
			}
			else
			{
				for (int k = 0; k < 3; k++)
				{
					if (this.itemObjects_[1 + k].active && this.missilePath_[k].IsFinish())
					{
						for (int l = 0; l < 3; l++)
						{
							this.SetEnableItemObject(GoItemWaterMissile.ItemObject.APPROACH_1 + l, false);
						}
						this.SetEnableItemObject(GoItemWaterMissile.ItemObject.EXPLORE, true);
						this.SetEnableItemObject(GoItemWaterMissile.ItemObject.EXPLORE_BUBBLE, true);
						this.applyItemParam_.senderId_ = this.srcKartIndex_;
						this.toPlayer_.Initialize(GameItem.WATER_MISSILE, this.applyItemParam_);
						MonoBehaviourExCenter.Instance.SendMessage(0, 24 + this.targetKartIndex_, this.toPlayer_);
						base.transform.position = this.dstKartTrans_.position;
						this.state_ = GoItemWaterMissile.State.EXPLORE;
						base.PlayFx(3, this.srcKartIndex_ == KartManager.PLAYER_KART_IDX);
						break;
					}
				}
			}
			break;
		}
		case GoItemWaterMissile.State.EXPLORE:
			if (!this.itemObjects_[4].animation.isPlaying)
			{
				this.SetEnableItemObject(GoItemWaterMissile.ItemObject.EXPLORE, false);
				this.SetEnableItemObject(GoItemWaterMissile.ItemObject.EXPLORE_BUBBLE, false);
				this.state_ = GoItemWaterMissile.State.DESTROY;
			}
			break;
		case GoItemWaterMissile.State.DESTROY:
			base.gameObject.SetActiveRecursively(false);
			break;
		}
	}

	private const int MISSILE_SIZE = 3;

	private GoItemWaterMissile.State state_;

	private GameObject[] itemObjects_ = new GameObject[6];

	private int srcKartIndex_;

	private int targetKartIndex_;

	private Transform srcKartTrans_;

	private Transform dstKartTrans_;

	private WaterMissilePath[] missilePath_;

	private ApproachObject[] approachObjects_;

	private float[][] MISSILE_START_POSITION_IPHONE = new float[][]
	{
		new float[] { 585f, -45f },
		new float[] { 645f, -45f },
		new float[] { 705f, -45f },
		new float[] { 50f, -45f },
		new float[] { 110f, -45f },
		new float[] { 170f, -45f },
		new float[] { 830f, 132f },
		new float[] { 830f, 156f },
		new float[] { 830f, 180f }
	};

	private float[][] MISSILE_END_POSITION_IPHONE = new float[][]
	{
		new float[] { 440f, 140f },
		new float[] { 240f, 200f },
		new float[] { 600f, 170f }
	};

	private float[][] MISSILE_START_POSITION_IPAD = new float[][]
	{
		new float[] { 300f, -40f },
		new float[] { 501f, -40f },
		new float[] { 651f, -40f },
		new float[] { -40f, 323f },
		new float[] { -40f, 111f },
		new float[] { -40f, 227f },
		new float[] { 1064f, 127f },
		new float[] { 1064f, 265f },
		new float[] { 1064f, 401f }
	};

	private float[][] MISSILE_END_POSITION_IPAD = new float[][]
	{
		new float[] { 601f, 206f },
		new float[] { 413f, 233f },
		new float[] { 637f, 260f }
	};

	private float[][] MISSILE_START_POSITION;

	private float[][] MISSILE_END_POSITION;

	private ApplyItemParam applyItemParam_;

	private MonoBehaviourMessage2Param<GameItem, ApplyItemParam> toPlayer_;

	private enum State
	{
		NONE,
		FIRE,
		APPROACH,
		EXPLORE,
		DESTROY,
		WAIT
	}

	private enum ItemObject
	{
		FIRE,
		APPROACH_1,
		APPROACH_2,
		APPROACH_3,
		EXPLORE,
		EXPLORE_BUBBLE,
		SIZE
	}

	private enum MissilePosition
	{
		SRC_X,
		SRC_Y,
		DST_X,
		DST_Y,
		DURATION
	}

	private enum FxType
	{
		FIRE,
		APPROACH,
		POP,
		TRAPPED
	}
}
