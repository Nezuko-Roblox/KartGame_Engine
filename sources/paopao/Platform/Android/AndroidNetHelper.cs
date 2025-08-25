using System;
using UnityEngine;

public class AndroidNetHelper : MonoBehaviour
{
	public void Start()
	{
	}

	public void ProcessPacket(string data)
	{
		string[] array = data.Split(new char[] { '|' });
		byte[] array2 = Convert.FromBase64String(array[1]);
		Session.inst_.PacketHandler.ProcessPacket(array2, array[0]);
	}

	public void PeerDisconnected(string peerID)
	{
		SessionMode sessionMode;
		if (Session.inst_.connectedServer_ == peerID)
		{
			sessionMode = SessionMode.SERVER;
		}
		else
		{
			sessionMode = SessionMode.CLIENT;
		}
		UserLeaveNoticePacket userLeaveNoticePacket = new UserLeaveNoticePacket(peerID, sessionMode);
		Session.inst_.PacketHandler.ProcessPacket(userLeaveNoticePacket, peerID);
	}

	public void TerminationEvent()
	{
		Session.inst_.PacketHandler.TerminationEvent();
	}

	public void WifiBluetoothOff()
	{
	}

	public void ConnectionFailed(string serverID)
	{
		Session.inst_.State = SessionState.AVAILABLE;
		Session.inst_.PacketHandler.ConnectionFailed(serverID);
	}

	public void ConnectionSuccess(string serverID)
	{
		Session.inst_.State = SessionState.CONNECTED;
	}
}
