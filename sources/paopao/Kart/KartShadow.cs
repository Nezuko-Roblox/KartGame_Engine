using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class KartShadow : MonoBehaviour
{
	private void Start()
	{
		if (base.GetComponent<MeshFilter>().mesh == null)
		{
			base.GetComponent<MeshFilter>().mesh = new Mesh();
		}
		this.mesh_ = base.GetComponent<MeshFilter>().mesh;
		int goKartCount = KartManager.Instance.GetGoKartCount();
		this.kartTransform_ = new Transform[goKartCount];
		this.controller_ = new KartBasicController[goKartCount];
		this.isCalculate_ = new bool[goKartCount];
		this.transformedVertices_ = new Vector3[4 * goKartCount];
		this.colors_ = new Color[4 * goKartCount];
		for (int i = 0; i < 4 * goKartCount; i++)
		{
			this.colors_[i] = Color.white;
		}
		this.triangles_ = new int[6 * goKartCount];
		int num = 0;
		int num2 = 0;
		for (int j = 0; j < goKartCount; j++)
		{
			this.triangles_[num] = num2;
			this.triangles_[num + 1] = num2 + 1;
			this.triangles_[num + 2] = num2 + 3;
			this.triangles_[num + 3] = num2;
			this.triangles_[num + 4] = num2 + 3;
			this.triangles_[num + 5] = num2 + 2;
			num += 6;
			num2 += 4;
		}
		this.uvs_ = new Vector2[4 * goKartCount];
		int num3 = 0;
		int[] array = new int[]
		{
			0, 1, 2, 3, 4, 5, 6, 7, 1, 2,
			3, 4, 5, 8, 2
		};
		int num4 = 2;
		for (int k = 0; k < 6; k++)
		{
			if (KartManager.Instance.goKart_[k] != null)
			{
				this.controller_[num3] = KartManager.Instance.goKart_[k].controller_;
				this.kartTransform_[num3] = this.controller_[num3].KartBodyTransform;
				int body_ = (int)KartManager.Instance.parameter_.kart_[k].body_;
				float num5 = (float)(array[body_] % 8) * 0.125f;
				float num6 = num5 + 0.125f;
				float num7 = (float)(num4 - 1 - array[body_] / 8) * 0.5f;
				float num8 = num7 + 0.5f;
				this.uvs_[num3 * 4] = new Vector2(num5, num8);
				this.uvs_[num3 * 4 + 1] = new Vector2(num6, num8);
				this.uvs_[num3 * 4 + 2] = new Vector2(num5, num7);
				this.uvs_[num3 * 4 + 3] = new Vector2(num6, num7);
				num3++;
			}
		}
		float num9 = -this.size_.x * 0.5f;
		float num10 = this.size_.x * 0.5f;
		float num11 = this.size_.y * 0.5f;
		float num12 = -this.size_.y * 0.5f;
		this.shadowStartPoint_[0] = new Vector3(num9 * this.xScale_, 0.5f, num11 * this.yScale_);
		this.shadowStartPoint_[1] = new Vector3(num10 * this.xScale_, 0.5f, num11 * this.yScale_);
		this.shadowStartPoint_[2] = new Vector3(num9 * this.xScale_, 0.5f, num12 * this.yScale_);
		this.shadowStartPoint_[3] = new Vector3(num10 * this.xScale_, 0.5f, num12 * this.yScale_);
		this.shadowEndPoint_[0] = new Vector3(num9 * 1.5f, -20f, num11 * 1.5f);
		this.shadowEndPoint_[1] = new Vector3(num10 * 1.5f, -20f, num11 * 1.5f);
		this.shadowEndPoint_[2] = new Vector3(num9 * 1.5f, -20f, num12 * 1.5f);
		this.shadowEndPoint_[3] = new Vector3(num10 * 1.5f, -20f, num12 * 1.5f);
	}

	private void Update()
	{
		this.mesh_.Clear();
		if (this.kartTransform_ == null)
		{
			return;
		}
		for (int i = 0; i < KartManager.Instance.GetGoKartCount(); i++)
		{
			if (this.controller_[i].InViewFrustum)
			{
				this.cpos = Vector3.zero;
				bool flag = true;
				for (int j = 0; j < 4; j++)
				{
					Vector3 vector = this.kartTransform_[i].localToWorldMatrix.MultiplyPoint(this.shadowStartPoint_[j]);
					Vector3 vector2 = this.kartTransform_[i].localToWorldMatrix.MultiplyPoint(this.shadowEndPoint_[j]);
					RaycastHit raycastHit;
					if (!Physics.Linecast(vector, vector2, out raycastHit, 256))
					{
						flag = false;
						break;
					}
					this.pos[j] = raycastHit.point + raycastHit.normal * 0.025f;
					this.cpos += this.pos[j];
				}
				if ((this.pos[0] - this.pos[2]).sqrMagnitude > 25f)
				{
					flag = false;
				}
				else if ((this.pos[1] - this.pos[2]).sqrMagnitude > 25f)
				{
					flag = false;
				}
				if (flag)
				{
					this.cpos *= 0.25f;
					for (int k = 0; k < 4; k++)
					{
						this.transformedVertices_[i * 4 + k] = this.cpos + (this.pos[k] - this.cpos) * 1.25f;
					}
					this.isCalculate_[i] = true;
				}
				else if (this.isCalculate_[i])
				{
					this.isCalculate_[i] = false;
					for (int l = 0; l < 4; l++)
					{
						this.transformedVertices_[i * 4 + l] = Vector3.zero;
					}
				}
			}
			else if (this.isCalculate_[i])
			{
				this.isCalculate_[i] = false;
				for (int m = 0; m < 4; m++)
				{
					this.transformedVertices_[i * 4 + m] = Vector3.zero;
				}
			}
		}
		this.mesh_.vertices = this.transformedVertices_;
		this.mesh_.triangles = this.triangles_;
		this.mesh_.colors = this.colors_;
		this.mesh_.uv = this.uvs_;
		this.mesh_.RecalculateNormals();
	}

	private Mesh mesh_;

	private Vector3[] shadowStartPoint_ = new Vector3[4];

	private Vector3[] shadowEndPoint_ = new Vector3[4];

	private Transform[] kartTransform_;

	private KartBasicController[] controller_;

	private bool[] isCalculate_;

	public float xScale_ = 1.2f;

	public float yScale_ = 1.22f;

	private Vector2 size_ = new Vector2(1.888127f, 1.697095f);

	private Vector3[] transformedVertices_;

	private Color[] colors_;

	private Vector2[] uvs_;

	private int[] triangles_;

	private Vector3[] pos = new Vector3[4];

	private Vector3 cpos = Vector3.zero;
}
