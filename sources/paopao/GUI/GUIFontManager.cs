using System;
using UnityEngine;

public class GUIFontManager
{
	public static GUIFontManager Instance
	{
		get
		{
			if (GUIFontManager.instance_ == null)
			{
				GUIFontManager.instance_ = new GUIFontManager();
			}
			return GUIFontManager.instance_;
		}
	}

	public GUIFontCalculatorEx GetFont(int idx, FiaTexture mainTex)
	{
		return new GUIFontCalculatorEx(this.FONT_WIDTH[idx], this.FONT_HEIGHT[idx], new Vector2(0f, this.START_POS_Y[idx]), mainTex.UV, mainTex.OrgRect, this.FONT_STRING[idx]);
	}

	public void Clear()
	{
		for (int i = 0; i < 6; i++)
		{
			this.fonts_[i] = null;
		}
	}

	public const string DEFAULT_FONT_STRING = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.:,(*!?')/;-= ";

	public const string CURRENCY_FONT_STRING = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.:,(*!?')/;-=$%^&@# ";

	public const string RANK_FONT_STRING = "0123456789abcd";

	public const int FONT_SIZE = 6;

	public static GUIFontManager instance_;

	private float[][][] FONT_WIDTH = new float[][][]
	{
		new float[][]
		{
			new float[]
			{
				0f, 23f, 42f, 64f, 85f, 102f, 118f, 140f, 160f, 166f,
				179f, 199f, 216f, 242f, 262f, 287f, 306f, 331f, 351f, 369f,
				389f, 409f, 432f, 461f, 482f, 503f, 523f
			},
			new float[]
			{
				0f, 17f, 35f, 53f, 71f, 89f, 104f, 122f, 139f, 145f,
				155f, 172f, 178f, 204f, 221f, 240f, 258f, 276f, 289f, 303f,
				317f, 334f, 353f, 378f, 396f, 414f, 430f
			},
			new float[]
			{
				0f, 17f, 29f, 46f, 63f, 82f, 100f, 118f, 135f, 152f,
				170f, 177f, 184f, 191f, 201f, 215f, 221f, 236f, 242f, 252f,
				266f, 273f, 287f, 304f, 314f
			}
		},
		new float[][]
		{
			new float[]
			{
				0f, 21f, 35f, 57f, 75f, 89f, 104f, 124f, 142f, 148f,
				160f, 177f, 192f, 215f, 232f, 255f, 271f, 294f, 311f, 328f,
				346f, 364f, 384f, 411f, 429f, 448f, 466f
			},
			new float[]
			{
				0f, 15f, 32f, 49f, 65f, 82f, 95f, 111f, 126f, 132f,
				141f, 156f, 162f, 185f, 201f, 219f, 236f, 252f, 264f, 277f,
				288f, 304f, 321f, 344f, 359f, 376f, 390f
			},
			new float[]
			{
				0f, 16f, 27f, 42f, 57f, 74f, 90f, 106f, 122f, 139f,
				155f, 161f, 167f, 174f, 183f, 194f, 201f, 216f, 223f, 232f,
				242f, 249f, 262f, 278f, 287f
			}
		},
		new float[][]
		{
			new float[]
			{
				0f, 19f, 35f, 53f, 71f, 86f, 100f, 118f, 134f, 141f,
				152f, 168f, 182f, 204f, 221f, 242f, 258f, 279f, 295f, 311f,
				328f, 345f, 364f, 389f, 406f, 424f, 441f
			},
			new float[]
			{
				0f, 14f, 30f, 45f, 60f, 76f, 88f, 103f, 117f, 123f,
				131f, 145f, 151f, 173f, 187f, 203f, 218f, 233f, 244f, 257f,
				269f, 283f, 299f, 320f, 335f, 351f, 365f
			},
			new float[]
			{
				0f, 16f, 27f, 42f, 56f, 72f, 87f, 102f, 117f, 132f,
				147f, 153f, 159f, 166f, 175f, 187f, 192f, 206f, 212f, 220f,
				232f, 238f, 250f, 264f, 273f
			}
		},
		new float[][]
		{
			new float[]
			{
				0f, 19f, 34f, 51f, 67f, 80f, 93f, 111f, 126f, 131f,
				142f, 157f, 171f, 192f, 208f, 228f, 243f, 263f, 278f, 293f,
				308f, 324f, 342f, 365f, 381f, 398f, 414f
			},
			new float[]
			{
				0f, 13f, 27f, 41f, 55f, 70f, 82f, 97f, 110f, 115f,
				123f, 136f, 141f, 162f, 176f, 191f, 205f, 219f, 230f, 242f,
				253f, 267f, 282f, 302f, 316f, 331f, 344f
			},
			new float[]
			{
				0f, 14f, 24f, 38f, 52f, 68f, 82f, 96f, 110f, 124f,
				138f, 144f, 150f, 156f, 165f, 176f, 182f, 195f, 201f, 210f,
				221f, 227f, 238f, 252f, 265f, 280f, 297f, 315f, 337f, 357f,
				366f
			}
		},
		new float[][]
		{
			new float[]
			{
				0f, 16f, 29f, 44f, 58f, 70f, 81f, 96f, 110f, 115f,
				124f, 137f, 149f, 167f, 181f, 198f, 211f, 229f, 242f, 255f,
				269f, 283f, 298f, 319f, 333f, 347f, 361f
			},
			new float[]
			{
				0f, 12f, 25f, 37f, 50f, 63f, 73f, 85f, 97f, 102f,
				109f, 120f, 125f, 143f, 154f, 168f, 180f, 192f, 201f, 212f,
				222f, 234f, 248f, 265f, 276f, 289f, 301f
			},
			new float[]
			{
				0f, 12f, 22f, 34f, 46f, 60f, 72f, 85f, 97f, 109f,
				121f, 126f, 131f, 136f, 143f, 153f, 158f, 170f, 175f, 182f,
				192f, 197f, 207f, 219f, 226f
			}
		},
		new float[][]
		{
			new float[]
			{
				0f, 12f, 22f, 34f, 46f, 55f, 64f, 79f, 87f, 91f,
				98f, 108f, 117f, 130f, 141f, 153f, 163f, 176f, 185f, 195f,
				206f, 217f, 229f, 244f, 254f, 266f, 276f
			},
			new float[]
			{
				0f, 10f, 20f, 30f, 39f, 48f, 56f, 66f, 76f, 81f,
				86f, 95f, 99f, 113f, 123f, 133f, 143f, 152f, 159f, 167f,
				174f, 183f, 193f, 206f, 216f, 226f, 235f
			},
			new float[]
			{
				0f, 10f, 17f, 27f, 40f, 47f, 57f, 67f, 77f, 87f,
				97f, 101f, 105f, 109f, 115f, 122f, 127f, 136f, 140f, 146f,
				154f, 159f, 167f, 176f, 183f
			}
		},
		new float[][] { new float[]
		{
			0f, 21f, 36f, 59f, 81f, 103f, 125f, 147f, 168f, 191f,
			213f, 240f
		} }
	};

	private float[] FONT_HEIGHT = new float[] { 36f, 32f, 31f, 28f, 25f, 19f, 32f };

	private string[] FONT_STRING = new string[] { "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.:,(*!?')/;-= ", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.:,(*!?')/;-= ", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.:,(*!?')/;-= ", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.:,(*!?')/;-=$%^&@# ", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.:,(*!?')/;-= ", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.:,(*!?')/;-= ", "0123456789abcd" };

	private float[] START_POS_Y = new float[] { 0f, 108f, 204f, 297f, 381f, 456f, 513f };

	private GUIFontCalculatorEx[] fonts_ = new GUIFontCalculatorEx[6];
}
