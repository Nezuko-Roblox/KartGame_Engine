using System;
using System.IO;

public class AppVersionPacket : Packet
{
	public AppVersionPacket()
	{
	}

	public AppVersionPacket(string programVersion, int multiplayerVersion)
	{
		this.programVersion_ = programVersion;
		this.multiplayerVersion_ = multiplayerVersion;
	}

	public override void WriteTo(BinaryWriter writer)
	{
		writer.Write(this.programVersion_);
		writer.Write(this.multiplayerVersion_);
	}

	public override void ReadFrom(BinaryReader reader)
	{
		this.programVersion_ = reader.ReadString();
		this.multiplayerVersion_ = reader.ReadInt32();
	}

	public override string ToString()
	{
		return string.Format("<AppVersionPacket> programVersion_={0} multiplayerVersion_={1}", this.programVersion_, this.multiplayerVersion_);
	}

	public string programVersion_;

	public int multiplayerVersion_;
}
