/* eslint-disable jsdoc/require-description-complete-sentence -- no */
/* eslint-disable shopify/typescript-prefer-pascal-case-enums -- no */
/** 系统优先级枚举，值越小优先级越高. */
export enum SystemPriority {
	/** 关键优先级 */
	CRITICAL = -2,
	/** 高优先级 */
	HIGH = -1,
	/** 标准优先级 */
	STANDARD = 0,
	/** 低优先级 */
	LOW = 1,
	/** 后台优先级 */
	BACKGROUND = 2,
}
