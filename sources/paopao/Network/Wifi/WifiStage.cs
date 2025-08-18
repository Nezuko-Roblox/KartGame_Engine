using System;
using UnityEngine;

public class WifiStage : MonoBehaviourStage, NetStage
{
	protected override void Start()
	{
		this.joinMsg_ = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.NETWORK_JOIN_MESSAGE);
		base.Start();
		this.RegistMonoBehaviour(514);
		NetworkManager.Inst.Stage = this;
		this.simpleMessage_ = (MonoBehaviourMessage2Param<int, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SIMPLE_MESSAGE);
		BackgroundNotifier.resign_ = new BackgroundDelegate(this.ResignedActive);
		BackgroundNotifier.active_ = new BackgroundDelegate(this.BecameActive);
		this.ResetNetwork();
	}

	private void ResetNetwork()
	{
		MonoBehaviourExCenter.Instance.SendMessage(0, 22, this.joinMsg_.Initialize(0));
		this.CreateSession();
		this.clientProgress_ = WaitForConnection.NOT_CONNECT;
		StageController.Instance.InputAutority = byte.MaxValue;
		LastReceivedPacket.gameParam_ = null;
		this.connection_ = null;
		base.CancelInvoke();
		this.ResetSelection();
		base.InvokeRepeating("UpdateRoomList", 1f, 0.5f);
	}

	private void CreateSession()
	{
		if (NetworkManager.Inst.Session != null)
		{
			NetworkManager.Inst.Session.Close();
		}
		string text = KartOptions.Instance.LastUserName;
		text = text.Replace("=", "..");
		if (Env.IsDesktop)
		{
			NetworkManager.Inst.Session = new MockSession(text, SessionMode.CLIENT);
		}
		else if (Env.IsAndroid)
		{
			NetworkManager.Inst.Session = new AndroidSession(text, SessionMode.CLIENT);
		}
		else
		{
			NetworkManager.Inst.Session = new Session(text, SessionMode.CLIENT);
		}
	}

	private void Update()
	{
		if (this.clientProgress_ == WaitForConnection.CHECKED_MULTIPLAYER_VERSION && NetworkManager.Inst.Session.State == SessionState.CONNECTED)
		{
			this.clientProgress_ = WaitForConnection.CONNECTED;
			MonoBehaviourExCenter.Instance.SendMessage(0, 22, this.joinMsg_.Initialize(3));
			PlayerPacket playerPacket = new PlayerPacket();
			playerPacket.id_ = NetworkManager.Inst.Session.ID;
			playerPacket.name_ = NetworkManager.Inst.Session.Name;
			playerPacket.characterName_ = (byte)KartOptions.Instance.Character;
			playerPacket.kartName_ = (byte)KartOptions.Instance.Kart;
			playerPacket.ready_ = false;
			NetworkManager.Inst.SendPacketToServer(playerPacket, SendDataMode.RELIABLE);
		}
	}

	private void UpdateRoomList()
	{
		if (this.clientProgress_ != WaitForConnection.NOT_CONNECT || StageController.Instance.IsWaitForReceiveReadyToChangeMessage)
		{
			return;
		}
		MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SIMPLE_MESSAGE);
		if (monoBehaviourMessage2Param != null)
		{
			base.SendMessage(514, monoBehaviourMessage2Param.Initialize(4, 0));
		}
	}

	public void ConnectionFailed(string serverID)
	{
		this.ConnectionFailed();
	}

	public void ConnectionFailed()
	{
		if (this.connection_ == null || this.connection_.tries_ > 1)
		{
			iOSEvent.Alert("들어갈 수 없는 방입니다. 새로 고침을 하고 다른 방을 선택해주세요.");
			this.ResetNetwork();
		}
		else
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("trying to connect again");
			}
			this.CreateSession();
			NetworkManager.Inst.Session.Connect(this.connection_.id_);
			this.connection_.tries_++;
			base.Invoke("ConnectionFailed", 10f);
		}
	}

	public bool ProcessPacket(Packet packet, string senderID)
	{
		if (packet.GetType() == typeof(AppVersionPacket))
		{
			AppVersionPacket appVersionPacket = (AppVersionPacket)packet;
			if (appVersionPacket.multiplayerVersion_ == KartOptions.Instance.MultiplayerVersion)
			{
				this.clientProgress_ = WaitForConnection.CHECKED_MULTIPLAYER_VERSION;
			}
			else
			{
				iOSEvent.Alert(string.Format("寃뚯엫 踰꾩쟾(v{0})???몄뒪?몄쓽 踰꾩쟾(v{1})怨??ㅻ쫭?덈떎.\n理쒖떊 踰꾩쟾?쇰줈 ?낅뜲?댄듃 ?섏꽭??", KartOptions.Instance.ProgramVersion, appVersionPacket.programVersion_));
				this.ResetNetwork();
			}
			return true;
		}
		if (packet.GetType() == typeof(GameParamPacket))
		{
			if (this.clientProgress_ == WaitForConnection.CONNECTED)
			{
				LastReceivedPacket.gameParam_ = (GameParamPacket)packet;
				StageController.Instance.ChangeStage(StageType.WAITROOM_CLIENT);
				return true;
			}
		}
		else if (packet.GetType() == typeof(UserLeaveNoticePacket))
		{
			this.ResetNetwork();
		}
		return false;
	}

	public override void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(senderId, msg);
		if (msg.type_ == MonoBehaviourMessageType.CHANGE_SCENE)
		{
			base.CancelInvoke();
			MonoBehaviourMessage1Param<StageType> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<StageType>)msg;
			if (monoBehaviourMessage1Param != null)
			{
				if (monoBehaviourMessage1Param.param_ == StageType.MAIN || monoBehaviourMessage1Param.param_ == StageType.GARAGE)
				{
					NetworkManager.Inst.Session.Close();
					base.ReadyToChangeScene();
				}
				else if (this.isHost_)
				{
					AssetSelection.selection_[2] = TrackAssetDefinitionManager.RANDOM_IDX_ARRAY[0];
					base.ReadyToChangeScene();
				}
				else
				{
					base.ReadyToChangeScene();
				}
				BackgroundNotifier.resign_ = null;
				BackgroundNotifier.active_ = null;
				NetworkManager.Inst.Stage = null;
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.SIMPLE_MESSAGE)
		{
			MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)msg;
			if (monoBehaviourMessage2Param != null)
			{
				if (monoBehaviourMessage2Param.lparam_ == 1)
				{
					base.CancelInvoke();
					if (NetworkManager.Inst.Session != null)
					{
						NetworkManager.Inst.Session.Close();
					}
					string text = KartOptions.Instance.LastUserName;
					text = text.Replace("=", "..");
					if (Env.IsDesktop)
					{
						NetworkManager.Inst.Session = new MockSession(text, SessionMode.SERVER);
					}
					else if (Env.IsAndroid)
					{
						NetworkManager.Inst.Session = new AndroidSession(text, SessionMode.SERVER);
					}
					else
					{
						NetworkManager.Inst.Session = new Session(text, SessionMode.SERVER);
					}
					this.isHost_ = true;
					StageController.Instance.ChangeStage(StageType.WAITROOM_HOST);
				}
				else if (monoBehaviourMessage2Param.lparam_ == 2)
				{
					if (this.clientProgress_ == WaitForConnection.NOT_CONNECT)
					{
						if (MathHelper.IsBetweenIE(this.selectedSession_, 0, this.servers_.Length))
						{
							this.connection_ = new PotentialConnection(this.servers_[this.selectedSession_].id_);
							NetworkManager.Inst.Session.Connect(this.connection_.id_);
							this.connection_.tries_ = 1;
							this.clientProgress_ = WaitForConnection.CONNECTING;
							StageController.Instance.InputAutority = 0;
							base.Invoke("ConnectionFailed", 20f);
							MonoBehaviourExCenter.Instance.SendMessage(0, 22, this.joinMsg_.Initialize(1));
						}
						else
						{
							this.ResetSelection();
						}
					}
				}
				else if (monoBehaviourMessage2Param.lparam_ == 3)
				{
					int rparam_ = monoBehaviourMessage2Param.rparam_;
					if (MathHelper.IsBetweenIE(rparam_, 0, this.servers_.Length))
					{
						this.selectedSession_ = rparam_;
					}
					else
					{
						this.ResetSelection();
					}
				}
				else if (monoBehaviourMessage2Param.lparam_ == 4 && Time.time > this.lastServerCheck_ + 0.1f)
				{
					string text2 = string.Empty;
					if (MathHelper.IsBetweenIE(this.selectedSession_, 0, this.servers_.Length))
					{
						text2 = this.servers_[this.selectedSession_].id_;
					}
					this.servers_ = NetworkManager.Inst.Session.AvailableServers();
					if (this.servers_ != null && this.servers_.Length > 0)
					{
						base.CancelInvoke("UpdateRoomList");
						base.InvokeRepeating("UpdateRoomList", 30f, 30f);
					}
					else
					{
						base.CancelInvoke("UpdateRoomList");
						base.InvokeRepeating("UpdateRoomList", 0.5f, 0.5f);
					}
					this.lastServerCheck_ = Time.time;
					this.selectedSession_ = -1;
					if (text2 != string.Empty)
					{
						for (int i = 0; i < this.servers_.Length; i++)
						{
							if (this.servers_[i].id_ == text2)
							{
								this.selectedSession_ = i;
								break;
							}
						}
					}
					this.UpdateListView(this.servers_);
				}
			}
		}
	}

	private void UpdateListView(Peer[] servers)
	{
		if (servers != null && servers.Length > 0)
		{
			if (!this.wifiRoomListController_.active)
			{
				this.wifiRoomListController_.SetActiveRecursively(true);
			}
			if (this.wifiRoomListEmptyController_.active)
			{
				this.wifiRoomListEmptyController_.SetActiveRecursively(false);
			}
			if (!this.refreshButton_.active)
			{
				this.refreshButton_.SetActiveRecursively(true);
			}
			MonoBehaviourMessage2Param<int, Peer[]> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, Peer[]>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.UPDATE_WIFI_ROOM_LIST);
			monoBehaviourMessage2Param.Initialize(this.selectedSession_, servers);
			base.SendMessage(1064, monoBehaviourMessage2Param);
			base.SendMessage(269, monoBehaviourMessage2Param);
		}
		else
		{
			if (!this.wifiRoomListEmptyController_.active)
			{
				this.wifiRoomListEmptyController_.SetActiveRecursively(true);
			}
			if (this.refreshButton_.active)
			{
				this.refreshButton_.SetActiveRecursively(false);
			}
			if (this.wifiRoomListController_.active)
			{
				MonoBehaviourMessage2Param<int, Peer[]> monoBehaviourMessage2Param2 = (MonoBehaviourMessage2Param<int, Peer[]>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.UPDATE_WIFI_ROOM_LIST);
				monoBehaviourMessage2Param2.Initialize(this.selectedSession_, servers);
				base.SendMessage(1064, monoBehaviourMessage2Param2);
				this.wifiRoomListController_.SetActiveRecursively(false);
			}
		}
	}

	private void ResetSelection()
	{
		this.selectedSession_ = -1;
		base.SendMessage(1064, this.simpleMessage_.Initialize(3, -1));
	}

	private void ResignedActive()
	{
		if (!Env.IsAndroid && NetworkManager.Inst.Session != null)
		{
			NetworkManager.Inst.Session.Close();
		}
	}

	private void BecameActive()
	{
		if (!Env.IsAndroid)
		{
			this.ResetNetwork();
		}
	}

	private const float SERVER_CHECK_RATE = 0.1f;

	private const float FAST_REFRESH_RATE = 0.5f;

	private const float SLOW_REFRESH_RATE = 30f;

	private MonoBehaviourMessage2Param<int, int> simpleMessage_;

	private Peer[] servers_ = new Peer[0];

	private float lastServerCheck_;

	private bool isHost_;

	private int selectedSession_ = -1;

	private PotentialConnection connection_;

	private WaitForConnection clientProgress_;

	private MonoBehaviourMessage1Param<int> joinMsg_;

	public GameObject wifiRoomListController_;

	public GameObject wifiRoomListEmptyController_;

	public GameObject refreshButton_;
}
