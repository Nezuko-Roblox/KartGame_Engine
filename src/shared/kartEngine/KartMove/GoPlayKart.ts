import { GoKart } from "./GoKart";
import { BoostKind } from "./BoostKind";
import { UnityVector3, Mathf } from "../KartShared/UnityMath";
import { Time } from "../KartShared/UnityTime";
import { Matrix3 } from "./Matrix3";
import { DriveFactor } from "./DriveFactor";
import { PhysicSpec } from "./PhysicSpec";
import { Suspension } from "./Suspension";
import { DriftControl } from "./DriftControl";
import { DriftGauge } from "./DriftGauge";
import { Control } from "./Control";
import { CollisionState } from "./CollisionState";
import { External } from "./External";
import { StuckHelper } from "./StuckHelper";
import { FirstPipelineValue } from "./FirstPipelineValue";
import { AdBoost } from "../Boost/AdBoost";
import { Physics } from "../KartShared/UnityEngine/Physics";
import { Transform } from "../KartShared/RobloxUnityAdapter";

export class GoPlayKart extends GoKart {
    // 初始化GoPlayKart特有属性
    public m_theGravity: UnityVector3 = new UnityVector3(0, -49, 0);
    public m_NetWForce: UnityVector3 = UnityVector3.zero;
    public m_NetLTorque: UnityVector3 = UnityVector3.zero;
    public m_boostLeft: number = 0;
    public m_slipBoost: boolean = false;
    public m_Contact: boolean = true;
    
    // 初始化各种结构体
    public m_spec: PhysicSpec = new PhysicSpec();
    public m_sus: Suspension = new Suspension();
    public m_drift: DriftControl = new DriftControl();
    public m_driftGauge: DriftGauge = new DriftGauge();
    public m_adBoost: AdBoost = new AdBoost();
    public m_ctrl: Control = new Control();
    public m_cState: CollisionState = new CollisionState();
    public m_extern: External = new External();
    public m_stuckHelper: StuckHelper = new StuckHelper();
    public m_first: FirstPipelineValue = new FirstPipelineValue();
    
    // 初始化DriveFactor数组
    public m_DriveFactor: DriveFactor[][] = [];
    
    public m_ort: Matrix3 = Matrix3.CreateMtxIdentity();
    
    // GoPlayKart特有属性
    public wheelLocalPos_: UnityVector3[] = [];
    public isCrash_: boolean = false;
    public crashVelocity_: number = 0;
    public isShock_: boolean = false;
    public shockVelocity_: number = 0;
    public m_slipReserveTime: number = 0;
    public m_steer: number = 0;
    public m_grip: number = 0;
    public m_slip: number[] = [2, 0.5];
    public m_DriveMode: number = 0;
    public m_reciprocalMass: UnityVector3 = UnityVector3.zero;
    
    // Unity中的其他属性
    public isBackupStreer: boolean[] = [false, false, false];
    public m_isDrift: boolean = false;
    public needReset_: boolean = false;
    public backupVelocity_: UnityVector3 = UnityVector3.zero;
    // BoostKind属性
    public m_BoostKind: BoostKind = BoostKind.NoBoost;
    public basicActionTick: number = 0;
    
    constructor() {
        super();
        
        // 初始化各种结构体
        this.m_spec.Initialize();
        this.m_sus.Initialize();
        this.m_drift.Initialize();
        this.m_driftGauge.Initialize();
        this.m_adBoost.Initialize();
        this.m_ctrl.Initialize();
        this.m_cState.Initialize();
        this.m_extern.Initialize();
        this.m_stuckHelper.Initialize();
        this.m_first.Initialize();
        
        // 初始化DriveFactor数组 - 对应Lua: for i = 1, 3; for j = 1, 2
        // Lua中使用[1][1], [1][2], [2][1], [2][2], [3][1], [3][2]
        // TypeScript中应该使用[0][0], [0][1], [1][0], [1][1], [2][0], [2][1]
        for (let i = 0; i < 3; i++) {
            this.m_DriveFactor[i] = [];
            for (let j = 0; j < 2; j++) {
                this.m_DriveFactor[i]![j] = new DriveFactor();
                this.m_DriveFactor[i]![j]!.Initialize();
            }
        }
        
        // 调用loadParam来加载参数
        this.loadParam();
    }
    
    // 加载参数方法
    public loadParam(): void {
        this.m_slipReserveTime = 0;
        this.m_steer = 0;
        this.m_grip = 0;
        this.m_slip[0] = 2;  // Lua: m_slip[1] = 2
        this.m_slip[1] = 0.5; // Lua: m_slip[2] = 0.5
        
        // 设置DriveFactor参数 - 注意Lua索引从1开始，TypeScript从0开始
        // Lua: m_DriveFactor[2][1] -> TypeScript: m_DriveFactor[1][0]
        this.m_DriveFactor[1]![0]!.speedLimit = 340;
        this.m_DriveFactor[1]![0]!.betaCut = 0.6;
        this.m_DriveFactor[1]![0]!.frontGripFactor = -1;
        this.m_DriveFactor[1]![0]!.rearGripFactor = -1;
        // Lua: m_DriveFactor[2][2] -> TypeScript: m_DriveFactor[1][1]
        this.m_DriveFactor[1]![1]!.betaCut = 0.85;
        // Lua: m_DriveFactor[3][1] -> TypeScript: m_DriveFactor[2][0]
        this.m_DriveFactor[2]![0]!.speedLimit = 180;
        this.m_DriveFactor[2]![0]!.betaCut = 0.6;
        this.m_DriveFactor[2]![0]!.driftSlipFactor = 0.5;
        // Lua: m_DriveFactor[3][2] -> TypeScript: m_DriveFactor[2][1]
        this.m_DriveFactor[2]![1]!.frontGripFactor = -2;
        this.m_DriveFactor[2]![1]!.rearGripFactor = -2;
    }
    
