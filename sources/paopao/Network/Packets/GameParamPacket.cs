using System;
using System.Collections.Generic;
using System.IO;

public class GameParamPacket : Packet
{
	public override void WriteTo(BinaryWriter writer)
	{
		base.WriteTo(writer);
		writer.Write(this.players_.Count);
		foreach (PlayerPacket playerPacket in this.players_)
		{
			playerPacket.WriteTo(writer);
		}
		writer.Write((byte)this.gameMode_);
		writer.Write(this.selectedTrack_);
		writer.Write(this.availableTrack_);
		writer.Write(this.startRace_);
		writer.Write(this.startPositions_.Count);
		foreach (int num in this.startPositions_)
		{
			writer.Write((byte)num);
		}
	}

	public override void ReadFrom(BinaryReader reader)
	{
		base.ReadFrom(reader);
		int num = reader.ReadInt32();
		this.players_ = new List<PlayerPacket>(num);
		for (int i = 0; i < num; i++)
		{
			PlayerPacket playerPacket = new PlayerPacket();
			playerPacket.ReadFrom(reader);
			this.players_.Add(playerPacket);
		}
		this.gameMode_ = (GameMode)reader.ReadByte();
		this.selectedTrack_ = reader.ReadSByte();
		this.availableTrack_ = reader.ReadByte();
		this.startRace_ = reader.ReadBoolean();
		int num2 = reader.ReadInt32();
		this.startPositions_ = new List<int>(num2);
		for (int j = 0; j < this.startPositions_.Capacity; j++)
		{
			this.startPositions_.Add((int)reader.ReadByte());
		}
	}

	public override string ToString()
	{
		string text = "<GameParamPacket> players_ = [";
		foreach (PlayerPacket playerPacket in this.players_)
		{
			text += playerPacket;
		}
		text += "]";
		string text2 = string.Empty;
		foreach (int num in this.startPositions_)
		{
			text2 = text2 + num.ToString() + ",";
		}
		return text + string.Format(" recvTick={0}, gameMode={1}, track={2}/{3}, startRace={4}, startPositions={5}", new object[] { this.recvTick_, this.gameMode_, this.selectedTrack_, this.availableTrack_, this.startRace_, text2 });
	}

	public List<PlayerPacket> players_;

	public GameMode gameMode_;

	public sbyte selectedTrack_;

	public byte availableTrack_;

	public bool startRace_;

	public List<int> startPositions_;
}
