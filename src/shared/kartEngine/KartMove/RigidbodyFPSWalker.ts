// RigidbodyFPSWalker class的TypeScript实现，继承自KartBasicController
// 完整迁移Unity中的RigidbodyFPSWalker类到Roblox - 严格按照Lua版本的逻辑

import { KartBasicController, PlayMode } from "./KartBasicController";
import { MonoBehaviour } from "../KartShared/MonoBehaviour";
import RobloxUnityAdapter from "../KartShared/RobloxUnityAdapter";
import { UnityVector3, Quaternion, Mathf } from "../KartShared/UnityMath";
import { MathHelper } from "./MathHelper";
import { Vector3Helper } from "./Vector3Helper";
import UnityTime from "../KartShared/UnityTime";
import { UnityTrigger } from "../KartShared/UnityTrigger";
import { UnityInput } from "../KartShared/UnityInput";

// Unity Physics相关模块
import { LayerMask } from "../KartShared/UnityEngine/LayerMask";
import { Physics } from "../KartShared/UnityEngine/Physics";

// 引入相关模块
import { KartManager } from "./KartManager";
import { GoPlayKartBuilder } from "../GameStage/GoPlayKartBuilder";
import { GoPlayKart } from "./GoPlayKart";
import { BoostKind } from "./BoostKind";

// 明确区分UnityVector3和Roblox原生Vector3
// UnityVector3 - 用于所有内部计算和物理
// Vector3 (Roblox原生) - 仅用于与Roblox API交互
// 不使用别名避免混淆，直接使用完整类名

// Roblox服务
const Players = game.GetService("Players");
const RunService = game.GetService("RunService");
const UserInputService = game.GetService("UserInputService");
const Workspace = game.GetService("Workspace");
const ReplicatedStorage = game.GetService("ReplicatedStorage");
const CollectionService = game.GetService("CollectionService");

// 组件注册表，仿照Unity的组件系统
// 使用弱引用避免内存泄漏
const ComponentRegistry = new WeakMap<BasePart, RigidbodyFPSWalker>();

// 转换辅助函数
function fromRobloxVector3(robloxVector: Vector3): UnityVector3 {
	return new UnityVector3(robloxVector.X, robloxVector.Y, robloxVector.Z);
}

function toRobloxVector3(unityVector: UnityVector3): Vector3 {
	return new Vector3(unityVector.X, unityVector.Y, unityVector.Z) as unknown as Vector3;
}

// 轮子接口定义，用于类型安全
interface WheelObject {
	localRotation?: Quaternion;
	// 根据需要添加其他轮子相关属性
}

// PrevState 内部类（完全对应Unity版本）
class PrevState {
	public position_ = UnityVector3.zero;
	public force_ = UnityVector3.zero;
	public velocity_ = UnityVector3.zero;
	public angular_ = UnityVector3.zero;
	public forward_ = UnityVector3.zero;
	public rotate_ = Quaternion.identity;
	public isGrounded_ = false;

	constructor() {}
}

export class RigidbodyFPSWalker extends KartBasicController {
	// 私有变量初始化（对应Unity的private字段）
	public grounded = false;
	private prevState_ = new PrevState();
	public goPlayKart_?: GoPlayKart;
	private isBooster_ = false;
	private thisBoxCollider_?: Instance;
	private childBoxCollider_?: Instance;
	private _targetUpVector = UnityVector3.up;
	private readonly SMALL_CRASH_VELOCITY = 15;
	private readonly BIG_CRASH_VELOCITY = 30;
	private wheelRotation = 0;
	private isSuddenChange = false;
	private fixedUpdateCount_ = 0;

	// Roblox特有的物理组件引用
	private rigidbody?: Instance; // 将在Awake中初始化

	// 初始化Unity风格的触发器系统
	private unityTrigger_?: UnityTrigger;

	constructor(robloxObject?: Instance) {
		super(robloxObject);
		
		print(`[RigidbodyFPSWalker] 构造函数 - robloxObject: ${robloxObject ? robloxObject.Name : "nil"}`);
		print(`[RigidbodyFPSWalker] 构造函数 - gameObject: ${this.gameObject ? "已设置" : "nil"}`);

		// 立即设置GameObject和transform，确保不为nil
		if (robloxObject) {
			this.SetGameObject(robloxObject);
			print(`[RigidbodyFPSWalker] 手动设置GameObject后 - gameObject: ${this.gameObject ? "已设置" : "nil"}`);
			print(`[RigidbodyFPSWalker] gameObject类型: ${this.gameObject ? typeOf(this.gameObject) : "nil"}`);
			if (this.gameObject) {
				const gameObjRecord = this.gameObject as unknown as Record<string, unknown>;
				print(`[RigidbodyFPSWalker] gameObject.robloxObject: ${gameObjRecord.robloxObject ? (gameObjRecord.robloxObject as Instance).Name : "nil"}`);
			}
		}

		// 初始化Unity风格的触发器系统
		this.unityTrigger_ = new UnityTrigger(this);

		// 注册生命周期回调到MonoBehaviour系统
		this.AddUpdateCallback((mono, dt) => this.Update());
		this.AddFixedUpdateCallback((mono, dt) => this.FixedUpdate());
		this.AddLateUpdateCallback((mono, dt) => this.LateUpdate());
	}

