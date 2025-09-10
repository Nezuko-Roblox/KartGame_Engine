/**
 * KartMove模块的统一导出文件
 * 提供完整的TypeScript卡丁车物理系统
 */

// 核心数学库
export { Vector2, UnityVector3, Quaternion, Mathf } from "../KartShared/UnityMath";

// 基础结构体和枚举
export { default as BoostKind } from "./BoostKind";
export { default as Control } from "./Control";
export { default as CollisionState } from "./CollisionState";
export { default as DriftControl } from "./DriftControl";
export { default as DriftGauge } from "./DriftGauge";

// 核心卡丁车类
export { default as GoKart } from "./GoKart";
export { GoPlayKart } from "./GoPlayKart";

// 控制器类
export { KartBasicController } from "./KartBasicController";
export { RigidbodyFPSWalker } from "./RigidbodyFPSWalker";

// 类型导出
export type { BoostKind as BoostKindType } from "./BoostKind";
export type { Control as ControlType } from "./Control";
export type { CollisionState as CollisionStateType } from "./CollisionState";
export type { DriftControl as DriftControlType } from "./DriftControl";
export type { DriftGauge as DriftGaugeType } from "./DriftGauge";
export type { GoKart as GoKartType } from "./GoKart";
export type { GoPlayKart as GoPlayKartType } from "./GoPlayKart";
export type { KartBasicController as KartBasicControllerType } from "./KartBasicController";
export type { RigidbodyFPSWalker as RigidbodyFPSWalkerType } from "./RigidbodyFPSWalker";

// 重新导入用于默认导出
import { Vector2, UnityVector3, Quaternion, Mathf } from "../KartShared/UnityMath";
import BoostKindType from "./BoostKind";
import ControlClass from "./Control";
import CollisionStateClass from "./CollisionState";
import DriftControlClass from "./DriftControl";
import DriftGaugeClass from "./DriftGauge";
import GoKartClass from "./GoKart";
import { GoPlayKart } from "./GoPlayKart";
import { KartBasicController } from "./KartBasicController";
import { RigidbodyFPSWalker } from "./RigidbodyFPSWalker";

// 默认导出整个模块
export default {
    // 数学库
    Vector2,
    Vector3,
    Quaternion,
    Mathf,
    
    // 枚举
    BoostKind: BoostKindType,
    
    // 结构体
    Control: ControlClass,
    CollisionState: CollisionStateClass,
    DriftControl: DriftControlClass,
    DriftGauge: DriftGaugeClass,
    
    // 核心类
    GoKart: GoKartClass,
    GoPlayKart,
    
    // 控制器
    KartBasicController,
    RigidbodyFPSWalker
};