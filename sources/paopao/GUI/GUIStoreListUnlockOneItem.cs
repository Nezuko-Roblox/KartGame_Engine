using System;
using UnityEngine;

public class GUIStoreListUnlockOneItem : GUIStoreListUnlockableItem
{
	public GUIStoreListUnlockOneItem(int listidx, MeshRenderer _meshRenderer, Vector2 offset, GUIListCtrl listctrl)
		: base(listidx, _meshRenderer, offset, listctrl)
	{
	}
}