	// 方法1: Unity的Awake()方法迁移 - 完全对应Unity版本逻辑
	public Awake(): void {
		// 调用基类Awake
		super.Awake();

		// 设置游戏对象名称
		if (this.gameObject) {
			// 如果kartIndex_已经被外部设置（远程玩家），使用它
			// 否则使用默认的本地玩家索引
			if (this.kartIndex_ === undefined) {
				this.kartIndex_ = KartManager.PLAYER_KART_IDX;
				this.gameObject.name = "player_kart";
			} else {
				this.gameObject.name = `remote_kart_${this.kartIndex_}`;
			}
		}

		// 获取Rigidbody组件并设置属性
		this.rigidbody = this.GetComponent("Rigidbody");
		if (this.rigidbody) {
			const rigidbody = this.rigidbody as unknown as {
				freezeRotation?: boolean;
				useGravity?: boolean;
				isKinematic?: boolean;
			};
			if (rigidbody.freezeRotation !== undefined) {
				rigidbody.freezeRotation = false;
			}
			if (rigidbody.useGravity !== undefined) {
				rigidbody.useGravity = false;
			}
			if (rigidbody.isKinematic !== undefined) {
				rigidbody.isKinematic = true;
			}
		}

		// 在Roblox中，Part本身就是碰撞器，不需要额外创建BoxCollider
		// 确保主Part的碰撞设置正确
		const basePart = this.gameObject?.gameObject;
		if (basePart && basePart.IsA("BasePart")) {
			const part = basePart as BasePart;
			part.CanCollide = true; // 确保可以碰撞
			part.CanTouch = true; // 确保可以触发Touch事件
			part.CanQuery = true; // 确保可以被射线检测

			// 注册组件到全局注册表，仿照Unity的组件系统
			ComponentRegistry.set(part, this);
		}

		// 调用基类初始化
		this.Initialize(0, 0, false);

		// 获取唯一的卡丁车索引（使用玩家UserId）
		const playerKartIndex = KartManager.GetPlayerKartIndex();

		// 设置GoPlayKart，使用唯一的卡丁车索引 - 严格按照Lua逻辑
		// Lua代码：self.goPlayKart_ = KartManager.Instance:SetKart(playerKartIndex, GoPlayKartBuilder.new(), self, self.wheels_)
		this.goPlayKart_ = KartManager.GetInstance().SetKart(
			playerKartIndex,
			new GoPlayKartBuilder(),
			this,              // controller参数：self
			this.wheels_       // wheelPos参数：self.wheels_ (1-based数组)
		) as GoPlayKart | undefined;

		// 通知ECS系统卡丁车已创建（如果ECS已准备好），使用唯一索引
		if ((_G as unknown as { ECS_OnKartCreated?: (index: number, kart: GoPlayKart) => void }).ECS_OnKartCreated) {
			(_G as unknown as { ECS_OnKartCreated: (index: number, kart: GoPlayKart) => void }).ECS_OnKartCreated(playerKartIndex, this.goPlayKart_!);
			print(`[RigidbodyFPSWalker] 通知ECS卡丁车创建，唯一索引: ${playerKartIndex}`);
		}
		// 如果ECS还没准备好，ECS初始化时会主动扫描已存在的卡丁车
	}

	// 方法2: Unity的Start()方法迁移 - 完全对应Unity版本
	public Start(): void {
		// 调用基类Start
		super.Start();

		// 设置Rigidbody质量和重心
		if (this.rigidbody && this.goPlayKart_) {
			// 暂时注释，等所有基础功能完成后再修复
			// if ("mass" in this.rigidbody) {
			//     (this.rigidbody as any).mass = this.goPlayKart_.getMass();
			// }
			if ("centerOfMass" in this.rigidbody) {
				(this.rigidbody as any).centerOfMass = toRobloxVector3(new UnityVector3(0, 0, 0));
			}
		}
	}

	// 方法3: Unity的Update()方法迁移 - 完全对应Unity版本
	public Update(): void {
		// 确保Start被调用（模拟MonoBehaviour的行为）
		if (!this.started) {
			this.Start();
			this.started = true;
		}

		this.InputUpdate();
	}

	// 方法4: Unity的LateUpdate()方法迁移 - 完全对应Unity版本
	public LateUpdate(): void {
		// 在Roblox中不需要UpdateCollider，因为Part本身就是碰撞器
	}

	// 方法6: InputUpdate方法迁移 - 使用Unity风格的平滑输入
	private InputUpdate(): void {
		// 只在客户端处理输入
		if (!RunService.IsClient()) {
			return;
		}

		if (!this.goPlayKart_) {
			return;
		}

		// 使用Unity的Input.GetAxis来获取平滑输入值
		// 这将模拟Unity中的平滑转向和加速/刹车响应
		const horizontal = UnityInput.GetAxis("Horizontal");
		const vertical = UnityInput.GetAxis("Vertical");

		// 设置输入值（注意Unity源码中使用的是正值，这里对应设置）
		this.goPlayKart_.setWheel(-horizontal); // Unity中是正值，不需要负号
		const axis = vertical;

		// 设置加速和刹车
		this.goPlayKart_.setAccel(axis > 0.1);
		this.goPlayKart_.setBrake(axis < -0.1);

		// 漂移控制 - 对应Unity的KeyCode.LeftShift
		const isDrifting = UnityInput.GetKey(Enum.KeyCode.LeftShift);
		if (isDrifting && this.goPlayKart_.getWheel() !== 0) {
			this.goPlayKart_.setDrift(true);
		} else {
			this.goPlayKart_.setDrift(false);
		}

		// 加速控制 - 对应Unity的KeyCode.Space
		// if (UnityInput.GetKey(Enum.KeyCode.Space)) {
		//     this.goPlayKart_.setBoost(1000, BoostKind.BoostNormal);
		// }
	}

