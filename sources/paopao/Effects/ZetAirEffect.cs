using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class ZetAirEffect : MonoBehaviour
{
	private void Awake()
	{
		this.m_airIdxQueue.setMaxNum(ZetAirEffect.kMaxAirNum);
		this.mesh_ = base.GetComponent<MeshFilter>().mesh;
	}

	private void Start()
	{
		this.initDefaultVertex();
		this.mainCam = CameraManager.Instance.mainCam_;
		if (this.mainCam == null)
		{
		}
	}

	private void Update()
	{
		if (KartManager.Instance.goPlayKart_ == null)
		{
			return;
		}
		if (this.mainCam == null)
		{
			return;
		}
		float kartSpeed = KartManager.Instance.goPlayKart_.GetKartSpeed();
		if (kartSpeed > (float)this.minSpd)
		{
			Vector3 kartWLVel = KartManager.Instance.goPlayKart_.m_KartWLVel;
			Matrix4x4 worldToCameraMatrix = this.mainCam.worldToCameraMatrix;
			worldToCameraMatrix.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
			this.setDirectionVector(worldToCameraMatrix * kartWLVel);
			this.setSpeed(kartSpeed, this.minSpd, this.maxSpd);
			this.UpdateZetAir();
			this.isActive = true;
		}
		else
		{
			this.clear();
		}
	}

	private void UpdateZetAir()
	{
		float time = Time.time;
		if (this.m_oldTick == 0f)
		{
			this.m_oldTick = time;
		}
		float num = Mathf.Tan(this.defaultFov) * this.defaultNearPlane;
		Vector4 vector = new Vector4(-num, 0f, -this.defaultFarPlane, 0f);
		vector.Normalize();
		float num2 = this.m_speed * (time - this.m_oldTick);
		int num3 = 0;
		int elem;
		while ((elem = this.m_airIdxQueue.getElem(num3++)) != this.m_airIdxQueue.tail)
		{
			Quaternion quaternion = Quaternion.AngleAxis(10f, Vector3.right);
			Vector3 vector2 = quaternion * this.m_direction;
			PerAirData[] airDatas = this.m_airDatas;
			int num4 = elem;
			ref Matrix4x4 ptr = ref airDatas[num4].trans;
			int num6;
			int num5 = (num6 = 0);
			int num8;
			int num7 = (num8 = 3);
			float num9 = ptr[num6, num8];
			airDatas[num4].trans[num5, num7] = num9 + vector2.x * num2;
			PerAirData[] airDatas2 = this.m_airDatas;
			int num10 = elem;
			ref Matrix4x4 ptr2 = ref airDatas2[num10].trans;
			int num11 = (num8 = 1);
			int num12 = (num6 = 3);
			num9 = ptr2[num8, num6];
			airDatas2[num10].trans[num11, num12] = num9 + vector2.y * num2;
			PerAirData[] airDatas3 = this.m_airDatas;
			int num13 = elem;
			ref Matrix4x4 ptr3 = ref airDatas3[num13].trans;
			int num14 = (num6 = 2);
			int num15 = (num8 = 3);
			num9 = ptr3[num6, num8];
			airDatas3[num13].trans[num14, num15] = num9 + vector2.z * num2;
		}
		if (time - this.m_oldTick > this.m_lifeTime)
		{
			float num16 = 1.57079637f;
			this.addZetAir(-num16, num16, 0.08726647f, this.m_maxAddZetNum);
			this.addZetAir(num16, 4.712389f, 0.08726647f, this.m_maxAddZetNum);
			this.m_oldTick = time;
		}
	}

	private void initDefaultVertex()
	{
		this.m_defaultMesh[0].xyz.x = 0f;
		this.m_defaultMesh[0].xyz.y = this.defaultAirHeight;
		this.m_defaultMesh[0].xyz.z = this.defaultAirLength;
		this.m_defaultMesh[0].xyz.w = 1f;
		this.m_defaultMesh[0].color = new Color(1f, 1f, 1f, 0.686f);
		this.m_defaultMesh[0].tu = 0f;
		this.m_defaultMesh[0].tv = 1f;
		this.m_defaultMesh[1].xyz.x = 0f;
		this.m_defaultMesh[1].xyz.y = -this.defaultAirHeight;
		this.m_defaultMesh[1].xyz.z = this.defaultAirLength;
		this.m_defaultMesh[1].xyz.w = 1f;
		this.m_defaultMesh[1].color = new Color(1f, 1f, 1f, 0.686f);
		this.m_defaultMesh[1].tu = 0f;
		this.m_defaultMesh[1].tv = 0f;
		this.m_defaultMesh[2].xyz.x = this.defaultGapToCenter;
		this.m_defaultMesh[2].xyz.y = this.defaultAirHeight;
		this.m_defaultMesh[2].xyz.z = this.defaultAirLength * this.defaultScaleVal;
		this.m_defaultMesh[2].xyz.w = 1f;
		this.m_defaultMesh[2].color = new Color(1f, 1f, 1f, 0.686f);
		this.m_defaultMesh[2].tu = 1f;
		this.m_defaultMesh[2].tv = 1f;
		this.m_defaultMesh[3].xyz.x = this.defaultGapToCenter;
		this.m_defaultMesh[3].xyz.y = -this.defaultAirHeight;
		this.m_defaultMesh[3].xyz.z = this.defaultAirLength * this.defaultScaleVal;
		this.m_defaultMesh[3].xyz.w = 1f;
		this.m_defaultMesh[3].color = new Color(1f, 1f, 1f, 0.686f);
		this.m_defaultMesh[3].tu = 1f;
		this.m_defaultMesh[3].tv = 0f;
	}

	private void setSpeed(float spd, int minSpd, int maxSpd)
	{
		int num = 5;
		if (spd > (float)maxSpd)
		{
			this.m_zetStep = num;
		}
		else
		{
			this.m_zetStep = (int)((spd - (float)minSpd) / (float)(maxSpd - minSpd) * (float)num);
		}
		this.m_speed = spd * 0.25f;
		switch (this.m_zetStep)
		{
		case 0:
			this.m_lifeTime = 0.12f;
			this.m_startPos = 38f;
			this.m_maxAddZetNum = 2;
			break;
		case 1:
			this.m_lifeTime = 0.11f;
			this.m_startPos = 37.5f;
			this.m_maxAddZetNum = 3;
			break;
		case 2:
			this.m_lifeTime = 0.1f;
			this.m_startPos = 37f;
			this.m_maxAddZetNum = 4;
			break;
		case 3:
			this.m_lifeTime = 0.08f;
			this.m_startPos = 36.5f;
			this.m_maxAddZetNum = 4;
			break;
		default:
			this.m_lifeTime = 0.07f;
			this.m_startPos = 36f;
			this.m_maxAddZetNum = 5;
			break;
		}
	}

	private void addZetAir(float startDegree, float endDegree, float randomSize, int num)
	{
		float num2 = Mathf.Tan(this.defaultFov) * this.defaultNearPlane;
		Vector4 vector = new Vector4(-num2, 0f, -this.defaultFarPlane, 0f);
		vector.Normalize();
		for (int i = 0; i < num; i++)
		{
			int tail = this.m_airIdxQueue.tail;
			this.m_airIdxQueue.addElem();
			float num3 = global::UnityEngine.Random.value * randomSize;
			float num4 = (startDegree + (endDegree - startDegree) / (float)num * (float)i + num3) * 180f / 3.14159274f;
			Quaternion quaternion = Quaternion.AngleAxis(num4, Vector3.forward);
			Vector4 vector2 = quaternion * vector;
			vector2 *= this.m_startPos;
			vector2.w = 1f;
			this.m_airDatas[tail].trans = Matrix4x4.TRS(vector2, quaternion, Vector3.one);
			if (this.m_zetStep < 2)
			{
				this.m_airDatas[tail].uvIdx = (int)(global::UnityEngine.Random.value * 10f) % 4 - 1;
			}
			else
			{
				this.m_airDatas[tail].uvIdx = ((global::UnityEngine.Random.value <= 0.5f) ? (-1) : 3);
			}
		}
	}

	private void clear()
	{
		this.mesh_.Clear();
		this.isActive = false;
	}

	private void LateUpdate()
	{
		this.mesh_.Clear();
		if (!this.isActive)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		while (this.m_airIdxQueue.getElem(num2++) != this.m_airIdxQueue.tail)
		{
			num++;
		}
		if (num == 0)
		{
			return;
		}
		Vector3[] array = new Vector3[num * 4];
		Color[] array2 = new Color[num * 4];
		Vector2[] array3 = new Vector2[num * 4];
		int[] array4 = new int[num * 6];
		num = 0;
		num2 = 0;
		int num3 = 128;
		int num4 = 42;
		int elem;
		while ((elem = this.m_airIdxQueue.getElem(num2++)) != this.m_airIdxQueue.tail)
		{
			PerAirData perAirData = this.m_airDatas[elem];
			for (int i = 0; i < 4; i++)
			{
				array[num * 4 + i] = this.mainCam.cameraToWorldMatrix * perAirData.trans * this.m_defaultMesh[i].xyz;
				array2[num * 4 + i] = this.m_defaultMesh[i].color;
			}
			array3[num * 4] = new Vector2(1f, 0f);
			array3[num * 4 + 1] = new Vector2(1f, (float)(num4 * perAirData.uvIdx) / (float)num3);
			array3[num * 4 + 2] = new Vector2(0f, 0f);
			array3[num * 4 + 3] = new Vector2(0f, (float)(num4 * perAirData.uvIdx) / (float)num3);
			array4[num * 6] = num * 4;
			array4[num * 6 + 2] = num * 4 + 1;
			array4[num * 6 + 1] = num * 4 + 2;
			array4[num * 6 + 3] = num * 4 + 2;
			array4[num * 6 + 5] = num * 4 + 1;
			array4[num * 6 + 4] = num * 4 + 3;
			num++;
		}
		this.mesh_.vertices = array;
		this.mesh_.triangles = array4;
		this.mesh_.colors = array2;
		this.mesh_.uv = array3;
		this.mesh_.RecalculateNormals();
	}

	public void setDirectionVector(Vector3 vec)
	{
		this.m_direction = vec;
		this.m_direction.Normalize();
	}

	private ZetVertex[] m_defaultMesh = new ZetVertex[4];

	private PerAirData[] m_airDatas = new PerAirData[ZetAirEffect.kMaxAirNum];

	private float defaultAirLength = 70f;

	private float defaultAirHeight = 0.2f;

	private float defaultGapToCenter = 0.3f;

	private float defaultScaleVal = 0.35f;

	private float defaultFov = 1.30899692f;

	private float defaultNearPlane = 1f;

	private float defaultFarPlane = 50f;

	private int minSpd = 170;

	private int maxSpd = 240;

	private CircleQueue m_airIdxQueue;

	public static int kMaxAirNum = 16;

	private float m_startPos;

	private int m_zetStep;

	private float m_lifeTime = 1f;

	private float m_speed;

	private int m_maxAddZetNum;

	private float m_oldTick;

	private Vector3 m_direction = Vector3.zero;

	private Camera mainCam;

	private bool isActive;

	private Mesh mesh_;
}
