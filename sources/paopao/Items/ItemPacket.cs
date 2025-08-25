using System;
using System.Collections.Generic;
using System.IO;

public class ItemPacket : Packet
{
	public ItemPacket()
	{
	}

	public ItemPacket(GameItem type, ItemParam param)
	{
		this.type_ = type;
		this.param_ = param;
	}

	private static void Initialize()
	{
		ItemPacket.stampToTypeMap_ = new Dictionary<int, Type>();
		ItemPacket.typeToStampMap_ = new Dictionary<Type, int>();
		for (int i = 0; i < ItemPacket.itemTypes_.Length; i++)
		{
			ItemPacket.stampToTypeMap_.Add(i, ItemPacket.itemTypes_[i]);
			ItemPacket.typeToStampMap_.Add(ItemPacket.itemTypes_[i], i);
		}
	}

	private static Type StampToType(int stamp)
	{
		if (ItemPacket.stampToTypeMap_ == null)
		{
			ItemPacket.Initialize();
		}
		return ItemPacket.stampToTypeMap_[stamp];
	}

	private static int TypeToStamp(Type type)
	{
		if (ItemPacket.typeToStampMap_ == null)
		{
			ItemPacket.Initialize();
		}
		return ItemPacket.typeToStampMap_[type];
	}

	public override void WriteTo(BinaryWriter writer)
	{
		base.WriteTo(writer);
		writer.Write((byte)this.type_);
		writer.Write((byte)ItemPacket.TypeToStamp(this.param_.GetType()));
		this.param_.WriteTo(writer);
	}

	public override void ReadFrom(BinaryReader reader)
	{
		base.ReadFrom(reader);
		this.type_ = (GameItem)reader.ReadByte();
		this.param_ = (ItemParam)Activator.CreateInstance(ItemPacket.StampToType((int)reader.ReadByte()));
		this.param_.ReadFrom(reader);
	}

	public MonoBehaviourMessage2Param<GameItem, ItemParam> ToItemMessage()
	{
		return ((MonoBehaviourMessage2Param<GameItem, ItemParam>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.ITEM)).Initialize(this.type_, this.param_);
	}

	public override string ToString()
	{
		return string.Format("<ItemPacket> type_={0}, param_={1}", this.type_, this.param_);
	}

	public GameItem type_;

	public ItemParam param_;

	private static Type[] itemTypes_ = new Type[]
	{
		typeof(ItemBananaParam),
		typeof(ItemUFOParam),
		typeof(ItemWaterFlyParam),
		typeof(ItemWaterBombParam),
		typeof(ItemWaterMissileParam),
		typeof(ItemFlipParam),
		typeof(ItemDevilParam)
	};

	private static Dictionary<int, Type> stampToTypeMap_;

	private static Dictionary<Type, int> typeToStampMap_;
}
