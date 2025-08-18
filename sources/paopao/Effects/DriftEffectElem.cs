using System;
using UnityEngine;

public class DriftEffectElem
{
	public DriftEffectElem(Vector3 localTranslate, float transX, float transY, float tilt, DriftEffectElem.EffectType effectType)
	{
		this.transX_ = transX;
		this.transY_ = transY;
		this.tilt_ = tilt;
		this.localTranslate_ = new Vector4(localTranslate.x, localTranslate.y, localTranslate.z, 1f);
		this.idxQueue_.setMaxNum(10);
		this.effectType_ = effectType;
		for (int i = 0; i < 10; i++)
		{
			this.driftDatas_[i] = new DriftData();
		}
		this.Init();
	}

	private void Init()
	{
		this.gap_ = 0.3f;
		this.lifeTime_ = 20U;
		this.turnPoint_ = 2U;
		if (this.effectType_ == DriftEffectElem.EffectType.TYPE1)
		{
			this.InitDefaultVertex();
		}
		else
		{
			this.InitDefaultVertex2();
		}
	}

	private void InitDefaultVertex()
	{
		float num = -1f;
		float num2 = 0.43f;
		float num3 = global::UnityEngine.Random.value;
		num3 = this.tilt_ * num3;
		this.defaultMesh_[0].xyz.x = this.transX_;
		this.defaultMesh_[0].xyz.y = 0f;
		this.defaultMesh_[0].xyz.z = num + this.transY_;
		this.defaultMesh_[0].xyz.w = 1f;
		this.defaultMesh_[0].color = Color.white;
		this.defaultMesh_[1].xyz.x = num3 + this.transX_;
		this.defaultMesh_[1].xyz.y = num2;
		this.defaultMesh_[1].xyz.z = num + this.transY_;
		this.defaultMesh_[1].xyz.w = 1f;
		this.defaultMesh_[1].color = Color.white;
		this.defaultMesh_[2].xyz.x = this.transX_;
		this.defaultMesh_[2].xyz.y = 0f;
		this.defaultMesh_[2].xyz.z = this.transY_;
		this.defaultMesh_[2].xyz.w = 1f;
		this.defaultMesh_[2].color = Color.white;
		this.defaultMesh_[3].xyz.x = num3 + this.transX_;
		this.defaultMesh_[3].xyz.y = num2;
		this.defaultMesh_[3].xyz.z = this.transY_;
		this.defaultMesh_[3].xyz.w = 1f;
		this.defaultMesh_[3].color = Color.white;
	}

	private void InitDefaultVertex2()
	{
		float num = -1f;
		float num2 = 0.12f;
		float num3 = global::UnityEngine.Random.value;
		num3 = num2 * num3;
		this.defaultMesh_[0].xyz.x = this.transX_ - num2;
		this.defaultMesh_[0].xyz.y = 0.05f;
		this.defaultMesh_[0].xyz.z = num + this.transY_;
		this.defaultMesh_[0].xyz.w = 1f;
		this.defaultMesh_[0].color = Color.white;
		this.defaultMesh_[1].xyz.x = num2 + this.transX_;
		this.defaultMesh_[1].xyz.y = 0.05f;
		this.defaultMesh_[1].xyz.z = num + this.transY_;
		this.defaultMesh_[1].xyz.w = 1f;
		this.defaultMesh_[1].color = Color.white;
		this.defaultMesh_[2].xyz.x = this.transX_ - num2;
		this.defaultMesh_[2].xyz.y = 0.05f;
		this.defaultMesh_[2].xyz.z = this.transY_;
		this.defaultMesh_[2].xyz.w = 1f;
		this.defaultMesh_[2].color = Color.white;
		this.defaultMesh_[3].xyz.x = num2 + this.transX_;
		this.defaultMesh_[3].xyz.y = 0.05f;
		this.defaultMesh_[3].xyz.z = this.transY_;
		this.defaultMesh_[3].xyz.w = 1f;
		this.defaultMesh_[3].color = Color.white;
	}

	public void AddDriftMark2(Vector3 trans, Quaternion rot, float scale)
	{
		int tail = this.idxQueue_.tail;
		this.idxQueue_.addElem();
		this.driftDatas_[tail].scale = scale;
		this.driftDatas_[tail].age = 0U;
		this.driftDatas_[tail].trans = Matrix4x4.TRS(trans, rot, Vector3.one);
		if (this.effectType_ == DriftEffectElem.EffectType.TYPE1)
		{
			this.SetRandomUV(ref this.driftDatas_[tail], 0.1f, 0.1f);
		}
		else
		{
			this.SetRandomUV2(ref this.driftDatas_[tail], 0.1f, 0.1f);
		}
	}

