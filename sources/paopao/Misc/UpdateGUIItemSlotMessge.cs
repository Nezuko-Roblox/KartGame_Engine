using System;

public class UpdateGUIItemSlotMessge : MonoBehaviourMessage
{
	public UpdateGUIItemSlotMessge()
		: base(MonoBehaviourMessageType.UPDATE_ITEMSLOTS)
	{
	}

	public UpdateGUIItemSlotMessge Initialize(GameItem[] itemSlots)
	{
		for (int i = 0; i < 2; i++)
		{
			this.itemSlots_[i] = itemSlots[i];
		}
		return this;
	}

	public GameItem[] itemSlots_ = new GameItem[2];
}
