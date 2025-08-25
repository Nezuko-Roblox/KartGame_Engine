using System;
using UnityEngine;

internal class GUIUpDownListItem : GUIListCtrlItem, MouseNotifier
{
	public GUIUpDownListItem(int listIdx, MeshRenderer _meshRenderer, Vector2 offset, GUIListCtrl listctrl)
		: base(listIdx, _meshRenderer, offset, listctrl)
	{
	}

	protected override void Initialize()
	{
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		this.image_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 210f, 231f, 609f, 2f, 659f, 39f }, fiaTexture, 5, new Vector3(2f, 2f, 12f));
		this.image_.SetUV(2);
		this.back_ = new GUIPanelEx3PartHorz(0, new float[] { 18f, 222f, 452f, 278f, 514f, 207f, 605f }, fiaTexture, 6, GUIFontCalculator.DEFAULT_GAP, 3f);
		this.clickRect_ = new Rect(18f, (float)(Screen.height - 222 - 56), 434f, 56f);
	}

	public override void MoveRectByWindowPos(float x, float y)
	{
		this.image_.MoveRectByWindowPos(x, y);
		this.back_.MoveRectByWindowPos(x, y);
		this.clickRect_.xMin = this.clickRect_.xMin + x;
		this.clickRect_.xMax = this.clickRect_.xMax + x;
		this.clickRect_.yMin = this.clickRect_.yMin - y;
		this.clickRect_.yMax = this.clickRect_.yMax - y;
	}

	public override void RegistPanelManager(GUIPanelManager manager)
	{
		manager.RegistGUIInterface(this.image_);
		manager.RegistGUIInterface(this.back_);
	}

	public override void RegistMouseManager(MouseManager manager)
	{
		base.RegistMouseManager(manager);
		manager.Insert(this.id_, this);
	}

	public bool Contains(Vector3 pos)
	{
		return this.clickRect_.Contains(pos);
	}

	public int GetPriority()
	{
		return 1;
	}

	public bool IsEnabled()
	{
		return this.isEnable_;
	}

	public bool Enable
	{
		get
		{
			return this.isEnable_;
		}
		set
		{
			this.isEnable_ = value;
			this.image_.Visible = this.isEnable_;
			this.back_.Visible = this.isEnable_;
		}
	}

	public bool IsUpButton()
	{
		return this.isUp_;
	}

	public void SetData(bool isUp)
	{
		this.isUp_ = isUp;
		this.image_.SetUV((!this.isUp_) ? 0 : 2);
	}

	public void SetUV(int uv_)
	{
		this.image_.SetUV(uv_);
	}

	private GUIPanelEx image_;

	private GUIPanelEx3PartHorz back_;

	private Rect clickRect_;

	private bool isEnable_ = true;

	private bool isUp_;
}
