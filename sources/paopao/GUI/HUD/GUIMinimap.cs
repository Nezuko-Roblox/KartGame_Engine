using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIMinimap : MonoBehaviourEx
{
	private void Start()
	{
		this.mesh_ = base.GetComponent<MeshFilter>().mesh;
		this.RegistMonoBehaviour(16);
		this.DefaultMeshSetting();
	}

	private int GetStartVerticeIdx(GUIMinimap.MinimapElemType type)
	{
		return (int)(type * (GUIMinimap.MinimapElemType)4);
	}

	private int GetStartTriangleIndex(GUIMinimap.MinimapElemType type)
	{
		return (int)(type * (GUIMinimap.MinimapElemType)6);
	}

	private void SetRectangle(GUIMinimap.MinimapElemType type, float x_half, float z_half)
	{
		Vector3[] array = new Vector3[]
		{
			new Vector3(-x_half, 0f, z_half),
			new Vector3(x_half, 0f, z_half),
			new Vector3(-x_half, 0f, -z_half),
			new Vector3(x_half, 0f, -z_half)
		};
		Vector2[] array2 = new Vector2[]
		{
			new Vector2(1f, 0f),
			new Vector2(0f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f)
		};
		int[] array3 = new int[] { 0, 1, 2, 1, 3, 2 };
		int startVerticeIdx = this.GetStartVerticeIdx(type);
		for (int i = 0; i < 4; i++)
		{
			this.vertices_[startVerticeIdx + i] = array[i];
			this.colors_[startVerticeIdx + i] = Color.white;
			this.uvs_[startVerticeIdx + i] = array2[i];
		}
		for (int j = 0; j < 6; j++)
		{
			this.triangles_[startVerticeIdx + j] = array3[j];
		}
	}

	private void DefaultMeshSetting()
	{
		int num = 1;
		this.vertices_ = new Vector3[num * 4];
		this.uvs_ = new Vector2[num * 4];
		this.colors_ = new Color[num * 4];
		this.triangles_ = new int[num * 6];
		this.SetRectangle(GUIMinimap.MinimapElemType.BOARD, 1024f, 1024f);
		this.mesh_.vertices = this.vertices_;
		this.mesh_.triangles = this.triangles_;
		this.mesh_.colors = this.colors_;
		this.mesh_.uv = this.uvs_;
		this.mesh_.RecalculateNormals();
	}

	private void Update()
	{
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.SHOW_UI)
		{
			MonoBehaviourMessage1Param<bool> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<bool>)msg;
			if (monoBehaviourMessage1Param != null)
			{
				base.gameObject.SetActiveRecursively(monoBehaviourMessage1Param.param_);
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.PAUSE || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL2)
		{
			base.gameObject.SetActiveRecursively(false);
		}
		else if (msg.type_ == MonoBehaviourMessageType.RESUME)
		{
			base.gameObject.SetActiveRecursively(true);
		}
	}

	private Vector3[] vertices_;

	private Color[] colors_;

	private Vector2[] uvs_;

	private int[] triangles_;

	private Mesh mesh_;

	public enum MinimapElemType
	{
		BOARD,
		OWNER,
		OTHER
	}
}