    // basicAction方法 - 对应Unity版本的完整逻辑
    public basicAction(tick: number): void {
        this.m_isDrift = false;
        this.basicActionTick = tick; // 保存tick用于调试
        // 获取真实的帧间隔时间，对应Unity的Time.fixedDeltaTime
        const fixedDeltaTime = Time.getFixedDeltaTime();
        
        if (!this.GetForcing() && !this.GetIsInResetState()) {
            // 处理加速剩余时间
            if (this.m_boostLeft > 0) {
                this.m_boostLeft = this.m_boostLeft - Mathf.Min(this.m_boostLeft, math.floor(fixedDeltaTime * 1000));
                if (this.m_boostLeft === 0) {
                    this.m_BoostKind = BoostKind.NoBoost;
                }
            }
            
            this.processAdBoostTime(fixedDeltaTime);
            this.beginNetForce(fixedDeltaTime);
            const flag = this.decideKartContact(fixedDeltaTime, false);
            
            if (flag) {
                this.calcNonpenetrateForce(fixedDeltaTime);
                this.calcKartTractionForce(fixedDeltaTime);
                this.calcKartSteeringForce(fixedDeltaTime);
            } else {
                this.calcFlyingKartForce();
            }
            
            this.calcResistForce();
            this.endNetForce(fixedDeltaTime);
            this.ProcessDriftGauge(fixedDeltaTime);
            this.m_isDrift = this.m_isDrift || this.m_drift.slipMode || this.m_drift.slipTime > 0 || this.m_drift.forceSlip;
        } else {
            this.m_KartWLVel = UnityVector3.zero;
            this.m_KartLAVel = UnityVector3.zero;
            this.m_NetWForce = UnityVector3.zero;
            this.m_NetLTorque = UnityVector3.zero;
            if (this.m_boostLeft > 0) {
                this.m_boostLeft = this.m_boostLeft - Mathf.Min(this.m_boostLeft, math.floor(fixedDeltaTime * 1000));
                if (this.m_boostLeft === 0) {
                    this.m_BoostKind = BoostKind.NoBoost;
                }
            }
        }
        
        // 调用父类basicAction
        super.basicAction(tick);
    }
    
    // calcResistForce方法 - Unity原版第109-121行
    public calcResistForce(): void {
        let vector = UnityVector3.zero;
        let vector2 = UnityVector3.zero;
        
        // Unity原版逻辑：完全对应，不添加额外的漂移阻力处理
        vector = vector.sub(this.m_KartWLVel.mul(this.m_spec.airFriction));
        vector2 = vector2.sub(this.m_KartLAVel.mul(this.m_spec.airFriction));
        if (this.m_Contact) {
            // 漂移时动态调整阻力，减少降速
            let driftDragFactor = this.m_extern.dragFactor;
            if (this.m_drift && (this.m_drift.slipMode || this.m_drift.forceSlip || this.m_drift.trigger)) {
                // 漂移时减少阻力系数，降低降速程度，使用PhysicSpec中的可配置参数
                driftDragFactor = driftDragFactor * this.m_spec.driftDragReduceFactor;
            }
            vector = vector.sub(this.m_KartWLVel.mul(this.m_KartWLVel.getMagnitude() * this.m_spec.dragFactor * driftDragFactor * this.m_extern.compensationDragFactor));
        }
        this.m_NetWForce = this.m_NetWForce.add(vector);
        this.m_NetLTorque = this.m_NetLTorque.add(vector2);
    }
    
    // calcNonpenetrateForce方法 - 完全对应Unity版本124-143行
    public calcNonpenetrateForce(deltaT: number): void {
        // Lua: for num = 0, 3 do, 访问 wheelContact[num + 1]
        // 所以实际访问的是索引 1, 2, 3, 4
        // 由于Suspension使用1基数组，我们需要访问 [num + 1]
        for (let num = 0; num < 4; num++) {
            let num2: number;
            if (this.m_sus.wheelContact[num + 1]) {
                if (this.m_sus.deltaTravel[num + 1]! <= 0) {
                    num2 = this.m_spec.springK * this.m_sus.travel[num + 1]! + this.m_spec.damperRebC * (this.m_sus.deltaTravel[num + 1]! / deltaT);
                } else {
                    num2 = this.m_spec.springK * this.m_sus.travel[num + 1]! + this.m_spec.damperCopC * (this.m_sus.deltaTravel[num + 1]! / deltaT);
                }
            } else {
                num2 = 0;
            }
            
            if (num2 <= 0) {
                num2 = 0;
            } else {
                num2 = UnityVector3.Dot(this.m_sus.wheelContactN[num + 1]!, this.m_first.up) * num2;
            }
            
            const vector = new UnityVector3(this.m_spec.width * this.m_sus.wheelOff[num + 1]!.X, 0, -this.m_spec.length * this.m_sus.wheelOff[num + 1]!.Y);
            const vector2 = UnityVector3.Cross(vector, new UnityVector3(0, num2, 0)).mul(0.1);
            this.m_NetLTorque = this.m_NetLTorque.add(vector2);
        }
        
        // 添加重力影响
        this.m_NetWForce = this.m_NetWForce.add(this.m_theGravity.mul(this.m_spec.mass * this.m_extern.gravityFactor * 0.8));
    }
    
