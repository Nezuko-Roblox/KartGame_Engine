using System;

internal class GUIPlayerElem
{
	public GUIPlayerElem(GUIIPadPlayerPanel panel, GUIIPadPlayerMark mark)
	{
		this.panel_ = panel;
		this.mark_ = mark;
		this.isVisible_ = true;
	}

	public void SetVisible(bool isVisible)
	{
		if (this.isVisible_ == isVisible)
		{
			return;
		}
		this.panel_.Visible = isVisible;
		this.mark_.Visible = isVisible;
		this.isVisible_ = isVisible;
	}

	public void Update(int panelIndex, bool isShort)
	{
		if (this.isVisible_)
		{
			this.panel_.Update(panelIndex, isShort);
			this.mark_.Update(panelIndex, isShort);
		}
	}

	public GUIIPadPlayerPanel panel_;

	public GUIIPadPlayerMark mark_;

	private bool isVisible_ = true;
}
