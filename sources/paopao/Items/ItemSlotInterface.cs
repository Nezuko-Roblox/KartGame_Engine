using System;

public abstract class ItemSlotInterface
{
	public abstract void Initialize();

	public abstract void Update(float tick);

	public abstract void SetItemSlotCnt(int cnt);

	public abstract int GetItemSlotCnt();

	public abstract bool AddItemSlotItem(int item);

	public abstract int GetFirstItemSlot();

	public abstract int GetItemType(int idx);

	public abstract int UseItemSlotItem();

	public abstract int UseItemSlotItem(int idx);

	public abstract void GetItemSlotItemInfo(out int[] itemVec);

	public abstract bool CanChangeSlotItems();

	public abstract void ChangeSlotItems(int tick);

	public abstract int GetSlotChangerNum();

	public abstract void SetSlotChangerNum(int num);

	public abstract void ResetForRestarting();
}