    // calcKartTractionForce方法 - 完全对应Unity源码146-234行
    public calcKartTractionForce(deltaT: number): void {
        if (this.m_extern.slip) {
            return;
        }
        
        const vector = UnityVector3.Cross(this.m_first.left, this.m_sus.contactN);
        
        if (this.m_ctrl.getRealAccel() !== 0 && !this.GetStuck()) {
            if (this.isRealBoost()) {
                let num = 4.5;
                if (this.m_BoostKind === BoostKind.BoostDrift) {
                    num = 5;
                }
                this.m_NetWForce = this.m_NetWForce.add(vector.mul(this.m_ctrl.getRealAccel() * num * 
                                  (!this.m_drift.forceSlip ? this.m_spec.forwardAccel : this.m_spec.driftEscapeForce)));
            } else {
                this.m_NetWForce = this.m_NetWForce.add(vector.mul(this.m_ctrl.getRealAccel() * 
                                  (!this.m_drift.forceSlip ? this.m_spec.forwardAccel : this.m_spec.driftEscapeForce)));
            }
            
            if (this.m_first.frontVel < 0) {
                if (this.m_drift.slipMode || this.m_drift.forceSlip) {
                    this.m_NetWForce = this.m_NetWForce.add(this.m_first.front.mul(this.m_first.speed * this.m_spec.mass * 9.8));
                } else {
                    this.m_NetWForce = this.m_NetWForce.add(this.m_first.front.mul(Mathf.Min(5, this.m_first.speed) * this.m_spec.mass * 9.8));
                }
            }
            this.m_ctrl.stayTime = 0;
            
        } else if (this.m_ctrl.getRealBrake() !== 0 || this.GetStuck()) {
            let flag = true;
            if (this.m_first.frontVel < 0.5) {
                this.m_ctrl.stayTime = this.m_ctrl.stayTime + deltaT;
                if (this.m_first.frontVel < -0.5) {
                    this.m_ctrl.stayTime = 1;
                }
                if (this.m_ctrl.stayTime > 0.2) {
                    if (this.GetStuck()) {
                        if (this.m_first.frontVel >= -0.5) {
                            this.m_KartWLVel = UnityVector3.zero;
                            flag = false;
                        }
                    } else {
                        this.m_NetWForce = this.m_NetWForce.add(vector.mul(this.m_ctrl.getRealBrake() * -this.m_spec.backwardAccel));
                        flag = false;
                    }
                } else if (Mathf.Abs(this.m_first.leftVel) < 0.2) {
                    this.m_KartWLVel = UnityVector3.zero;
                    flag = false;
                }
            }
            
            if (flag || (this.m_extern.speedLimit > 0 && this.m_KartWLVel.getSqrMagnitude() > 0)) {
                let vector2 = this.m_KartWLVel.getNormalized();
                const dotProduct = UnityVector3.Dot(vector2, this.m_first.up);
                vector2 = vector2.sub(this.m_first.up.mul(dotProduct));
                if (UnityVector3.Dot(vector2, vector) > 0.8) {
                    this.m_NetWForce = this.m_NetWForce.sub(vector2.mul(this.m_spec.gripBrake));
                } else {
                    this.m_NetWForce = this.m_NetWForce.sub(vector2.mul(this.m_spec.slipBrake));
                }
            }
            
        } else if (this.m_first.frontVel <= 0.5 && this.m_first.frontVel >= -0.5) {
            this.m_ctrl.stayTime = this.m_ctrl.stayTime + deltaT;
        }
    }
    
    // 获取转向角度（弧度转度数）
    public getSteerAngle(): number {
        return (this.m_ctrl.steerAngle || 0) * 180 / math.pi;
    }
    
