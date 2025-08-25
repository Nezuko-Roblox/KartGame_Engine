using System;
using UnityEngine;

public class ItemSlotInterfaceWeb : ItemSlotInterface
{
	public override void Initialize()
	{
	}

	public override void Update(float tick)
	{
		for (int i = 0; i < this.itemCount_; i++)
		{
			if (this.itemSlots_[i] != null)
			{
				this.itemSlots_[i].Update(1);
			}
		}
	}

	public override void SetItemSlotCnt(int cnt)
	{
	}

	public override int GetItemSlotCnt()
	{
		return this.itemCount_;
	}

	public override bool AddItemSlotItem(int item)
	{
		for (int i = 0; i < this.itemCount_; i++)
		{
			if (this.itemSlots_[i].GetItem() == -1)
			{
				this.itemSlots_[i].SetItem(item);
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
		return this.itemSlots_[0].GetItem();
	}

	public override int GetItemType(int idx)
	{
		if (this.itemCount_ < 1)
		{
			return -1;
		}
		return this.itemSlots_[idx].GetItem();
	}

	public override int UseItemSlotItem()
	{
		int item = this.itemSlots_[0].GetItem();
		for (int i = 1; i < this.itemCount_; i++)
		{
			this.itemSlots_[i - 1].SetItem(this.itemSlots_[i].GetItem());
			bool flag = this.itemSlots_[i].IsSlotChangeItemMode();
			if (flag)
			{
				int changeItemIdx = this.itemSlots_[i].GetChangeItemIdx();
				this.itemSlots_[i - 1].SetItemChangeEffect(true, changeItemIdx);
			}
		}
		if (this.itemCount_ != 0)
		{
			this.itemSlots_[this.itemCount_ - 1].SetItem(-1);
		}
		return item;
	}

	public override int UseItemSlotItem(int idx)
	{
		return -1;
	}

	public override void GetItemSlotItemInfo(out int[] itemVec)
	{
		itemVec = null;
	}

	public override bool CanChangeSlotItems()
	{
		if (this.cntSlotChanger_ == 0)
		{
			return false;
		}
		if (this.itemCount_ < 2)
		{
			return false;
		}
		int num = 0;
		for (int i = 0; i < this.itemCount_; i++)
		{
			if (this.itemSlots_[i].GetItem() != -1)
			{
				num++;
			}
		}
		if (num < 2)
		{
			return false;
		}
		int num2 = 0;
		bool flag = true;
		for (int j = 0; j < this.itemCount_; j++)
		{
			if (this.itemSlots_[j].GetItem() != -1)
			{
				num2 = this.itemSlots_[j].GetItem();
			}
		}
		for (int k = 0; k < this.itemCount_; k++)
		{
			if (this.itemSlots_[k].GetItem() != -1 && this.itemSlots_[k].GetItem() != num2)
			{
				flag = false;
			}
		}
		if (flag)
		{
			return false;
		}
		for (int l = 0; l < this.itemCount_; l++)
		{
			if (this.itemSlots_[l].IsMovingSlot())
			{
				return false;
			}
		}
		return true;
	}

	public override void ChangeSlotItems(int tick)
	{
		int num = 0;
		for (int i = 0; i < this.itemCount_; i++)
		{
			if (this.itemSlots_[i].GetItem() != -1)
			{
				num++;
			}
		}
		if (num < 2)
		{
			return;
		}
		Vector2[] array = new Vector2[num];
		for (int j = 0; j < num; j++)
		{
			array[j] = this.itemSlots_[j].GetLeftTopPosByWindowSpace();
		}
		this.itemSlots_[0].ChangeSlot(tick, array[num - 1], ItemSlot.ItemSlotSize.NORMAL);
		for (int k = 1; k < num; k++)
		{
			this.itemSlots_[k].ChangeSlot(tick, array[k - 1], (k != 1) ? ItemSlot.ItemSlotSize.NORMAL : ItemSlot.ItemSlotSize.BIG);
		}
		ItemSlot itemSlot = this.itemSlots_[0];
		for (int l = 0; l < num - 1; l++)
		{
			this.itemSlots_[l] = this.itemSlots_[l + 1];
		}
		this.itemSlots_[num - 1] = itemSlot;
	}

	public override int GetSlotChangerNum()
	{
		return this.cntSlotChanger_;
	}

	public override void SetSlotChangerNum(int num)
	{
		this.cntSlotChanger_ = num;
	}

	public override void ResetForRestarting()
	{
		for (int i = 0; i < this.itemCount_; i++)
		{
			if (this.itemSlots_[i] != null)
			{
				this.itemSlots_[i].ResetForRestarting();
			}
		}
	}

	public const int MAX_ITEM_SLOT = 0;

	public ItemSlot[] itemSlots_;

	public int itemCount_;

	public int cntSlotChanger_;
}
