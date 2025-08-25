using System;

public interface NetStage
{
	bool ProcessPacket(Packet packet, string senderID);

	void ConnectionFailed(string serverID);
}
