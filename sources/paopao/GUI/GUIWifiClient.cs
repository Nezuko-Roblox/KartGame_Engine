using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIWifiClient : FiaGUILayer
{
	public override void DoInit()
	{
		this.RegistMonoBehaviour(1034);
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		FiaTexture fiaTexture2 = new FiaTexture(this.meshRenderer_.materials[3].mainTexture);
		FiaTexture fiaTexture3 = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		FiaTexture fiaTexture4 = new FiaTexture(this.meshRenderer_.materials[2].mainTexture);
		GUIPanelEx guipanelEx = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 15f, 257f, 962f, 44f, 995f, 79f }, fiaTexture, 4, GUIFontCalculator.X2_GAP);
		this.host_ = new GUIPosMoverWS(guipanelEx, new Vector2(0f, this.gapY));
		this.panelManager_.RegistGUIInterface(guipanelEx);
		for (int i = 0; i < 4; i++)
		{
			this.username_[i] = new GUIString(new Vector2(50f, 260f + (float)i * this.gapY), 2, 10, GUIString.Alignment.LEFT, 0, fiaTexture2);
			this.username_[i].SubMeshIndex = 3;
			this.username_[i].SetColor(Color.black);
			this.panelManager_.RegistGUIInterface(this.username_[i]);
			GUIPanelEx[] array = this.ready_;
			int num = i;
			GUIPanelFactory instance = GUIPanelFactory.Instance;
			int num2 = 0;
			float[] array2 = new float[] { 354f, 0f, 781f, 2f, 879f, 45f };
			array2[1] = 257f + (float)i * this.gapY;
			array[num] = instance.CreateByWindowSpace(num2, array2, fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP);
			this.panelManager_.RegistGUIInterface(this.ready_[i]);
			this.ready_[i].Visible = false;
		}
		GUIPanelEx3PartHorz guipanelEx3PartHorz = new GUIPanelEx3PartHorz(0, new float[] { 18f, 249f, 452f, 304f, 560f, 2f, 607f }, fiaTexture, 4, GUIFontCalculator.DEFAULT_GAP, 3f);
		this.mine_ = new GUIPosMoverWS(guipanelEx3PartHorz, new Vector2(0f, this.gapY));
		this.panelManager_.RegistGUIInterface(guipanelEx3PartHorz);
		this.backTop_ = new GUIPanelEx3PartHorz(0, new float[] { 12f, 70f, 458f, 249f, 2f, 120f, 93f }, fiaTexture, 5, GUIFontCalculator.DEFAULT_GAP, 3f);
		this.backTop_.RegistPanelManager(this.panelManager_);
		this.trackIcon_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 18f, 84f, 2f, 108f, 128f, 212f }, fiaTexture3, 4, new Vector3(2f, 2f, 16f));
		this.trackIcon_.SubMeshIndex = 1;
		this.panelManager_.RegistGUIInterface(this.trackIcon_);
		this.trackName_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 153f, 88f, 2f, 2f, 298f, 32f }, fiaTexture4, 3, GUIFontCalculator.Y2_GAP);
		this.trackName_.SubMeshIndex = 2;
		this.trackName_.RegistPanelManager(this.panelManager_);
		this.backMiddle_ = new GUIPanelEx3PartHorz(0, new float[] { 12f, 194f, 458f, 249f, 514f, 150f, 605f }, fiaTexture, 5, GUIFontCalculator.X2_GAP, 3f);
		this.backMiddle_.RegistPanelManager(this.panelManager_);
		this.mode_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 134f, 194f, 188f, 150f, 512f, 205f }, fiaTexture, 5, GUIFontCalculator.Y2_GAP);
		this.mode_.RegistPanelManager(this.panelManager_);
		this.backBottom_ = new GUIPanelEx3PartHorz(0, new float[] { 12f, 249f, 458f, 529f, 2f, 246f, 93f }, fiaTexture, 5, GUIFontCalculator.DEFAULT_GAP, 3f);
		this.backBottom_.RegistPanelManager(this.panelManager_);
	}

	protected override void FirstUpdate()
	{
		MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SIMPLE_MESSAGE);
		base.SendMessage(515, monoBehaviourMessage2Param.Initialize(4, 0));
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
				this.mode_.SetUV((int)param_.gameMode_);
				int num = (int)param_.selectedTrack_;
				if (TrackAssetDefinitionManager.Instance.IsValidIndex(num))
				{
					this.trackIcon_.SetUV(num);
					this.trackName_.SetUV(1 + num);
				}
				else
				{
					this.trackIcon_.SetUV(TrackAssetDefinitionManager.RANDOM_IDX_ARRAY[0]);
					this.trackName_.SetUV(0);
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

	private GUIPanelEx mode_;

	private GUIPosMoverWS host_;

	private GUIString[] username_ = new GUIString[4];

	private GUIPanelEx[] ready_ = new GUIPanelEx[4];

	private GUIPosMoverWS mine_;

	private GUIPanelEx3PartHorz backTop_;

	private GUIPanelEx trackIcon_;

	private GUIPanelEx trackName_;

	private GUIPanelEx3PartHorz backMiddle_;

	private GUIPanelEx3PartHorz backBottom_;

	private float gapY = 55f;
}
