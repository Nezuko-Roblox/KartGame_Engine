// Unity Input.GetAxis的Roblox等效实现
// 模拟Unity的平滑输入机制

import { RunService, UserInputService } from "@rbxts/services";

interface AxisConfig {
    positiveKeys: Enum.KeyCode[];
    negativeKeys: Enum.KeyCode[];
    gravity: number;      // 松开键时的减速度
    sensitivity: number;  // 按下键时的加速度
    snap: boolean;        // 反向输入时立即切换
    deadZone: number;     // 死区
}

interface AxisState {
    value: number;
    targetValue: number;
    config: AxisConfig;
}

class UnityInputSystem {
    private axisStates: Map<string, AxisState> = new Map();

    // 默认配置（精确模拟Unity的Input Manager设置）
    private readonly axisConfig: Map<string, AxisConfig> = new Map([
        ["Horizontal", {
            positiveKeys: [Enum.KeyCode.D, Enum.KeyCode.Right],
            negativeKeys: [Enum.KeyCode.A, Enum.KeyCode.Left],
            gravity: 3.0,      // Unity默认值：松开键时的减速度
            sensitivity: 3.0,  // Unity默认值：按下键时的加速度  
            snap: true,        // Unity默认值：反向输入时立即切换
            deadZone: 0.001    // Unity默认值：死区
        }],
        ["Vertical", {
            positiveKeys: [Enum.KeyCode.W, Enum.KeyCode.Up],
            negativeKeys: [Enum.KeyCode.S, Enum.KeyCode.Down],
            gravity: 3.0,      // Unity默认值
            sensitivity: 3.0,  // Unity默认值
            snap: true,        // Unity默认值
            deadZone: 0.001    // Unity默认值
        }]
    ]);

    constructor() {
        // 初始化轴状态
        for (const [axisName, config] of this.axisConfig) {
            this.axisStates.set(axisName, {
                value: 0.0,
                targetValue: 0.0,
                config: config
            });
        }

        this.initialize();
    }

    // 检查某个轴的按键状态
    private getAxisInput(axisName: string): number {
        const config = this.axisConfig.get(axisName);
        if (!config) return 0;

        let positivePressed = false;
        let negativePressed = false;

        // 检查正向按键
        for (const key of config.positiveKeys) {
            if (UserInputService.IsKeyDown(key)) {
                positivePressed = true;
                print(`[输入调试] ${axisName} 正向按键被按下: ${key}`);
                break;
            }
        }

        // 检查负向按键
        for (const key of config.negativeKeys) {
            if (UserInputService.IsKeyDown(key)) {
                negativePressed = true;
                print(`[输入调试] ${axisName} 负向按键被按下: ${key}`);
                break;
            }
        }

        // 计算目标值
        if (positivePressed && negativePressed) {
            return 0;  // 两个方向同时按下，抵消
        } else if (positivePressed) {
            return 1;
        } else if (negativePressed) {
            return -1;
        } else {
            return 0;
        }
    }

    // 更新轴状态
    private updateAxis(axisName: string, deltaTime: number): void {
        const state = this.axisStates.get(axisName);
        if (!state) return;

        const config = state.config;
        const currentValue = state.value;
        const targetValue = this.getAxisInput(axisName);

        // 如果启用了snap且方向发生反转，立即切换
        if (config.snap && ((currentValue > 0 && targetValue < 0) || (currentValue < 0 && targetValue > 0))) {
            state.value = 0;
            return;
        }

        // 计算新值
        let newValue = currentValue;

        if (targetValue !== 0) {
            // 向目标值移动（加速）
            const direction = targetValue > currentValue ? 1 : -1;
            newValue = currentValue + direction * config.sensitivity * deltaTime;

            // 限制在目标值范围内
            if (direction > 0) {
                newValue = math.min(newValue, targetValue);
            } else {
                newValue = math.max(newValue, targetValue);
            }
        } else {
            // 向0移动（减速）
            if (math.abs(currentValue) > config.deadZone) {
                const direction = currentValue > 0 ? -1 : 1;
                newValue = currentValue + direction * config.gravity * deltaTime;

                // 防止过冲
                if ((currentValue > 0 && newValue < 0) || (currentValue < 0 && newValue > 0)) {
                    newValue = 0;
                }
            } else {
                newValue = 0;
            }
        }

        // 应用死区
        if (math.abs(newValue) < config.deadZone) {
            newValue = 0;
        }

        // 限制在[-1, 1]范围内
        newValue = math.max(-1, math.min(1, newValue));

        state.value = newValue;
    }

    // 获取轴值（主要接口）
    public GetAxis(axisName: string): number {
        const state = this.axisStates.get(axisName);
        if (!state) return 0;
        return state.value;
    }

    // 获取原始轴值（不经过平滑）
    public GetAxisRaw(axisName: string): number {
        return this.getAxisInput(axisName);
    }

    // 检查按键是否被按下
    public GetKey(keyCode: Enum.KeyCode): boolean {
        return UserInputService.IsKeyDown(keyCode);
    }

    // 检查按键是否刚被按下
    public GetKeyDown(keyCode: Enum.KeyCode): boolean {
        // 这需要额外的状态跟踪，暂时简化实现
        return UserInputService.IsKeyDown(keyCode);
    }

    // 设置轴配置
    public SetAxisConfig(axisName: string, config: Partial<AxisConfig>): void {
        const currentConfig = this.axisConfig.get(axisName);
        if (currentConfig) {
            const newConfig = { ...currentConfig, ...config };
            this.axisConfig.set(axisName, newConfig);
            
            const state = this.axisStates.get(axisName);
            if (state) {
                state.config = newConfig;
            }
        }
    }

    // 初始化输入系统
    private initialize(): void {
        // 只在客户端连接到RenderStepped来更新输入状态
        if (RunService.IsClient()) {
            print("[Unity输入系统] 客户端输入系统初始化");
            
            // 检查键盘输入是否可用
            print(`[Unity输入系统] 键盘输入状态: ${UserInputService.KeyboardEnabled}`);
            
            RunService.RenderStepped.Connect((deltaTime) => {
                for (const [axisName] of this.axisStates) {
                    this.updateAxis(axisName, deltaTime);
                }
            });
        } else {
            print("[Unity输入系统] 服务器端，跳过输入系统初始化");
        }
    }
}

// 创建全局实例
const unityInputSystem = new UnityInputSystem();

// 导出Unity风格的静态访问接口
export const UnityInput = {
    GetAxis: (axisName: string) => unityInputSystem.GetAxis(axisName),
    GetAxisRaw: (axisName: string) => unityInputSystem.GetAxisRaw(axisName),
    GetKey: (keyCode: Enum.KeyCode) => unityInputSystem.GetKey(keyCode),
    GetKeyDown: (keyCode: Enum.KeyCode) => unityInputSystem.GetKeyDown(keyCode),
    SetAxisConfig: (axisName: string, config: Partial<AxisConfig>) => unityInputSystem.SetAxisConfig(axisName, config)
};

// 兼容Unity的Input静态类风格
export const Input = UnityInput;

// 导出类型
export { AxisConfig };