    // calcKartSteeringForce方法 - 完全对应Unity源码242-457行
    public calcKartSteeringForce(deltaT: number): void {
        if (this.m_extern.slip) {
            return;
        }
        const num = Mathf.Sqrt(this.m_first.frontVel * this.m_first.frontVel + this.m_first.leftVel * this.m_first.leftVel);
        const num2 = (this.m_first.frontVel <= 0) ? -1 : 1;
        
        const baseSteerAngle = this.m_ctrl.getRealSteer() * this.m_spec.getMaxSteerRad();
        const steerReduction = Mathf.Exp(-Mathf.Abs(this.m_first.frontVel / this.m_spec.steerConstraint * this.m_extern.wheelFactor));
        this.m_ctrl.steerAngle = baseSteerAngle * steerReduction;
        
        // 初始化isBackupStreer数组 - Lua中索引是1,2,3，TypeScript中是0,1,2
        this.isBackupStreer[0] = this.m_ctrl.getRealAccel() !== 0;  // Lua: isBackupStreer[1]
        this.isBackupStreer[1] = this.m_ctrl.oldSteerAngle * this.m_ctrl.steerAngle > 0; // Lua: isBackupStreer[2]
        this.isBackupStreer[2] = Mathf.Abs(this.m_ctrl.oldSteerAngle) < Mathf.Abs(this.m_ctrl.steerAngle); // Lua: isBackupStreer[3]
        
        if (this.isBackupStreer[0] && this.isBackupStreer[1] && this.isBackupStreer[2]) {
            // 空实现块
        } else {
            this.m_ctrl.oldSteerAngle = this.m_ctrl.steerAngle;
        }
        
        const num3 = 0.5;
        const num4 = 0.5;
        let flag = false;
        let num5 = 0;
        const flag2 = this.m_drift.slipMode || this.m_drift.forceSlip;
        this.m_drift.forceSlip = false;
        
        if (num > 5) {
            const num6 = this.m_KartLAVel.Y * num3 / num;
            const num7 = this.m_KartLAVel.Y * num4 / num;
            const num8 = this.m_first.leftVel / num;
            let flag3 = false;
            
            // Unity原版漂移触发条件：第288行
            if (!this.m_drift.slipMode && !this.m_drift.trigger && Mathf.Abs(this.m_first.leftVel) > Mathf.Abs(this.m_first.frontVel) * 1.2 && num > 15) {
                this.m_drift.forceSlip = true;
            }
            
            let num9 = 0;
            let num10: number, num11: number;
            
            if (this.m_drift.trigger) {
                this.m_steer = 0;
                this.m_grip = 0;
                num10 = 0;
                num11 = -(9.8 * this.m_spec.mass) * this.m_spec.frontGripFactor * (this.m_ctrl.getRealSteer() * this.m_spec.getMaxSteerRad() * this.m_spec.driftTrigFactor);
                
                if (this.m_drift.triggerTime <= 0) {
                    this.m_drift.triggerTime = this.m_spec.driftTrigTime;
                    this.m_drift.slipTime = this.m_drift.triggerTime * 2;
                } else {
                    this.m_drift.triggerTime = this.m_drift.triggerTime - deltaT;
                    if (this.m_drift.triggerTime <= 0) {
                        this.m_drift.triggerTime = 0;
                        this.m_drift.trigger = false;
                        if (!this.m_driftGauge.progressOn) {
                            this.m_driftGauge.progressOn = true;
                            this.m_driftGauge.progressTime = 0;
                            this.m_driftGauge.progress = 0;
                        }
                    }
                }
                
            } else if (this.m_drift.slipMode || this.m_drift.slipTime > 0 || this.m_drift.forceSlip || this.m_slipReserveTime > 0) {
                if (this.m_DriveMode === 1) {
                    const num12 = num / this.m_DriveFactor[this.m_DriveMode + 1]![1]!.speedLimit;
                    let num13 = num12 * num12;
                    if (num13 > 1) {
                        num13 = 1;
                    }
                    
                    let num14: number, num15: number, num16: number;
                    if (this.m_drift.slipMode) {
                        this.m_steer = this.m_steer + 0.000001 * (1 - this.m_steer) * num13;
                        this.m_grip = this.m_grip + 0.005 * (1 - this.m_grip);
                        this.m_NetWForce = this.m_NetWForce.mul(1 - this.m_grip);
                        num14 = this.m_ctrl.getRealSteer() * this.m_spec.getMaxSteerRad() * this.m_DriveFactor[this.m_DriveMode + 1]![1]!.onDriftSteerFactor;
                        num15 = this.m_DriveFactor[this.m_DriveMode + 1]![1]!.frontGripFactor;
                        num16 = this.m_DriveFactor[this.m_DriveMode + 1]![1]!.rearGripFactor;
                    } else {
                        this.m_steer = this.m_steer + 0.001 * (1 - this.m_steer);
                        this.m_grip = 0;
                        num14 = this.m_ctrl.steerAngle * this.m_DriveFactor[this.m_DriveMode + 1]![1]!.onRestTimeSteerFactor;
                        num15 = this.m_DriveFactor[this.m_DriveMode + 1]![1]!.backFrontGripFactor;
                        num16 = this.m_DriveFactor[this.m_DriveMode + 1]![1]!.backRearGripFactor;
                    }
                    
                    let num17 = this.m_spec.frontGripFactor + num15;
                    let num18 = this.m_spec.rearGripFactor + num16;
                    if (this.m_steer > 1) {
                        this.m_steer = 1;
                    }
                    
                    num18 = num18 - this.m_grip;
                    num10 = 9.8 * this.m_spec.mass * num17 * (num14 * num2 - num8 - num6);
                    num11 = 9.8 * this.m_spec.mass * num18 * (-num8 + num7);
                    num10 = num10 * this.m_spec.driftSlipFactor * this.m_steer;
                    num11 = num11 * this.m_spec.driftSlipFactor * this.m_steer;
                    this.m_slipReserveTime = Mathf.Max(this.m_slipReserveTime - deltaT, 0);
                    
                } else if (this.m_drift.slipMode) {
                    num10 = 9.8 * this.m_spec.mass * (this.m_spec.frontGripFactor + this.m_DriveFactor[this.m_DriveMode + 1]![1]!.frontGripFactor) * (this.m_ctrl.getRealSteer() * this.m_spec.getMaxSteerRad() * num2 - num8 - num6);
                    num11 = 9.8 * this.m_spec.mass * (this.m_spec.rearGripFactor + this.m_DriveFactor[this.m_DriveMode + 1]![1]!.rearGripFactor) * (-num8 + num7);
                    num10 = num10 * this.m_spec.driftSlipFactor * this.m_DriveFactor[this.m_DriveMode + 1]![1]!.driftSlipFactor;
                    num11 = num11 * this.m_spec.driftSlipFactor * this.m_DriveFactor[this.m_DriveMode + 1]![1]!.driftSlipFactor;
                } else {
                    num10 = 9.8 * this.m_spec.mass * this.m_spec.frontGripFactor * (this.m_ctrl.steerAngle * num2 - num8 - num6);
                    num11 = 9.8 * this.m_spec.mass * this.m_spec.rearGripFactor * (-num8 + num7);
                    num10 = num10 * this.m_spec.driftSlipFactor;
                    num11 = num11 * this.m_spec.driftSlipFactor;
                }
                
                num9 = ((this.m_first.speed <= 10) ? (-(num10 + num11) * this.m_spec.driftLeanFactor * 0.5) : (-(num10 + num11) * this.m_spec.driftLeanFactor));
                this.m_drift.slipTime = Mathf.Max(this.m_drift.slipTime - deltaT, 0);
                
            } else {
                flag3 = true;
                if (this.m_DriveMode > 0) {
                    const num19 = num / this.m_DriveFactor[this.m_DriveMode + 1]![0]!.speedLimit;
                    let num20 = this.m_slip[0]! * num19 * num19 + this.m_slip[1]!;
                    if (num20 > 1) {
                        num20 = 1;
                    }
                    
                    let num21: number;
                    if (this.m_DriveMode === 2) {
                        num21 = this.m_ctrl.getRealSteer() * this.m_spec.getMaxSteerRad() * num20;
                    } else {
                        num21 = this.m_ctrl.steerAngle * (1 + num20);
                    }
                    
                    num10 = 9.8 * this.m_spec.mass * this.m_spec.frontGripFactor * (num21 * num2 - num8 - num6);
                    num11 = 9.8 * this.m_spec.mass * this.m_spec.rearGripFactor * (-num8 + num7);
                    num10 = num10 * this.m_spec.driftSlipFactor * this.m_DriveFactor[this.m_DriveMode + 1]![0]!.driftSlipFactor;
                    num11 = num11 * this.m_spec.driftSlipFactor * this.m_DriveFactor[this.m_DriveMode + 1]![0]!.driftSlipFactor;
                } else {
                    num10 = 9.8 * this.m_spec.mass * this.m_spec.frontGripFactor * (this.m_ctrl.steerAngle * num2 - num8 - num6);
                    num11 = 9.8 * this.m_spec.mass * this.m_spec.rearGripFactor * (-num8 + num7);
                }
                
                num9 = -(num10 + num11) * this.m_spec.steerLeanFactor;
                if (this.m_driftGauge.progressOn) {
                    this.m_driftGauge.gauge = Mathf.Min(this.m_spec.driftMaxGauge, this.m_driftGauge.gauge + this.m_driftGauge.progress);
                    this.m_driftGauge.progressOn = false;
                    this.m_driftGauge.progressTime = 0;
                    this.m_driftGauge.lastProgress = this.m_driftGauge.progress;
                    this.m_driftGauge.progress = 0;
                }
            }
            
            const forceVector = this.m_ort.mul(new UnityVector3(num10 + num11, 0, flag3 ? (-Mathf.Abs(num10 + num11) * this.m_spec.cornerDrawFactor) : 0)) as UnityVector3;
            this.m_NetWForce = this.m_NetWForce.add(forceVector);
            this.m_NetLTorque = this.m_NetLTorque.add(new UnityVector3(0, num3 * num10 - num4 * num11, num9));
        } else {
            this.m_drift.slipMode = false;
            this.m_drift.slipTime = 0;
            this.m_drift.trigger = false;
            this.m_drift.triggerTime = 0;
            this.m_adBoost.validTrigger = false;
            
            if (this.m_driftGauge.progressOn) {
                this.m_driftGauge.gauge = Mathf.Min(this.m_spec.driftMaxGauge, this.m_driftGauge.gauge + this.m_driftGauge.progress);
            }
            this.m_driftGauge.progressOn = false;
            this.m_driftGauge.progressTime = 0;
            this.m_driftGauge.lastProgress = this.m_driftGauge.progress;
            this.m_driftGauge.progress = 0;
            
            const num22 = this.m_KartLAVel.Y * num3 / 5;
            const num23 = this.m_KartLAVel.Y * num4 / 5;
            const num24 = this.m_first.leftVel / 5;
            const num25 = 9.8 * this.m_spec.mass * this.m_spec.frontGripFactor * (((num >= 0.5) ? (this.m_ctrl.steerAngle * num2) : 0) - num24 - num22);
            const num26 = 9.8 * this.m_spec.mass * this.m_spec.rearGripFactor * (-num24 + num23);
            const lowSpeedForceVector = this.m_ort.mul(new UnityVector3(num25 + num26, 0, 0)) as UnityVector3;
            this.m_NetWForce = this.m_NetWForce.add(lowSpeedForceVector);
            this.m_NetLTorque = this.m_NetLTorque.add(new UnityVector3(0, num3 * num25 - num4 * num26, 0));
        }
        
        if (flag) {
            this.m_DriveMode = num5;
        }
    }
    
