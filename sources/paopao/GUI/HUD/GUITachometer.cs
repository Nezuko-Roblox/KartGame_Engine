using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUITachometer : MonoBehaviourEx
{
	public GUITachometer()
	{
		int[] array = new int[28];
		array[24] = 4;
		this.panelbuilder_ = array;
		base..ctor();
	}

	private void GUIPanelBuilderSetting()
	{
		GUIPanelFactory.Instance.RegistBuilder(4, new GUISpeedArrowBuilder());
	}

	private void Awake()
	{
		this.RegistMonoBehaviour(6);
		GUIPanelManager.ingame_ = new GUIPanelManager();
		this.GUIPanelBuilderSetting();
	}

	private void Start()
	{
		if (GUIPanelManager.ingame_ == null)
		{
			return;
		}
		if (this.textures_.Length != 28 || this.panelbuilder_.Length != 28 || this.textures_.Length != 28 || this.panelInfo_.Length != 28)
		{
		}
		GUIPanelManager.ingame_.SetCamera(CameraManager.Instance.guiCam_);
		GUIAtlas atlas = GUIAtlasManager.GetAtlas("ingame");
		this.meshRenderer_ = base.GetComponent<MeshRenderer>();
		this.meshRenderer_.castShadows = false;
		this.meshRenderer_.receiveShadows = false;
		this.meshRenderer_.material = atlas.material_;
		this.panels_ = new GUIPanelEx[28];
		for (int i = 0; i < 28; i++)
		{
			FiaTexture fiaTexture = new FiaTexture(atlas, this.textures_[i]);
			this.panels_[i] = GUIPanelFactory.Instance.CreateByWindowSpace(this.panelbuilder_[i], this.panelInfo_[i], fiaTexture, this.layers_[i]);
			GUIPanelManager.ingame_.RegistGUIInterface(this.panels_[i]);
		}
		this.guiSpeedArrow_ = (GUISpeedArrow)this.panels_[24];
	}

	private void Update()
	{
		if (GUIPanelManager.ingame_ == null)
		{
			return;
		}
		this.UpdateUVOfCharIdx(KartManager.Instance.goCourse_.MaxLap, 4);
		int num = Mathf.Clamp(KartManager.Instance.goCourse_.GetLap(KartManager.PLAYER_KART_IDX), 0, KartManager.Instance.goCourse_.MaxLap);
		this.UpdateUVOfCharIdx(num, 2);
		this.UpdateUVOfTime(KartManager.Instance.GetPlayTime(), 5);
		this.UpdateUVOfTime(KartManager.Instance.goCourse_.GetBestLapTime(), 13);
		this.UpdateUVOfSpeed(KartManager.Instance.goPlayKart_.GetKartSpeed(), 21);
		this.guiSpeedArrow_.Update(KartManager.Instance.goPlayKart_.GetKartSpeed());
		this.UpdateUVOfCharIdx(KartManager.Instance.goCourse_.GetMyRank(), 27);
		GUIPanelManager.ingame_.Update();
	}

	private void Dump(Vector3[] v, Vector2[] uv, int cnt)
	{
		string text = string.Empty;
		for (int i = 0; i < cnt; i++)
		{
			if (i % 4 == 0)
			{
				text = text + (i / 4).ToString() + "\n";
			}
			string text2 = text;
			text = string.Concat(new string[]
			{
				text2,
				Vector3Helper.ToStringVector3(v[i]),
				" ",
				GUIFontCalculator.ToStringVector2(uv[i]),
				"\n"
			});
			if (i % 4 == 3)
			{
				text += "\n";
			}
		}
	}

	private void UpdateUVOfCharIdx(int charIdx, int panelIdx)
	{
		this.panels_[panelIdx].UV = charIdx;
	}

	private void UpdateUVOfTime(float t, int idx)
	{
		int num = (int)t;
		this.UpdateUVOfCharIdx((int)((float)num / 60f), idx + 1);
		this.UpdateUVOfCharIdx(num % 60 / 10, idx + 3);
		this.UpdateUVOfCharIdx(num % 10, idx + 4);
		int num2 = (int)(t * 100f) % 100;
		this.UpdateUVOfCharIdx(num2 / 10, idx + 6);
		this.UpdateUVOfCharIdx(num2 % 10, idx + 7);
	}

	private void UpdateUVOfSpeed(float t, int idx)
	{
		this.UpdateUVOfCharIdx((int)(t / 100f), idx);
		this.UpdateUVOfCharIdx((int)(t / 10f) % 10, idx + 1);
		this.UpdateUVOfCharIdx((int)(t % 10f), idx + 2);
	}

	private void FixedUpdate()
	{
		Mesh mesh = base.GetComponent<MeshFilter>().mesh;
		mesh.Clear();
		if (GUIPanelManager.ingame_ == null)
		{
			return;
		}
		GUIPanelManager.ingame_.UpdateMesh(ref mesh);
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.SHOW_UI)
		{
			MonoBehaviourMessage1Param<bool> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<bool>)msg;
			if (monoBehaviourMessage1Param != null)
			{
				base.gameObject.SetActiveRecursively(monoBehaviourMessage1Param.param_);
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.PAUSE || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL2)
		{
			base.gameObject.SetActiveRecursively(false);
		}
		else if (msg.type_ == MonoBehaviourMessageType.RESUME)
		{
			base.gameObject.SetActiveRecursively(true);
		}
	}

	public override void OnUnloadStage()
	{
		GUIPanelManager.ingame_ = null;
		GUIAtlasManager.RemoveAtlas("ingame");
	}

	private MeshRenderer meshRenderer_;

	private float[][] panelInfo_ = new float[][]
	{
		new float[8],
		new float[] { 568f, 51f, 644f, 70f, 158f, 175f, 234f, 194f },
		new float[] { 648f, 49f, 667f, 73f, 0f, 88f, 19f, 112f },
		new float[] { 667f, 49f, 686f, 73f, 190f, 88f, 209f, 111f },
		new float[] { 686f, 49f, 705f, 73f, 0f, 88f, 19f, 112f },
		new float[] { 568f, 78f, 644f, 97f, 158f, 194f, 244f, 213f },
		new float[] { 648f, 76f, 667f, 100f, 0f, 88f, 19f, 112f },
		new float[] { 664f, 76f, 683f, 100f, 209f, 88f, 225f, 112f },
		new float[] { 686f, 76f, 705f, 100f, 0f, 88f, 19f, 112f },
		new float[] { 705f, 76f, 724f, 100f, 0f, 88f, 19f, 112f },
		new float[] { 721f, 76f, 740f, 100f, 209f, 88f, 225f, 112f },
		new float[] { 743f, 76f, 762f, 100f, 0f, 88f, 19f, 112f },
		new float[] { 762f, 76f, 781f, 100f, 0f, 88f, 19f, 112f },
		new float[] { 568f, 105f, 644f, 124f, 158f, 213f, 234f, 232f },
		new float[] { 648f, 103f, 667f, 127f, 0f, 88f, 19f, 112f },
		new float[] { 664f, 103f, 683f, 127f, 209f, 88f, 225f, 111f },
		new float[] { 686f, 103f, 705f, 127f, 0f, 88f, 19f, 112f },
		new float[] { 705f, 103f, 724f, 127f, 0f, 88f, 19f, 112f },
		new float[] { 721f, 103f, 740f, 127f, 209f, 88f, 225f, 111f },
		new float[] { 743f, 103f, 762f, 127f, 0f, 88f, 19f, 112f },
		new float[] { 762f, 103f, 781f, 127f, 0f, 88f, 19f, 112f },
		new float[] { 709f, 514f, 738f, 552f, 0f, 0f, 29f, 38f },
		new float[] { 736f, 514f, 765f, 552f, 0f, 0f, 29f, 38f },
		new float[] { 763f, 514f, 792f, 552f, 0f, 0f, 29f, 38f },
		new float[] { 580f, 498f, 652f, 514f, 0f, 176f, 72f, 192f },
		new float[] { 637f, 493f, 663f, 518f, 93f, 175f, 119f, 200f },
		new float[] { 677f, 534f, 716f, 550f, 119f, 175f, 158f, 191f },
		new float[] { 30f, 80f, 93f, 130f, 0f, 38f, 63f, 87f }
	};

	private string[] textures_ = new string[]
	{
		"aw_01@zz", "aw_01@zz", "aw_02@zz", "aw_02@zz", "aw_02@zz", "aw_01@zz", "aw_02@zz", "aw_02@zz", "aw_02@zz", "aw_02@zz",
		"aw_02@zz", "aw_02@zz", "aw_02@zz", "aw_01@zz", "aw_02@zz", "aw_02@zz", "aw_02@zz", "aw_02@zz", "aw_02@zz", "aw_02@zz",
		"aw_02@zz", "aw_02@zz", "aw_02@zz", "aw_02@zz", "aw_01@zz", "aw_01@zz", "aw_01@zz", "aw_02@zz"
	};

	private int[] layers_ = new int[]
	{
		2, 2, 1, 1, 1, 2, 1, 1, 1, 1,
		1, 1, 1, 2, 1, 1, 1, 1, 1, 1,
		1, 1, 1, 1, 2, 2, 2, 1
	};

	private int[] panelbuilder_;

	private GUIPanelEx[] panels_;

	private GUISpeedArrow guiSpeedArrow_;

	private enum PanelType
	{
		TACHOMETER,
		LAPINFO,
		MYLAP,
		SLASH,
		TOTALLAP,
		TIMEINFO,
		TIMEINFO_M,
		TIMEINFO_COLON_1,
		TIMEINFO_10S,
		TIMEINFO_1S,
		TIMEINFO_COLON_2,
		TIMEINFO_100MS,
		TIMEINFO_10MS,
		BESTINFO,
		BESTINFO_M,
		BESTINFO_COLON_1,
		BESTINFO_10S,
		BESTINFO_1S,
		BESTINFO_COLON_2,
		BESTINFO_100MS,
		BESTINFO_10MS,
		SPEED_100,
		SPEED_10,
		SPEED_1,
		HAND,
		HANDMARK,
		KMH,
		RANK,
		SIZE
	}

	private enum TimeIndex
	{
		TIME_M = 1,
		TIME_10S = 3,
		TIME_1S,
		TIME_100MS = 6,
		TIME_10MS
	}
}
