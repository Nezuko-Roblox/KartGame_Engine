using System;
using UnityEngine;

public class ItemSlot
{
	public ItemSlot(ItemSlot.ItemSlotSize size, Vector2 basePos, int kartId, int characterId)
	{
		this.slotSize_ = size;
		this.isChangeItemMode_ = false;
		this.isMoveSlotMode_ = false;
		this.moveBlinkSlotTimeLength_ = 200;
		this.moveSlotTimeLength_ = 350;
		this.InitItemSlotSize(basePos);
		this.itemidx_ = -1;
		this.SetItem(this.itemidx_);
		this.freezeState_ = ItemSlot.FreezeState.UNFREEZE;
		this.freezeCount_ = 0;
	}

	public void ResetForRestarting()
	{
		this.isChangeItemMode_ = false;
		this.isMoveSlotMode_ = false;
		this.moveBlinkSlotTimeLength_ = 200;
		this.moveSlotTimeLength_ = 350;
		this.itemidx_ = -1;
		this.SetItem(this.itemidx_);
		this.freezeState_ = ItemSlot.FreezeState.UNFREEZE;
		this.freezeCount_ = 0;
	}

	public void Update(int tick)
	{
		if (this.isChangeItemMode_)
		{
			bool flag = tick % 200 < 100;
			this.itemPanel_.Visible = flag;
			this.itemChangePanel_.Visible = !flag;
		}
		if (this.isMoveSlotMode_)
		{
			this.UpdateChangeSlot(tick);
		}
		switch (this.freezeState_)
		{
		case ItemSlot.FreezeState.FREEZING:
			if (this.freezeTick_ > tick)
			{
				this.freezeTick_ = tick;
			}
			if (tick - this.freezeTick_ < 500)
			{
				this.freezePanel_.Alpha = (float)(tick - this.freezeTick_) / 500f;
			}
			else
			{
				this.freezePanel_.Alpha = 1f;
				this.freezeState_ = ItemSlot.FreezeState.FREEZED;
			}
			break;
		case ItemSlot.FreezeState.UNFREEZING:
			if (this.freezeTick_ > tick)
			{
				this.freezeTick_ = tick;
			}
			if (tick - this.freezeTick_ >= 2000)
			{
				this.freezePanel_.Visible = false;
				this.freezeState_ = ItemSlot.FreezeState.UNFREEZE;
			}
			else
			{
				this.freezePanel_.Visible = (tick - this.freezeTick_) % 500 >= 250;
			}
			break;
		}
	}

	public void SetItem(int idx)
	{
		this.itemidx_ = idx;
		this.SetItemChangeEffect(false, -1);
		this.itemPanel_.Visible = idx != -1;
		if (idx != -1)
		{
			string text = "item" + ((this.slotSize_ != ItemSlot.ItemSlotSize.SMALL) ? string.Empty : "_s") + idx.ToString();
			int idx2 = this.atlas_.GetIdx(text);
			if (idx2 < 0)
			{
				return;
			}
			this.itemPanel_.SetUV(this.atlas_.uvs_[idx2]);
		}
	}

	public int GetItem()
	{
		return this.itemidx_;
	}

	public void SetReverseUser(bool isReverse)
	{
	}

	public void SetItemChangeEffect(bool mode, int idx)
	{
		if (!mode || idx == -1)
		{
			this.isChangeItemMode_ = false;
			this.itemPanel_.Visible = true;
			this.itemChangePanel_.Visible = false;
		}
		else
		{
			this.isChangeItemMode_ = true;
			this.itemChangeidx_ = idx;
			this.itemPanel_.Visible = false;
			this.itemChangePanel_.Visible = true;
			string text = "item" + ((this.slotSize_ != ItemSlot.ItemSlotSize.SMALL) ? string.Empty : "_s") + idx.ToString();
			int idx2 = this.atlas_.GetIdx(text);
			if (idx2 < 0)
			{
				return;
			}
			this.itemChangePanel_.SetUV(this.atlas_.uvs_[idx2]);
		}
	}

	public bool IsSlotChangeItemMode()
	{
		return this.isChangeItemMode_;
	}

	public int GetChangeItemIdx()
	{
		return this.itemChangeidx_;
	}

	public Vector2 GetLeftTopPosByWindowSpace()
	{
		return this.panel_.GetLeftTopByWindowPos();
	}

