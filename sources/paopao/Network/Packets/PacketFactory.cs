using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PacketFactory
{
	private PacketFactory()
	{
		for (int i = 0; i < this.packetTypes_.Length; i++)
		{
			this.stampToTypeMap_.Add(i, this.packetTypes_[i]);
			this.typeToStampMap_.Add(this.packetTypes_[i], i);
		}
	}

	public static PacketFactory Inst
	{
		get
		{
			if (PacketFactory.inst_ == null)
			{
				PacketFactory.inst_ = new PacketFactory();
			}
			return PacketFactory.inst_;
		}
	}

	public Packet Deserialize(byte[] bytes)
	{
		Packet packet2;
		using (MemoryStream memoryStream = new MemoryStream(bytes))
		{
			using (BinaryReader binaryReader = new BinaryReader(memoryStream))
			{
				try
				{
					binaryReader.ReadByte();
					Packet packet = (Packet)Activator.CreateInstance(this.stampToTypeMap_[(int)binaryReader.ReadByte()]);
					packet.ReadFrom(binaryReader);
					packet.recvTick_ = Time.time;
					packet2 = packet;
				}
				catch (Exception ex)
				{
					packet2 = null;
				}
			}
		}
		return packet2;
	}

	public byte[] Serialize(Packet packet, SendDataMode mode)
	{
		byte[] array;
		using (MemoryStream memoryStream = new MemoryStream(128))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				if (!this.typeToStampMap_.ContainsKey(packet.GetType()))
				{
					throw new ArgumentException(string.Format("{0} is not a valid packet type", packet.GetType()));
				}
				binaryWriter.Write((byte)mode);
				binaryWriter.Write((byte)this.typeToStampMap_[packet.GetType()]);
				packet.WriteTo(binaryWriter);
				binaryWriter.Flush();
				array = memoryStream.ToArray();
			}
		}
		return array;
	}

	private static PacketFactory inst_;

	private Type[] packetTypes_ = new Type[]
	{
		typeof(GameKartPacket),
		typeof(TimeSyncPacket),
		typeof(GameControlPacket),
		typeof(PlayerPacket),
		typeof(GameParamPacket),
		typeof(PlayerResultPacket),
		typeof(GameResultPacket),
		typeof(UserLeaveNoticePacket),
		typeof(ItemPacket),
		typeof(GuardEffectPacket),
		typeof(ShieldEffectPacket),
		typeof(DestroyBananaPacket),
		typeof(AppVersionPacket),
		typeof(ItemSuccessPacket)
	};

	private Dictionary<int, Type> stampToTypeMap_ = new Dictionary<int, Type>();

	private Dictionary<Type, int> typeToStampMap_ = new Dictionary<Type, int>();
}
