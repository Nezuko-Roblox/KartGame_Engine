using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GoCourse
{
	public GoCourse()
	{
		this.sectionInfo_ = new SectionInfo[6];
		PassingLog.Instance.Clear();
	}

	public void SetKart(int kartIdx, GoKart kart)
	{
		if (this.sectionInfo_[kartIdx] != null)
		{
			Debug.LogError("Already Exist Section : " + kartIdx.ToString());
		}
		this.sectionInfo_[kartIdx] = new SectionInfo(kart, this.maxLap_);
	}

	public void ParsePlaneInfo(TextAsset planeInfo)
	{
		StringReader stringReader = new StringReader(planeInfo.text);
		if (stringReader != null)
		{
			string text = string.Empty;
			text = stringReader.ReadLine();
			int num = int.Parse(text);
			this.passPlane_ = new PassPlane[num];
			text = string.Empty;
			for (int i = 0; i < num; i++)
			{
				text = stringReader.ReadLine();
				if (text != null)
				{
					this.passPlane_[i] = new PassPlane(text);
				}
			}
		}
		stringReader.Close();
	}

	public void ParsePlaneInfo(BinaryAsset planeInfo)
	{
		BinaryReader binaryReader = new BinaryReader(new MemoryStream(planeInfo.content_));
		if (binaryReader != null)
		{
			int num = 0;
			int num2 = (int)binaryReader.BaseStream.Length;
			binaryReader.ReadInt16();
			num += 2;
			int num3 = (int)binaryReader.ReadInt16();
			num += 2;
			this.passPlane_ = new PassPlane[num3];
			for (int i = 0; i < num3; i++)
			{
				this.passPlane_[i] = new PassPlane(ref binaryReader);
				num += 72;
			}
			if (num != num2)
			{
			}
			binaryReader.Close();
		}
	}

	public void Initialize(TextAsset planeInfo)
	{
		this.ParsePlaneInfo(planeInfo);
		this.passPlaneSequence_ = new PassPlaneSequence(this.passPlane_.Length);
	}

	public void Initialize(BinaryAsset planeInfo)
	{
		this.ParsePlaneInfo(planeInfo);
		this.passPlaneSequence_ = new PassPlaneSequence(this.passPlane_.Length);
	}

	public void Initialize(TextAsset planeInfo, TextAsset sequenceInfo)
	{
		this.ParsePlaneInfo(planeInfo);
		this.passPlaneSequence_ = new PassPlaneSequence(sequenceInfo);
	}

	public void Initialize(BinaryAsset planeInfo, BinaryAsset sequenceInfo)
	{
		this.ParsePlaneInfo(planeInfo);
		this.passPlaneSequence_ = new PassPlaneSequence(sequenceInfo);
	}

	public void InitPassPlane()
	{
		this.passPlaneTrackDistanceDictionary = new float[this.passPlane_.Length, this.passPlane_.Length];
		this.passPlaneTrackDistance = new float[this.passPlane_.Length];
		for (int i = 0; i < this.passPlane_.Length; i++)
		{
			this.passPlaneTrackDistance[i] = -1f;
		}
		this.passPlaneTrackDistance[0] = 0f;
		this.InitPassPlane(0);
	}

	private void InitPassPlane(int currentPassPlane)
	{
		int[] next = this.passPlaneSequence_.GetNext(currentPassPlane);
		foreach (int num in next)
		{
			if (num < this.passPlane_.Length)
			{
				float num2 = Vector3.Distance(this.passPlane_[currentPassPlane].centerPos_, this.passPlane_[num].centerPos_);
				float num3 = this.passPlaneTrackDistance[currentPassPlane] + num2;
				this.passPlaneTrackDistanceDictionary[currentPassPlane, num] = num3;
				if (num != 0)
				{
					if (this.passPlaneTrackDistance[num] < 0f || this.passPlaneTrackDistance[num] < num3)
					{
						this.passPlaneTrackDistance[num] = num3;
					}
					this.InitPassPlane(num);
				}
			}
		}
	}

	public float GetPassPlaneTrackDistance(int passPlane)
	{
		return this.GetPassPlaneTrackDistance(passPlane, 1);
	}

	public float GetPassPlaneTrackDistance(int passPlane, int lapCount)
	{
		if (passPlane >= 0 && passPlane < this.passPlaneTrackDistance.Length)
		{
			return this.passPlaneTrackDistance[passPlane] + (float)(lapCount - 1) * this.passPlaneTrackDistance[this.passPlaneTrackDistance.Length - 1];
		}
		return 0f;
	}

	public float GetPassPlaneTrackDistanceByKartIndex(int kartIndex)
	{
		int lap = KartManager.Instance.goCourse_.GetLap(kartIndex);
		int distancePlane = KartManager.Instance.goCourse_.GetDistancePlane(kartIndex);
		return KartManager.Instance.goCourse_.GetDistance(kartIndex) + KartManager.Instance.goCourse_.GetPassPlaneTrackDistance(distancePlane, lap);
	}

	public List<int> GenerateStartInfos()
	{
		List<int> list;
		FiaUtil.GetRandomList(0, 5, out list);
		this.GenerateStartInfos(list);
		return list;
	}

	public void GenerateStartInfos(List<int> randomList)
	{
		this.startInfos_ = new RegenInfo[6];
		RegenInfo regenInfo = this.GetRegenInfo(0);
		RegenInfo regenInfo2 = this.GetRegenInfo(this.passPlane_.Length - 1);
		Vector3 vector = Vector3.Cross(regenInfo.direction_, Vector3.up);
		float num = ((randomList.Count <= 4) ? 4f : 2.6f);
		Vector3 vector2 = (regenInfo.position_ + regenInfo2.position_) / 2f + vector * num * -((float)randomList.Count * 0.5f - 0.5f);
		int num2 = 0;
		foreach (int num3 in randomList)
		{
			this.startInfos_[num2] = new RegenInfo();
			this.startInfos_[num2].direction_ = regenInfo.direction_;
			this.startInfos_[num2].position_ = vector2 + (float)num3 * num * vector;
			num2++;
		}
	}

	public RegenInfo GetStartInfo(int idx)
	{
		if (this.startInfos_ == null)
		{
			this.GenerateStartInfos();
		}
		return this.startInfos_[idx];
	}

	public RegenInfo GetRegenInfo(int passPlaneIdx)
	{
		if (passPlaneIdx < 0 || passPlaneIdx > this.passPlane_.Length - 1)
		{
			passPlaneIdx = 0;
		}
		RegenInfo regenInfo = new RegenInfo();
		regenInfo.direction_ = this.passPlane_[passPlaneIdx].normal_;
		regenInfo.position_ = this.passPlane_[passPlaneIdx].GetRegenPos();
		RegenInfo regenInfo2 = regenInfo;
		regenInfo2.position_.y = regenInfo2.position_.y + 0.01f;
		return regenInfo;
	}

	public void Update()
	{
		for (int i = 0; i < 6; i++)
		{
			if (this.sectionInfo_[i] != null)
			{
				if (!this.IsKartGoalIn(i))
				{
					GameObject kart = this.sectionInfo_[i].kart_.m_kart;
					Vector3 position = kart.transform.position;
					if (this.sectionInfo_[i].lastKartPos_ == Vector3.zero)
					{
						this.sectionInfo_[i].lastKartPos_ = position;
					}
					if (Vector3.Distance(this.sectionInfo_[i].lastKartPos_, position) >= 0.5f)
					{
						bool flag = false;
						int latestPassingPlane_ = this.sectionInfo_[i].latestPassingPlane_;
						int[] array;
						if (latestPassingPlane_ == -1 || this.sectionInfo_[i].passCorrect_)
						{
							array = this.passPlaneSequence_.GetNext(latestPassingPlane_);
						}
						else
						{
							array = this.passPlaneSequence_.GetNext(this.passPlaneSequence_.GetPrev(latestPassingPlane_)[0]);
						}
						int j = 0;
						while (j < array.Length)
						{
							int num = array[j];
							int num2 = this.CheckPlanePassing(this.passPlane_[num], this.sectionInfo_[i].lastKartPos_, position);
							if (num2 > 0)
							{
								this.sectionInfo_[i].latestPassingPlane_ = num;
								this.sectionInfo_[i].passCorrect_ = true;
								if (this.passPlaneSequence_.IsNextPlane(this.sectionInfo_[i].lastCorrectDirectionPassingPlane_, this.sectionInfo_[i].latestPassingPlane_) || this.passPlaneSequence_.IsExclusivePlane(this.sectionInfo_[i].lastCorrectDirectionPassingPlane_, this.sectionInfo_[i].latestPassingPlane_))
								{
									if (this.sectionInfo_[i].lap_ == 0 && this.passPlaneSequence_.IsFirstPlane(this.sectionInfo_[i].latestPassingPlane_))
									{
										this.sectionInfo_[i].lap_++;
										this.sectionInfo_[i].lastCorrectDirectionPassingPlane_ = this.sectionInfo_[i].latestPassingPlane_;
									}
									else
									{
										this.sectionInfo_[i].lastCorrectDirectionPassingPlane_ = this.sectionInfo_[i].latestPassingPlane_;
										if (this.passPlaneSequence_.IsLastPlane(this.sectionInfo_[i].lastCorrectDirectionPassingPlane_))
										{
											this.sectionInfo_[i].lapTime_[this.sectionInfo_[i].lap_ - 1] = KartManager.Instance.GetPlayTime();
											this.sectionInfo_[i].lap_++;
											this.sectionInfo_[i].lastCorrectDirectionPassingPlane_ = -1;
										}
									}
								}
								array = this.passPlaneSequence_.GetNext(num);
								j = 0;
								flag = true;
							}
							else
							{
								j++;
							}
							if (num2 != 0 && i == KartManager.PLAYER_KART_IDX)
							{
								PassingLogElem passingLogElem = new PassingLogElem();
								passingLogElem.pos1_ = this.sectionInfo_[i].lastKartPos_;
								passingLogElem.pos2_ = position;
								passingLogElem.passingIndex_ = num;
								passingLogElem.isCorrect_ = this.sectionInfo_[i].passCorrect_;
								passingLogElem.resultPassing_ = num2;
								PassingLog.Instance.AddElem(passingLogElem);
							}
						}
						if (!flag)
						{
							int latestPassingPlane_2 = this.sectionInfo_[i].latestPassingPlane_;
							int[] array2;
							if (latestPassingPlane_2 == -1 || !this.sectionInfo_[i].passCorrect_)
							{
								array2 = this.passPlaneSequence_.GetPrev(latestPassingPlane_2);
							}
							else
							{
								array2 = this.passPlaneSequence_.GetPrev(this.passPlaneSequence_.GetNext(latestPassingPlane_2)[0]);
							}
							j = 0;
							while (j < array2.Length)
							{
								int num3 = array2[j];
								int num4 = this.CheckPlanePassing(this.passPlane_[num3], this.sectionInfo_[i].lastKartPos_, position);
								if (num4 < 0)
								{
									this.sectionInfo_[i].latestPassingPlane_ = num3;
									this.sectionInfo_[i].passCorrect_ = false;
									array2 = this.passPlaneSequence_.GetPrev(num3);
									j = 0;
								}
								else
								{
									j++;
								}
								if (num4 != 0 && i == KartManager.PLAYER_KART_IDX)
								{
									PassingLogElem passingLogElem2 = new PassingLogElem();
									passingLogElem2.pos1_ = this.sectionInfo_[i].lastKartPos_;
									passingLogElem2.pos2_ = position;
									passingLogElem2.passingIndex_ = num3;
									passingLogElem2.isCorrect_ = this.sectionInfo_[i].passCorrect_;
									passingLogElem2.resultPassing_ = num4;
									PassingLog.Instance.AddElem(passingLogElem2);
								}
							}
						}
						PlayerType type_ = KartManager.Instance.parameter_.kart_[i].type_;
						if ((type_ == PlayerType.AI || type_ == PlayerType.PLAYER) && Physics.Linecast(this.sectionInfo_[i].lastKartPos_, position, 4096))
						{
							this.sectionInfo_[i].needReset_ = true;
						}
						this.sectionInfo_[i].lastKartPos_ = position;
						if (this.sectionInfo_[i].latestPassingPlane_ != -1)
						{
							int num5;
							if (this.sectionInfo_[i].passCorrect_)
							{
								num5 = this.sectionInfo_[i].latestPassingPlane_;
								int num6 = this.passPlaneSequence_.GetNext(num5)[0];
							}
							else
							{
								int num6 = this.sectionInfo_[i].latestPassingPlane_;
								num5 = this.passPlaneSequence_.GetPrev(num6)[0];
							}
							Vector3 toNextPlane_ = this.passPlane_[num5].toNextPlane_;
							Vector3 vector = this.sectionInfo_[i].lastKartPos_ - this.passPlane_[num5].centerPos_;
							this.sectionInfo_[i].distance_ = Vector3.Dot(vector, toNextPlane_) / toNextPlane_.magnitude;
							this.sectionInfo_[i].distancePlane_ = num5;
						}
						else
						{
							this.sectionInfo_[i].distance_ = 0f;
							this.sectionInfo_[i].distancePlane_ = 0;
						}
					}
				}
			}
		}
		this.CalcRank();
	}

	private void CalcRank()
	{
		for (int i = 0; i < 6; i++)
		{
			if (this.sectionInfo_[i] != null)
			{
				if (this.IsKartGoalIn(i))
				{
					this.sectionInfo_[i].rankValue = (double)((long)((this.maxLap_ + 1) * 1000) << 32) - (double)this.GetFinishTime(i);
				}
				else if (KartManager.Instance.parameter_.Stage == StageType.GAME_WIFI && i != KartManager.PLAYER_KART_IDX)
				{
					this.sectionInfo_[i].rankValue = this.sectionInfo_[i].networkRankValue;
				}
				else
				{
					int num = this.sectionInfo_[i].lap_;
					if (this.sectionInfo_[i].lastCorrectDirectionPassingPlane_ < this.sectionInfo_[i].latestPassingPlane_)
					{
						num--;
					}
					this.sectionInfo_[i].rankValue = (double)((long)(num * 1000 + this.sectionInfo_[i].distancePlane_) << 32) + (double)this.sectionInfo_[i].distance_;
				}
				this.sectionInfo_[i].rank_ = 0;
				for (int j = 0; j < i; j++)
				{
					if (this.sectionInfo_[j] != null)
					{
						this.sectionInfo_[(this.sectionInfo_[i].rankValue <= this.sectionInfo_[j].rankValue) ? i : j].rank_++;
					}
				}
			}
		}
	}

	public string GetRankDebugString()
	{
		string text = string.Empty;
		for (int i = 0; i < 6; i++)
		{
			if (this.sectionInfo_[i] == null)
			{
				text += string.Format("{0} : empty\n", i);
			}
			else
			{
				text += string.Format("{0} : {1} : {2}\n", i, this.sectionInfo_[i].rank_, this.sectionInfo_[i].rankValue);
			}
		}
		return text;
	}

	public void DrawDebug()
	{
		for (int i = 0; i < this.passPlane_.Length; i++)
		{
			Debug.DrawLine(this.passPlane_[i].pt1_[0], this.passPlane_[i].pt1_[1], Color.gray);
			Debug.DrawLine(this.passPlane_[i].pt1_[1], this.passPlane_[i].pt1_[2], Color.gray);
			Debug.DrawLine(this.passPlane_[i].pt1_[2], this.passPlane_[i].pt1_[0], Color.gray);
			Debug.DrawLine(this.passPlane_[i].pt2_[0], this.passPlane_[i].pt2_[1], Color.gray);
			Debug.DrawLine(this.passPlane_[i].pt2_[1], this.passPlane_[i].pt2_[2], Color.gray);
			Debug.DrawLine(this.passPlane_[i].pt2_[2], this.passPlane_[i].pt2_[0], Color.gray);
		}
	}

	public int CheckPlanePassing(PassPlane plane, Vector3 p0, Vector3 p1)
	{
		Vector3 vector = p1 - p0;
		if (MathHelper.RayFaceIntersect(plane.pt1_, p0, vector) || MathHelper.RayFaceIntersect(plane.pt2_, p0, vector))
		{
			return (Vector3.Dot(vector, plane.normal_) < 0f) ? (-1) : 1;
		}
		return 0;
	}

	public string GetDebugString()
	{
		string text = this.sectionInfo_[KartManager.PLAYER_KART_IDX].ToString();
		return text + ((!this.IsWrongWay()) ? " [ CORRECT ]" : " [ WRONG] ");
	}

	public bool IsWrongWay()
	{
		GoKart kart_ = this.sectionInfo_[KartManager.PLAYER_KART_IDX].kart_;
		int latestPassingPlane_ = this.sectionInfo_[KartManager.PLAYER_KART_IDX].latestPassingPlane_;
		if (kart_ == null)
		{
			return false;
		}
		if (kart_.m_KartWLVel.magnitude <= 5f)
		{
			return false;
		}
		if (latestPassingPlane_ == -1)
		{
			return false;
		}
		Vector3 normalized = this.passPlane_[latestPassingPlane_].toNextPlane_.normalized;
		return Vector3.Dot(kart_.m_kart.transform.forward, normalized) <= -0.5f && Vector3.Dot(kart_.m_KartWLVel.normalized, normalized) <= -0.5f;
	}

	public void DebugRegenPos()
	{
		string text = string.Empty;
		for (int i = 0; i < this.passPlane_.Length; i++)
		{
			Vector3 zero = Vector3.zero;
			bool regenPos = this.passPlane_[i].GetRegenPos(ref zero);
			string text2 = text;
			text = string.Concat(new string[]
			{
				text2,
				" [ ",
				i.ToString(),
				" ] ",
				regenPos.ToString(),
				" ",
				zero.ToString(),
				"\n"
			});
		}
	}

	public float GetBestLapTime()
	{
		return this.sectionInfo_[KartManager.PLAYER_KART_IDX].GetBestLapTime();
	}

	public int MaxLap
	{
		get
		{
			return this.maxLap_;
		}
		set
		{
			this.maxLap_ = value;
		}
	}

	public int GetLap(int kartIdx)
	{
		return this.sectionInfo_[kartIdx].lap_;
	}

	public float GetLapTime(int kartIdx, int lap)
	{
		if (lap <= 0)
		{
			return 0f;
		}
		return this.sectionInfo_[kartIdx].lapTime_[lap - 1];
	}

	public float GetFinishTime()
	{
		return this.GetFinishTime(KartManager.PLAYER_KART_IDX);
	}

	public float GetFinishTime(int kartIdx)
	{
		if (this.sectionInfo_[kartIdx] == null)
		{
			return 0f;
		}
		return this.sectionInfo_[kartIdx].lapTime_[this.maxLap_ - 1];
	}

	public int GetLatestPassingPlane(int kartIdx)
	{
		return this.sectionInfo_[kartIdx].latestPassingPlane_;
	}

	public int GetLatestPassingPlane()
	{
		return this.sectionInfo_[KartManager.PLAYER_KART_IDX].latestPassingPlane_;
	}

	public int GetLastCorrectDirectionPassingPlane()
	{
		return this.sectionInfo_[KartManager.PLAYER_KART_IDX].lastCorrectDirectionPassingPlane_;
	}

	public int GetDistancePlane(int kartIdx)
	{
		return this.sectionInfo_[kartIdx].distancePlane_;
	}

	public float GetDistance(int kartIdx)
	{
		return this.sectionInfo_[kartIdx].distance_;
	}

	public int GetKartIndexByRank(int rank)
	{
		for (int i = 0; i < 6; i++)
		{
			if (this.sectionInfo_[i] != null && this.sectionInfo_[i].rank_ == rank)
			{
				return i;
			}
		}
		return -1;
	}

	public int[] GetKartIndexByRank()
	{
		int[] array = new int[KartManager.Instance.GetGoKartCount()];
		for (int i = 0; i < 6; i++)
		{
			if (this.sectionInfo_[i] != null)
			{
				array[this.sectionInfo_[i].rank_] = i;
			}
		}
		return array;
	}

	public int GetRankValue()
	{
		int num = 0;
		for (int i = 0; i < 6; i++)
		{
			num *= 10;
			if (this.sectionInfo_[i] != null)
			{
				num += this.sectionInfo_[i].rank_;
			}
		}
		return num;
	}

	public double GetInternalRankValue(int kartIndex)
	{
		if (kartIndex >= 0 && kartIndex < 6)
		{
			return this.sectionInfo_[kartIndex].rankValue;
		}
		return 0.0;
	}

	public int GetRank(int kartIdx)
	{
		if (this.sectionInfo_[kartIdx] == null)
		{
			return -1;
		}
		return this.sectionInfo_[kartIdx].rank_;
	}

	public int GetMyRank()
	{
		return this.sectionInfo_[KartManager.PLAYER_KART_IDX].rank_;
	}

	public bool IsFinalLap(int kartIdx)
	{
		return this.GetLap(kartIdx) == this.MaxLap;
	}

	public bool IsKartGoalIn()
	{
		return this.IsKartGoalIn(KartManager.PLAYER_KART_IDX);
	}

	public bool IsKartGoalIn(int kartIdx)
	{
		return this.sectionInfo_[kartIdx] != null && this.sectionInfo_[kartIdx].lapTime_[this.maxLap_ - 1] > 0f;
	}

	public void ResetKart(int kartIdx)
	{
		RegenInfo regenInfo = this.GetRegenInfo(this.sectionInfo_[kartIdx].latestPassingPlane_);
		MonoBehaviourExCenter.Instance.SendMessage(0, this.sectionInfo_[kartIdx].kart_.controller_.id_, ((WarpMessage)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.WARP)).Initialize(regenInfo.position_, Quaternion.LookRotation(regenInfo.direction_, Vector3.up), true, true));
		int latestPassingPlane_ = this.sectionInfo_[kartIdx].latestPassingPlane_;
		if (latestPassingPlane_ != -1)
		{
			this.sectionInfo_[kartIdx].passCorrect_ = this.CheckPlanePassing(this.passPlane_[latestPassingPlane_], regenInfo.position_, regenInfo.position_ + regenInfo.direction_) <= 0;
			this.sectionInfo_[kartIdx].lastKartPos_ = regenInfo.position_;
		}
	}

	public bool IsKartNeedReset(int kartIdx)
	{
		bool needReset_ = this.sectionInfo_[kartIdx].needReset_;
		this.sectionInfo_[kartIdx].needReset_ = false;
		return needReset_;
	}

	public void ResetAIKart(int kartIdx, int latestPassingPlane, bool passCorrect)
	{
		this.sectionInfo_[kartIdx].passCorrect_ = passCorrect;
		this.sectionInfo_[kartIdx].latestPassingPlane_ = latestPassingPlane;
	}

	public Vector3 GetCourseFromKart(int kartIdx, float distance, out int plane)
	{
		float num = distance + this.sectionInfo_[kartIdx].distance_;
		int num2 = this.sectionInfo_[kartIdx].distancePlane_;
		plane = -1;
		Vector3 vector;
		float magnitude;
		for (;;)
		{
			int num3 = this.passPlaneSequence_.GetNext(num2)[0];
			if (num2 >= 0 && num3 >= 0)
			{
				vector = this.passPlane_[num3].centerPos_ - this.passPlane_[num2].centerPos_;
				magnitude = vector.magnitude;
				if (num <= magnitude)
				{
					break;
				}
				num -= magnitude;
				num2 = num3;
			}
			else
			{
				num2 = num3;
			}
			if (num < 0f)
			{
				goto Block_5;
			}
		}
		Vector3 vector2 = vector * (num / magnitude) + this.passPlane_[num2].centerPos_;
		float distanceFromCenterToBottom = this.passPlane_[num2].GetDistanceFromCenterToBottom();
		RaycastHit raycastHit;
		if (Physics.Raycast(vector2, -Vector3.up, out raycastHit, distanceFromCenterToBottom, 256))
		{
			plane = num2;
			return raycastHit.point;
		}
		return Vector3.zero;
		Block_5:
		return Vector3.zero;
	}

	public int Get1stKartIndexInRacing()
	{
		int num = 6;
		int num2 = -1;
		for (int i = 0; i < 6; i++)
		{
			if (this.sectionInfo_[i] != null && !this.IsKartGoalIn(i) && this.sectionInfo_[i].rank_ < num)
			{
				num2 = i;
				num = this.sectionInfo_[i].rank_;
			}
		}
		return num2;
	}

	public int GetRankInRancing(int kartIdx)
	{
		if (this.sectionInfo_[kartIdx] == null)
		{
			return -1;
		}
		int num = this.sectionInfo_[kartIdx].rank_;
		int num2 = num;
		for (int i = 0; i < 6; i++)
		{
			if (this.sectionInfo_[i] != null && this.sectionInfo_[i].rank_ < num2 && this.IsKartGoalIn(num))
			{
				num--;
			}
		}
		return num;
	}

	public int GetKartNoInRacing()
	{
		int num = 0;
		for (int i = 0; i < 6; i++)
		{
			if (this.sectionInfo_[i] != null && !this.IsKartGoalIn(i))
			{
				num++;
			}
		}
		return num;
	}

	public RaceResult GetRaceResult()
	{
		RaceResult raceResult = new RaceResult();
		for (int i = 0; i < 6; i++)
		{
			if (this.sectionInfo_[i] != null)
			{
				raceResult.SetResult(i, this.sectionInfo_[i].rank_, this.sectionInfo_[i].lapTime_[this.maxLap_ - 1]);
			}
		}
		return raceResult;
	}

	public void ResetForRestarting()
	{
		for (int i = 0; i < this.sectionInfo_.Length; i++)
		{
			if (this.sectionInfo_[i] != null)
			{
				this.sectionInfo_[i].ResetForRestarting();
			}
		}
	}

	private const int NO_PLANE = -1;

	private const float KART_START_INTERVAL_4 = 4f;

	private const float KART_START_INTERVAL_6 = 2.6f;

	public PassPlane[] passPlane_;

	public PassPlaneSequence passPlaneSequence_;

	public SectionInfo[] sectionInfo_;

	public int maxLap_ = 2;

	public RegenInfo[] startInfos_;

	private float[,] passPlaneTrackDistanceDictionary;

	private float[] passPlaneTrackDistance;
}
