using System;

public class ItemSlotInterfaceIPad : ItemSlotInterface
{
	public override void Initialize()
	{
		this.itemSlots_ = new GameItem[2];
	}

	public override void Update(float tick)
	{
	}

	public override void SetItemSlotCnt(int cnt)
	{
		for (int i = 0; i < 2; i++)
		{
			this.itemSlots_[i] = GameItem.NONE;
		}
		this.itemCount_ = cnt;
	}

	public override int GetItemSlotCnt()
	{
		return this.itemCount_;
	}

	public override bool AddItemSlotItem(int item)
	{
		for (int i = 0; i < this.itemCount_; i++)
		{
			if (this.itemSlots_[i] == GameItem.NONE)
			{
				this.itemSlots_[i] = (GameItem)item;
				UpdateGUIItemSlotMessge updateGUIItemSlotMessge = (UpdateGUIItemSlotMessge)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.UPDATE_ITEMSLOTS);
				updateGUIItemSlotMessge.Initialize(this.itemSlots_);
				MonoBehaviourExCenter.Instance.SendMessage(0, 3, updateGUIItemSlotMessge);
				return true;
			}
		}
		return false;
	}

	public override int GetFirstItemSlot()
	{
		if (this.itemCount_ < 1)
		{
			return -1;
		}
		return (int)this.itemSlots_[0];
	}

	public override int GetItemType(int idx)
	{
		if (this.itemCount_ < 1)
		{
			return -1;
		}
		return (int)this.itemSlots_[idx];
	}

	public override int UseItemSlotItem()
	{
		int num = (int)this.itemSlots_[0];
		if (num == -1)
		{
			return num;
		}
		for (int i = 1; i < this.itemCount_; i++)
		{
			this.itemSlots_[i - 1] = this.itemSlots_[i];
		}
		if (this.itemCount_ != 0)
		{
			this.itemSlots_[this.itemCount_ - 1] = GameItem.NONE;
		}
		MonoBehaviourExCenter.Instance.SendMessage(0, 3, ((UpdateGUIItemSlotMessge)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.UPDATE_ITEMSLOTS)).Initialize(this.itemSlots_));
		return num;
	}

	public override int UseItemSlotItem(int idx)
	{
		int num = (int)this.itemSlots_[idx];
		this.itemSlots_[idx] = GameItem.NONE;
		MonoBehaviourExCenter.Instance.SendMessage(0, 3, ((UpdateGUIItemSlotMessge)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.UPDATE_ITEMSLOTS)).Initialize(this.itemSlots_));
		return num;
	}

	public override void GetItemSlotItemInfo(out int[] itemVec)
	{
		itemVec = new int[this.itemCount_];
		for (int i = 0; i < this.itemCount_; i++)
		{
			itemVec[i] = (int)this.itemSlots_[i];
		}
	}

	public override bool CanChangeSlotItems()
	{
		return false;
	}

	public override void ChangeSlotItems(int tick)
	{
	}

	public override int GetSlotChangerNum()
	{
		return 0;
	}

	public override void SetSlotChangerNum(int num)
	{
	}

	public override void ResetForRestarting()
	{
		for (int i = 0; i < 2; i++)
		{
			this.itemSlots_[i] = GameItem.NONE;
		}
	}

	public const int MAX_ITEM_SLOT = 2;

	public GameItem[] itemSlots_;

	public int itemCount_;
}
