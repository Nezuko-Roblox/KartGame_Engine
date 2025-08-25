using System;

public interface PacketHandler
{
	bool ProcessPacket(byte[] packet, string senderID);

	bool ProcessPacket(Packet packet, string senderID);

	void TerminationEvent();

	void ConnectionFailed(string serverID);
}
