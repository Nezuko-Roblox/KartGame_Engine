using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TestDriver : MonoBehaviour
{
	private void Awake()
	{
		base.rigidbody.freezeRotation = false;
		base.rigidbody.useGravity = false;
		for (int i = 0; i < 2; i++)
		{
			this.booster_[i] = null;
		}
		this.ObjectSetting();
		this.goPlayKart_ = new GoPlayKart();
		this.goPlayKart_.setReKartOld(base.gameObject, this.wheels_);
		KartManager.Instance.goPlayKart_ = this.goPlayKart_;
	}

	private void PrintQuaternion(ref Quaternion p)
	{
	}

	private void ObjectSetting()
	{
		foreach (object obj in base.transform)
		{
			Transform transform = (Transform)obj;
			if (transform.name == "tire0")
			{
				this.wheels_[0] = transform;
			}
			else if (transform.name == "tire1")
			{
				this.wheels_[1] = transform;
			}
			else if (transform.name == "tire2")
			{
				this.wheels_[2] = transform;
			}
			else if (transform.name == "tire3")
			{
				this.wheels_[3] = transform;
			}
			else if (transform.name == "booster_wave")
			{
				this.boosterWave_ = transform.gameObject;
				this.boosterWave_.SetActiveRecursively(false);
			}
			else if (transform.name.Contains("port"))
			{
				int num = int.Parse(transform.name.Substring(transform.name.Length - 1));
				Transform transform2 = transform.Find("booster");
				if (MathHelper.IsBetweenIE(num, 0, 2) && transform2 != null)
				{
					this.booster_[num] = transform2.gameObject;
					this.booster_[num].SetActiveRecursively(false);
				}
			}
			else if (transform.name == "seat")
			{
				Transform transform3 = transform.Find("dao");
				if (transform3 != null)
				{
					this.dao_ = transform3.gameObject;
				}
			}
		}
	}

	private void DumpInit()
	{
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
		base.rigidbody.mass = 1000f;
		base.rigidbody.centerOfMass = new Vector3(0f, 0f, 0f);
	}

	private void UpdateDaoAnimation()
	{
		if (this.dao_ == null)
		{
			return;
		}
		string text = "idle";
		WrapMode wrapMode = WrapMode.Default;
		GoPlayKart goPlayKart = this.goPlayKart_;
		if (this.dao_.animation.IsPlaying("small_accident") || this.dao_.animation.IsPlaying("big_accident"))
		{
			return;
		}
		if (goPlayKart.isCrash_ && goPlayKart.crashVelocity_ >= this.SMALL_CRASH_VELOCITY)
		{
			if (goPlayKart.crashVelocity_ >= this.BIG_CRASH_VELOCITY)
			{
				text = "big_accident";
			}
			else
			{
				text = "small_accident";
			}
			this.dao_.animation.wrapMode = wrapMode;
			this.dao_.animation.Play(text);
			return;
		}
		if (goPlayKart.isRealBoost())
		{
			text = "boost";
			wrapMode = WrapMode.Loop;
		}
		else if (goPlayKart.getRealBrake() != 0f)
		{
			if (goPlayKart.getRealSteer() == -1f)
			{
				text = "turn_back_right";
			}
			else
			{
				text = "turn_back_left";
			}
		}
		else if (goPlayKart.getRealSteer() != 0f)
		{
			wrapMode = WrapMode.Loop;
			if (goPlayKart.getRealSteer() == 1f)
			{
				text = "turn_right";
			}
			else if (goPlayKart.getRealSteer() == -1f)
			{
				text = "turn_left";
			}
		}
		if (this.daoAnimation != text)
		{
			this.dao_.animation.wrapMode = wrapMode;
			this.dao_.animation.CrossFade(text);
			this.daoAnimation = text;
		}
	}

	private void Update()
	{
		this.InputUpdate();
	}

	private void LateUpdate()
	{
	}

	private void InputUpdate2()
	{
		this.goPlayKart_.Wheel = Input.GetAxis("Horizontal");
		float axis = Input.GetAxis("Vertical");
		this.goPlayKart_.setAccel((double)axis == 1.0);
		this.goPlayKart_.setBrake((double)axis == -1.0);
	}

	private void InputUpdate()
	{
		this.inputManager.InputUpdate();
		this.goPlayKart_.setAccel(this.inputManager.GetKeyPushTime(InputType.UP) > 0f);
		this.goPlayKart_.setBrake(this.inputManager.GetKeyPushTime(InputType.DOWN) > 0f);
		float keyPushTime = this.inputManager.GetKeyPushTime(InputType.LEFT);
		float keyPushTime2 = this.inputManager.GetKeyPushTime(InputType.RIGHT);
		if (keyPushTime == 0f && keyPushTime2 == 0f)
		{
			this.goPlayKart_.Wheel = 0f;
		}
		else
		{
			this.goPlayKart_.Wheel = ((keyPushTime <= keyPushTime2) ? 1f : (-1f));
		}
		if (this.inputManager.GetKeyPushTime(InputType.DRIFT) > 0f && this.goPlayKart_.Wheel != 0f)
		{
			this.goPlayKart_.setDrift(true);
		}
		else
		{
			this.goPlayKart_.setDrift(false);
		}
		if (this.inputManager.GetKeyPushTime(InputType.ITEM) > 0f)
		{
			this.goPlayKart_.setBoost(1000, BoostKind.BoostNormal);
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
		this.wheelRotation = Mathf.Repeat(this.wheelRotation + base.rigidbody.velocity.magnitude * Time.deltaTime * 180f / 3.14159274f, 360f);
		Quaternion quaternion = Quaternion.Euler(this.wheelRotation, this.goPlayKart_.getSteerAngle() * 3f, 0f);
		Quaternion quaternion2 = Quaternion.Euler(this.wheelRotation, 0f, 0f);
		this.wheels_[1].localRotation = quaternion;
		this.wheels_[0].localRotation = quaternion;
		this.wheels_[3].localRotation = quaternion2;
		this.wheels_[2].localRotation = quaternion2;
	}

	private void ResetDebugText()
	{
		if (this.debugText_ == null)
		{
			return;
		}
		this.debugText_.text = string.Empty;
	}

	private void AddDebugText(string t, bool isNewline)
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

	private void FixedUpdate()
	{
		this.fixedUpdateCount_++;
		this.ResetDebugText();
		if (this.goPlayKart_.isRealBoost())
		{
			this.AddDebugText("boost", false);
		}
		if (this.goPlayKart_.m_isDrift)
		{
			this.AddDebugText("drift", false);
		}
		if (this.goPlayKart_.m_Contact)
		{
			string text = string.Empty;
			text += " contact(";
			for (int i = 0; i < 4; i++)
			{
				text += ((!this.goPlayKart_.m_sus.wheelContact[i]) ? "0" : "1");
			}
			text += ")";
			this.AddDebugText(text, false);
		}
		this.AddDebugText("csc : " + this.collisionStayCount_.ToString() + " ", false);
		this.AddDebugText(string.Concat(new string[]
		{
			"speed ",
			(base.rigidbody.velocity.magnitude * 3.6f).ToString(),
			" ",
			base.rigidbody.velocity.ToString(),
			" [ ",
			this.goPlayKart_.m_KartWLVel.magnitude.ToString(),
			" ] "
		}), true);
		this.goPlayKart_.basicAction(Time.time);
		this.UpdateWheels();
		this.UpdateWheelGraphic();
		if (this.isBooster_ != this.goPlayKart_.isRealBoost())
		{
			this.isBooster_ = this.goPlayKart_.isRealBoost();
			if (this.boosterWave_ != null)
			{
				this.boosterWave_.SetActiveRecursively(this.isBooster_);
			}
			for (int j = 0; j < this.booster_.Length; j++)
			{
				if (this.booster_[j] != null)
				{
					this.booster_[j].SetActiveRecursively(this.isBooster_);
				}
			}
		}
		if (this.grounded)
		{
			this.goPlayKart_.m_KartLAVel.x = 0f;
			this.goPlayKart_.m_KartLAVel.z = 0f;
			this.AddDebugText(string.Concat(new string[]
			{
				this.goPlayKart_.getSteerAngle().ToString(),
				" ",
				this.goPlayKart_.getRealSteer().ToString(),
				" ",
				Vector3Helper.ToStringVector3(this.goPlayKart_.m_KartLAVel)
			}), true);
			Quaternion localRotation = base.transform.localRotation;
			Quaternion quaternion = MathHelper.CreateQuaternion(0f, this.goPlayKart_.m_KartLAVel);
			Quaternion quaternion2 = localRotation * quaternion;
			MathHelper.QuaMulScala(ref quaternion2, 0.5f * Time.deltaTime);
			MathHelper.QuaAdd(ref localRotation, quaternion2);
			MathHelper.QuaNormalize(ref localRotation);
			base.transform.localRotation = localRotation;
			this.prevState_.rotate_ = localRotation;
			Vector3 velocity = base.rigidbody.velocity;
			Vector3 vector = this.goPlayKart_.m_KartWLVel - velocity;
			vector.x = Mathf.Clamp(vector.x, -this.maxVelocityChange, this.maxVelocityChange);
			vector.z = Mathf.Clamp(vector.z, -this.maxVelocityChange, this.maxVelocityChange);
			vector.y = 0f;
			base.rigidbody.AddForce(vector, ForceMode.VelocityChange);
			base.rigidbody.angularVelocity = Vector3.zero;
		}
		base.rigidbody.AddForce(new Vector3(0f, -this.gravity * base.rigidbody.mass, 0f));
		this.AddDebugText(" Distance : " + (base.rigidbody.position - this.prevState_.position_).ToString(), true);
		this.prevState_.position_ = base.rigidbody.position;
		this.prevState_.velocity_ = this.goPlayKart_.m_KartWLVel;
		this.prevState_.angular_ = base.rigidbody.angularVelocity;
		this.prevState_.forward_ = base.transform.forward;
		this.prevState_.isGrounded_ = this.grounded;
		this.grounded = false;
		if (this.isSuddenChange)
		{
			this.isSuddenChange = false;
			this.OutputDebugString(this.collisionStayCount_.ToString());
		}
		this.collisionStayCount_ = 0;
		this.goPlayKart_.ResetCrash();
		this.goPlayKart_.ResetShock();
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
		this.collisionStayCount_++;
		if ((base.rigidbody.velocity - this.prevState_.velocity_).magnitude >= 10f)
		{
			this.isSuddenChange = true;
			this.OutputDebugString(string.Concat(new string[]
			{
				collisionInfo.collider.name,
				" ",
				base.rigidbody.velocity.ToString(),
				this.prevState_.velocity_.ToString(),
				(base.rigidbody.velocity - this.prevState_.velocity_).ToString()
			}));
		}
		int num = 0;
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		foreach (ContactPoint contactPoint in collisionInfo.contacts)
		{
			vector2 += contactPoint.point;
			vector += contactPoint.normal;
			num++;
		}
		vector2 /= (float)num;
		vector /= (float)num;
		float num2 = Vector3.Dot(vector, this.prevState_.velocity_);
		if (num2 < 0f)
		{
			if ((double)vector.y >= 0.65)
			{
				this.grounded = true;
				if (!this.prevState_.isGrounded_)
				{
					this.goPlayKart_.SetShock(Mathf.Abs(num2));
				}
			}
			else
			{
				float num3 = Mathf.Abs(num2);
				this.goPlayKart_.SetCrash(num3);
				this.OutputDebugString(string.Concat(new string[]
				{
					"Crash Velocity : ",
					num3.ToString(),
					" [ ",
					this.prevState_.velocity_.magnitude.ToString(),
					" ] => [ ",
					base.rigidbody.velocity.magnitude.ToString(),
					" ] "
				}));
			}
		}
		else
		{
			this.grounded = true;
		}
		this.goPlayKart_.m_KartWLVel = base.rigidbody.velocity;
	}

	private const int MAX_BOOSTER = 2;

	public float speed = 10f;

	public float gravity = 10f;

	public float maxVelocityChange = 100f;

	public bool canJump = true;

	public float jumpHeight = 2f;

	private bool grounded;

	public float rotate = 1f;

	private TestDriver.PrevState prevState_ = new TestDriver.PrevState();

	private int collisionStayCount_;

	private GoPlayKart goPlayKart_;

	public GUIText debugText_;

	private InputManager inputManager = new InputManager();

	private Skidmarks skidmark;

	private float[] skidmarkTime;

	private Transform[] wheels_ = new Transform[4];

	public float suspensionDistance = 0.2f;

	private GameObject boosterWave_;

	private GameObject[] booster_ = new GameObject[2];

	private GameObject dao_;

	private bool isBooster_;

	private string daoAnimation = "idle";

	public TextAsset planeInfo_;

	private float SMALL_CRASH_VELOCITY = 15f;

	private float BIG_CRASH_VELOCITY = 30f;

	private int[] lastSkidMark = new int[4];

	private float wheelRotation;

	private bool isSuddenChange;

	private int fixedUpdateCount_;

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

	private enum CallOrder
	{
		FIXED_UPDATE,
		COLLISION_STAY,
		UPDATE
	}

	public enum KartWheelType
	{
		Front = 1,
		Back,
		All
	}

	public enum WheelType
	{
		FRONT_LEFT,
		FRONT_RIGHT,
		BACK_LEFT,
		BACK_RIGHT,
		WHEEL_TYPE_SIZE
	}
}