    // 其他必要的方法
    public processAdBoostTime(deltaT: number): void {
        if (this.m_adBoost.validTime > 0) {
            this.m_adBoost.validTime = this.m_adBoost.validTime - deltaT;
            this.m_adBoost.validTime = Mathf.Max(0, this.m_adBoost.validTime);
        }
        if (this.m_adBoost.useLeftTime > 0) {
            this.m_adBoost.useLeftTime = this.m_adBoost.useLeftTime - deltaT;
            if (this.m_adBoost.useLeftTime <= 0) {
                this.m_adBoost.useLeftTime = 0;
                if (this.m_BoostKind === BoostKind.BoostDrift) {
                    this.m_BoostKind = BoostKind.NoBoost;
                }
            }
        }
    }
    
    public beginNetForce(deltaT: number): void {
        this.m_NetWForce = this.m_extern.force;
        this.m_NetLTorque = this.m_extern.torque;
        this.m_extern.force = UnityVector3.zero;
        this.m_extern.torque = UnityVector3.zero;
        this.m_extern.liftVel = UnityVector3.zero;
        // 通过适配器获取统一的方向向量
        if (this.controller_ && (this.controller_ as { transform?: unknown }).transform) {
            const transform = (this.controller_ as { transform: { right: UnityVector3, forward: UnityVector3, up: UnityVector3 } }).transform;
            const right = transform.right;
            const forward = transform.forward;
            const up = transform.up;
            this.m_first.left = new UnityVector3(right.X, right.Y, right.Z);
            this.m_first.front = new UnityVector3(forward.X, forward.Y, forward.Z);
            this.m_first.up = new UnityVector3(up.X, up.Y, up.Z);
        }
        this.m_first.frontVel = UnityVector3.Dot(this.m_KartWLVel, this.m_first.front);
        this.m_first.leftVel = UnityVector3.Dot(this.m_KartWLVel, this.m_first.left);
        this.m_first.upVel = UnityVector3.Dot(this.m_KartWLVel, this.m_first.up);
        this.m_first.speed = this.m_KartWLVel.getMagnitude();
        this.m_ort.setCol(this.m_first.left, this.m_first.up, this.m_first.front);
    }
    
    public decideKartContact(deltaT: number, wallInc: boolean): boolean {
        this.m_cState.shock = this.m_Contact;
        this.m_Contact = false;
        
        if (this.m_kart === undefined) {
            return this.m_Contact;
        }
        
        // 防御性检查，确保 m_first 的属性已经初始化
        if (!this.m_first.left || !this.m_first.front || !this.m_first.up) {
            // 如果 m_first 的属性没有正确初始化，使用默认值
            this.m_first.left = new UnityVector3(1, 0, 0);
            this.m_first.front = new UnityVector3(0, 0, 1);
            this.m_first.up = new UnityVector3(0, 1, 0);
        }
        
        // 获取卡丁车的位置，通过GameObject的transform获取
        if (!this.m_kart || !(this.m_kart as { transform?: unknown }).transform) {
            return this.m_Contact;
        }
        
        // 直接调用方法，不提取变量，确保生成冒号调用
        const kartPosition = (this.m_kart as { transform: { getPosition(): UnityVector3 } }).transform.getPosition();
        
        // 检查4个车轮的地面接触
        for (let num = 1; num <= 4; num++) {
            let vector = kartPosition;
            vector = vector.add(this.m_first.left.mul(this.m_spec.width * this.m_sus.wheelOff[num]!.X * 0.8));
            vector = vector.add(this.m_first.front.mul(this.m_spec.length * this.m_sus.wheelOff[num]!.Y * 0.8));
            vector = vector.add(this.m_first.up.mul(this.m_sus.maxTravel));
            const rayOrigin = vector.add(this.m_first.up);
            const rayLength = this.m_sus.maxTravel * 2 + 1;
            
            // 转换为Roblox Vector3进行射线检测
            const robloxOrigin = new Vector3(rayOrigin.X, rayOrigin.Y, rayOrigin.Z);
            const rayDirection = this.m_first.up.mul(-1);
            const robloxDirection = new Vector3(rayDirection.X, rayDirection.Y, rayDirection.Z);
            
            // 使用Physics模块进行射线检测，与Unity保持一致
            const [hit, raycastHit] = Physics.Raycast(robloxOrigin, robloxDirection, rayLength, 256);
            if (hit && raycastHit) {
                this.m_Contact = true;
                this.m_sus.wheelContact[num] = true;
                this.m_sus.wheelContactN[num] = new UnityVector3(raycastHit.normal.X, raycastHit.normal.Y, raycastHit.normal.Z);
                
                const hitPointUnity = new UnityVector3(raycastHit.point.X, raycastHit.point.Y, raycastHit.point.Z);
                const num3 = UnityVector3.Dot(hitPointUnity, this.m_first.up) - 
                            (UnityVector3.Dot(kartPosition, this.m_first.up) - this.m_sus.maxTravel);
                const num4 = this.m_sus.travel[num]!;
                this.m_sus.travel[num] = Mathf.Max(0, Mathf.Min(num3, this.m_sus.maxTravel * 2));
                this.m_sus.deltaTravel[num] = this.m_sus.travel[num]! - num4;
            } else {
                this.m_sus.wheelContact[num] = false;
                this.m_sus.travel[num] = 0;
                this.m_sus.deltaTravel[num] = 0;
            }
        }
        
        this.m_sus.contactN = UnityVector3.zero;
        if (this.m_Contact) {
            let num5 = 0;
            for (let num6 = 1; num6 <= 4; num6++) {
                if (this.m_sus.wheelContact[num6]) {
                    num5 = num5 + 1;
                    this.m_sus.contactN = this.m_sus.contactN.add(this.m_sus.wheelContactN[num6]!);
                }
            }
            this.m_sus.contactN = this.m_sus.contactN.div(num5);
            this.m_cState.shock = !this.m_cState.shock;
            if (num5 > 2) {
                this.m_cState.hop = false;
            }
        } else {
            this.m_cState.shock = false;
        }
        
        return this.m_Contact;
    }
    