	// 方法7: UpdateWheels方法迁移 - 使用Roblox原生CFrame然后转换为Quaternion
	private UpdateWheels(): void {
		if (!this.goPlayKart_) return;

		// 完全按照Unity源代码逻辑：第94行 - Unity使用Time.deltaTime
		const deltaTime = UnityTime.getDeltaTime();

		// 完全按照Unity源代码逻辑：第94行
		this.wheelRotation = Mathf.Repeat(
			this.wheelRotation + (this.goPlayKart_.GetKartRealSpeed() * deltaTime * 180) / 3.14159274,
			360,
		);

		// 使用Roblox原生CFrame旋转系统生成旋转，然后转换为Quaternion
		// 将角度转换为弧度，注意Roblox坐标系需要反转Y轴转向
		const wheelRotationRad = math.rad(this.wheelRotation);
		const steerAngleRad = math.rad(-this.goPlayKart_.getSteerAngle() * 3);

		// 前轮CFrame：X轴滚动 + Y轴转向
		const frontWheelCFrame = CFrame.Angles(wheelRotationRad, steerAngleRad, 0);
		// 后轮CFrame：只有X轴滚动
		const rearWheelCFrame = CFrame.Angles(wheelRotationRad, 0, 0);

		const frontQuaternion = Quaternion.fromCFrame(frontWheelCFrame);
		const rearQuaternion = Quaternion.fromCFrame(rearWheelCFrame);

		// 设置轮子本地旋转
		if (this.wheels_) {
			// 前轮（索引1,2对应Unity的0,1）：有转向角度
			if (this.wheels_[0]) {
				(this.wheels_[0] as WheelObject).localRotation = frontQuaternion;
			}
			if (this.wheels_[1]) {
				(this.wheels_[1] as WheelObject).localRotation = frontQuaternion;
			}

			// 后轮（索引3,4对应Unity的2,3）：只滚动
			if (this.wheels_[2]) {
				(this.wheels_[2] as WheelObject).localRotation = rearQuaternion;
			}
			if (this.wheels_[3]) {
				(this.wheels_[3] as WheelObject).localRotation = rearQuaternion;
			}
		}
	}

