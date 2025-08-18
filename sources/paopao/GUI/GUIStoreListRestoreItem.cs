using System;
using UnityEngine;

public class GUIStoreListRestoreItem : GUIStoreListItem
{
	public GUIStoreListRestoreItem(int listidx, MeshRenderer _meshRenderer, Vector2 offset, GUIListCtrl listctrl)
		: base(listidx, _meshRenderer, offset, listctrl)
	{
	}

	protected override void Initialize()
	{
		base.Initialize();
		this.itemIcon_.SetUV(0);
	}

	public override void Update()
	{
		int pushedKey = this.mouseManager_.GetPushedKey();
		if (pushedKey == this.id_ || this.listctrl_.SelectedId == this.listIdx_)
		{
			this.itemIcon_.SetUV(1);
		}
		else
		{
			this.itemIcon_.SetUV(0);
		}
		int selectedKey = this.mouseManager_.GetSelectedKey();
		if (selectedKey == this.id_)
		{
			StageController.Instance.PlaySound(StageController.FxType.SELECT);
			base.SendMessageToListCtrl(1, this.listIdx_);
		}
	}
}