    public calcFlyingKartForce(): void {
        this.m_drift.slipMode = false;
        this.m_drift.forceSlip = false;
        this.m_drift.trigger = false;
        this.m_NetWForce = this.m_NetWForce.add(this.m_theGravity.mul(this.m_spec.mass * this.m_extern.gravityFactor));
        this.m_NetLTorque = this.m_NetLTorque.sub(this.m_KartLAVel.mul(30));
        this.m_ctrl.oldSteerAngle = 0;
        
        if (this.m_cState.hop) {
            if (this.m_first.up.Y < 0.05) {
                const torqueZ = this.m_first.left.Y <= 0 ? 
                    (1 - this.m_first.up.Y) * -90 : 
                    (1 - this.m_first.up.Y) * 90;
                this.m_NetLTorque = this.m_NetLTorque.add(new UnityVector3(0, 0, torqueZ));
            }
            if (this.m_first.front.Y > 0.5) {
                this.m_NetLTorque = this.m_NetLTorque.add(new UnityVector3((this.m_first.front.Y + 1) * 90, 0, 0));
            } else if (this.m_first.front.Y < -0.5) {
                this.m_NetLTorque = this.m_NetLTorque.add(new UnityVector3(-(1 - this.m_first.front.Y) * 90, 0, 0));
            }
        }
    }
    
    public endNetForce(deltaT: number): void {
        this.m_NetWForce = this.m_NetWForce.add(this.m_extern.annexForce);
        this.m_KartWLVel = this.m_KartWLVel.add(this.m_NetWForce.div(this.m_spec.mass).mul(deltaT));
        
        // 计算角速度更新
        const scaledTorque = UnityVector3.Scale(this.m_reciprocalMass, this.m_NetLTorque);
        const crossProduct = UnityVector3.Cross(this.m_KartLAVel, UnityVector3.Scale(this.m_reciprocalMass, this.m_KartLAVel));
        const torqueDelta = scaledTorque.sub(crossProduct).mul(deltaT);
        const scaledTorqueDelta = UnityVector3.Scale(this.m_reciprocalMass, torqueDelta);
        this.m_KartLAVel = this.m_KartLAVel.add(scaledTorqueDelta);
        
        // 平滑限制角速度，提供多种平滑算法
        if (this.m_drift && (this.m_drift.slipMode || this.m_drift.forceSlip || this.m_drift.trigger)) {
            const maxAngularSpeed = 2.3; // 每秒最大2.3弧度
            const angularSpeed = this.m_KartLAVel.getMagnitude();
            if (angularSpeed > maxAngularSpeed) {
                // 指数平滑衰减 - 最自然的平滑方式
                const smoothFactor = 1.0 - Mathf.Exp(-8.0 * deltaT); // 基于时间的平滑系数
                const targetSpeed = maxAngularSpeed;
                const newSpeed = angularSpeed + (targetSpeed - angularSpeed) * smoothFactor;
                this.m_KartLAVel = this.m_KartLAVel.mul(newSpeed / angularSpeed);
            }
        }
    }
    
    public ProcessDriftGauge(deltaT: number): void {
        if (this.m_driftGauge.progressOn && this.m_Contact && this.m_first.frontVel >= 0) {
            this.m_driftGauge.progressTime = this.m_driftGauge.progressTime + deltaT;
            if (this.m_driftGauge.progressTime < 0.2) {
                this.m_driftGauge.progress = this.m_driftGauge.progress + 2 * deltaT * this.m_first.leftVel * this.m_first.leftVel * 3;
            } else if (this.m_driftGauge.progressTime < 0.5) {
                this.m_driftGauge.progress = this.m_driftGauge.progress + 2 * deltaT * this.m_first.leftVel * this.m_first.leftVel * 1.5;
            } else {
                this.m_driftGauge.progress = this.m_driftGauge.progress + 2 * (deltaT * this.m_first.leftVel * this.m_first.leftVel) / (this.m_driftGauge.progressTime * 2);
            }
        }
    }
    
    // 控制方法
    public setAccel(accel: boolean): void {
        if (accel) {
            this.m_ctrl.accel = 1;
            this.m_slipBoost = true;
        } else {
            this.m_ctrl.accel = 0;
            if (!this.isZoneBoost()) {
                this.m_boostLeft = 0;
                this.m_BoostKind = BoostKind.NoBoost;
            }
        }
    }
    
    public setBrake(brake: boolean): void {
        if (brake) {
            this.m_ctrl.brake = 1;
        } else {
            this.m_ctrl.brake = 0;
        }
    }
    
    // Wheel属性
    public getWheel(): number {
        return this.m_ctrl.steer;
    }
    
    public setWheel(value: number): void {
        this.m_ctrl.steer = value;
    }
    
    public setDrift(drift: boolean): void {
        if (drift) {
            if (this.m_drift.slipTime <= 0) {
                this.m_drift.slipMode = true;
                this.m_drift.trigger = true;
            }
        } else {
            if (this.m_DriveMode === 1 && this.m_drift.slipMode && this.m_slipReserveTime <= 0) {
                const num = this.m_first.speed / 120;
                this.m_slipReserveTime = 10 * num * num;
            }
            this.m_drift.slipMode = false;
        }
    }
    
    // 速度相关方法
    public GetKartSpeed(): number {
        return this.m_KartWLVel.getMagnitude() * 3.6;
    }
    
    public GetKartRealSpeed(): number {
        return this.m_KartRealVelocity.getMagnitude() * 3.6;
    }
    
    // 碰撞相关方法
    public ResetCrash(): void {
        this.isCrash_ = false;
        this.crashVelocity_ = 0;
    }
    
