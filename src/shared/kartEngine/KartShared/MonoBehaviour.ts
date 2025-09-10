import { RunService } from "@rbxts/services";
import RobloxUnityAdapter, { GameObject, Transform } from "./RobloxUnityAdapter";

export class MonoBehaviour {
    public scriptName: string;
    public gameObject: GameObject | undefined;
    public transform: Transform | undefined;
    public enabled: boolean;
    public started: boolean;
    public destroyed: boolean;
    
    public awakeCallbacks: ((self: MonoBehaviour) => void)[] = [];
    public startCallbacks: ((self: MonoBehaviour) => void)[] = [];
    public updateCallbacks: ((self: MonoBehaviour, deltaTime: number) => void)[] = [];
    public fixedUpdateCallbacks: ((self: MonoBehaviour, fixedDeltaTime: number) => void)[] = [];
    public lateUpdateCallbacks: ((self: MonoBehaviour, deltaTime: number) => void)[] = [];
    public destroyCallbacks: ((self: MonoBehaviour) => void)[] = [];
    
    public _heartbeatConn: RBXScriptConnection | undefined;
    public _steppedConn: RBXScriptConnection | undefined;
    
    constructor(scriptName?: string) {
        this.scriptName = scriptName || "UnknownScript";
        this.gameObject = undefined;
        this.transform = undefined;
        this.enabled = true;
        this.started = false;
        this.destroyed = false;
        
        this.BindToRoblox();
        
        task.defer(() => {
            this.Awake();
        });
    }
    
    public Awake(): void {
        for (const callback of this.awakeCallbacks) {
            callback(this);
        }
    }
    
    public Start(): void {
        if (!this.started) {
            this.started = true;
            for (const callback of this.startCallbacks) {
                callback(this);
            }
        }
    }
    
    public Update(deltaTime: number): void {
        if (!this.enabled || this.destroyed) return;
        if (!this.started) this.Start();
        
        for (const callback of this.updateCallbacks) {
            callback(this, deltaTime);
        }
    }
    
    public FixedUpdate(fixedDeltaTime: number): void {
        if (!this.enabled || this.destroyed) return;
        
        for (const callback of this.fixedUpdateCallbacks) {
            callback(this, fixedDeltaTime);
        }
    }
    
    public LateUpdate(deltaTime: number): void {
        if (!this.enabled || this.destroyed) return;
        
        for (const callback of this.lateUpdateCallbacks) {
            callback(this, deltaTime);
        }
    }
    
    public OnDestroy(): void {
        this.destroyed = true;
        
        if (this._heartbeatConn) {
            this._heartbeatConn.Disconnect();
            this._heartbeatConn = undefined;
        }
        if (this._steppedConn) {
            this._steppedConn.Disconnect();
            this._steppedConn = undefined;
        }
        
        for (const callback of this.destroyCallbacks) {
            callback(this);
        }
    }
    
    public BindToRoblox(): void {
        if (this._heartbeatConn) {
            this._heartbeatConn.Disconnect();
        }
        if (this._steppedConn) {
            this._steppedConn.Disconnect();
        }
        
        this._heartbeatConn = RunService.Heartbeat.Connect((dt) => {
            if (this.enabled && !this.destroyed) {
                this.Update(dt);
                this.LateUpdate(dt);
            }
        });
        
        this._steppedConn = RunService.Stepped.Connect((_, fixedDt) => {
            if (this.enabled && !this.destroyed) {
                this.FixedUpdate(fixedDt);
            }
        });
    }
    
    public SetEnabled(enabled: boolean): void {
        this.enabled = enabled;
    }
    
    public Destroy(): void {
        if (!this.destroyed) {
            this.OnDestroy();
        }
    }
    
    public AddAwakeCallback(callback: (self: MonoBehaviour) => void): void {
        this.awakeCallbacks.push(callback);
    }
    
    public AddStartCallback(callback: (self: MonoBehaviour) => void): void {
        this.startCallbacks.push(callback);
    }
    
    public AddUpdateCallback(callback: (self: MonoBehaviour, deltaTime: number) => void): void {
        this.updateCallbacks.push(callback);
    }
    
    public AddFixedUpdateCallback(callback: (self: MonoBehaviour, fixedDeltaTime: number) => void): void {
        this.fixedUpdateCallbacks.push(callback);
    }
    
    public AddLateUpdateCallback(callback: (self: MonoBehaviour, deltaTime: number) => void): void {
        this.lateUpdateCallbacks.push(callback);
    }
    
    public AddDestroyCallback(callback: (self: MonoBehaviour) => void): void {
        this.destroyCallbacks.push(callback);
    }
    
    public SetGameObject(robloxObject: Instance): void {
        print(`[MonoBehaviour] SetGameObject - 接收到robloxObject: ${robloxObject ? robloxObject.Name : "nil"}`);
        if (!robloxObject) {
            warn("SetGameObject: robloxObject is nil, skipping GameObject creation");
            return;
        }
        this.gameObject = RobloxUnityAdapter.GameObject.create(robloxObject);
        print(`[MonoBehaviour] SetGameObject - 创建的gameObject: ${this.gameObject ? "成功" : "失败"}`);
        this.transform = this.gameObject!.transform;
        print(`[MonoBehaviour] SetGameObject - 创建的transform: ${this.transform ? "成功" : "失败"}`);
        print(`[MonoBehaviour] SetGameObject - 验证this.gameObject存在: ${this.gameObject !== undefined}`);
    }
    
    public GetComponent(componentType: string): any {
        if (this.gameObject) {
            return this.gameObject.GetComponent(componentType);
        }
        return undefined;
    }
    
    public GetComponentInChildren(componentType: string): Instance | undefined {
        if (this.gameObject) {
            return this.gameObject.GetComponentInChildren(componentType);
        }
        return undefined;
    }
    
    public GetComponentsInChildren(componentType: string): Instance[] {
        if (this.gameObject) {
            return this.gameObject.GetComponentsInChildren(componentType);
        }
        return [];
    }
    
    public AddComponent(componentType: string): Instance | undefined {
        if (this.gameObject) {
            return this.gameObject.AddComponent(componentType);
        }
        return undefined;
    }
}