	// 方法8: Unity的FixedUpdate()方法迁移 - 完全对应Unity版本（第一部分）
	public FixedUpdate(): void {
		// 如果GoPlayKart还没创建，跳过物理更新
		if (!this.goPlayKart_) {
			return;
		}

		// 调用基类FixedUpdate
		super.FixedUpdate();

		this.fixedUpdateCount_ += 1;
		
		// 备份速度
		if (this.rigidbody && "velocity" in this.rigidbody) {
			const rigidbodyVelocity = (this.rigidbody as unknown as { velocity: Vector3 }).velocity;
			this.goPlayKart_.setBackupVelocity(fromRobloxVector3(rigidbodyVelocity));
		}

		// 执行赛车基本动作 - 传递真实时间，对应Unity的Time.time
		this.goPlayKart_.basicAction(UnityTime.getTime());

		// 更新轮子
		this.UpdateWheels();

		// 处理加速效果
		const flag = this.goPlayKart_.isRealBoost() && this.playMode_ === PlayMode.NORMAL;
		if (this.isBooster_ !== flag) {
			this.isBooster_ = this.goPlayKart_.isRealBoost();
			this.SetEnableBooster(this.isBooster_);
		}

		// 主要物理更新逻辑
		if (!this.goPlayKart_.GetForcing() && !this.goPlayKart_.GetIsInResetState()) {
			// 安全检查：确保transform存在
			if (!this.transform) {
				warn("RigidbodyFPSWalker: transform is nil, skipping physics update");
				return;
			}

			// 完全按照Unity源代码逻辑：第122行使用Time.deltaTime
			const deltaTime = UnityTime.getDeltaTime();
		// 使用Unity风格的触发器系统进行地面检测
		if (this.unityTrigger_) {
			this.unityTrigger_.UpdateTriggers();
		}

			// 目标向上向量处理（按Unity源码顺序）
			const targetUpVector = this._targetUpVector; // 使用上一帧的targetUpVector
			const right = this.transform.getRight();
			let forward = this.transform.getForward();
			forward = UnityVector3.Cross(right, targetUpVector);

			const num = this.grounded ? 10 : 1;
			const quaternion = Quaternion.LookRotation(forward, targetUpVector);
			const quaternion2 = Quaternion.Slerp(this.transform.getLocalRotation(), quaternion, deltaTime * num);

			this.transform.setLocalRotation(quaternion2);
			this._targetUpVector = UnityVector3.up; // 重置为Vector3.up（Unity源码第124行的位置）

			if (this.grounded) {
				// 地面旋转处理
				this.goPlayKart_.m_KartLAVel = new UnityVector3(0, this.goPlayKart_.m_KartLAVel.Y, 0);

				// 完全按照Lua源码的四元数旋转逻辑
				const localRotation = this.transform.getLocalRotation();
				const quaternion3 = MathHelper.CreateQuaternion(0, toRobloxVector3(this.goPlayKart_.m_KartLAVel));
				let localQuaternion = new Quaternion(localRotation.X, localRotation.Y, localRotation.Z, localRotation.w);
				const helperQuat3 = new Quaternion(quaternion3.X, quaternion3.Y, quaternion3.Z, quaternion3.w);
				const quaternion4 = localQuaternion.mul(helperQuat3);
				// 完全按照Unity源代码逻辑：第132行 - 使用Time.deltaTime
				MathHelper.QuaMulScala(quaternion4, 0.5 * UnityTime.getDeltaTime());
				const localRotationData = {
					X: localRotation.X,
					Y: localRotation.Y,
					Z: localRotation.Z,
					w: localRotation.w
				};
				MathHelper.QuaAdd(localRotationData, quaternion4);
				MathHelper.QuaNormalize(localRotationData);
				this.transform.setLocalRotation(new Quaternion(localRotationData.X, localRotationData.Y, localRotationData.Z, localRotationData.w));
				this.prevState_.rotate_ = new Quaternion(localRotationData.X, localRotationData.Y, localRotationData.Z, localRotationData.w);
			}

			// 位置更新
			const velocity_ = this.prevState_.velocity_;
			let vector2 = this.goPlayKart_.m_KartWLVel.sub(velocity_);
			vector2 = new UnityVector3(
				Mathf.Clamp(vector2.X, -100, 100),
				0,
				Mathf.Clamp(vector2.Z, -100, 100),
			);

			const kartWLVel = this.goPlayKart_.m_KartWLVel;
			// 完全按照Unity源代码逻辑：第144行 - 使用Time.deltaTime
			const vector3 = kartWLVel.mul(UnityTime.getDeltaTime());
			const position = this.transform.getPosition();
			let vector4 = position.add(vector3);
			const vector5 = vector4.sub(position);

			// 优化 - 在Roblox中直接使用Part的Size和Position
			const robloxObject = this.gameObject;
			if (robloxObject) {
				const gameObjectPart = robloxObject.gameObject;
				if (gameObjectPart && gameObjectPart.IsA("BasePart")) {
					const part = gameObjectPart as BasePart;
					const size = fromRobloxVector3(part.Size);
					const num2 = Mathf.Min(size.X, size.Y, size.Z);

					if (vector5.getMagnitude() >= num2 / 2) {
						// Unity源码使用bounds.center，这里应该使用transform.position而不是robloxObject.Position
						const center = position; // 使用transform.position作为中心点
						const vector6 = center.add(vector3);
						const checkedVector6 = this.GetCollisionCheckedPosition(center, vector6);
						vector4 = position.add(checkedVector6.sub(center));
					}
				}
			}
			this.transform.setPosition(vector4);
		}

		// 计算实际速度
		if (this.rigidbody && "position" in this.rigidbody) {
			const rigidbodyPosition = fromRobloxVector3((this.rigidbody as unknown as { position: Vector3 }).position);
			const vector7 = rigidbodyPosition.sub(this.prevState_.position_);
			// 完全按照Unity源代码逻辑：第165行 - 使用Time.deltaTime
			this.goPlayKart_.m_KartRealVelocity = vector7.div(UnityTime.getDeltaTime());

			// 保存当前帧状态
			this.prevState_.position_ = rigidbodyPosition;
		}

		this.prevState_.velocity_ = this.goPlayKart_.m_KartWLVel;
		if (this.rigidbody && "angularVelocity" in this.rigidbody) {
			const rigidbodyAngularVelocity = (this.rigidbody as unknown as { angularVelocity: Vector3 }).angularVelocity;
			this.prevState_.angular_ = fromRobloxVector3(rigidbodyAngularVelocity);
		}
		if (this.transform) {
			this.prevState_.forward_ = this.transform.getForward();
		}
		this.prevState_.isGrounded_ = this.grounded;

		if (this.isSuddenChange) {
			this.isSuddenChange = false;
		}
		// 重置状态
		this.goPlayKart_.ResetCrash();
		this.goPlayKart_.ResetShock();
		// 按Unity源代码逻辑：在帧末尾重置grounded状态（对应Unity源码第170-171行）
		this.grounded = false;
	}

