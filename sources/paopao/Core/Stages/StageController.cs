using System;
using System.Collections.Generic;
using UnityEngine;

public class StageController : MonoBehaviour
{
	public static StageController Instance
	{
		get
		{
			if (StageController.instance_ == null)
			{
				GameObject gameObject = (GameObject)global::UnityEngine.Object.Instantiate(Resources.Load("Prefabs/stage_controller"));
				if (gameObject != null)
				{
					StageController.instance_ = gameObject.GetComponent<StageController>();
					if (StageController.instance_ != null)
					{
						global::UnityEngine.Object.DontDestroyOnLoad(StageController.instance_);
					}
				}
			}
			return StageController.instance_;
		}
	}

	public static bool IsInstantiated()
	{
		return StageController.instance_ != null;
	}

	public static bool IsInputAuthorized(byte inputAuthority)
	{
		return !StageController.IsInstantiated() || (StageController.instance_.inputAuthority_ & inputAuthority) != 0;
	}

	public void RegistMonoBehaviour(int id, MonoBehaviourStage behaviour)
	{
		if (!this.listener_.ContainsKey(id))
		{
			this.listener_.Add(id, behaviour);
		}
		else if (Debug.isDebugBuild)
		{
			Debug.LogError("Already Exist MonobehaviourCenter : " + id.ToString());
		}
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
		this.SetAudioVolume(this.currentVolume_);
		if (this.fx_ == null)
		{
			this.fx_ = new AudioSourceEx[3];
			for (int i = 0; i < 3; i++)
			{
				AudioClip audioClip = (AudioClip)Resources.Load(this.fxPath[i], typeof(AudioClip));
				if (audioClip != null)
				{
					global::UnityEngine.Object.DontDestroyOnLoad(audioClip);
					SoundController.CreateAudioSource(base.gameObject, new FxClipSetting(audioClip, false), out this.fx_[i], AudioSourceType.FX);
				}
			}
		}
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
		if (this.currentVolume_ != this.targetVolume_)
		{
			if (Mathf.Abs(this.currentVolume_ - this.targetVolume_) < Mathf.Abs(this.deltaVolume_) * Time.deltaTime)
			{
				this.currentVolume_ = this.targetVolume_;
				this.SetAudioVolume(this.currentVolume_);
				this.deltaVolume_ = 0f;
			}
			else
			{
				this.SetAudioVolume(this.currentVolume_ + this.deltaVolume_ * Time.deltaTime);
			}
		}
	}

	public bool IsWaitForReceiveReadyToChangeMessage
	{
		get
		{
			return this.isWaitForReceiveReadyToChangeMessage_;
		}
		set
		{
			this.isWaitForReceiveReadyToChangeMessage_ = value;
		}
	}

