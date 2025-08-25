using System;
using UnityEngine;

public class AndroidNetwork
{
	public AndroidNetwork()
	{
		this.InitBluetoothNetwork();
		if (GameObject.Find("NetHelper") == null)
		{
			GameObject gameObject = new GameObject("NetHelper", new Type[] { typeof(AndroidNetHelper) });
			global::UnityEngine.Object.DontDestroyOnLoad(gameObject);
		}
	}

	public static AndroidNetwork instance
	{
		get
		{
			if (AndroidNetwork.instance_ == null)
			{
				AndroidNetwork.instance_ = new AndroidNetwork();
			}
			return AndroidNetwork.instance_;
		}
	}

	protected void InitBluetoothNetwork()
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nexon.kartriderrush.android.demo.Bluetooth.BTService");
		this.joBTService_ = androidJavaClass.GetStatic<AndroidJavaObject>("instance");
	}

	public void StartServer(string name, int maxPeers)
	{
		this.name_ = name;
		this.joBTService_.Call("setName", new object[] { name });
		this.joBTService_.Call("startDiscoverable", new object[0]);
		this.joBTService_.Call("start", new object[0]);
	}

	public void StartClient(string name, int maxPeers)
	{
		this.name_ = name;
		this.joBTService_.Call("start", new object[0]);
	}

	public void Close()
	{
		this.joBTService_.Call("stop", new object[0]);
	}

	public string GetID()
	{
		return this.joBTService_.Call<string>("getId", new object[0]);
	}

	public string Peers()
	{
		return this.joBTService_.Call<string>("getConnectedPeers", new object[0]);
	}

	public void SendPacket(byte[] packet, int size, string receiverID)
	{
		if (AndroidNetwork.send_packet)
		{
			this.joBTService_.Call("sendPacket", new object[]
			{
				packet,
				size,
				int.Parse(receiverID)
			});
		}
	}

	public void SendPacketToAll(byte[] packet, int size)
	{
		if (AndroidNetwork.send_packet)
		{
			this.joBTService_.Call("broadcast", new object[] { packet, size });
		}
	}

	public void RejectConnection()
	{
		this.joBTService_.Call("stopListen", new object[0]);
	}

	public void AcceptConnections()
	{
		this.joBTService_.Call("startListen", new object[0]);
	}

	public void Kick(string peerID)
	{
		this.joBTService_.Call("disconnect", new object[] { int.Parse(peerID) });
	}

	public string AvailableServers()
	{
		return this.joBTService_.Call<string>("getAvailableServers", new object[0]);
	}

	public void Connect(string peerID)
	{
		this.joBTService_.Call<bool>("connect", new object[] { int.Parse(peerID) });
	}

	private static AndroidNetwork instance_;

	private AndroidJavaObject joBTService_;

	private string name_;

	public static bool send_packet = true;
}