	// 方法9: GetCollisionCheckedPosition方法迁移 - 完全对应Unity版本
	private GetCollisionCheckedPosition(fromPosition: UnityVector3, toPosition: UnityVector3): UnityVector3 {
		// 对应Unity源码：Vector3 vector = toPosition - fromPosition
		const vector = toPosition.sub(fromPosition);
		// 对应Unity源码：Vector3 normalized = vector.normalized
		const normalized = vector.getNormalized();
		// 对应Unity源码：float magnitude = vector.magnitude
		const magnitude = vector.getMagnitude();

		// 对应Unity源码：RaycastHit[] array = Physics.RaycastAll(fromPosition, vector, vector.magnitude, 8413440)
		const array = Physics.RaycastAll(
			new Vector3(fromPosition.X, fromPosition.Y, fromPosition.Z), 
			new Vector3(vector.X, vector.Y, vector.Z), 
			magnitude, 
			8413440
		);

		// 对应Unity源码：if (array != null && array.Length > 0)
		if (array && array.size() > 0) {
			// 对应Unity源码：float num = magnitude
			let num = magnitude;
			// 对应Unity源码：foreach (RaycastHit raycastHit in array)
			for (const raycastHit of array) {
				// 对应Unity源码：if (!raycastHit.collider.gameObject.Equals(base.gameObject) && raycastHit.distance < num)
				// 注意：在Physics模块中已经排除了Player标签的物体，所以不需要再检查是否是自己
				if (raycastHit.distance < num) {
					// 对应Unity源码：toPosition = fromPosition + normalized * (raycastHit.distance - 0.1f)
					toPosition = fromPosition.add(normalized.mul(raycastHit.distance - 0.1));
					// 对应Unity源码：num = raycastHit.distance
					num = raycastHit.distance;
				}
			}
		}

		return toPosition;
	}

	// 方法11: OnCollisionStay方法迁移 - 完全对应Unity版本
	public OnCollisionStay(collisionInfo: any): void {
		this.OnCollisionDetection(collisionInfo);
	}

	// 方法12: OnCollisionEnter方法迁移 - 完全对应Unity版本
	public OnCollisionEnter(collisionInfo: any): void {
		this.OnCollisionDetection(collisionInfo);
	}

	// 方法13: OnCollisionDetection方法迁移 - 完全对应Unity版本
	private OnCollisionDetection(collisionInfo: unknown): void {
		const gameObject = (collisionInfo as { gameObject?: { Name?: string } }).gameObject;
		const name = gameObject?.Name || "unknown";
		print("碰撞到物体: " + name);
	}

	// 方法16: OnTriggerEnter方法迁移 - 修复Roblox适配
	public OnTriggerEnter(collider: BasePart): void {
		this.UpdateTriggerPosition(collider, true);
	}

	// 方法17: OnTriggerStay方法迁移 - 完全对应Unity版本
	public OnTriggerStay(collider: BasePart): void {
		this.UpdateTriggerPosition(collider, false);
	}

