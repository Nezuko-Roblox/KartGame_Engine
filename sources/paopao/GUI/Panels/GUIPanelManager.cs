using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIPanelManager
{
	public GUIPanelManager()
	{
		for (int i = 0; i < 8; i++)
		{
			this.layerNode_[i] = null;
		}
		for (int j = 0; j < this.subMeshCounter_.Length; j++)
		{
			this.subMeshCounter_[j] = 0;
		}
	}

	public override string ToString()
	{
		string text = string.Empty;
		LinkedListNode<GUIPanelEx> linkedListNode = this.panels_.First;
		int num = 0;
		while (linkedListNode != null)
		{
			string text2 = text;
			text = string.Concat(new string[]
			{
				text2,
				num.ToString(),
				" ",
				linkedListNode.Value.Visible.ToString(),
				"\n"
			});
			linkedListNode = linkedListNode.Next;
			num++;
		}
		return text;
	}

	public void SetCamera(Camera cam)
	{
		this.mainCam_ = cam;
	}

	public int RegistPanel(GUIPanelEx panel)
	{
		if (!this.isSealed_)
		{
			if (this.layerNode_[panel.Layer] == null)
			{
				for (int i = panel.Layer + 1; i < 8; i++)
				{
					if (this.layerNode_[i] != null)
					{
						this.layerNode_[panel.Layer] = this.layerNode_[i];
						break;
					}
				}
				if (this.layerNode_[panel.Layer] == null)
				{
					this.layerNode_[panel.Layer] = this.panels_.AddFirst(panel);
				}
				else
				{
					this.layerNode_[panel.Layer] = this.panels_.AddAfter(this.layerNode_[panel.Layer], panel);
				}
			}
			else
			{
				this.layerNode_[panel.Layer] = this.panels_.AddAfter(this.layerNode_[panel.Layer], panel);
			}
			if (MathHelper.IsBetweenIE(panel.SubMeshIndex, 0, this.subMeshCounter_.Length))
			{
				this.subMeshCounter_[panel.SubMeshIndex]++;
			}
			else
			{
				Debug.LogError(" SubMeshCounter Over " + panel.SubMeshIndex.ToString());
			}
			return this.panels_.Count - 1;
		}
		return -1;
	}

	public int RegistGUIInterface(GUIInterface e)
	{
		if (!this.isSealed_ && e != null)
		{
			e.RegistPanelManager(this);
			return this.panels_.Count - 1;
		}
		return -1;
	}

	private void Seal()
	{
		int count = this.panels_.Count;
		this.vertices_ = new Vector3[count * 4];
		this.transVertices_ = new Vector3[count * 4];
		this.uvs_ = new Vector2[count * 4];
		this.colors_ = new Color[count * 4];
		this.triangles_ = new int[8][];
		for (int i = 0; i < 8; i++)
		{
			this.triangles_[i] = null;
			if (this.subMeshCounter_[i] > 0)
			{
				this.triangles_[i] = new int[this.subMeshCounter_[i] * 6];
			}
		}
		int[] array = new int[8];
		for (int j = 0; j < 8; j++)
		{
			array[j] = 0;
		}
		int[] array2 = new int[] { 0, 1, 2, 1, 3, 2 };
		int num = 0;
		for (LinkedListNode<GUIPanelEx> linkedListNode = this.panels_.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			linkedListNode.Value.GetVerticesUVs(ref this.vertices_, ref this.uvs_, num * 4);
			for (int k = num * 4; k < (num + 1) * 4; k++)
			{
				this.colors_[k] = linkedListNode.Value.VerticeColor;
			}
			for (int l = 0; l < 6; l++)
			{
				this.triangles_[linkedListNode.Value.SubMeshIndex][array[linkedListNode.Value.SubMeshIndex] * 6 + l] = array2[l] + num * 4;
			}
			linkedListNode.Value.SubMeshCounter = array[linkedListNode.Value.SubMeshIndex];
			array[linkedListNode.Value.SubMeshIndex]++;
			num++;
		}
		for (int m = 0; m < this.subMeshCounter_.Length; m++)
		{
			if (this.subMeshCounter_[m] <= 0)
			{
				break;
			}
			this.usefulSubMesh_++;
		}
		this.isSealed_ = true;
	}

	public bool Update()
	{
		bool flag = false;
		if (!this.isSealed_)
		{
			this.Seal();
			if (this.mainCam_ != null)
			{
				int count = this.panels_.Count;
				for (int i = 0; i < count * 4; i++)
				{
					this.transVertices_[i] = this.vertices_[i];
					Vector3[] array = this.transVertices_;
					int num = i;
					array[num].z = array[num].z + this.mainCam_.nearClipPlane;
					this.transVertices_[i] = this.mainCam_.ScreenToWorldPoint(this.transVertices_[i]);
				}
			}
			flag = true;
		}
		bool flag2 = false;
		int num2 = 0;
		for (LinkedListNode<GUIPanelEx> linkedListNode = this.panels_.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			GUIPanelEx value = linkedListNode.Value;
			int num3 = value.Dirty | ((!flag2) ? 0 : 255);
			if (num3 != 0)
			{
				if ((num3 & 12) != 0)
				{
					value.GetVerticesUVs(ref this.vertices_, ref this.uvs_, num2 * 4);
					if ((num3 & 8) != 0 && this.mainCam_ != null)
					{
						for (int j = num2 * 4; j < (num2 + 1) * 4; j++)
						{
							this.transVertices_[j] = this.vertices_[j];
							Vector3[] array2 = this.transVertices_;
							int num4 = j;
							array2[num4].z = array2[num4].z + this.mainCam_.nearClipPlane;
							this.transVertices_[j] = this.mainCam_.ScreenToWorldPoint(this.transVertices_[j]);
							if (ScreenController.Instance.NeedToBeForced() && ScreenController.Instance.IsDisplayingLandscapeRight())
							{
								Vector3.Scale(this.transVertices_[j], new Vector3(-1f, -1f, 1f));
							}
						}
					}
				}
				if ((num3 & 2) != 0)
				{
					for (int k = num2 * 4; k < (num2 + 1) * 4; k++)
					{
						this.colors_[k] = value.VerticeColor;
					}
				}
				if ((num3 & 1) != 0)
				{
					int num5 = ((!value.Visible) ? 1 : 0);
					for (int l = 0; l < 6; l++)
					{
						this.triangles_[linkedListNode.Value.SubMeshIndex][linkedListNode.Value.SubMeshCounter * 6 + l] = num2 * 4 + this.TRIANGLE_INDEX[num5][l];
					}
				}
				value.Dirty = 0;
				flag = true;
			}
			num2++;
		}
		return flag;
	}

	public void UpdateMesh(ref Mesh mesh)
	{
		mesh.Clear();
		if (!this.isSealed_)
		{
			return;
		}
		if (this.mainCam_ == null)
		{
			return;
		}
		mesh.vertices = this.transVertices_;
		if (this.usefulSubMesh_ <= 1)
		{
			mesh.subMeshCount = 0;
			mesh.triangles = this.triangles_[0];
		}
		else
		{
			mesh.subMeshCount = this.usefulSubMesh_;
			for (int i = 0; i < this.triangles_.Length; i++)
			{
				if (this.triangles_[i] != null)
				{
					mesh.SetTriangles(this.triangles_[i], i);
				}
			}
		}
		mesh.colors = this.colors_;
		mesh.uv = this.uvs_;
		mesh.RecalculateNormals();
	}

	private const int MAX_SUBMESH_COUNT = 8;

	public static GUIPanelManager ingame_;

	public LinkedList<GUIPanelEx> panels_ = new LinkedList<GUIPanelEx>();

	private bool isSealed_;

	private Vector3[] vertices_;

	public Vector3[] transVertices_;

	private Color[] colors_;

	public Vector2[] uvs_;

	private int[][] triangles_;

	private Camera mainCam_;

	private LinkedListNode<GUIPanelEx>[] layerNode_ = new LinkedListNode<GUIPanelEx>[8];

	private int[] subMeshCounter_ = new int[8];

	protected int usefulSubMesh_;

	protected ScreenOrientation prevOrientation_ = ScreenOrientation.LandscapeLeft;

	private int[][] TRIANGLE_INDEX = new int[][]
	{
		new int[] { 0, 1, 2, 1, 3, 2 },
		new int[] { 0, 2, 1, 1, 2, 3 }
	};
}
