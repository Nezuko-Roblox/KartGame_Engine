using System;
using System.Collections.Generic;
using System.IO;

public class GameResultPacket : Packet
{
	public GameResultPacket()
	{
	}

	public GameResultPacket(IEnumerable<Slot> slots)
	{
		this.players_ = new List<PlayerResultPacket>();
		foreach (Slot slot in slots)
		{
			PlayerResultPacket playerResultPacket = new PlayerResultPacket();
			playerResultPacket.raceEndTime_ = slot.raceEndTime_;
			playerResultPacket.index_ = slot.index_;
			playerResultPacket.id_ = slot.id_;
			this.players_.Add(playerResultPacket);
		}
		this.players_.Sort(new GameResultPacket.sortByRaceEndTime());
		IEnumerator<PlayerResultPacket> enumerator2 = this.players_.GetEnumerator();
		for (int i = 0; i < this.players_.Count; i++)
		{
			enumerator2.MoveNext();
			enumerator2.Current.rank_ = i;
		}
	}

	public RaceResult ToRaceResult()
	{
		RaceResult raceResult = new RaceResult();
		for (int i = 0; i < this.players_.Count; i++)
		{
			raceResult.SetResult(this.players_[i].index_, this.players_[i].rank_, this.players_[i].raceEndTime_);
		}
		return raceResult;
	}

	public override void WriteTo(BinaryWriter writer)
	{
		base.WriteTo(writer);
		writer.Write((byte)this.players_.Count);
		foreach (PlayerResultPacket playerResultPacket in this.players_)
		{
			playerResultPacket.WriteTo(writer);
		}
	}

	public override void ReadFrom(BinaryReader reader)
	{
		base.ReadFrom(reader);
		int num = (int)reader.ReadByte();
		this.players_ = new List<PlayerResultPacket>(num);
		for (int i = 0; i < num; i++)
		{
			PlayerResultPacket playerResultPacket = new PlayerResultPacket();
			playerResultPacket.ReadFrom(reader);
			this.players_.Add(playerResultPacket);
		}
	}

	public override string ToString()
	{
		string text = "<GameResultPacket> players_ = [";
		foreach (PlayerResultPacket playerResultPacket in this.players_)
		{
			text += playerResultPacket;
		}
		text += "]";
		return text;
	}

	public List<PlayerResultPacket> players_;

	private class sortByRaceEndTime : Comparer<PlayerResultPacket>
	{
		public override int Compare(PlayerResultPacket x, PlayerResultPacket y)
		{
			if (x.raceEndTime_ == 0f)
			{
				return 1;
			}
			if (y.raceEndTime_ == 0f)
			{
				return -1;
			}
			return (x.raceEndTime_ >= y.raceEndTime_) ? 1 : (-1);
		}
	}
}