	// 方法18: UpdateTriggerPosition方法迁移 - 完全对应Unity版本（第245-337行）
	private UpdateTriggerPosition(collider: BasePart, firstTrigger: boolean): void {
		if (!this.goPlayKart_) return;

		// 对应Unity源码第247行：float num = 0.65f
		const num = 0.65;
		// 对应Unity源码第248行：if (1 << collider.gameObject.layer == 256)
		// collider.gameObject是Unity风格的对象，需要获取真实的Roblox Part
		// 处理Terrain特殊情况
		let robloxPart = collider;
		if (collider !== Workspace.Terrain) {
			// 在Roblox中，Part没有transform属性，直接使用Part本身
			if (collider.IsA("BasePart")) {
				robloxPart = collider;
			} else {
				const gameObject = (collider as unknown as { gameObject?: BasePart }).gameObject;
				robloxPart = gameObject || collider;
			}
		}

		const isTrackLayer = LayerMask.CheckLayer(robloxPart, LayerMask.LayerConst.TRACK);

		if (isTrackLayer) {
			// 对应Unity源码第250行：Vector3 position = base.transform.position
			const position = this.transform!.getPosition();
			// 对应Unity源码第251行：Vector3 vector = -base.transform.up
			const vector = this.transform!.getUp().mul(-1);
			// 对应Unity源码第252行：Vector3 vector2 = position - vector
			const vector2 = position.sub(vector);
			// 对应Unity源码第253行：float magnitude = vector.magnitude
			const magnitude = vector.getMagnitude();

			// 对应Unity源码第254行：Ray ray = new Ray(vector2, vector)
			// 对应Unity源码第255行：RaycastHit[] array = Physics.RaycastAll(ray, magnitude * 2f, 8413440)
			const array = Physics.RaycastAllFromRay(
				{ origin: toRobloxVector3(vector2), direction: toRobloxVector3(vector) }, 
				magnitude * 2.0, 
				8413440
			);

			// 对应Unity源码第256行：if (array != null && array.Length > 0)
			if (array && array.size() > 0) {
				// 对应Unity源码第258行：bool flag = false
				let flag = false;
				// 对应Unity源码第259行：float num2 = 0f
				let num2 = 0;
				// 对应Unity源码第260行：Vector3 vector3 = Vector3.zero
				let vector3 = UnityVector3.zero;
				// 对应Unity源码第261行：int num3 = 0
				let num3 = 0;
				// 对应Unity源码第262行：int num4 = 0
				let num4 = 0;

				// 对应Unity源码第263行：foreach (RaycastHit raycastHit in array)
				for (const raycastHit of array) {
					// 对应Unity源码第265行：if (raycastHit.normal.y > 0f && (!flag || raycastHit.distance < num2))
					if (raycastHit.normal.Y > 0 && (!flag || raycastHit.distance < num2)) {
						// 对应Unity源码第267行：num2 = raycastHit.distance
						num2 = raycastHit.distance;
						// 对应Unity源码第268行：vector3 = raycastHit.normal
						vector3 = raycastHit.normal;
						// 对应Unity源码第269行：num4 = raycastHit.collider.gameObject.layer
						num4 = raycastHit.layer;
						// 对应Unity源码第270行：flag = true
						flag = true;
					}
					// 对应Unity源码第272行：num3++
					num3 += 1;
				}

				// 对应Unity源码第274行：if (flag)
				if (flag) {
					// 对应Unity源码第276行：bool flag2 = false
					let flag2 = false;
					// 对应Unity源码第277行：if (magnitude - num2 > 0f)
					// 注意：这里检查的是车子是否在地面以下
					// magnitude是1（向下单位向量长度），num2是射线击中距离
					// 如果num2 < 1，说明地面距离车子不到1个单位，可能有穿透
					if (magnitude - num2 > 0) {
						// 对应Unity源码第279行：this._targetUpVector = vector3
						this._targetUpVector = vector3;
						// 对应Unity源码第280行：flag2 = true
						flag2 = true;
					}

					// 对应Unity源码第282行：if (this.goPlayKart_.m_Contact)
					if (this.goPlayKart_.m_Contact) {
						// 对应Unity源码第284行：vector3 = this.goPlayKart_.m_sus.contactN
						vector3 = this.goPlayKart_.m_sus.contactN;
						// 对应Unity源码第285行：this._targetUpVector = vector3
						this._targetUpVector = vector3;
						// 对应Unity源码第286行：flag2 = true
						flag2 = true;
					}

					// 对应Unity源码第288行：if (flag2)
					if (flag2) {
						// 内联Unity源码第290-324行的完整逻辑
						// 对应Unity源码第290行：float num5 = (float)(1 << num4)
						const num5 = LayerMask.LayerToBitMask(num4);

						// 对应Unity源码第291行：if (magnitude - num2 > 0f && num5 != 16384f)
						if (magnitude - num2 > 0 && num5 !== LayerMask.LayerConst.AI) {
							// 对应Unity源码第293行：Vector3 vector4 = -vector * (magnitude - num2)
							const vector4 = vector.mul(-(magnitude - num2));
							// 对应Unity源码第294行：base.transform.position += vector4
							if (this.transform) {
								this.transform.setPosition(this.transform.getPosition().add(vector4));
							}
						}

						// 对应Unity源码第296行：float num6 = Vector3.Dot(this.goPlayKart_.m_KartWLVel, vector3)
						const num6 = UnityVector3.Dot(this.goPlayKart_.m_KartWLVel, vector3);
						// 对应Unity源码第297行：Vector3 vector5 = vector3 * num6
						const vector5 = vector3.mul(num6);
						// 对应Unity源码第298行：Vector3 vector6 = this.goPlayKart_.m_KartWLVel - vector5
						const vector6 = this.goPlayKart_.m_KartWLVel.sub(vector5);

						// 对应Unity源码第299行：if (vector3.y > num)
						if (vector3.Y > num) {
							// 对应Unity源码第301行：float num7 = Mathf.Abs(Vector3.Dot(this.goPlayKart_.m_first.front, vector3)) * 0.7f
							const num7 = Mathf.Abs(UnityVector3.Dot(this.goPlayKart_.m_first.front, vector3)) * 0.7;
							// 对应Unity源码第302行：this.goPlayKart_.m_KartWLVel = vector5 * -num7 + vector6
							this.goPlayKart_.m_KartWLVel = vector5.mul(-num7).add(vector6);
							// 对应Unity源码第303行：this.goPlayKart_.shockVelocity_ = vector5.magnitude
							this.goPlayKart_.shockVelocity_ = vector5.getMagnitude();
							// 对应Unity源码第304行：this.grounded = true
							this.grounded = true;
							// 对应Unity源码第305-308行：if (!this.prevState_.isGrounded_)
							if (!this.prevState_.isGrounded_) {
								this.goPlayKart_.SetShock(Mathf.Abs(num6));
							}
						} else if (this.goPlayKart_.isCrash_) {
							// 对应Unity源码第310-318行
							const num8 = Mathf.Abs(num6);
							this.goPlayKart_.SetCrash(num8);
							if (num8 >= this.SMALL_CRASH_VELOCITY) {
								this.goPlayKart_.ResetDriftGauge();
							}
						}

						// 对应Unity源码第319-320行
						const num9 = UnityVector3.Dot(UnityVector3.Cross(vector3, UnityVector3.up), this.goPlayKart_.m_first.front);
						if (this.transform) {
							this.transform.RotateAroundLocal(this.transform.getUp(), num9 * 0.3 * UnityTime.getDeltaTime());
						}
					}
				}
			}
		}

		// 对应Unity源码第325-327行
		if (this.goPlayKart_.m_Contact) {
			// 接触处理（暂时为空）
		}

		// 对应Unity源码第328-336行
		if (!this.goPlayKart_.GetForcing() && !this.goPlayKart_.GetIsInResetState()) {
			// 在Roblox中直接使用Part的Position和Size
			const gameObjectPart = this.gameObject?.gameObject;
			const center = gameObjectPart && gameObjectPart.IsA("BasePart") 
				? fromRobloxVector3((gameObjectPart as BasePart).Position) 
				: this.transform!.getPosition();
			const size = gameObjectPart && gameObjectPart.IsA("BasePart") 
				? fromRobloxVector3((gameObjectPart as BasePart).Size) 
				: new UnityVector3(2, 2, 2);
			this.HandleCollision(this.transform!.getRight(), center, size.X / 2, true, num);
			this.HandleCollision(this.transform!.getRight().mul(-1), center, size.X / 2, true, num);
			this.HandleCollision(this.transform!.getForward(), center, size.Z * 0.75, true, num);
			this.HandleCollision(this.transform!.getForward().mul(-1), center, size.Z / 2, true, num);
		}
	}

