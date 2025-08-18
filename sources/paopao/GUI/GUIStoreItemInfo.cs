using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIStoreItemInfo : FiaGUILayer
{
	public override void DoInit()
	{
		base.DoInit();
		this.RegistMonoBehaviour(1046);
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		FiaTexture fiaTexture2 = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		GUIPanelEx guipanelEx = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 1f, 1f, 1f, 1f, 1f, 1f }, fiaTexture, 4, GUIFontCalculator.DEFAULT_GAP);
		guipanelEx.SubMeshIndex = 0;
		guipanelEx.Visible = false;
		GUIPanelEx guipanelEx2 = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 1f, 1f, 1f, 1f, 1f, 1f }, fiaTexture, 4, GUIFontCalculator.DEFAULT_GAP);
		guipanelEx2.SubMeshIndex = 1;
		guipanelEx2.Visible = false;
		GUIPanelEx guipanelEx3 = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 1f, 1f, 1f, 1f, 1f, 1f }, fiaTexture, 4, GUIFontCalculator.DEFAULT_GAP);
		guipanelEx3.SubMeshIndex = 2;
		guipanelEx3.Visible = false;
		guipanelEx.RegistPanelManager(this.panelManager_);
		guipanelEx2.RegistPanelManager(this.panelManager_);
		guipanelEx3.RegistPanelManager(this.panelManager_);
		float[] array = new float[]
		{
			252f, 90f, 776f, 396f, 2f, 2f, 510f, 380f, 2f, 2f,
			510f, 596f, 2f, 2f, 510f, 596f, 2f, 2f, 510f, 596f,
			2f, 2f, 510f, 596f, 2f, 2f, 510f, 380f, 2f, 2f,
			510f, 380f, 2f, 2f, 510f, 730f
		};
		ScrollBarInfo scrollBarInfo = new ScrollBarInfo();
		scrollBarInfo.isVert_ = true;
		scrollBarInfo.viewRegion_ = GUIBase.ConvertWSToUS(array, 0);
		scrollBarInfo.listRegion_ = GUIBase.ConvertWSToUS(array, array.Length - 4);
		scrollBarInfo.material_ = this.meshRenderer_.materials[0];
		scrollBarInfo.texture_ = fiaTexture;
		scrollBarInfo.scrollbarInfo_ = new float[] { 778f, 102f, 785f, 384f, 188f, 2f, 23f };
		GUIScrollBar guiscrollBar = null;
		if (this.scrollBarPrefab_ != null && scrollBarInfo != null)
		{
			guiscrollBar = (GUIScrollBar)global::UnityEngine.Object.Instantiate(this.scrollBarPrefab_);
			guiscrollBar.transform.parent = base.transform.parent;
			guiscrollBar.info_ = scrollBarInfo;
			guiscrollBar.Initialize();
		}
		this.contents_ = new GUIUVScrollImage(0, array, fiaTexture2, 2, guiscrollBar);
		this.contents_.SubMeshIndex = 1;
		this.contents_.RegistPanelManager(this.panelManager_);
	}

	protected override void BeforePanelUpdate()
	{
		base.BeforePanelUpdate();
		this.contents_.Update();
	}

	protected void SetData(Product p)
	{
		this.meshRenderer_.materials[1].mainTexture = (Texture2D)Resources.Load("i18n/" + iOSUtil.Locale + "/store/" + p.Info);
		this.contents_.SetUV(p.Index);
	}

	public override void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(senderId, msg);
		if (base.gameObject.active && msg.type_ == MonoBehaviourMessageType.UPDATE_STORE_ITEM_INFO)
		{
			MonoBehaviourMessage1Param<string> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<string>)msg;
			if (monoBehaviourMessage1Param != null && monoBehaviourMessage1Param.param_ != null)
			{
				this.SetData(FiaStore.Inst.FindProductWithID(monoBehaviourMessage1Param.param_));
			}
		}
	}

	private const float START_X = 252f;

	private const float START_Y = 90f;

	private const int ALERT_X_POS = 58;

	public GUIScrollBar scrollBarPrefab_;

	private GUIUVScrollImage contents_;

	private GUIPanelEx[] alertIcons_;

	protected int index_ = 7;
}