	public void AddDriftMark(Matrix4x4 kartTrans, Quaternion kartDir, float scale)
	{
		int tail = this.idxQueue_.tail;
		this.idxQueue_.addElem();
		this.driftDatas_[tail].scale = scale;
		this.driftDatas_[tail].age = 0U;
		this.driftDatas_[tail].trans = Matrix4x4.TRS(kartTrans * this.localTranslate_, kartDir, Vector3.one);
		if (this.effectType_ == DriftEffectElem.EffectType.TYPE1)
		{
			this.SetRandomUV(ref this.driftDatas_[tail], 0.1f, 0.1f);
		}
		else
		{
			this.SetRandomUV2(ref this.driftDatas_[tail], 0.1f, 0.1f);
		}
	}

	private void SetRandomUVDefault(ref DriftData driftData)
	{
		driftData.tu[0] = 1f;
		driftData.tv[1] = 1f;
		driftData.tu[1] = 1f;
		driftData.tv[0] = 0f;
		driftData.tu[2] = 0f;
		driftData.tv[3] = 1f;
		driftData.tu[3] = 0f;
		driftData.tv[2] = 0f;
	}

	private void SetRandomUV(ref DriftData driftData, float horizArea, float vertArea)
	{
		float value = global::UnityEngine.Random.value;
		driftData.tu[0] = value;
		driftData.tv[0] = 0.8f - value * 0.8f;
		driftData.tu[1] = value;
		driftData.tv[1] = 1f;
		driftData.tu[2] = 0.5f + value;
		driftData.tv[2] = 0.8f - value * 0.8f;
		driftData.tu[3] = 0.5f + value;
		driftData.tv[3] = 1f;
	}

	private void SetRandomUV2(ref DriftData driftData, float horizArea, float vertArea)
	{
		float value = global::UnityEngine.Random.value;
		driftData.tu[0] = value;
		driftData.tv[0] = 0f;
		driftData.tu[1] = value;
		driftData.tv[1] = 1f;
		driftData.tu[2] = 0.5f + value;
		driftData.tv[2] = 0f;
		driftData.tu[3] = 0.5f + value;
		driftData.tv[3] = 1f;
	}

	public void UpdateTransform(int tick)
	{
		int num = 0;
		int elem;
		while ((elem = this.idxQueue_.getElem(num++)) != this.idxQueue_.tail)
		{
			this.driftDatas_[elem].age += 1U;
			if (this.driftDatas_[elem].age > this.lifeTime_)
			{
				this.idxQueue_.delElem();
			}
			float num2 = ((this.driftDatas_[elem].age >= this.turnPoint_) ? this.gap_ : 2f);
			Matrix4x4 matrix4x = Matrix4x4.Scale(new Vector3(this.driftDatas_[elem].scale, this.driftDatas_[elem].scale, 1f));
			this.driftDatas_[elem].finalTrans = this.driftDatas_[elem].trans * matrix4x;
			this.driftDatas_[elem].scale *= num2;
		}
	}

	public int GetSegmentCount()
	{
		return this.idxQueue_.getNumElem();
	}

	public int UpdateVertexBuffer(ref Vector3[] vertices, ref Color[] colors, ref Vector2[] uvs, ref int[] triangles, int startSegment)
	{
		if (this.idxQueue_.getNumElem() <= 0)
		{
			return startSegment;
		}
		int num = startSegment;
		int num2 = 0;
		int elem;
		while ((elem = this.idxQueue_.getElem(num2++)) != this.idxQueue_.tail)
		{
			DriftData driftData = this.driftDatas_[elem];
			for (int i = 0; i < 4; i++)
			{
				vertices[num * 4 + i] = driftData.finalTrans * this.defaultMesh_[i].xyz;
				colors[num * 4 + i] = this.defaultMesh_[i].color;
				uvs[num * 4 + i] = new Vector2(driftData.tu[i], driftData.tv[i]);
			}
			triangles[num * 6] = num * 4;
			triangles[num * 6 + 2] = num * 4 + 1;
			triangles[num * 6 + 1] = num * 4 + 2;
			triangles[num * 6 + 3] = num * 4 + 2;
			triangles[num * 6 + 5] = num * 4 + 1;
			triangles[num * 6 + 4] = num * 4 + 3;
			num++;
		}
		return num;
	}

	public const int MAX_DRIFT_NUM = 10;

	private DriftData[] driftDatas_ = new DriftData[10];

	private Vector4 localTranslate_;

	private float gap_;

	private float tilt_;

	private float transX_;

	private float transY_;

	private uint lifeTime_;

	private uint turnPoint_;

	private DriftVertex[] defaultMesh_ = new DriftVertex[4];

	private CircleQueue idxQueue_;

	private DriftEffectElem.EffectType effectType_;

	public enum EffectType
	{
		TYPE1,
		TYPE2
	}
}
