using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ExhaustEffect : MonoBehaviour
{
	private void Awake()
	{
		this.m_idxQueue.Initialize();
		int num = this.maxGasNum * this.maxGasSet;
		this.m_idxQueue.setMaxNum(num);
		this.m_defaultMesh = new GasVertex[4];
		this.m_gasSet = new GasSet[this.maxGasSet];
		this.m_gasDatas = new GasData[num];
		this.initDefaultVertex();
		if (base.GetComponent<MeshFilter>().mesh == null)
		{
			base.GetComponent<MeshFilter>().mesh = new Mesh();
		}
		this.meshRenderer_ = base.GetComponent<MeshRenderer>();
		this.meshRenderer_.castShadows = false;
		this.meshRenderer_.receiveShadows = false;
		this.mesh_ = base.GetComponent<MeshFilter>().mesh;
		this.vertices = new Vector3[num * 4];
		this.colors = new Color[num * 4];
		this.uvs = new Vector2[num * 4];
		for (int i = 0; i < num * 4; i++)
		{
			this.vertices[i] = Vector3.zero;
			this.colors[i] = Color.white;
			this.uvs[i] = Vector2.zero;
		}
		this.triangles = new int[num * 6];
		for (int j = 0; j < num * 6; j++)
		{
			this.triangles[j] = 0;
		}
	}

	private void Start()
	{
		this.mainCam_ = CameraManager.Instance.mainCam_;
		Transform kartBodyTransform = KartManager.Instance.goPlayKart_.controller_.KartBodyTransform;
		if (kartBodyTransform != null)
		{
			foreach (object obj in kartBodyTransform)
			{
				Transform transform = (Transform)obj;
				if (transform.gameObject.name.Contains("port"))
				{
					this.m_gasSet[this.m_gasSetNum++].localTranslate = transform.localPosition;
				}
			}
		}
	}

	private void Update()
	{
		if (KartManager.Instance.goPlayKart_ == null)
		{
			return;
		}
		GameObject kart = KartManager.Instance.goPlayKart_.m_kart;
		if (kart == null)
		{
			return;
		}
		this.m_generateGap = 0.12f - KartManager.Instance.goPlayKart_.m_KartWLVel.magnitude * 0.01f;
		float deltaTime = Time.deltaTime;
		if (this.mainCam_ == null)
		{
			return;
		}
		Vector3 forward = this.mainCam_.transform.forward;
		float num = Mathf.Atan2(forward.x, forward.z) * 180f / 3.14159274f;
		Quaternion quaternion = Quaternion.AngleAxis(num, Vector3.up);
		int i = 0;
		int elem;
		while ((elem = this.m_idxQueue.getElem(i++)) != this.m_idxQueue.tail)
		{
			GasData[] gasDatas = this.m_gasDatas;
			int num2 = elem;
			gasDatas[num2].pos = gasDatas[num2].pos + (this.m_gasDatas[elem].dir * this.m_gasDatas[elem].spd * deltaTime + this.m_gasDatas[elem].kartDir * this.m_gasDatas[elem].kartDirSpeed * deltaTime);
			this.m_gasDatas[elem].spd = Mathf.Max(0f, this.m_gasDatas[elem].spd - this.m_spdDecrement);
			GasData[] gasDatas2 = this.m_gasDatas;
			int num3 = elem;
			gasDatas2[num3].kartDirSpeed = gasDatas2[num3].kartDirSpeed * 0.9f;
			this.m_gasDatas[elem].alpha = Mathf.Max(0, (int)((float)this.m_gasDatas[elem].alpha - this.m_alphaDecrement * deltaTime));
			GasData[] gasDatas3 = this.m_gasDatas;
			int num4 = elem;
			gasDatas3[num4].scale = gasDatas3[num4].scale + deltaTime * this.m_scaleFactor;
			this.m_gasDatas[elem].finalTrans = Matrix4x4.TRS(this.m_gasDatas[elem].pos, quaternion, Vector3.one * this.m_gasDatas[elem].scale);
		}
		if (Time.time - this.m_oldGenerateTick > this.m_generateGap)
		{
			for (i = 0; i < this.m_gasSetNum; i++)
			{
				float num5 = global::UnityEngine.Random.value * 0.2f - 0.1f;
				Vector3 vector = kart.transform.TransformDirection(new Vector3(num5, num5, -1f));
				vector.Normalize();
				this.generateGas(i, vector, 5f, kart);
			}
			this.m_oldGenerateTick = Time.time;
		}
	}

	private void initDefaultVertex()
	{
		float num = 0.13f;
		float num2 = 0.13f;
		this.m_defaultMesh[0].xyz.x = -num2;
		this.m_defaultMesh[0].xyz.z = 0f;
		this.m_defaultMesh[0].xyz.y = -num;
		this.m_defaultMesh[0].xyz.w = 1f;
		this.m_defaultMesh[0].diffuse = Color.white;
		this.m_defaultMesh[0].tu = 0f;
		this.m_defaultMesh[0].tv = 0f;
		this.m_defaultMesh[1].xyz.x = num2;
		this.m_defaultMesh[1].xyz.z = 0f;
		this.m_defaultMesh[1].xyz.y = -num;
		this.m_defaultMesh[1].xyz.w = 1f;
		this.m_defaultMesh[1].diffuse = Color.white;
		this.m_defaultMesh[1].tu = 1f;
		this.m_defaultMesh[1].tv = 0f;
		this.m_defaultMesh[2].xyz.x = -num2;
		this.m_defaultMesh[2].xyz.z = 0f;
		this.m_defaultMesh[2].xyz.y = num;
		this.m_defaultMesh[2].xyz.w = 1f;
		this.m_defaultMesh[2].diffuse = Color.white;
		this.m_defaultMesh[2].tu = 0f;
		this.m_defaultMesh[2].tv = 1f;
		this.m_defaultMesh[3].xyz.x = num2;
		this.m_defaultMesh[3].xyz.z = 0f;
		this.m_defaultMesh[3].xyz.y = num;
		this.m_defaultMesh[3].xyz.w = 1f;
		this.m_defaultMesh[3].diffuse = Color.white;
		this.m_defaultMesh[3].tu = 1f;
		this.m_defaultMesh[3].tv = 1f;
	}

	private void generateGas(int setIdx, Vector3 dir, float spd, GameObject kartObject)
	{
		int tail = this.m_idxQueue.tail;
		this.m_idxQueue.addElem();
		float num = global::UnityEngine.Random.value * 0.1f;
		Transform transform = kartObject.transform;
		Vector3 kartWLVel = KartManager.Instance.goPlayKart_.m_KartWLVel;
		this.m_gasDatas[tail].age = 0;
		this.m_gasDatas[tail].pos = transform.TransformPoint(this.m_gasSet[setIdx].localTranslate) + dir * num;
		this.m_gasDatas[tail].scale = 1f;
		this.m_gasDatas[tail].dir = dir;
		this.m_gasDatas[tail].spd = spd;
		this.m_gasDatas[tail].alpha = 125;
		this.m_gasDatas[tail].kartDirSpeed = kartWLVel.magnitude;
		this.m_gasDatas[tail].kartDir = kartWLVel.normalized;
		Vector3 forward = this.mainCam_.transform.forward;
		float num2 = Mathf.Atan2(forward.x, forward.z) * 180f / 3.14159274f;
		Quaternion quaternion = Quaternion.AngleAxis(num2, Vector3.up);
		this.m_gasDatas[tail].finalTrans = Matrix4x4.TRS(this.m_gasDatas[tail].pos, quaternion, Vector3.one);
	}

	private void LateUpdate()
	{
		this.mesh_.Clear();
		if (KartManager.Instance.goPlayKart_ == null)
		{
			return;
		}
		if (KartManager.Instance.goPlayKart_.isRealBoost())
		{
			return;
		}
		if (KartManager.Instance.goPlayKart_.KartBodyAnim != KartBodyAnimation.IDLE)
		{
			return;
		}
		if (this.m_idxQueue.getNumElem() == 0)
		{
			return;
		}
		int i = 0;
		int num = 0;
		int elem;
		while ((elem = this.m_idxQueue.getElem(num++)) != this.m_idxQueue.tail)
		{
			GasData gasData = this.m_gasDatas[elem];
			for (int j = 0; j < 4; j++)
			{
				this.vertices[i * 4 + j] = gasData.finalTrans * this.m_defaultMesh[j].xyz;
				this.colors[i * 4 + j] = new Color(this.m_defaultMesh[j].diffuse.r, this.m_defaultMesh[j].diffuse.g, this.m_defaultMesh[j].diffuse.b, (float)gasData.alpha / 255f);
				this.uvs[i * 4 + j] = new Vector2(this.m_defaultMesh[j].tu, this.m_defaultMesh[j].tv);
			}
			this.triangles[i * 6] = i * 4;
			this.triangles[i * 6 + 2] = i * 4 + 1;
			this.triangles[i * 6 + 1] = i * 4 + 2;
			this.triangles[i * 6 + 3] = i * 4 + 2;
			this.triangles[i * 6 + 5] = i * 4 + 1;
			this.triangles[i * 6 + 4] = i * 4 + 3;
			i++;
		}
		while (i < this.maxGasNum * this.maxGasSet)
		{
			for (int k = 0; k < 4; k++)
			{
				this.vertices[i * 4 + k] = Vector3.zero;
				this.colors[i * 4 + k] = Color.white;
				this.uvs[i * 4 + k] = Vector2.zero;
			}
			for (int l = 0; l < 6; l++)
			{
				this.triangles[i * 6 + l] = 0;
			}
			i++;
		}
		this.mesh_.vertices = this.vertices;
		this.mesh_.triangles = this.triangles;
		this.mesh_.colors = this.colors;
		this.mesh_.uv = this.uvs;
		this.mesh_.RecalculateNormals();
	}

	private int maxGasNum = 5;

	private int maxGasSet = 2;

	private float m_spdDecrement = 0.01f;

	private float m_oldGenerateTick;

	private float m_generateGap = 0.1f;

	private float m_alphaDecrement = 750f;

	private float m_scaleFactor = 7f;

	private int m_gasSetNum;

	private GasSet[] m_gasSet;

	private GasData[] m_gasDatas;

	private GasVertex[] m_defaultMesh;

	private CircleQueue m_idxQueue;

	private Camera mainCam_;

	private MeshRenderer meshRenderer_;

	private Mesh mesh_;

	private Vector3[] vertices;

	private Color[] colors;

	private Vector2[] uvs;

	private int[] triangles;
}
