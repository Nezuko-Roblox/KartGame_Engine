using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUIRankingListInSingle : GUIListCtrl
{
	protected override int GetCompareIndex()
	{
		return 1;
	}

	protected override Rect GetAvailableRegion()
	{
		return GUIBase.ConvertWSToUS(18f, 222f, 452f, 474f);
	}

	protected override void InitializeListctrl()
	{
		for (int i = 0; i < 10; i++)
		{
			this.items_[i] = new GUIRankingListItem(i, this.meshRenderer_, new Vector2(0f, (float)i * this.DISTANCE), this);
			this.items_[i].RegistPanelManager(this.panelManager_);
		}
		for (int j = 0; j < 2; j++)
		{
			Vector2 vector = new Vector2(0f, (j != 0) ? (10f * this.DISTANCE) : (-this.DISTANCE));
			this.updownItems_[j] = new GUIUpDownListItem(0, this.meshRenderer_, vector, this);
			this.updownItems_[j].SetData(j == 0);
			this.updownItems_[j].RegistMouseManager(this.mouseManager_);
			this.updownItems_[j].RegistPanelManager(this.panelManager_);
		}
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		int num = 0;
		float[] array = new float[] { 18f, 222f, 452f, 0f, 560f, 2f, 607f };
		array[3] = 222f + this.CELL_HEIGHT;
		this.playerRankingBar_ = new GUIPanelEx3PartHorz(num, array, fiaTexture, 5, GUIFontCalculator.DEFAULT_GAP, 3f);
		this.panelManager_.RegistGUIInterface(this.playerRankingBar_);
		this.UpdateListItemData();
	}

	private void OnImageUpdateSuccessWith(string fbid_)
	{
		string text = FiaUtil.docPath;
		text = Path.Combine(text, "fbImage");
		string text2 = Path.Combine(text, "_" + fbid_ + ".png");
		text = Path.Combine(text, fbid_ + ".png");
		if (File.Exists(text))
		{
			File.Delete(text);
		}
		File.Copy(text2, text);
		FiaCoroutine fiaCoroutine = new FiaCoroutine(this.ReadImageWith(fbid_));
		base.StartCoroutine(fiaCoroutine);
	}

	private void OnImageUpdateFailure(Exception ex)
	{
	}

	private IEnumerator ReadImageWith(string fbid_)
	{
		string path_ = FiaUtil.docPath;
		path_ = Path.Combine(path_, "fbImage");
		path_ = Path.Combine(path_, fbid_ + ".png");
		path_ = "file://" + path_;
		WWW www_ = new WWW(path_);
		yield return www_;
		if (www_.isDone)
		{
			if (www_.isDone)
			{
			}
		}
		if (this.records_ != null && www_.isDone)
		{
			for (int i = 0; i < this.records_.Count; i++)
			{
				if (this.records_[i].fbid_ == fbid_)
				{
					if (i >= this.startIdx_ && i < this.startIdx_ + 10)
					{
						this.fbImageGameObjects_[i - this.startIdx_].GetComponent<MeshRenderer>().material.mainTexture = www_.texture;
					}
					break;
				}
			}
		}
		yield break;
	}

	protected override void GetMinMaxRange(out float minRange, out float maxRange)
	{
		int num = ((this.records_ == null) ? 0 : ((this.records_.Count - this.startIdx_ >= 10) ? 10 : (this.records_.Count - this.startIdx_)));
		maxRange = 222f + (float)(num + ((!this.updownItems_[1].Enable) ? 0 : 1)) * this.DISTANCE - (float)Screen.height;
		if (maxRange <= 0f)
		{
			maxRange = 0f;
		}
		minRange = ((!this.updownItems_[0].Enable) ? 0f : (-this.DISTANCE));
	}

	private void UpdateListItemData()
	{
		int num = ((this.records_ != null) ? this.records_.Count : 0);
		this.updownItems_[0].Enable = this.startIdx_ > 0;
		this.updownItems_[1].Enable = this.startIdx_ + 10 < num;
		this.playerRankingBar_.Visible = false;
		for (int i = 0; i < 10; i++)
		{
			this.fbImageGameObjects_[i].SetActiveRecursively(false);
			if (this.records_ != null && this.startIdx_ + i < this.records_.Count)
			{
				if (this.startIdx_ + i == this.playerRank_ - 1)
				{
					this.playerRankingBar_.SetRectByWindowSpace(18f, 222f + (float)i * this.DISTANCE);
					this.playerRankingBar_.Visible = true;
				}
				Record record = this.records_[this.startIdx_ + i];
				string fbid_ = record.fbid_;
				string text = FiaUtil.docPath;
				text = Path.Combine(text, "fbImage");
				text = Path.Combine(text, fbid_ + ".png");
				if (!File.Exists(text))
				{
					this.fbImageGameObjects_[i].GetComponent<MeshRenderer>().material.mainTexture = this.defaultImage_;
				}
				else
				{
					FiaCoroutine fiaCoroutine = new FiaCoroutine(this.ReadImageWith(fbid_));
					base.StartCoroutine(fiaCoroutine);
				}
				string text2 = Facebook.Inst.PictureURLForID(fbid_);
				FBProfileImageUpdater fbprofileImageUpdater = new FBProfileImageUpdater("_" + fbid_ + ".png", text2);
				FiaCoroutine fiaCoroutine2 = new FiaCoroutine(fbprofileImageUpdater.Update(), new OnSuccessWith(this.OnImageUpdateSuccessWith), fbid_, new OnFailure(this.OnImageUpdateFailure));
				base.StartCoroutine(fiaCoroutine2);
				this.items_[i].SetData(record.fbname_, 0, record.time_, this.startIdx_ + i + 1);
				this.fbImageGameObjects_[i].SetActiveRecursively(true);
			}
		}
	}

	protected override ScrollBarInfo GetScrollbarInfo()
	{
		ScrollBarInfo scrollBarInfo = new ScrollBarInfo();
		scrollBarInfo.material_ = this.meshRenderer_.materials[0];
		scrollBarInfo.texture_ = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		scrollBarInfo.scrollbarInfo_ = new float[] { 461f, 194f, 468f, 474f, 560f, 95f, 116f };
		scrollBarInfo.viewRegion_ = this.GetAvailableRegion();
		float num;
		float num2;
		this.GetMinMaxRange(out num, out num2);
		scrollBarInfo.listRegion_ = new Rect(0f, 0f, 0f, num2 + scrollBarInfo.viewRegion_.height - num);
		return scrollBarInfo;
	}

	public override void DoInit()
	{
		this.RegistMonoBehaviour(1027);
		base.gameObject.SetActiveRecursively(false);
		GameObject gameObject = base.gameObject;
		this.fbImageGameObjects_ = new GameObject[10];
		for (int i = 0; i < 10; i++)
		{
			this.fbImageGameObjects_[i] = (GameObject)global::UnityEngine.Object.Instantiate(this.fbImagePrefab_);
			FiaUtil.AttachChild(ref gameObject, ref this.fbImageGameObjects_[i]);
			GUIImage component = this.fbImageGameObjects_[i].GetComponent<GUIImage>();
			component.Texture = new Rect(0f, 0f, 64f, 64f);
			component.Rect = new Rect(24f, 223f + (float)i * this.DISTANCE, 54f, 54f);
			component.Layer = 5;
			this.fbImageGameObjects_[i].SetActiveRecursively(false);
		}
		base.DoInit();
	}

	protected override void BeforePanelUpdate()
	{
		if (this.isMoveToTrack_ || this.isReturnToRanking_)
		{
			return;
		}
		base.BeforePanelUpdate();
		for (int i = 0; i < 10; i++)
		{
			if (this.items_[i] != null)
			{
				this.items_[i].Update();
			}
		}
		int pushedKey = this.mouseManager_.GetPushedKey();
		this.updownItems_[0].SetUV((pushedKey != this.updownItems_[0].Id) ? 2 : 3);
		this.updownItems_[1].SetUV((pushedKey != this.updownItems_[1].Id) ? 0 : 1);
	}

	protected void FixedUpdate()
	{
		this.isMoveToTrack_ = false;
		this.isReturnToRanking_ = false;
	}

	protected override void AfterPanelUpdate()
	{
		if (this.isMoveToTrack_ || this.isReturnToRanking_)
		{
			return;
		}
		int selectedKey = this.mouseManager_.GetSelectedKey();
		if (selectedKey != -1)
		{
			StageController.Instance.PlaySound(StageController.FxType.SELECT);
			if (selectedKey == this.updownItems_[0].Id)
			{
				this.startIdx_ -= 10;
			}
			else if (selectedKey == this.updownItems_[1].Id)
			{
				this.startIdx_ += 10;
			}
			this.UpdateListItemData();
			base.Recalculate();
			base.ResetLocalPosition(0f);
		}
	}

	public override void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(senderId, msg);
		if (msg.type_ == MonoBehaviourMessageType.SHOW_RANKING)
		{
			MonoBehaviourMessage2Param<int, RankingType> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, RankingType>)msg;
			if (monoBehaviourMessage2Param != null)
			{
				this.trackIdx_ = monoBehaviourMessage2Param.lparam_;
				RankingType rparam_ = monoBehaviourMessage2Param.rparam_;
				GameMode gameMode_ = KartManager.Instance.parameter_.gameMode_;
				int num = ((gameMode_ != GameMode.SINGLE_SPEED) ? 1 : 0);
				Ranking ranking = null;
				if (rparam_ == RankingType.ALL_TIME)
				{
					ranking = new AllTimeRanking(Facebook.Inst.FBID, gameMode_, this.trackIdx_);
				}
				else if (rparam_ == RankingType.THIS_MONTH)
				{
					ranking = new MonthlyRanking(Facebook.Inst.FBID, gameMode_, this.trackIdx_);
				}
				this.playerRank_ = 0;
				if (ranking != null)
				{
					this.records_ = ((!ranking.Load()) ? null : ranking.records_);
					if (this.records_ != null)
					{
						for (int i = 0; i < this.records_.Count; i++)
						{
							Record record = this.records_[i];
							if (record.fbid_.CompareTo(Facebook.Inst.FBID) == 0)
							{
								this.playerRank_ = i + 1;
							}
						}
					}
				}
				else
				{
					this.records_ = null;
				}
				this.startIdx_ = ((this.playerRank_ >= 1) ? ((this.playerRank_ - 1) / 10 * 10) : 0);
				this.UpdateListItemData();
				base.Recalculate();
				base.ResetLocalPosition((this.playerRank_ >= 1) ? ((float)(this.playerRank_ - 1 - this.startIdx_) * this.DISTANCE) : 0f);
				this.isReturnToRanking_ = true;
				if (this.panelManager_.Update())
				{
					this.panelManager_.UpdateMesh(ref this.mesh_);
				}
			}
		}
	}

	private const int RANKING_COUNT = 10;

	public GameObject fbImagePrefab_;

	private GameObject[] fbImageGameObjects_;

	public Texture2D defaultImage_;

	private GUIRankingListItem[] items_ = new GUIRankingListItem[10];

	private GUIUpDownListItem[] updownItems_ = new GUIUpDownListItem[2];

	private GUIPanelEx3PartHorz playerRankingBar_;

	protected int playerRank_ = -1;

	public int selectedRanking_ = -1;

	protected int trackIdx_ = -1;

	public int startIdx_;

	private bool isMoveToTrack_;

	private bool isReturnToRanking_;

	private List<Record> records_;

	private float DISTANCE = 56f;

	private float CELL_HEIGHT = 56f;
}
