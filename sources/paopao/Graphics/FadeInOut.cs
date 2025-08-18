using System;
using UnityEngine;

public class FadeInOut : MonoBehaviour
{
	public static FadeInOut Instance
	{
		get
		{
			if (FadeInOut.instance_ == null)
			{
				GameObject gameObject = (GameObject)global::UnityEngine.Object.Instantiate(Resources.Load("Prefabs/fade_in_out"));
				if (gameObject != null)
				{
					FadeInOut.instance_ = gameObject.GetComponent<FadeInOut>();
					if (FadeInOut.instance_ != null)
					{
						global::UnityEngine.Object.DontDestroyOnLoad(FadeInOut.instance_);
					}
				}
			}
			return FadeInOut.instance_;
		}
	}

	public static bool IsInstantiated()
	{
		return FadeInOut.instance_ != null;
	}

	private void Awake()
	{
		if (base.guiTexture == null)
		{
			base.gameObject.AddComponent<GUITexture>();
		}
		if (this.fadeTexture_ == null)
		{
			this.fadeTexture_ = new Texture2D(1, 1);
			this.fadeTexture_.SetPixel(0, 0, Color.black);
			this.fadeTexture_.Apply();
			global::UnityEngine.Object.DontDestroyOnLoad(this.fadeTexture_);
		}
		base.guiTexture.texture = this.fadeTexture_;
		base.guiTexture.pixelInset = new Rect(0f, 0f, (float)Screen.width, (float)Screen.height);
		this.SetScreenOverlayColor(this.currentScreenOverlayColor_);
	}

	private void FixedUpdate()
	{
		if (this.currentScreenOverlayColor_ != this.targetScreenOverlayColor_)
		{
			if (Mathf.Abs(this.currentScreenOverlayColor_.a - this.targetScreenOverlayColor_.a) < Mathf.Abs(this.deltaColor_.a) * Time.deltaTime)
			{
				this.currentScreenOverlayColor_ = this.targetScreenOverlayColor_;
				this.SetScreenOverlayColor(this.currentScreenOverlayColor_);
				this.deltaColor_.a = (this.deltaColor_.r = (this.deltaColor_.g = (this.deltaColor_.b = 0f)));
			}
			else
			{
				this.SetScreenOverlayColor(this.currentScreenOverlayColor_ + this.deltaColor_ * Time.deltaTime);
			}
		}
	}

	public void SetScreenOverlayColor(Color newScreenOverlayColor)
	{
		this.currentScreenOverlayColor_ = newScreenOverlayColor;
		if (base.guiTexture != null)
		{
			base.guiTexture.color = newScreenOverlayColor;
		}
	}

	public void StartFade(Color newScreenOverlayColor, float fadeDuration)
	{
		if (fadeDuration <= 0f)
		{
			this.SetScreenOverlayColor(newScreenOverlayColor);
		}
		else
		{
			this.targetScreenOverlayColor_ = newScreenOverlayColor;
			this.deltaColor_ = (this.targetScreenOverlayColor_ - this.currentScreenOverlayColor_) / fadeDuration;
		}
	}

	public void FadeIn()
	{
		this.fadeInOutState_ = FadeInOutState.FADE_IN;
		this.StartFade(new Color(0f, 0f, 0f, 0f), this.DEFAULT_FADE_TIME);
	}

	public void FadeOut()
	{
		this.fadeInOutState_ = FadeInOutState.FADE_OUT;
		this.StartFade(new Color(0f, 0f, 0f, 1f), this.DEFAULT_FADE_TIME);
	}

	public bool IsFadeEnd()
	{
		return this.fadeInOutState_ != FadeInOutState.NO_FADE && this.currentScreenOverlayColor_ == this.targetScreenOverlayColor_;
	}

	public bool IsFadeOutEnd()
	{
		return this.fadeInOutState_ == FadeInOutState.FADE_OUT && this.currentScreenOverlayColor_ == this.targetScreenOverlayColor_;
	}

	public bool IsFadeInEnd()
	{
		return this.fadeInOutState_ == FadeInOutState.FADE_IN && this.currentScreenOverlayColor_ == this.targetScreenOverlayColor_;
	}

	public void ResetFadeState()
	{
		this.fadeInOutState_ = FadeInOutState.NO_FADE;
	}

	private Texture2D fadeTexture_;

	private Color currentScreenOverlayColor_ = new Color(0f, 0f, 0f, 1f);

	private Color targetScreenOverlayColor_ = new Color(0f, 0f, 0f, 1f);

	private Color deltaColor_ = new Color(0f, 0f, 0f, 0f);

	private FadeInOutState fadeInOutState_;

	public static FadeInOut instance_;

	private float DEFAULT_FADE_TIME = 1f;
}
