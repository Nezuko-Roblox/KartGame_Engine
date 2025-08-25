using System;
using UnityEngine;

public class GUITrackListItem : GUIListCtrlItem
{
	public GUITrackListItem(int listidx, MeshRenderer _meshRenderer, Vector2 offset, GUIListCtrl listctrl)
		: base(listidx, _meshRenderer, offset, listctrl)
	{
	}

	protected override void Initialize()
	{
		this.backId_ = this.id_ * 2;
		this.rankId_ = this.id_ * 2 + 1;
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[3].mainTexture);
		FiaTexture fiaTexture2 = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		FiaTexture fiaTexture3 = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		FiaTexture fiaTexture4 = new FiaTexture(this.meshRenderer_.materials[5].mainTexture);
		this.back_ = new GUIPanelEx3PartHorz(0, new float[] { 0f, 0f, 446f, 116f, 2f, 2f, 93f }, fiaTexture2, 5, new Vector3(93f, 788f, 14f), 3f);
		this.trackIcon_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 6f, 6f, 2f, 108f, 128f, 212f }, fiaTexture3, 4, new Vector3(2f, 2f, 16f));
		this.trackIcon_.SubMeshIndex = 1;
		this.cupIcon_ = new GUIPanelEx[3];
		for (int i = 0; i < this.cupIcon_.Length; i++)
		{
			GUIPanelEx[] array = this.cupIcon_;
			int num = i;
			GUIPanelFactory instance = GUIPanelFactory.Instance;
			int num2 = 0;
			float[] array2 = new float[] { 0f, 72f, 791f, 47f, 831f, 87f };
			array2[0] = (float)(136 + i * 42);
			array[num] = instance.CreateByWindowSpace(num2, array2, fiaTexture2, 4, GUIFontCalculator.X2_GAP);
		}
		this.trackName_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 152f, 12f, 2f, 2f, 298f, 32f }, fiaTexture4, 4, new Vector3(2f, 2f, 2f));
		this.trackName_.SubMeshIndex = 5;
		this.bestTime_ = new GUIString(new Vector2(161f, 46f), 2, 7, GUIString.Alignment.LEFT, 3, fiaTexture);
		this.bestTime_.SubMeshIndex = 3;
		this.rank_ = new GUIString(new Vector2(398f, 76f), 2, 15, GUIString.Alignment.RIGHT, 4, fiaTexture);
		this.rank_.SubMeshIndex = 3;
		this.rankings_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 347f, 75f, 713f, 54f, 767f, 70f }, fiaTexture2, 3, GUIFontCalculator.DEFAULT_GAP);
		this.rankButton_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 406f, 60f, 713f, 2f, 745f, 52f }, fiaTexture2, 3, GUIFontCalculator.X2_GAP);
		this.lockIcon_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 11f, 38f, 881f, 2f, 995f, 42f }, fiaTexture2, 3, GUIFontCalculator.Y2_GAP);
		this.lockIcon_.SubMeshIndex = 2;
		this.itemCost_ = new GUIString(new Vector2(68f, 32f), 2, 1, GUIString.Alignment.LEFT, 4, fiaTexture);
		this.itemCost_.SubMeshIndex = 3;
		this.alert_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 0f, 0f, 875f, 47f, 960f, 97f }, fiaTexture2, 2, GUIFontCalculator.DEFAULT_GAP);
		this.alert_.Visible = false;
		this.alert_.SubMeshIndex = 2;
	}

	public override void MoveRectByWindowPos(float x, float y)
	{
		this.back_.MoveRectByWindowPos(x, y);
		this.trackIcon_.MoveRectByWindowPos(x, y);
		for (int i = 0; i < this.cupIcon_.Length; i++)
		{
			this.cupIcon_[i].MoveRectByWindowPos(x, y);
		}
		this.lockIcon_.MoveRectByWindowPos(x, y);
		this.itemCost_.MoveRectByWindowPos(x, y);
		this.trackName_.MoveRectByWindowPos(x, y);
		this.bestTime_.MoveRectByWindowPos(x, y);
		this.rank_.MoveRectByWindowPos(x, y);
		this.rankButton_.MoveRectByWindowPos(x, y);
		this.rankings_.MoveRectByWindowPos(x, y);
		this.alert_.MoveRectByWindowPos(x, y);
	}

	public override void RegistPanelManager(GUIPanelManager manager)
	{
		manager.RegistGUIInterface(this.back_);
		manager.RegistGUIInterface(this.trackIcon_);
		for (int i = 0; i < this.cupIcon_.Length; i++)
		{
			manager.RegistGUIInterface(this.cupIcon_[i]);
		}
		manager.RegistGUIInterface(this.lockIcon_);
		manager.RegistGUIInterface(this.itemCost_);
		manager.RegistGUIInterface(this.trackName_);
		manager.RegistGUIInterface(this.bestTime_);
		manager.RegistGUIInterface(this.rank_);
		manager.RegistGUIInterface(this.rankings_);
		manager.RegistGUIInterface(this.rankButton_);
		manager.RegistGUIInterface(this.alert_);
	}

	public override void RegistMouseManager(MouseManager manager)
	{
		base.RegistMouseManager(manager);
		manager.Insert(this.id_ * 2, this.back_);
		manager.Insert(this.id_ * 2 + 1, this.rankButton_);
	}

	private bool IsSelectedItem()
	{
		return this.listctrl_.SelectedId == this.trackIdx_;
	}

	public override void Update()
	{
		int pushedKey = this.mouseManager_.GetPushedKey();
		int selectedKey = this.mouseManager_.GetSelectedKey();
		if (this.itemLockType_ == GUITrackListItem.enItemLockType.NONE)
		{
			this.back_.SetUV((pushedKey != this.backId_) ? ((!this.IsSelectedItem()) ? 0 : 1) : 2);
			this.trackIcon_.VerticeColor = ((this.itemLockType_ != GUITrackListItem.enItemLockType.NONE) ? Color.gray : Color.white);
		}
		else if (this.itemLockType_ == GUITrackListItem.enItemLockType.CASH)
		{
			this.back_.SetUV((pushedKey != this.backId_) ? 4 : 5);
		}
		else if (this.itemLockType_ == GUITrackListItem.enItemLockType.QUEST)
		{
			this.back_.SetUV((pushedKey != this.backId_) ? 6 : 7);
		}
		if (this.rankButton_.Visible)
		{
			if (this.IsSelectedItem())
			{
				this.rankButton_.VerticeColor = Color.white;
				this.rankButton_.SetUV((pushedKey != this.rankId_) ? 0 : 1);
			}
			else
			{
				this.rankButton_.VerticeColor = Color.gray;
				this.rankButton_.SetUV(0);
			}
		}
		if (this.itemLockType_ == GUITrackListItem.enItemLockType.NONE && (selectedKey == this.backId_ || selectedKey == this.rankId_))
		{
			base.SendMessageToListCtrl(2, this.trackIdx_);
			StageController.Instance.PlaySound(StageController.FxType.CLICK);
			return;
		}
		if (selectedKey == this.rankId_)
		{
			base.SendMessageToListCtrl(0, this.trackIdx_);
			StageController.Instance.PlaySound(StageController.FxType.CLICK);
			return;
		}
		if (selectedKey == this.backId_)
		{
			base.SendMessageToListCtrl(1, this.trackIdx_);
			StageController.Instance.PlaySound(StageController.FxType.SELECT);
		}
	}

	public void SetData(int trackIdx, int playerRank, int no, GUITrackListItem.enItemLockType lockType, bool alert)
	{
		Debug.Log(string.Concat(new object[]
		{
			"____________________",
			trackIdx,
			" lockType:",
			lockType.ToString(),
			" ",
			alert
		}));
		this.trackIdx_ = trackIdx;
		this.itemLockType_ = lockType;
		this.back_.Visible = true;
		this.back_.SetUV((this.listctrl_.SelectedId != this.trackIdx_) ? 0 : 1);
		if (lockType == GUITrackListItem.enItemLockType.NONE)
		{
			this.back_.SetUV((this.listctrl_.SelectedId != this.trackIdx_) ? 0 : 1);
		}
		else
		{
			this.back_.SetUV((lockType != GUITrackListItem.enItemLockType.CASH) ? 5 : 4);
		}
		this.trackIcon_.Visible = true;
		this.trackIcon_.SetUV(this.trackIdx_);
		this.trackIcon_.VerticeColor = ((this.itemLockType_ != GUITrackListItem.enItemLockType.NONE) ? Color.gray : Color.white);
		this.trackName_.Visible = true;
		this.trackName_.SetUV(0);
		if (!TrackAssetDefinitionManager.Instance.IsRandomTrackIndex(this.trackIdx_))
		{
			this.trackName_.SetUV(1 + this.trackIdx_);
		}
		if (this.itemLockType_ != GUITrackListItem.enItemLockType.NONE)
		{
			this.bestTime_.SetString(string.Empty);
			this.bestTime_.SetColor(FiaColor.lightGrey);
			this.rank_.SetString(string.Empty);
			this.rankings_.Visible = false;
			this.rankButton_.Visible = false;
			for (int i = 0; i < this.cupIcon_.Length; i++)
			{
				this.cupIcon_[i].Visible = false;
			}
			this.lockIcon_.Visible = true;
			if (this.itemLockType_ == GUITrackListItem.enItemLockType.QUEST)
			{
				this.lockIcon_.SetUV(0);
				this.itemCost_.SetString(string.Empty);
			}
			else
			{
				this.lockIcon_.SetUV(1);
				this.itemCost_.SetString(string.Empty);
				TrackAssetDefinition trackAssetDefinition = (TrackAssetDefinition)TrackAssetDefinitionManager.Instance.GetAssetDefinition(this.trackIdx_);
				if (trackAssetDefinition != null && trackAssetDefinition.ProductIDs.Length > 0)
				{
					string text = FiaStore.Inst.FindProductWithID(trackAssetDefinition.ProductIDString).LocalizedTitle;
					text = text.Replace("Bundle ", string.Empty);
					this.itemCost_.SetString(text);
				}
			}
		}
		else if (!TrackAssetDefinitionManager.Instance.IsValidIndex(this.trackIdx_))
		{
			this.trackName_.SetUV(0);
			this.bestTime_.SetString(string.Empty);
			this.rank_.SetString(string.Empty);
			this.rankings_.Visible = false;
			this.rankButton_.Visible = false;
			for (int j = 0; j < this.cupIcon_.Length; j++)
			{
				this.cupIcon_[j].Visible = false;
			}
			this.lockIcon_.Visible = false;
			this.itemCost_.SetString(string.Empty);
		}
		else
		{
			Facebook facebook = ((!Env.IsDesktop) ? Facebook.Inst : MockFacebook.Inst);
			this.bestTime_.SetString(string.Empty);
			GhostFilenameInfo bestInfo = KartOptions.Instance.GetBestInfo((byte)this.trackIdx_, (int)KartManager.Instance.parameter_.gameMode_);
			if (bestInfo != null && !facebook.LoggedIn)
			{
				float finishTime_ = bestInfo.finishTime_;
				string text2 = (int)(finishTime_ / 60f) + string.Empty;
				string text3 = (int)(finishTime_ % 60f) + string.Empty;
				string text4 = (int)(finishTime_ % 1f * 100f) + string.Empty;
				string text5 = string.Concat(new string[]
				{
					text2,
					":",
					(text3.Length >= 2) ? text3 : ("0" + text3),
					":",
					(text4.Length >= 2) ? text4 : ("0" + text4)
				});
				this.bestTime_.SetString(text5);
			}
			this.bestTime_.SetColor(FiaColor.lightGrey);
			this.lockIcon_.Visible = false;
			this.itemCost_.SetString(string.Empty);
			this.rankButton_.Visible = true;
			if (facebook.LoggedIn)
			{
				if (playerRank < 0)
				{
					string text6;
					if (no >= 99)
					{
						text6 = "???";
					}
					else if (no >= 9)
					{
						text6 = "??";
					}
					else
					{
						text6 = "?";
					}
					this.rank_.SetString(string.Format("{0} / {1}", text6, text6));
				}
				else
				{
					this.rank_.SetString(string.Format("{0} / {1}", playerRank, no));
				}
			}
			else
			{
				this.rank_.SetString(string.Empty);
			}
			this.rankings_.Visible = !facebook.LoggedIn;
			GhostFilenameInfo bestInfo2 = KartOptions.Instance.GetBestInfo((byte)this.trackIdx_, (int)KartManager.Instance.parameter_.gameMode_);
			int num = 0;
			if (bestInfo2 != null)
			{
				num = Mathf.Clamp(KartOptions.Instance.GetWinCounter((byte)this.trackIdx_)[(int)KartManager.Instance.parameter_.gameMode_], 0, 3);
			}
			for (int k = 0; k < this.cupIcon_.Length; k++)
			{
				this.cupIcon_[k].Visible = true;
				this.cupIcon_[k].SetUV((k >= num) ? 0 : 1);
			}
		}
		this.alert_.Visible = alert;
	}

	public void Invisible()
	{
		this.back_.Visible = false;
		this.trackIcon_.Visible = false;
		for (int i = 0; i < this.cupIcon_.Length; i++)
		{
			this.cupIcon_[i].Visible = false;
		}
		this.trackName_.Visible = false;
		this.rank_.SetString(string.Empty);
		this.rankButton_.Visible = false;
		this.rankings_.Visible = false;
		this.lockIcon_.Visible = false;
		this.itemCost_.SetString(string.Empty);
		this.trackIdx_ = -1;
	}

	public GUITrackListItem.enItemLockType ItemLockType
	{
		get
		{
			return this.itemLockType_;
		}
	}

	public int TrackIdx
	{
		get
		{
			return this.trackIdx_;
		}
	}

	public const int CLICK_RANKING = 0;

	public const int CLICK_TRACK = 1;

	public const int VIEW_PACKAGE = 2;

	private const int MAX_CUP_COUNT = 3;

	private GUIPanelEx3PartHorz back_;

	private GUIPanelEx trackIcon_;

	private GUIPanelEx[] cupIcon_;

	private GUIPanelEx trackName_;

	private GUIString bestTime_;

	private GUIString rank_;

	private GUIPanelEx rankings_;

	private GUIPanelEx rankButton_;

	private GUIPanelEx lockIcon_;

	private GUIString itemCost_;

	private GUIPanelEx alert_;

	private int trackIdx_;

	private int backId_;

	private int rankId_;

	private GUITrackListItem.enItemLockType itemLockType_;

	public enum enItemLockType
	{
		NONE,
		CASH,
		QUEST
	}

	private enum enBackUVIdx
	{
		NORMAL,
		SELECTED,
		PUSHED,
		EMPTY,
		CASH,
		QUEST
	}
}