	private void Update()
	{
		if (this.nextStage_ != StageType.NONE && this.IsFadeOutEnd())
		{
			if (!this.isWaitForReceiveReadyToChangeMessage_)
			{
				this.readyToChangeIdList_.Clear();
				foreach (KeyValuePair<int, MonoBehaviourStage> keyValuePair in this.listener_)
				{
					this.readyToChangeIdList_.Add(keyValuePair.Key);
				}
				MonoBehaviourMessage1Param<StageType> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<StageType>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.CHANGE_SCENE);
				MonoBehaviourExCenter.Instance.BroadcastMessage(0, monoBehaviourMessage1Param.Initialize(this.nextStage_));
				this.isWaitForReceiveReadyToChangeMessage_ = true;
			}
			else if (this.readyToChangeIdList_.Count == 0)
			{
				this.isWaitForReceiveReadyToChangeMessage_ = false;
				this.stageControllerState_ = StageControllerState.NO_FADE;
				MonoBehaviourExCenter.Instance.UnloadStage();
				this.listener_.Clear();
				this.inputAuthority_ = byte.MaxValue;
				GUIFontManager.Instance.Clear();
				StageType stage = this.nextStage_;
				if (KartDefine.NEED_LOAING_SCENE[(int)this.nextStage_] != KartDefine.BetweenStageOption.NONE)
				{
					GameLoadingStageStaticVariable.prevStage_ = KartManager.Instance.parameter_.Stage;
					GameLoadingStageStaticVariable.nextStage_ = this.nextStage_;
					KartManager.Instance.parameter_.Stage = ((KartDefine.NEED_LOAING_SCENE[(int)this.nextStage_] != KartDefine.BetweenStageOption.GAME_LOADING) ? StageType.LOADING_DEFAULT : StageType.LOADING_GAME);
					if (this.bgmName_ != this.BGM_ARRAY[(int)GameLoadingStageStaticVariable.nextStage_])
					{
						stage = KartManager.Instance.parameter_.Stage;
					}
				}
				else
				{
					KartManager.Instance.parameter_.Stage = this.nextStage_;
				}
				KartOptions.Instance.SaveRegistry();
				GC.Collect();
				this.nextStage_ = StageType.NONE;
				Application.LoadLevel(KartDefine.STAGE_SCNE_NAME[(int)KartManager.Instance.parameter_.Stage]);
				this.SetBgm(stage);
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

	public void SetAudioVolume(float volume)
	{
		this.currentVolume_ = volume;
		AudioListener.volume = volume;
	}

	public void StartFade(Color newScreenOverlayColor, float fadeDuration)
	{
		if (fadeDuration <= 0f)
		{
			this.targetScreenOverlayColor_ = newScreenOverlayColor;
			this.currentScreenOverlayColor_ = this.targetScreenOverlayColor_;
			this.SetScreenOverlayColor(this.currentScreenOverlayColor_);
			this.deltaColor_.a = (this.deltaColor_.r = (this.deltaColor_.g = (this.deltaColor_.b = 0f)));
		}
		else
		{
			this.targetScreenOverlayColor_ = newScreenOverlayColor;
			this.deltaColor_ = (this.targetScreenOverlayColor_ - this.currentScreenOverlayColor_) / fadeDuration;
		}
	}

	public void StartVolumeFade(float newVolume, float fadeDuration)
	{
		if (fadeDuration <= 0f)
		{
			this.SetAudioVolume(newVolume);
		}
		else
		{
			this.targetVolume_ = newVolume;
			this.deltaVolume_ = (this.targetVolume_ - this.currentVolume_) / fadeDuration;
		}
	}

	public void BeginStage()
	{
		if (this.nextStage_ != StageType.NONE)
		{
			return;
		}
		this.stageControllerState_ = StageControllerState.FADE_IN;
		this.SetScreenOverlayColor(this.FADE_OUT_END_COLOR);
		this.StartFade(this.FADE_IN_END_COLOR, this.DEFAULT_FADE_TIME);
		this.FadeInBgm();
		ScreenController.Instance.RestoreDisplayingOrientation();
		if (iPhoneKeyboard.hideInput)
		{
			iPhoneKeyboard.hideInput = false;
		}
	}

	public void ChangeStage(StageType nextStage)
	{
		this.inputAuthority_ = 0;
		this.nextStage_ = nextStage;
		this.stageControllerState_ = StageControllerState.FADE_OUT;
		this.StartFade(this.FADE_OUT_END_COLOR, this.DEFAULT_FADE_TIME);
		this.FadeOutBgm(this.nextStage_);
	}

	public bool Lock
	{
		get
		{
			return this.stageControllerState_ != StageControllerState.NO_FADE && !this.IsFadeInEnd();
		}
	}

	private bool IsFadeEnd()
	{
		return this.stageControllerState_ != StageControllerState.NO_FADE && this.currentScreenOverlayColor_ == this.targetScreenOverlayColor_;
	}

	private bool IsFadeOutEnd()
	{
		return this.stageControllerState_ == StageControllerState.FADE_OUT && this.currentScreenOverlayColor_ == this.targetScreenOverlayColor_;
	}

	private bool IsFadeInEnd()
	{
		return this.stageControllerState_ == StageControllerState.FADE_IN && this.currentScreenOverlayColor_ == this.targetScreenOverlayColor_;
	}

	public void ReadyToChangeScene(int id)
	{
		this.readyToChangeIdList_.Remove(id);
	}

	public byte InputAutority
	{
		get
		{
			return this.inputAuthority_;
		}
		set
		{
			this.inputAuthority_ = value;
		}
	}

	public StageType NextStage
	{
		get
		{
			return this.nextStage_;
		}
	}

	public void PlaySound(StageController.FxType fxType)
	{
		if (this.fx_ != null && this.fx_[(int)fxType] != null && !this.fx_[(int)fxType].isPlaying)
		{
			this.fx_[(int)fxType].Play();
		}
	}

	public void InitBgmVolume()
	{
		this.currentVolume_ = 0f;
		this.targetVolume_ = 0f;
		this.deltaVolume_ = 0f;
		AudioListener.volume = 0f;
	}

	public void FadeOutBgm(StageType nextStage)
	{
		if (this.bgmName_ != this.BGM_ARRAY[(int)nextStage])
		{
			this.StartVolumeFade(0f, this.DEFAULT_FADE_TIME);
		}
	}

	public void FadeInBgm()
	{
		this.StartVolumeFade(1f, this.DEFAULT_FADE_TIME);
		if (this.bgm_ != null && !this.bgm_.isPlaying && KartOptions.Instance.Bgm)
		{
			this.bgm_.Play();
		}
	}

	public void SetBgm(StageType stage)
	{
		if (this.bgmName_ != this.BGM_ARRAY[(int)stage])
		{
			this.InitBgmVolume();
			this.bgmName_ = this.BGM_ARRAY[(int)stage];
			if (this.bgm_ != null)
			{
				global::UnityEngine.Object.DestroyImmediate(this.bgm_.GetAudioSource());
				this.bgm_ = null;
			}
			if (this.bgmName_ != string.Empty)
			{
				string text = string.Format("bgm/menu/{0}", this.bgmName_);
				AudioClip audioClip = (AudioClip)Resources.Load(text, typeof(AudioClip));
				if (audioClip != null)
				{
					SoundController.CreateAudioSource(base.gameObject, new FxClipSetting(audioClip, true), out this.bgm_, AudioSourceType.BGM);
					global::UnityEngine.Object.DontDestroyOnLoad(this.bgm_.GetAudioSource());
				}
			}
		}
	}

	public void DidBecomeActive()
	{
		if (this.bgm_ != null && KartOptions.Instance.Bgm && !this.bgm_.isPlaying)
		{
			this.bgm_.Play();
		}
	}

	public void WillResignActive()
	{
		if (this.bgm_ != null)
		{
			this.bgm_.Stop();
		}
	}

	public const byte INPUT_LOCK = 0;

	public const byte INPUT_UNLOCK_ALL = 3;

	public const byte INPUT_UNLOCK = 255;

	private Texture2D fadeTexture_;

	private Color currentScreenOverlayColor_ = new Color(0f, 0f, 0f, 1f);

	private Color targetScreenOverlayColor_ = new Color(0f, 0f, 0f, 1f);

	private Color deltaColor_ = new Color(0f, 0f, 0f, 0f);

	private float currentVolume_;

	private float targetVolume_;

	private float deltaVolume_;

	private StageControllerState stageControllerState_;

	private byte inputAuthority_ = byte.MaxValue;

	private float DEFAULT_FADE_TIME = 0.5f;

	private StageType nextStage_;

	public static StageController instance_;

	private Dictionary<int, MonoBehaviourStage> listener_ = new Dictionary<int, MonoBehaviourStage>();

	private string[] fxPath = new string[] { "fx/click", "fx/mouse_over", "fx/start_button" };

	protected AudioSourceEx[] fx_;

	private List<int> readyToChangeIdList_ = new List<int>();

	private bool isWaitForReceiveReadyToChangeMessage_;

	private Color FADE_OUT_END_COLOR = new Color(0f, 0f, 0f, 1f);

	private Color FADE_IN_END_COLOR = new Color(0f, 0f, 0f, 0f);

	private string[] BGM_ARRAY = new string[]
	{
		string.Empty,
		"title",
		"single",
		"single",
		"multi",
		"multi",
		"multi",
		"shop",
		string.Empty,
		string.Empty,
		"title",
		string.Empty,
		string.Empty,
		"title",
		"shop"
	};

	private string bgmName_ = string.Empty;

	protected AudioSourceEx bgm_;

	public enum FxType
	{
		CLICK,
		SELECT,
		START,
		SIZE
	}
}
