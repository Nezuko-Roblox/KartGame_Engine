using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUINewRecord : MonoBehaviourEx
{
	private void Awake()
	{
		this.RegistMonoBehaviour(18);
		this.panelManager_ = new GUIPanelManager();
		this.panelInfo_ = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? this.PANELINFO_FOR_IPHONE : this.PANELINFO_FOR_IPHONE);
	}

	private void Start()
	{
		this.meshRenderer_ = base.GetComponent<MeshRenderer>();
		this.meshRenderer_.castShadows = false;
		this.meshRenderer_.receiveShadows = false;
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		this.panelManager_.SetCamera(CameraManager.Instance.guiCam_);
		this.back_ = new GUIPanelEx3PartHorz(0, this.panelInfo_[0], fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP, 3f);
		this.panelManager_.RegistGUIInterface(this.back_);
		float[] array = new float[8];
		array[0] = this.panelInfo_[1][0];
		array[1] = this.panelInfo_[1][1];
		array[2] = array[0] + this.panelInfo_[1][4] - this.panelInfo_[1][2];
		array[3] = array[1] + this.panelInfo_[1][5] - this.panelInfo_[1][3];
		array[4] = this.panelInfo_[1][2];
		array[5] = this.panelInfo_[1][3];
		array[6] = this.panelInfo_[1][4];
		array[7] = this.panelInfo_[1][5];
		array[0] = array[0] * (float)Screen.width / 800f;
		array[1] = array[1] * (float)Screen.height / 480f;
		array[2] = array[2] * (float)Screen.width / 800f;
		array[3] = array[3] * (float)Screen.height / 480f;
		this.newRecord_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, array, fiaTexture, 2, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.newRecord_);
		this.newRecord_.Visible = false;
		array[0] = this.panelInfo_[2][0];
		array[1] = this.panelInfo_[2][1];
		array[2] = array[0] + this.panelInfo_[2][4] - this.panelInfo_[2][2];
		array[3] = array[1] + this.panelInfo_[2][5] - this.panelInfo_[2][3];
		array[4] = this.panelInfo_[2][2];
		array[5] = this.panelInfo_[2][3];
		array[6] = this.panelInfo_[2][4];
		array[7] = this.panelInfo_[2][5];
		array[0] = array[0] * (float)Screen.width / 800f;
		array[1] = array[1] * (float)Screen.height / 480f;
		array[2] = array[2] * (float)Screen.width / 800f;
		array[3] = array[3] * (float)Screen.height / 480f;
		this.newMonthlyRecord_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, array, fiaTexture, 2, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.newMonthlyRecord_);
		this.newMonthlyRecord_.Visible = false;
		array[0] = this.panelInfo_[3][0];
		array[1] = this.panelInfo_[3][1];
		array[2] = array[0] + this.panelInfo_[3][4] - this.panelInfo_[3][2];
		array[3] = array[1] + this.panelInfo_[3][5] - this.panelInfo_[3][3];
		array[4] = this.panelInfo_[3][2];
		array[5] = this.panelInfo_[3][3];
		array[6] = this.panelInfo_[3][4];
		array[7] = this.panelInfo_[3][5];
		array[0] = array[0] * (float)Screen.width / 800f;
		array[1] = array[1] * (float)Screen.height / 480f;
		array[2] = array[2] * (float)Screen.width / 800f;
		array[3] = array[3] * (float)Screen.height / 480f;
		this.loading_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, array, fiaTexture, 1, new Vector3(2f, 0f, 1f));
		this.panelManager_.RegistGUIInterface(this.loading_);
		this.timeInfo_ = new GUIPanelEx[7];
		int guitype = (int)GUIBase.GetGUIType();
		float[][] array2 = new float[][]
		{
			new float[] { 285f, 218f, 166f, 104f, 182f, 130f },
			new float[] { 239f, 161f, 214f, 116f, 228f, 138f }
		};
		float[] array3 = new float[array2[guitype].Length];
		float num = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? 14f : 18f);
		float num2 = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? 10f : 12f);
		float num3 = 0f;
		for (int i = 0; i < 7; i++)
		{
			Array.Copy(array2[guitype], array3, array2[guitype].Length);
			array3[0] += num3;
			num3 += ((i != 1 && i != 4) ? num : num2);
			float[] array4 = new float[8];
			array4[0] = array3[0];
			array4[1] = array3[1];
			array4[2] = array4[0] + array3[4] - array3[2];
			array4[3] = array4[1] + array3[5] - array3[3];
			array4[4] = array3[2];
			array4[5] = array3[3];
			array4[6] = array3[4];
			array4[7] = array3[5];
			array4[0] = array4[0] * (float)Screen.width / 800f;
			array4[1] = array4[1] * (float)Screen.height / 480f;
			array4[2] = array4[2] * (float)Screen.width / 800f;
			array4[3] = array4[3] * (float)Screen.height / 480f;
			this.timeInfo_[i] = GUIPanelFactory.Instance.CreateByWindowSpace(0, array4, fiaTexture, 1, new Vector3(2f, 0f, 1f));
			this.panelManager_.RegistGUIInterface(this.timeInfo_[i]);
		}
		this.mesh_ = base.GetComponent<MeshFilter>().mesh;
		if (this.panelManager_.Update())
		{
			this.panelManager_.UpdateMesh(ref this.mesh_);
		}
		base.gameObject.SetActiveRecursively(false);
	}

	private void Update()
	{
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.NEW_RECORD)
		{
			MonoBehaviourMessage2Param<float, bool> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<float, bool>)msg;
			if (monoBehaviourMessage2Param != null)
			{
				if (monoBehaviourMessage2Param.rparam_)
				{
					this.UpdateUVOfTime(monoBehaviourMessage2Param.lparam_, this.timeInfo_);
					this.newRecord_.Visible = KartManager.Instance.result_.IsBestRecord;
					this.newMonthlyRecord_.Visible = KartManager.Instance.result_.IsMonthlyBestRecord;
					base.gameObject.SetActiveRecursively(true);
					base.InvokeRepeating("LoadingAnimation", 0.1f, 0.1f);
				}
				else
				{
					base.Invoke("GoResult", 0.1f);
				}
			}
		}
		if (this.panelManager_.Update())
		{
			this.panelManager_.UpdateMesh(ref this.mesh_);
		}
	}

	private void GoResult()
	{
		base.CancelInvoke("LoadingAnimation");
		base.gameObject.SetActiveRecursively(false);
		if (this.newRecord_.Visible == this.newMonthlyRecord_.Visible)
		{
		}
		bool flag = false;
		if (KartManager.Instance.parameter_.Stage == StageType.GAME && KartManager.Instance.result_.IsUserWinner())
		{
			int num = KartOptions.Instance.GetWinCounter(KartManager.Instance.parameter_.track_)[(int)KartManager.Instance.parameter_.gameMode_];
			if (num <= 3)
			{
				MonoBehaviourMessage2Param<AssetType, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<AssetType, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_INFO);
				MonoBehaviourExCenter.Instance.SendMessage(0, 266, monoBehaviourMessage2Param.Initialize(AssetType.SIZE, num - 1));
				flag = true;
			}
		}
		if (!flag)
		{
			MonoBehaviourExCenter.Instance.SendMessage(0, 7, ((MonoBehaviourMessage2Param<bool, bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_RESULT)).Initialize(true, true));
		}
	}

	private void LoadingAnimation()
	{
		int uv = this.loading_.UV;
		this.loading_.UV = (uv + 1) % 4;
		if (this.panelManager_.Update())
		{
			this.panelManager_.UpdateMesh(ref this.mesh_);
		}
	}

	private void UpdateUVOfTime(float t, GUIPanelEx[] panels)
	{
		int num = (int)t;
		panels[0].UV = (int)((float)num / 60f % 10f);
		panels[1].UV = 10;
		panels[2].UV = num % 60 / 10;
		panels[3].UV = num % 10;
		panels[4].UV = 10;
		int num2 = (int)(t * 100f) % 100;
		panels[5].UV = num2 / 10;
		panels[6].UV = num2 % 10;
	}

	private MeshRenderer meshRenderer_;

	private GUIPanelManager panelManager_;

	private Mesh mesh_;

	private float[][] PANELINFO_FOR_IPHONE = new float[][]
	{
		new float[] { 209f, 159f, 597f, 280f, 266f, 295f, 373f },
		new float[] { 230f, 147f, 2f, 372f, 226f, 420f },
		new float[] { 230f, 147f, 2f, 422f, 226f, 470f },
		new float[] { 471f, 200f, 166f, 228f, 249f, 293f }
	};

	private float[][] panelInfo_;

	private GUIPanelEx3PartHorz back_;

	private GUIPanelEx newRecord_;

	private GUIPanelEx newMonthlyRecord_;

	private GUIPanelEx loading_;

	private GUIPanelEx[] timeInfo_;

	private enum TimeInfo
	{
		_1M,
		_COLON_1,
		_10S,
		_1S,
		_COLON_2,
		_100MS,
		_10MS,
		SIZE
	}
}
