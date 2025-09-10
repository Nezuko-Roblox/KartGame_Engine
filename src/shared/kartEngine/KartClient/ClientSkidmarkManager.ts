// 简单的轮胎印记系统
import { RunService, Debris, Workspace } from "@rbxts/services";
import { RigidbodyFPSWalker } from "../KartMove/RigidbodyFPSWalker";
import { GoPlayKart } from "../KartMove/GoPlayKart";

export class ClientSkidmarkManager {
    private rigidbodyWalker: RigidbodyFPSWalker;
    private goPlayKart: GoPlayKart | undefined = undefined;
    private skidmarkFolder: Folder;
    private updateConnection?: RBXScriptConnection;

    constructor(rigidbodyWalker: RigidbodyFPSWalker) {
        this.rigidbodyWalker = rigidbodyWalker;
        this.goPlayKart = undefined;
        
        // 痕迹容器
        this.skidmarkFolder = Workspace.FindFirstChild("Skidmarks") as Folder;
        if (!this.skidmarkFolder) {
            this.skidmarkFolder = new Instance("Folder");
            this.skidmarkFolder.Name = "Skidmarks";
            this.skidmarkFolder.Parent = Workspace;
        }
        
        // 开始更新循环
        this.updateConnection = RunService.Heartbeat.Connect(() => {
            this.Update();
        });
    }

    Update(): void {
        // 获取 goPlayKart
        if (!this.goPlayKart) {
            this.goPlayKart = (this.rigidbodyWalker as unknown as { goPlayKart_: GoPlayKart }).goPlayKart_;
            if (!this.goPlayKart) {
                return;
            }
        }
        
        // 使用现成的状态变量：漂移或滑行时生成印记
        const isDrifting = this.goPlayKart.getIsDrift();  // 漂移状态
        const isSlipping = this.goPlayKart.getDriftSlipMode();  // 滑行状态
        
        // 漂移或滑行时生成印记
        if (isDrifting || isSlipping) {
            this.CreateSkidmarks();
        }
    }

    CreateSkidmarks(): void {
        // 获取后轮位置
        const rearWheelPositions = this.GetRearWheelPositions();
        if (!rearWheelPositions) {
            return;
        }
        
        // 在每个后轮位置生成印记
        for (const position of rearWheelPositions) {
            this.CreateSkidmarkAt(position);
        }
    }

    GetRearWheelPositions(): Vector3[] | undefined {
        const rigidbody = (this.rigidbodyWalker as unknown as { rigidbody: unknown }).rigidbody;
        if (!rigidbody) {
            return undefined;
        }
        
        const actualPart = (rigidbody as unknown as { GameObject: { gameObject: Instance } }).GameObject.gameObject;
        if (!actualPart) {
            return undefined;
        }
        
        // 直接从轮子Part获取位置
        const rearWheelPositions: Vector3[] = [];
        
        // 查找后轮Part（tire2和tire3）
        const tire2 = (actualPart as Instance).FindFirstChild("tire2") as BasePart | undefined;  // 后左轮
        const tire3 = (actualPart as Instance).FindFirstChild("tire3") as BasePart | undefined;  // 后右轮
        
        if (tire2) {
            rearWheelPositions.push(tire2.Position);
        }
        
        if (tire3) {
            rearWheelPositions.push(tire3.Position);
        }
        
        return rearWheelPositions.size() > 0 ? rearWheelPositions : undefined;
    }

    CreateSkidmarkAt(position: Vector3): void {
        // 直接在轮子位置下方创建印记
        const groundPos = new Vector3(position.X, position.Y - 0.5, position.Z);  // 向下偏移0.5单位
        
        // 创建简单的印记Part
        const skidmark = new Instance("Part");
        skidmark.Name = "Skidmark";
        skidmark.Size = new Vector3(0.8, 0.01, 0.3);
        skidmark.Position = groundPos;
        skidmark.Anchored = true;
        skidmark.CanCollide = false;
        skidmark.CanTouch = false;
        skidmark.CanQuery = false;
        skidmark.Material = Enum.Material.Concrete;
        skidmark.BrickColor = new BrickColor("Really black");
        skidmark.Parent = this.skidmarkFolder;
        
        // 5秒后自动删除
        Debris.AddItem(skidmark, 5);
    }

    Destroy(): void {
        if (this.updateConnection) {
            this.updateConnection.Disconnect();
        }
        
        if (this.skidmarkFolder) {
            this.skidmarkFolder.ClearAllChildren();
        }
    }
}