	// HandleCollision方法迁移 - 对应Unity源码第340-376行
	private HandleCollision(
		localRayDirection: UnityVector3,
		rayOrigin: UnityVector3,
		rayLength: number,
		handleCrash: boolean,
		gndLimit: number,
	): boolean {
		if (!this.goPlayKart_ || !this.transform) return false;

		// 对应Unity源码第342-343行：RaycastHit raycastHit; if (Physics.Raycast...)
		const [hit, raycastHit] = Physics.Raycast(
			toRobloxVector3(rayOrigin), 
			toRobloxVector3(localRayDirection), 
			rayLength, 
			8413440
		);
		if (hit && raycastHit) {
			// 对应Unity源码第345行：Vector3 vector = raycastHit.normal
			let vector = raycastHit.normal;
			// 对应Unity源码第346行：if (raycastHit.distance < rayLength && vector.y < gndLimit)
			if (raycastHit.distance < rayLength && vector.Y < gndLimit) {
				// 对应Unity源码第348行：float num = Vector3.Dot(vector, localRayDirection)
				const num = UnityVector3.Dot(vector, localRayDirection);
				// 对应Unity源码第349-352行
				if (num > 0) {
					vector = vector.mul(-1);
				}
				// 对应Unity源码第353行：float num2 = Mathf.Abs(num)
				const num2 = Mathf.Abs(num);
				// 对应Unity源码第354行：float num3 = (float)(1 << raycastHit.collider.gameObject.layer)
				const num3 = LayerMask.LayerToBitMask(raycastHit.layer);
				// 对应Unity源码第355-359行
				if (num3 !== LayerMask.LayerConst.AI) {
					const vector2 = vector.mul(-(raycastHit.distance - rayLength) * num2);
					this.transform.setPosition(this.transform.getPosition().add(vector2));
				}
				// 对应Unity源码第360-375行
				if (handleCrash && !this.goPlayKart_.isCrash_) {
					const num4 = UnityVector3.Dot(vector, this.prevState_.velocity_);
					if (num4 < 0) {
						const num5 = Mathf.Abs(num4);
						this.goPlayKart_.SetCrash(num5);
						// 对应Unity源码第367-370行
						// Unity源码检查的是base.GetComponent<Collider>().gameObject.layer（自己的layer）
						// 但逻辑上应该检查碰撞物体是否是墙，使用num3（碰撞物体的layer）
						if (num3 === LayerMask.LayerConst.WALL && num5 >= this.SMALL_CRASH_VELOCITY) {
							this.goPlayKart_.ResetDriftGauge();
						}

						// 对应Unity源码第371-469行：完整的碰撞处理逻辑
						// 第371行：this.goPlayKart_.m_ctrl.oldSteerAngle = 0f;
						this.goPlayKart_.m_ctrl.oldSteerAngle = 0;
						// 第372行：float num6 = 0.618f;
						const num6 = 0.618;
						// 第373行：Vector3 vector3 = Vector3.zero;
						let vector3 = UnityVector3.zero;

						// 第374-381行：检查碰撞对象是否有RigidbodyFPSWalker组件
						// 仿照Unity的GetComponent模式
						const colliderObj = raycastHit.collider as unknown as { gameObject?: { transform?: Instance } };
						if (raycastHit.collider && colliderObj.gameObject && colliderObj.gameObject.transform) {
							const hitPart = colliderObj.gameObject.transform; // 被击中的Part
							// Unity中访问parent，在Roblox中应该访问Parent属性
							const parent = (hitPart as Instance).Parent;
							if (parent) {
								// 从组件注册表中查找RigidbodyFPSWalker组件
								if (parent.IsA("BasePart")) {
									const rigidbodyFPSWalker = ComponentRegistry.get(parent as BasePart);
									if (rigidbodyFPSWalker && rigidbodyFPSWalker.goPlayKart_) {
										vector3 = rigidbodyFPSWalker.goPlayKart_.m_KartWLVel;
									}
								}
							}
						}

						// 第382-385行：计算速度差和限制
						let vector4 = this.goPlayKart_.m_KartWLVel.sub(vector3.mul(num6));
						vector4 = new UnityVector3(
							Mathf.Clamp(vector4.X, -20, 20),
							Mathf.Clamp(vector4.Y, -5, 5),
							Mathf.Clamp(vector4.Z, -20, 20),
						);

						// 第386-387行：计算法线方向的速度分量
						const vector5 = vector.mul(UnityVector3.Dot(vector4, vector));
						const vector6 = this.goPlayKart_.m_KartWLVel.sub(vector5);

						// 第388-393行：处理垂直碰撞
						if (vector.Y > gndLimit) {
							const num7 = Mathf.Abs(UnityVector3.Dot(this.goPlayKart_.m_first.front, vector)) * 0.7;
							this.goPlayKart_.m_KartWLVel = vector5.mul(-num7).add(vector6);
							this.goPlayKart_.m_cState.shockVel = vector5.getMagnitude();
						} else {
							// 第395-399行：重置各种状态
							this.goPlayKart_.m_adBoost.validTrigger = false;
							this.goPlayKart_.m_driftGauge.progressOn = false;
							this.goPlayKart_.m_driftGauge.progressTime = 0;
							this.goPlayKart_.m_driftGauge.progress = 0;

							// 第400-402行：计算归一化方向
							const magnitude = vector6.getMagnitude();
							const vector7 = magnitude > 0 ? vector6.getNormalized() : UnityVector3.zero;

							// 第403行：计算反弹速度
							let vector8 = vector5.mul(-1.2).sub(vector7.mul(Mathf.Min(vector5.getMagnitude() * 1.2, magnitude * 0.02)));

							// 第404-444行：处理墙壁碰撞时的特殊旋转
							if (!this.goPlayKart_.m_drift.slipMode && this.goPlayKart_.m_Contact) {
								let flag = false;
								let num8 = 0;
								let num9 = 0;

								// 第409-423行：检查是否需要特殊旋转
								if (Mathf.Abs(UnityVector3.Dot(vector, this.transform!.getForward())) > 0.5 &&
									Mathf.Approximately(this.goPlayKart_.m_ctrl.getRealAccel(), 1)) {
									if (Mathf.Approximately(this.goPlayKart_.m_ctrl.steer, 1)) {
										flag = true;
										num8 = (!this.goPlayKart_.m_ctrl.wheelFlip && !this.goPlayKart_.m_ctrl.wheelDevil) ? 1 : -1;
										num9 = 1;
									} else if (Mathf.Approximately(this.goPlayKart_.m_ctrl.steer, -1)) {
										flag = true;
										num8 = (!this.goPlayKart_.m_ctrl.wheelFlip && !this.goPlayKart_.m_ctrl.wheelDevil) ? -1 : 1;
										num9 = -1;
									}
								}

								// 第424-444行：执行特殊旋转
								if (flag) {
									const vector9 = vector;
									let vector10 = this.goPlayKart_.m_first.front.mul(-1);
									vector10 = vector10.getNormalized();
									let num10 = UnityVector3.Dot(vector9, vector10);
									let num11: number;

									if (UnityVector3.Dot(vector9, this.goPlayKart_.m_first.left.mul(num9)) < 0) {
										num10 = 2 - num10;
										num11 = Mathf.Max(num10, 1.5);
									} else {
										num11 = num10 * num10 * num10;
									}

									vector8 = vector8.sub(vector5.getNormalized().mul(3 * num10 + 1));
									vector8 = vector8.add(this.goPlayKart_.m_first.left.mul((3 * num10 + 1) * num8));
									const num12 = (6 * num11 + 5) * num8 * UnityTime.getDeltaTime();
									if (this.transform) {
										this.transform.RotateAroundLocal(UnityVector3.up, num12);
									}
								}
							}
							// 第446行：应用计算后的速度
							this.goPlayKart_.m_KartWLVel = this.goPlayKart_.m_KartWLVel.add(vector8);

							// 第447-469行：处理侧向碰撞旋转
							const num13 = UnityVector3.Dot(vector, this.goPlayKart_.m_first.front);
							const num14 = UnityVector3.Dot(vector, this.goPlayKart_.m_first.left);
							const num15 = 3;

							if (Mathf.Abs(num13 * 0.8) > Mathf.Abs(num14)) {
								// 第452-458行：前后碰撞处理
								const zero = new UnityVector3(
									0,
									0,
									num14 *
										(num13 > 0 ? -1 : 1) *
										Mathf.Max(
											1,
											Mathf.Min(30, Mathf.Abs(UnityVector3.Dot(vector, this.goPlayKart_.m_KartWLVel) * 0.5)),
										),
								);

								if (UnityVector3.Dot(zero, this.goPlayKart_.m_KartLAVel) <= 1) {
									const num16 = zero.Z * UnityTime.getDeltaTime() * num15;
									if (this.transform) {
										this.transform.RotateAroundLocal(UnityVector3.up, num16);
									}
								}
							} else {
								// 第461-468行：侧向碰撞处理
								const zero2 = new UnityVector3(
									0,
									0,
									num13 *
										(num14 > 0 ? 1 : -1) *
										Mathf.Max(
											1,
											Mathf.Min(30, Mathf.Abs(UnityVector3.Dot(vector, this.goPlayKart_.m_KartWLVel) * 0.5)),
										),
								);

								if (UnityVector3.Dot(zero2, this.goPlayKart_.m_KartLAVel) <= 1) {
									const num17 = zero2.Z * UnityTime.getDeltaTime() * num15;
									if (this.transform) {
										this.transform.RotateAroundLocal(UnityVector3.up, num17);
									}
								}
							}
						}
						return true;
					}
				}
			}
		}
		return false;
	}
}
