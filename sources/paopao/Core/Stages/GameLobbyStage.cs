using System;
using UnityEngine;

public class GameLobbyStage : MonoBehaviour, NetStage
{
	public void Awake()
	{
		int num = (int)(global::UnityEngine.Random.value * (float)(this.DEVELOPER_NAME.Length - 1));
		this.userName_ = this.DEVELOPER_NAME[num];
		NetworkManager.Inst.Stage = this;
		if (Env.IsDesktop)
		{
			NetworkManager.Inst.Session = new MockSession(this.userName_, SessionMode.CLIENT);
		}
		else if (Env.IsAndroid)
		{
			NetworkManager.Inst.Session = new AndroidSession(this.userName_, SessionMode.CLIENT);
		}
		else
		{
			NetworkManager.Inst.Session = new Session(this.userName_, SessionMode.CLIENT);
		}
		this.SetupButtons();
	}

	private void SetupButtons()
	{
		if (Env.IsIPhoneRes)
		{
			this.backButton_ = new Rect(25f, 20f, 50f, 25f);
		}
		else if (Env.IsIPadRes)
		{
			this.backButton_ = new Rect(131f, 184f, 250f, 50f);
		}
	}

	private void CreateBackButton()
	{
		if (GUI.Button(this.backButton_, "Back"))
		{
			NetworkManager.Inst.Stage = null;
			Application.LoadLevel("track_loader_demo");
		}
	}

	public void OnGUI()
	{
		switch (NetworkManager.Inst.Session.State)
		{
		case SessionState.AVAILABLE:
		{
			this.CreateBackButton();
			if (Time.time > this.lastServerCheck_ + 1f)
			{
				this.servers_ = NetworkManager.Inst.Session.AvailableServers();
				this.lastServerCheck_ = Time.time;
			}
			for (int i = 0; i < this.servers_.Length; i++)
			{
				if (GUI.Button(new Rect(40f, (float)(100 + 40 * i), 200f, 30f), this.servers_[i].name_))
				{
					NetworkManager.Inst.Session.Connect(this.servers_[i].id_);
				}
			}
			if (GUI.Button(new Rect(285f, 130f, 140f, 100f), "Host Game"))
			{
				NetworkManager.Inst.Session.Close();
				if (Env.IsDesktop)
				{
					NetworkManager.Inst.Session = new MockSession(this.userName_, SessionMode.SERVER);
				}
				else if (Env.IsAndroid)
				{
					NetworkManager.Inst.Session = new AndroidSession(this.userName_, SessionMode.SERVER);
				}
				else
				{
					NetworkManager.Inst.Session = new Session(this.userName_, SessionMode.SERVER);
				}
				NetworkManager.Inst.Stage = null;
				Application.LoadLevel("game_ready_stage");
			}
			break;
		}
		case SessionState.CONNECTED:
			NetworkManager.Inst.Stage = null;
			Application.LoadLevel("game_ready_stage");
			break;
		}
	}

	public void ConnectionFailed(string serverID)
	{
		iOSEvent.Alert("?몄뒪?몄뿉 ?곌껐?????놁뒿?덈떎. ?몄썝??媛\u0080??李?諛⑹씠??寃뚯엫 以묒씤 諛⑹뿉???ㅼ뼱媛????놁뒿?덈떎.");
	}

	public bool ProcessPacket(Packet packet, string senderID)
	{
		return false;
	}

	private const string DEFAULT_USER_NAME = "Lupin";

	private const float SERVER_CHECK_RATE = 1f;

	private Peer[] servers_ = new Peer[0];

	private string userName_ = "Lupin";

	private float lastServerCheck_;

	private Rect backButton_;

	private string[] DEVELOPER_NAME = new string[] { "Han Jeongmin", "Ahn Minu", "Han Daehoon", "Jung Sehoon", "Park Jiyoung", "Ryu Jieun", "Kim Junghyun", "Lee Seungchan" };
}
