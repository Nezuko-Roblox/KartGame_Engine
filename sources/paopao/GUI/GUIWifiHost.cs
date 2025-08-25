using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIWifiHost : FiaGUILayer
{
	public override void DoInit()
	{
		this.RegistMonoBehaviour(1032);
		this.mouseManager_ = new MouseManager();
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		FiaTexture fiaTexture2 = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		FiaTexture fiaTexture3 = new FiaTexture(this.meshRenderer_.materials[2].mainTexture);
		this.itemMode_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 20f, 248f, 188f, 264f, 401f, 340f }, fiaTexture, 4, GUIFontCalculator.X2_GAP);
		this.panelManager_.RegistGUIInterface(this.itemMode_);
		this.speedMode_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 237f, 248f, 188f, 342f, 401f, 418f }, fiaTexture, 4, GUIFontCalculator.X2_GAP);
		this.panelManager_.RegistGUIInterface(this.speedMode_);
		GUIPanelEx guipanelEx = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 15f, 334f, 962f, 44f, 995f, 79f }, fiaTexture, 4, GUIFontCalculator.DEFAULT_GAP);
		this.host_ = new GUIPosMoverWS(guipanelEx, new Vector2(0f, 34f));
		this.panelManager_.RegistGUIInterface(guipanelEx);
		this.trackName_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 22f, 210f, 2f, 2f, 298f, 32f }, fiaTexture2, 3, new Vector3(2f, 2f, 124f));
		this.trackName_.SubMeshIndex = 1;
		this.panelManager_.RegistGUIInterface(this.trackName_);
		this.trackName_.SetUV(0);
		for (int i = 0; i < 4; i++)
		{
			this.username_[i] = new GUIString(new Vector2(50f, (float)(336 + i * 34)), 2, 10, GUIString.Alignment.LEFT, 0, fiaTexture3);
			this.username_[i].SubMeshIndex = 2;
			this.username_[i].SetColor(Color.black);
			this.panelManager_.RegistGUIInterface(this.username_[i]);
			GUIPanelEx[] array = this.ready_;
			int num = i;
			GUIPanelFactory instance = GUIPanelFactory.Instance;
			int num2 = 0;
			float[] array2 = new float[] { 354f, 0f, 781f, 2f, 879f, 45f };
			array2[1] = (float)(332 + i * 34);
			array[num] = instance.CreateByWindowSpace(num2, array2, fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP);
			this.panelManager_.RegistGUIInterface(this.ready_[i]);
			this.ready_[i].Visible = false;
		}
		GUIPanelEx3PartHorz guipanelEx3PartHorz = new GUIPanelEx3PartHorz(0, new float[] { 18f, 334f, 452f, 368f, 560f, 59f, 607f }, fiaTexture, 4, GUIFontCalculator.DEFAULT_GAP, 3f);
		this.mine_ = new GUIPosMoverWS(guipanelEx3PartHorz, new Vector2(0f, 34f));
		this.panelManager_.RegistGUIInterface(guipanelEx3PartHorz);
		this.mouseManager_.Insert(0, this.itemMode_);
		this.mouseManager_.Insert(1, this.speedMode_);
	}

	protected override void BeforePanelUpdate()
	{
		this.mouseManager_.Update();
		int selectedKey = this.mouseManager_.GetSelectedKey();
		if (selectedKey != -1)
		{
			this.mode_ = selectedKey;
			StageController.Instance.PlaySound(StageController.FxType.CLICK);
			MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SIMPLE_MESSAGE);
			base.SendMessage(515, monoBehaviourMessage2Param.Initialize(2, this.mode_));
		}
		this.itemMode_.SetUV((this.mode_ != 0) ? 0 : 1);
		this.speedMode_.SetUV((this.mode_ != 1) ? 0 : 1);
	}

	protected override void AfterPanelUpdate()
	{
	}

	protected override void FirstUpdate()
	{
		MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SIMPLE_MESSAGE);
		base.SendMessage(515, monoBehaviourMessage2Param.Initialize(4, 0));
		this.itemMode_.SetUV((this.mode_ != 0) ? 0 : 1);
		this.speedMode_.SetUV((this.mode_ != 1) ? 0 : 1);
		GUIAccomplishPopup.OpenStaticPopup();
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (!base.IsInitialized())
		{
			return;
		}
		if (msg.type_ == MonoBehaviourMessageType.UPDATE_WAITROOM)
		{
			MonoBehaviourMessage1Param<GameParamPacket> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<GameParamPacket>)msg;
			if (monoBehaviourMessage1Param != null)
			{
				GameParamPacket param_ = monoBehaviourMessage1Param.param_;
				this.mode_ = (int)param_.gameMode_;
				this.itemMode_.SetUV((this.mode_ != 0) ? 0 : 1);
				this.speedMode_.SetUV((this.mode_ != 1) ? 0 : 1);
				int num = (int)param_.selectedTrack_;
				this.trackName_.SetUV(0);
				if (!TrackAssetDefinitionManager.Instance.IsRandomTrackIndex(num))
				{
					this.trackName_.SetUV(1 + num);
				}
				int i = 0;
				string connectedServer = NetworkManager.Inst.Session.ConnectedServer;
				string id = NetworkManager.Inst.Session.ID;
				foreach (PlayerPacket playerPacket in param_.players_)
				{
					string text = playerPacket.name_;
					if (text.Length > 10)
					{
						text = text.Substring(0, 9) + '=';
					}
					this.username_[i].SetString(text);
					this.ready_[i].Visible = playerPacket.ready_;
					if (playerPacket.id_ == connectedServer)
					{
						this.host_.SetPos(i);
					}
					if (playerPacket.id_ == id)
					{
						this.mine_.SetPos(i);
					}
					i++;
				}
				while (i < this.username_.Length)
				{
					this.username_[i].SetString(string.Empty);
					this.ready_[i].Visible = false;
					i++;
				}
			}
		}
	}

	private MouseManager mouseManager_;

	private GUIPanelEx itemMode_;

	private GUIPanelEx speedMode_;

	private GUIPosMoverWS host_;

	private GUIString[] username_ = new GUIString[4];

	private GUIPanelEx[] ready_ = new GUIPanelEx[4];

	private GUIPosMoverWS mine_;

	private GUIPanelEx trackName_;

	private int mode_;
}
