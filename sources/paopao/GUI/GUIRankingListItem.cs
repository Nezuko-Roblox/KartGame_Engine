using System;
using UnityEngine;

internal class GUIRankingListItem : GUIListCtrlItem
{
	public GUIRankingListItem(int listIdx, MeshRenderer _meshRenderer, Vector2 offset, GUIListCtrl listctrl)
		: base(listIdx, _meshRenderer, offset, listctrl)
	{
	}

	protected override void Initialize()
	{
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		FiaTexture fiaTexture2 = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		this.noRecord_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 100f, 252f, 760f, 89f, 843f, 112f }, fiaTexture, 2, GUIFontCalculator.DEFAULT_GAP);
		this.noRecord_.SubMeshIndex = 0;
		this.noRecord_.Visible = false;
		this.name_ = new GUIString(new Vector2(84f, 224f), 4, 18, GUIString.Alignment.LEFT, 3, fiaTexture2);
		this.name_.SubMeshIndex = 1;
		this.name_.SetColor(FiaColor.black);
		this.time_ = new GUIString(new Vector2(100f, 252f), 4, 14, GUIString.Alignment.LEFT, 3, fiaTexture2);
		this.time_.SubMeshIndex = 1;
		this.rank_ = new GUIString(new Vector2(445f, 244f), 4, 5, GUIString.Alignment.RIGHT, 6, fiaTexture2);
		this.rank_.SubMeshIndex = 1;
	}

	public override void MoveRectByWindowPos(float x, float y)
	{
		this.rank_.MoveRectByWindowPos(x, y);
		this.time_.MoveRectByWindowPos(x, y);
		this.noRecord_.MoveRectByWindowPos(x, y);
		this.name_.MoveRectByWindowPos(x, y);
	}

	public override void RegistPanelManager(GUIPanelManager manager)
	{
		manager.RegistGUIInterface(this.rank_);
		manager.RegistGUIInterface(this.time_);
		manager.RegistGUIInterface(this.noRecord_);
		manager.RegistGUIInterface(this.name_);
	}

	public void SetData(string name, int pictureIdx, float finishTime, int ranking)
	{
		this.name_.SetString(name);
		if (finishTime >= 1200f)
		{
			this.time_.SetString(string.Empty);
			this.rank_.SetString(string.Empty);
			this.noRecord_.Visible = true;
			return;
		}
		int num = (int)finishTime;
		int num2 = (int)(finishTime * 100f) % 100;
		string text = string.Format("{0}:{1}{2}:{3}{4}", new object[]
		{
			(int)((float)num / 60f % 10f),
			num % 60 / 10,
			num % 10,
			num2 / 10,
			num2 % 10
		});
		this.time_.SetString(text);
		this.noRecord_.Visible = false;
		string text2 = string.Empty;
		string text3 = string.Empty;
		if (ranking > 0)
		{
			text3 = "a";
			text2 = string.Format("{0}{1}", ranking, text3);
		}
		this.rank_.SetString(text2);
	}

	public void Invisible()
	{
		this.rank_.SetString(string.Empty);
		this.time_.SetString(string.Empty);
		this.noRecord_.Visible = false;
		this.name_.SetString(string.Empty);
	}

	private GUIString rank_;

	private GUIString time_;

	private GUIString name_;

	private GUIPanelEx noRecord_;
}