	public void InitItemSlotSize(Vector2 basePos)
	{
		float[][] array = null;
		switch (this.slotSize_)
		{
		case ItemSlot.ItemSlotSize.SMALL:
			array = new float[][]
			{
				new float[] { 0f, 0f, 23f, 23f, 0f, 105f, 23f, 128f },
				new float[] { 0f, 0f, 23f, 23f, 0f, 105f, 23f, 128f },
				new float[] { 0f, 0f, 24f, 24f, 0f, 0f, 24f, 24f },
				new float[] { 0f, 0f, 24f, 24f, 0f, 0f, 24f, 24f }
			};
			break;
		case ItemSlot.ItemSlotSize.NORMAL:
			array = new float[][]
			{
				new float[] { 0f, 0f, 56f, 56f, 0f, 0f, 56f, 56f },
				new float[] { 0f, 0f, 56f, 56f, 0f, 0f, 56f, 56f },
				new float[] { 4f, 4f, 52f, 52f, 0f, 0f, 48f, 48f },
				new float[] { 4f, 4f, 52f, 52f, 0f, 0f, 48f, 48f }
			};
			break;
		case ItemSlot.ItemSlotSize.BIG:
			array = new float[][]
			{
				new float[] { 0f, 0f, 72f, 72f, 56f, 0f, 128f, 72f },
				new float[] { 0f, 0f, 72f, 72f, 56f, 0f, 128f, 72f },
				new float[] { 4f, 4f, 68f, 68f, 0f, 0f, 64f, 64f },
				new float[] { 4f, 4f, 68f, 68f, 0f, 0f, 64f, 64f }
			};
			break;
		}
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				array[i][j] += basePos[j % 2];
			}
		}
		this.atlas_ = GUIAtlasManager.GetAtlas("ingame");
		FiaTexture fiaTexture = new FiaTexture(this.atlas_, "slot");
		FiaTexture fiaTexture2 = new FiaTexture(this.atlas_, "freeze_slot");
		FiaTexture fiaTexture3 = new FiaTexture(this.atlas_, "item_s0");
		FiaTexture fiaTexture4 = new FiaTexture(this.atlas_, "item0");
		this.panel_ = GUIPanelFactory.Instance.CreateByWindowSpace(array[0], fiaTexture, 2);
		this.freezePanel_ = GUIPanelFactory.Instance.CreateByWindowSpace(array[1], fiaTexture2, 1);
		this.freezePanel_.Visible = false;
		if (this.slotSize_ == ItemSlot.ItemSlotSize.SMALL)
		{
			this.itemPanel_ = GUIPanelFactory.Instance.CreateByWindowSpace(array[2], fiaTexture3, 1);
			this.itemChangePanel_ = GUIPanelFactory.Instance.CreateByWindowSpace(array[3], fiaTexture3, 1);
		}
		else
		{
			this.itemPanel_ = GUIPanelFactory.Instance.CreateByWindowSpace(array[2], fiaTexture4, 1);
			this.itemChangePanel_ = GUIPanelFactory.Instance.CreateByWindowSpace(array[3], fiaTexture4, 1);
		}
	}

	public void ChangeSlot(int tick, Vector2 pos, ItemSlot.ItemSlotSize size)
	{
		if (this.isMoveSlotMode_)
		{
			return;
		}
		this.isMoveSlotMode_ = true;
		this.startMoveSlotTick_ = tick;
		this.moveSrcPos_ = this.GetLeftTopPosByWindowSpace();
		this.moveDstPos_ = pos;
		switch (this.slotSize_)
		{
		case ItemSlot.ItemSlotSize.SMALL:
			this.moveSrcSize_ = new Vector2(23f, 23f);
			this.moveItemSrcPos_ = new Vector2(0f, 0f);
			this.moveItemSrcSize_ = new Vector2(24f, 24f);
			break;
		case ItemSlot.ItemSlotSize.NORMAL:
			this.moveSrcSize_ = new Vector2(56f, 56f);
			this.moveItemSrcPos_ = new Vector2(4f, 4f);
			this.moveItemSrcSize_ = new Vector2(52f, 52f);
			break;
		case ItemSlot.ItemSlotSize.BIG:
			this.moveSrcSize_ = new Vector2(72f, 72f);
			this.moveItemSrcPos_ = new Vector2(4f, 4f);
			this.moveItemSrcSize_ = new Vector2(68f, 68f);
			break;
		}
		switch (size)
		{
		case ItemSlot.ItemSlotSize.SMALL:
			this.moveDstSize_ = new Vector2(23f, 23f);
			this.moveItemDstPos_ = new Vector2(0f, 0f);
			this.moveItemDstSize_ = new Vector2(24f, 24f);
			break;
		case ItemSlot.ItemSlotSize.NORMAL:
			this.moveDstSize_ = new Vector2(56f, 56f);
			this.moveItemDstPos_ = new Vector2(4f, 4f);
			this.moveItemDstSize_ = new Vector2(52f, 52f);
			break;
		case ItemSlot.ItemSlotSize.BIG:
			this.moveDstSize_ = new Vector2(72f, 72f);
			this.moveItemDstPos_ = new Vector2(4f, 4f);
			this.moveItemDstSize_ = new Vector2(68f, 68f);
			break;
		}
		this.slotSize_ = size;
	}

	public void SettleSlotPosition()
	{
		if (this.isMoveSlotMode_)
		{
			this.isMoveSlotMode_ = false;
			this.panel_.SetRectByWindowSpace(this.moveDstPos_.x, this.moveDstPos_.y, this.moveDstPos_.x + this.moveDstSize_.x, this.moveDstPos_.y + this.moveDstSize_.y);
			this.freezePanel_.SetRectByWindowSpace(0f, 0f, this.moveDstSize_.x, this.moveDstSize_.y);
			this.itemPanel_.SetRectByWindowSpace(this.moveItemDstPos_.x, this.moveItemDstPos_.y, this.moveItemDstSize_.x, this.moveItemDstSize_.y);
			this.itemChangePanel_.SetRectByWindowSpace(this.moveItemDstPos_.x, this.moveItemDstPos_.y, this.moveItemDstSize_.x, this.moveItemDstSize_.y);
		}
	}

	private void UpdateChangeSlot(int tick)
	{
		if (tick > this.startMoveSlotTick_ + this.moveSlotTimeLength_)
		{
			this.SettleSlotPosition();
			return;
		}
		if (tick <= this.startMoveSlotTick_ + this.moveBlinkSlotTimeLength_)
		{
			int num = tick - this.startMoveSlotTick_;
			float num2 = (float)num / (float)this.moveBlinkSlotTimeLength_ * 3.14159274f;
			int num3 = (int)(Mathf.Sin(num2) * 20f);
			Vector2 vector = (this.moveDstPos_ - this.moveSrcPos_) * (float)num / (float)this.moveBlinkSlotTimeLength_ + this.moveSrcPos_;
			Vector2 vector2 = (this.moveDstSize_ - this.moveSrcSize_) * (float)num / (float)this.moveBlinkSlotTimeLength_ + this.moveSrcSize_;
			if (this.moveDstPos_.x < this.moveSrcPos_.x)
			{
				vector.y += (float)num3;
			}
			else
			{
				vector.y -= (float)num3;
			}
			this.panel_.SetRectByWindowSpace(vector.x, vector.y, vector.x + vector2.x, vector.y + vector2.y);
			Vector2 vector3 = (this.moveItemDstPos_ - this.moveItemSrcPos_) * (float)num / (float)this.moveBlinkSlotTimeLength_ + this.moveItemSrcPos_;
			Vector2 vector4 = (this.moveItemDstSize_ - this.moveItemSrcSize_) * (float)num / (float)this.moveBlinkSlotTimeLength_ + this.moveItemSrcSize_;
			this.itemPanel_.SetRectByWindowSpace(vector3.x, vector3.y, vector4.x, vector4.y);
			this.itemChangePanel_.SetRectByWindowSpace(vector3.x, vector3.y, vector4.x, vector4.y);
		}
		else
		{
			int num4 = tick - this.startMoveSlotTick_ - this.moveBlinkSlotTimeLength_;
			int num5 = this.moveSlotTimeLength_ - this.moveBlinkSlotTimeLength_;
			int num6 = num5 / 2;
			int num7;
			if (num4 > num6)
			{
				num7 = (num5 - num4) * 15 / num6 + 100;
			}
			else
			{
				num7 = num4 * 15 / num6 + 100;
			}
			Vector2 vector5 = this.moveDstPos_ - this.moveDstSize_ * (float)(num7 - 100) / 200f;
			Vector2 vector6 = this.moveDstSize_ * (float)num7 / 100f;
			this.panel_.SetRectByWindowSpace(vector5.x, vector5.y, vector5.x + vector6.x, vector5.y + vector6.y);
			Vector2 vector7 = this.moveItemDstPos_;
			Vector2 vector8 = this.moveItemDstSize_ * (float)num7 / 100f;
			this.itemPanel_.SetRectByWindowSpace(vector7.x, vector7.y, vector8.x, vector8.y);
			this.itemChangePanel_.SetRectByWindowSpace(vector7.x, vector7.y, vector8.x, vector8.y);
		}
	}

	public bool IsMovingSlot()
	{
		return this.isMoveSlotMode_;
	}

	public Vector2 GetRightTopPos()
	{
		Vector2 leftTopPosByWindowSpace = this.GetLeftTopPosByWindowSpace();
		switch (this.slotSize_)
		{
		case ItemSlot.ItemSlotSize.SMALL:
			leftTopPosByWindowSpace.x += 23f;
			break;
		case ItemSlot.ItemSlotSize.NORMAL:
			leftTopPosByWindowSpace.x += 56f;
			break;
		case ItemSlot.ItemSlotSize.BIG:
			leftTopPosByWindowSpace.x += 72f;
			break;
		}
		return leftTopPosByWindowSpace;
	}

	public Vector2 GetSize()
	{
		float[] array = new float[] { 24f, 56f, 72f };
		return new Vector2(array[(int)this.slotSize_], array[(int)this.slotSize_]);
	}

	public void Freeze(bool frz)
	{
		if (frz)
		{
			this.freezeState_ = ItemSlot.FreezeState.FREEZING;
			this.freezePanel_.Visible = true;
			this.freezePanel_.Alpha = 0f;
			this.freezeCount_++;
		}
		else
		{
			if (this.freezeCount_ > 0)
			{
				this.freezeCount_--;
			}
			if (this.freezeCount_ == 0)
			{
				this.freezeState_ = ItemSlot.FreezeState.UNFREEZING;
			}
		}
		this.freezeTick_ = (int)(Time.time * 1000f);
	}

	public void SetKartId(int kartId)
	{
	}

	public void RegistPanels(ref GUIPanelManager manager)
	{
		manager.RegistGUIInterface(this.panel_);
		manager.RegistGUIInterface(this.itemPanel_);
		manager.RegistGUIInterface(this.itemChangePanel_);
		manager.RegistGUIInterface(this.freezePanel_);
	}

	public override string ToString()
	{
		GUIPanelEx[] array = new GUIPanelEx[] { this.panel_, this.itemPanel_, this.itemChangePanel_, this.freezePanel_ };
		string text = string.Empty;
		for (int i = 0; i < 4; i++)
		{
			text += ((array[i] == null) ? (i.ToString() + "is null\n") : (array[i].ToString() + "\n"));
		}
		return text;
	}

	public static int MAX_SLOT = 3;

	private ItemSlot.ItemSlotSize slotSize_;

	private int itemidx_;

	private bool isChangeItemMode_;

	private int itemChangeidx_;

	private int freezeTick_;

	private ItemSlot.FreezeState freezeState_;

	private int freezeCount_;

	private bool isMoveSlotMode_;

	private int moveSlotTimeLength_;

	private int moveBlinkSlotTimeLength_;

	private int startMoveSlotTick_;

	private Vector2 moveSrcPos_;

	private Vector2 moveDstPos_;

	private Vector2 moveSrcSize_;

	private Vector2 moveDstSize_;

	private Vector2 moveItemSrcPos_;

	private Vector2 moveItemDstPos_;

	private Vector2 moveItemSrcSize_;

	private Vector2 moveItemDstSize_;

	private GUIPanelEx panel_;

	private GUIPanelEx itemPanel_;

	private GUIPanelEx itemChangePanel_;

	private GUIPanelEx freezePanel_;

	private GUIAtlas atlas_;

	public enum ItemSlotSize
	{
		SMALL,
		NORMAL,
		BIG
	}

	public enum FreezeState
	{
		UNFREEZE,
		FREEZING,
		FREEZED,
		UNFREEZING
	}

	private enum PanelInfoEnum
	{
		MAIN,
		FREEZE,
		ITEM,
		CHANGE
	}
}
