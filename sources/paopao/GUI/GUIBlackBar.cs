using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIBlackBar : MonoBehaviourEx
{
	private void Awake()
	{
		this.RegistMonoBehaviour(14);
	}

	private void Start()
	{
		this.guiManager_ = new GUIPanelManager();
		this.guiManager_.SetCamera(CameraManager.Instance.guiCam_);
		float[][] array = new float[][]
		{
			new float[]
			{
				0f,
				0f,
				(float)Screen.width,
				75f
			},
			new float[]
			{
				0f,
				(float)(Screen.height - 75),
				(float)Screen.width,
				(float)Screen.height
			}
		};
		for (int i = 0; i < 2; i++)
		{
			array[i][1] = (float)Screen.height - array[i][1];
			array[i][3] = (float)Screen.height - array[i][3];
			this.blackBarSet_[i] = new GUIBlackBarPanel(array[i]);
			this.guiManager_.RegistGUIInterface(this.blackBarSet_[i]);
		}
		base.gameObject.layer = LayerMask.NameToLayer("Gui");
		this.material_ = new Material(GUIBlackBar.blackBarShader_);
		MeshRenderer component = base.GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.material = this.material_;
		}
		this.mesh_ = base.GetComponent<MeshFilter>().mesh;
		base.gameObject.SetActiveRecursively(false);
	}

	private void Update()
	{
		if (this.blackBarMode_ != GUIBlackBar.BlackBarMode.SETTLED)
		{
			float deltaTime = Time.deltaTime;
			if (this.blackBarMode_ == GUIBlackBar.BlackBarMode.SHOWING)
			{
				this.blackBarSet_[0].value_ += deltaTime * this.blackBarSet_[0].speed_;
				this.blackBarSet_[1].value_ += deltaTime * this.blackBarSet_[1].speed_;
				if (this.blackBarSet_[0].value_ >= this.blackBarSet_[0].max_)
				{
					this.blackBarSet_[0].value_ = this.blackBarSet_[0].max_;
					this.blackBarSet_[1].value_ = this.blackBarSet_[1].max_;
					this.blackBarMode_ = GUIBlackBar.BlackBarMode.SETTLED;
				}
			}
			else
			{
				this.blackBarSet_[0].value_ -= deltaTime * this.blackBarSet_[0].speed_;
				this.blackBarSet_[1].value_ -= deltaTime * this.blackBarSet_[1].speed_;
				if (this.blackBarSet_[0].value_ <= 0f)
				{
					if (this.blackBarMode_ != GUIBlackBar.BlackBarMode.HIDING_DONT_SHOW_UI)
					{
						MonoBehaviourMessage1Param<bool> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_UI);
						monoBehaviourMessage1Param.Initialize(true);
						base.BroadcastMessage(monoBehaviourMessage1Param);
					}
					this.blackBarSet_[0].value_ = 0f;
					this.blackBarSet_[1].value_ = 0f;
					this.blackBarMode_ = GUIBlackBar.BlackBarMode.SETTLED;
					base.gameObject.SetActiveRecursively(false);
				}
			}
			this.blackBarSet_[0].SetRectByWindowSpace(0f, 0f, (float)Screen.width, this.blackBarSet_[0].value_);
			this.blackBarSet_[1].SetRectByWindowSpace(0f, (float)Screen.height - this.blackBarSet_[1].value_, (float)Screen.width, (float)Screen.height);
		}
		if (this.guiManager_.Update())
		{
			this.guiManager_.UpdateMesh(ref this.mesh_);
		}
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.BLACK_BAR)
		{
			BlackBarMessage blackBarMessage = (BlackBarMessage)msg;
			if (blackBarMessage != null)
			{
				if (blackBarMessage.isShow_)
				{
					base.gameObject.SetActiveRecursively(true);
					if (!blackBarMessage.isSmooth_)
					{
						this.blackBarSet_[0].SetRectByWindowSpace(0f, 0f, (float)Screen.width, this.blackBarSet_[0].max_);
						this.blackBarSet_[1].SetRectByWindowSpace(0f, (float)Screen.height - this.blackBarSet_[1].max_, (float)Screen.width, (float)Screen.height);
						this.blackBarMode_ = GUIBlackBar.BlackBarMode.SETTLED;
					}
					else
					{
						this.blackBarSet_[0].SetRectByWindowSpace(0f, 0f, (float)Screen.width, 0f);
						this.blackBarSet_[1].SetRectByWindowSpace(0f, (float)Screen.height, (float)Screen.width, (float)Screen.height);
						this.blackBarMode_ = GUIBlackBar.BlackBarMode.SHOWING;
					}
				}
				else if (!blackBarMessage.isSmooth_)
				{
					base.gameObject.SetActiveRecursively(false);
					this.blackBarMode_ = GUIBlackBar.BlackBarMode.SETTLED;
					if (!blackBarMessage.isShowOnlyBlackBar_)
					{
						base.BroadcastMessage(((MonoBehaviourMessage1Param<bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_UI)).Initialize(true));
					}
				}
				else
				{
					base.gameObject.SetActiveRecursively(true);
					float num = (float)Screen.width;
					float num2 = (float)Screen.height;
					this.blackBarSet_[0].SetRectByWindowSpace(0f, 0f, num, num2 * 0.09375f + 0.5f);
					this.blackBarSet_[1].SetRectByWindowSpace(0f, num2 - num2 * 0.09375f + 0.5f, num, num2);
					this.blackBarMode_ = ((!blackBarMessage.isShowOnlyBlackBar_) ? GUIBlackBar.BlackBarMode.HIDING : GUIBlackBar.BlackBarMode.HIDING_DONT_SHOW_UI);
				}
			}
		}
	}

	private GUIPanelManager guiManager_;

	private GUIBlackBarPanel[] blackBarSet_ = new GUIBlackBarPanel[2];

	private static string blackBarShader_ = "Shader \"BlackBarShader\" {\r\n\tProperties {\r\n\t}\r\n\tSubShader {\r\n        ZWrite Off\r\n\t\tAlphatest Greater 0\r\n        Tags {Queue=Transparent}\r\n\t\tBlend SrcAlpha OneMinusSrcAlpha\r\n\t\tPass {\r\n            BindChannels {\r\n               Bind \"Vertex\", vertex\r\n               Bind \"Color\", color\r\n            }\r\n            Lighting Off\r\n\t\t}\r\n\t}\r\n\tFallback off\r\n}";

	private Material material_;

	private GUIBlackBar.BlackBarMode blackBarMode_;

	private Mesh mesh_;

	private enum BlackBarMode
	{
		SETTLED,
		SHOWING,
		HIDING,
		HIDING_DONT_SHOW_UI
	}
}