    public SetCrash(vel: number): void {
        this.isCrash_ = true;
        if (this.crashVelocity_ <= vel) {
            this.crashVelocity_ = vel;
        }
    }
    
    // Boost 相关方法
    public setBoost(time: number, kind: BoostKind): void {
        if (this.isZoneBoost(kind) || this.m_ctrl.getRealAccel() !== 0) {
            this.m_boostLeft = time;
            this.m_BoostKind = kind;
        }
    }
    
    public setBoostForce(time: number, kind: BoostKind): void {
        this.m_boostLeft = time;
        this.m_BoostKind = kind;
    }
    
    // 加速判断方法
    public isRealBoost(kind?: BoostKind): boolean {
        if (kind === undefined) {
            kind = this.m_BoostKind;
        }
        return kind === BoostKind.BoostStart || kind === BoostKind.BoostNormal || 
               kind === BoostKind.BoostTeam || kind === BoostKind.BoostDrift || kind === BoostKind.BoostAnimal;
    }
    
    public isItemBoost(kind?: BoostKind): boolean {
        if (kind === undefined) {
            kind = this.m_BoostKind;
        }
        return kind === BoostKind.BoostNormal || kind === BoostKind.BoostTeam || kind === BoostKind.BoostAnimal;
    }
    
    public isZoneBoost(kind?: BoostKind): boolean {
        if (kind === undefined) {
            kind = this.m_BoostKind;
        }
        return kind === BoostKind.BoostZone || kind === BoostKind.BoostJumpZone || kind === BoostKind.BoostDelivery;
    }
    
    public isBoost(kind: BoostKind): boolean {
        return kind === this.m_BoostKind;
    }
    
    // Shock 相关方法
    public IsShock(): boolean {
        return this.isShock_;
    }
    
    public ResetShock(): void {
        this.isShock_ = false;
        this.shockVelocity_ = 0;
    }
    
    public SetShock(vel: number): void {
        this.isShock_ = true;
        if (this.shockVelocity_ <= vel) {
            this.shockVelocity_ = vel;
        }
    }
    
    // setReKart重写父类方法
    public setReKart(controller: unknown, wheels: unknown): void {
        super.setReKart(controller, wheels);
        this.wheelLocalPos_ = [];
        
        // 完全按照Unity源代码逻辑：第461-468行
        const wheelsArray = wheels as { localPosition: Vector3 }[];
        for (let i = 1; i <= 4; i++) {
            if (wheelsArray && wheelsArray[i]) {
                // 第464行：this.wheelLocalPos_[i] = wheels[i].localPosition;
                const localPos = wheelsArray[i]!.localPosition;
                this.wheelLocalPos_[i] = new UnityVector3(localPos.X, localPos.Y, localPos.Z);
                // 第467行：array[num].x = array[num].x + 0.2f * ((i % 2 != 0) ? 1f : (-1f));
                // 在TypeScript中：i从1开始，所以要调整条件判断
                const adjustment = ((i % 2 !== 0) ? 1 : -1) * 0.2;
                this.wheelLocalPos_[i] = new UnityVector3(
                    this.wheelLocalPos_[i]!.X + adjustment,
                    this.wheelLocalPos_[i]!.Y,
                    this.wheelLocalPos_[i]!.Z
                );
            } else {
                print("警告: wheels[" + i + "] 是 nil!");
            }
        }
        
        // 获取模型的尺寸
        let x: number, z: number;
        // 获取底层的Roblox对象
        let robloxObject = this.m_kart;
        // 安全地检查是否有gameObject属性
        if (this.m_kart && typeOf(this.m_kart) === "table" && (this.m_kart as { gameObject?: unknown }).gameObject) {
            robloxObject = (this.m_kart as { gameObject: unknown }).gameObject;
        }
        
        if (robloxObject && (robloxObject as { IsA?: (className: string) => boolean }).IsA) {
            const obj = robloxObject as Instance;
            if (obj.IsA("Model")) {
                // 对于Model，使用GetBoundingBox获取整体尺寸
                const model = obj as Model;
                const [cf, size] = model.GetBoundingBox();
                x = size.X;
                z = size.Z;
            } else if (obj.IsA("BasePart")) {
                // 对于Part，直接使用Size
                const part = obj as BasePart;
                x = part.Size.X;
                z = part.Size.Z;
            } else {
                // 默认值
                x = 2;
                z = 3;
            }
        } else {
            // 默认值
            x = 2;
            z = 3;
        }
        
        // 注意：在Roblox中，Part的Size属性已经包含了所有的缩放
        // 不需要额外乘以scale，否则会重复计算
        this.m_spec.width = Mathf.Min(0.98, x * 0.5);
        this.m_spec.length = z * 0.5;
        this.setDefaultSpec();
    }
    
    public setDefaultSpec(): void {
        this.m_spec.springK = this.m_spec.mass * Mathf.Abs(this.m_theGravity.Y) * 0.5;
        this.m_spec.damperCopC = 0;
        this.m_spec.damperRebC = this.m_spec.springK * 0.2;
        this.m_reciprocalMass = UnityVector3.zero;
        // Unity原版：Vector3Helper.SetVector3(ref this.m_reciprocalMass, 12f / this.m_spec.mass)
        const reciprocalValue = 12 / this.m_spec.mass;
        this.m_reciprocalMass = new UnityVector3(reciprocalValue, reciprocalValue, reciprocalValue);
    }
    
