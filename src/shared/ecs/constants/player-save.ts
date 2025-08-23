/** 玩家存档数据结构 V1 版本. */
export interface PlayerSaveV1 {
	/** 最后登录时间戳. */
	lastLogin: number;
	/** 最后保存时间戳. */
	lastSave: number;
	/** 玩家设置. */
	settings: {
		/** 是否启用音乐. */
		musicEnabled: boolean;
		/** 音乐音量. */
		musicVolume: number;
		/** 音效音量. */
		sfxVolume: number;
	};
	/** 存档版本号. */
	version: 1;
}

/** 任意版本的存档类型联合. */
export type AnySave = PlayerSaveV1;
/** 当前使用的存档类型. */
export type CurrentPlayerSave = PlayerSaveV1;
/** 当前存档版本号. */
export const CurrentSaveVersion = 1;