    // setReKartOld方法 - 旧版本的setReKart，处理直接的Roblox对象
    public setReKartOld(obj: unknown, wheels: unknown): void {
        this.m_kart = obj;
        this.wheelLocalPos_ = [];
        
        // 计算轮子相对于车体的本地偏移量
        const robloxObj = obj as { Position: Vector3 };
        const wheelsArray = wheels as { Position: Vector3 }[];
        const carPosition = robloxObj.Position;
        
        for (let i = 1; i <= 4; i++) {
            if (wheelsArray && wheelsArray[i]) {
                // 计算轮子相对于车体的本地偏移
                const wheelWorldPos = wheelsArray[i]!.Position;
                const localOffset = wheelWorldPos.sub(carPosition);
                
                // 转换为UnityMath Vector3格式
                this.wheelLocalPos_[i] = new UnityVector3(localOffset.X, localOffset.Y, localOffset.Z);
            }
        }
        
        // 获取模型的尺寸
        let x: number, z: number;
        // 获取底层的Roblox对象
        let robloxObject = this.m_kart;
        // 安全地检查是否有gameObject属性
        if (this.m_kart && typeOf(this.m_kart) === "table" && (this.m_kart as { gameObject?: unknown }).gameObject) {
            robloxObject = (this.m_kart as { gameObject: unknown }).gameObject;
        }
        
        if (robloxObject && (robloxObject as { IsA?: (className: string) => boolean }).IsA) {
            const robj = robloxObject as Instance;
            if (robj.IsA("Model")) {
                // 对于Model，使用GetBoundingBox获取整体尺寸
                const model = robj as Model;
                const [cf, size] = model.GetBoundingBox();
                x = size.X;
                z = size.Z;
            } else if (robj.IsA("BasePart")) {
                // 对于Part，直接使用Size
                const part = robj as BasePart;
                x = part.Size.X;
                z = part.Size.Z;
            } else {
                // 默认值
                x = 2;
                z = 3;
            }
        } else {
            // 默认值
            x = 2;
            z = 3;
        }
        
        // 注意：在Roblox中，Part的Size属性已经包含了所有的缩放
        // 不需要额外乘以scale，否则会重复计算
        this.m_spec.width = Mathf.Min(0.98, x * 0.5);
        this.m_spec.length = z * 0.5;
        this.setDefaultSpec();
    }
    
    // getRealSteer方法
    public getRealSteer(): number {
        return this.m_ctrl.getRealSteer();
    }
    
    // Unity的getRealAccel方法
    public getRealAccel(): number {
        return this.m_ctrl.getRealAccel();
    }
    
    // Unity的getRealBrake方法
    public getRealBrake(): number {
        return this.m_ctrl.getRealBrake();
    }
    
    // BackupVelocity 相关方法
    public getBackupVelocity(): UnityVector3 {
        return this.backupVelocity_;
    }
    
    public setBackupVelocity(value: UnityVector3): void {
        this.backupVelocity_ = value;
    }
    
    // ResetDriftGauge方法 - 重置漂移计量器
    public ResetDriftGauge(): void {
        this.m_driftGauge.progressOn = false;
        this.m_driftGauge.progressTime = 0;
        this.m_driftGauge.progress = 0;
    }
    
    // 漂移状态相关方法
    public getIsDrift(): boolean {
        return this.m_isDrift;
    }   
    
    public getDriftSlipMode(): boolean {
        return this.m_drift.slipMode;
    }
    
    // Warp方法 - 传送并重置状态
    public Warp(pos: UnityVector3, rot: UnityVector3, flush: boolean, resetVel: boolean): void {
        super.Warp(pos, rot, flush, resetVel);
        this.m_NetWForce = UnityVector3.zero;
        this.m_NetLTorque = UnityVector3.zero;
        if (this.m_boostLeft > 0) {
            this.m_boostLeft = 0;
            this.m_BoostKind = BoostKind.NoBoost;
        }
    }
    
    // ResetForRestarting方法 - 重新开始时重置所有状态
    public ResetForRestarting(): void {
        super.ResetForRestarting();
        this.m_theGravity = new UnityVector3(0, -49, 0);
        this.m_NetWForce = UnityVector3.zero;
        this.m_NetLTorque = UnityVector3.zero;
        this.m_KartRealVelocity = UnityVector3.zero;
        this.m_boostLeft = 0;
        this.m_slipBoost = false;
        this.m_Contact = true;
        
        // 重新初始化所有结构体
        this.m_sus.Initialize();
        this.m_drift.Initialize();
        this.m_driftGauge.Initialize();
        this.m_adBoost.Initialize();
        this.m_ctrl.Initialize();
        this.m_cState.Initialize();
        this.m_extern.Initialize();
        this.m_stuckHelper.Initialize();
        
        this.m_ort = Matrix3.CreateMtxIdentity();
        this.m_slipReserveTime = 0;
        this.m_steer = 0;
        this.m_grip = 0;
        this.m_slip[0] = 2;   // TypeScript中数组从0开始
        this.m_slip[1] = 0.5;
        this.m_isDrift = false;
        this.m_BoostKind = BoostKind.NoBoost;
        this.isCrash_ = false;
        this.crashVelocity_ = 0;
        this.isShock_ = false;
        this.shockVelocity_ = 0;
        this.needReset_ = false;
        this.backupVelocity_ = UnityVector3.zero;
    }
    
    // initGoKart方法 - 重写父类方法
    public initGoKart(): void {
        // GoPlayKart特有的初始化
        this.wheelLocalPos_ = [];
        this.isCrash_ = false;
        this.crashVelocity_ = 0;
        this.isShock_ = false;
        this.shockVelocity_ = 0;
    }
    
    // GetWheelPos方法 - 获取轮子位置
    public GetWheelPos(i: number): UnityVector3 {
        // 简化实现，返回轮子本地偏移位置
        if (this.wheelLocalPos_ && this.wheelLocalPos_[i]) {
            return this.wheelLocalPos_[i];
        }
        return UnityVector3.zero;
    }
    
    // ResetKart方法 - 重置卡丁车状态
    public ResetKart(): void {
        this.m_KartWLVel = UnityVector3.zero;
        this.m_KartLAVel = UnityVector3.zero;
        this.m_NetWForce = UnityVector3.zero;
        this.m_NetLTorque = UnityVector3.zero;
        if (this.m_boostLeft > 0) {
            this.m_boostLeft = 0;
            this.m_BoostKind = BoostKind.NoBoost;
        }
    }
    
    // 漂移计量器相关方法
    public GetDriftMaxGauge(): number {
        return this.m_spec.driftMaxGauge;
    }
    
    public GetDriftGauge(): number {
        return this.m_driftGauge.gauge;
    }
    
    public GetDriftGaugeProgress(): number {
        return Mathf.Min(this.m_spec.driftMaxGauge, this.m_driftGauge.gauge + this.m_driftGauge.progress);
    }
    
    public UseDriftGauge(gauge: number): boolean {
        if (gauge > this.m_driftGauge.gauge) {
            return false;
        }
        this.m_driftGauge.gauge = this.m_driftGauge.gauge - gauge;
        return true;
    }
    
    public GetDriftLastProgress(): number {
        const lastProgress = this.m_driftGauge.lastProgress;
        this.m_driftGauge.lastProgress = 0;
        return lastProgress;
    }
}