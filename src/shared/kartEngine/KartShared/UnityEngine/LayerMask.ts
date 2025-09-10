/**
 * LayerMask的TypeScript实现
 * 与Lua版本完全一致
 */

import { CollectionService, Workspace } from "@rbxts/services";

export class LayerMask {
    // Unity Layer配置表 - 基于LayerConst.cs的官方定义
    public static readonly LayerConst = {
        TRACK: 256,                // layer 8 (1 << 8) - 地面/跑道
        GUI: 512,                  // layer 9 (1 << 9)
        CHARACTER: 1024,           // layer 10 (1 << 10)
        MINIMAP: 2048,             // layer 11 (1 << 11)
        RESPAWN: 4096,             // layer 12 (1 << 12)
        PLAYER: 8192,              // layer 13 (1 << 13)
        AI: 16384,                 // layer 14 (1 << 14)
        AI_RESPAWN: 32768,         // layer 15 (1 << 15)
        AI_SECTION: 65536,         // layer 16 (1 << 16) - 触发器区域
        BOOSTER_ENHANCER: 4194304, // layer 22 (1 << 22)
        WALL: 8388608              // layer 23 (1 << 23) - 墙壁
    };

    // 反向映射表（从二进制值到layer编号）
    private static BitMaskToLayerNumber: Record<number, number> = {};

    static {
        // 初始化反向映射表
        for (const [name, bitMask] of pairs(this.LayerConst)) {
            this.BitMaskToLayerNumber[bitMask] = math.log(bitMask) / math.log(2);
        }
    }

    // 将layer编号转换为Unity的二进制值
    public static LayerToBitMask(layerNumber: number): number {
        return 2 ** layerNumber;
    }

    // 将Unity二进制值转换为layer编号
    public static BitMaskToLayer(bitMask: number): number {
        return this.BitMaskToLayerNumber[bitMask] || 0;
    }

    // 获取GameObject的layer（模拟Unity的gameObject.layer属性）
    public static GetLayer(gameObject: Instance): number {
        if (!gameObject) return 0;
        
        // 特殊处理Terrain
        if (gameObject === Workspace.Terrain) {
            return 8; // TRACK layer - Terrain总是地面
        }
        
        // 检查预定义的层标签
        if (CollectionService.HasTag(gameObject, "Track")) {
            return 8;  // TRACK
        } else if (CollectionService.HasTag(gameObject, "Wall")) {
            return 23; // WALL
        } else if (CollectionService.HasTag(gameObject, "Player")) {
            return 13; // PLAYER
        } else if (CollectionService.HasTag(gameObject, "AI")) {
            return 14; // AI
        }
        
        return 0; // 默认layer
    }

    // 设置GameObject的layer
    public static SetLayer(gameObject: Instance, layerNumber: number): void {
        // 移除所有现有的layer标签
        for (const [bitMask, existingLayerNumber] of pairs(this.BitMaskToLayerNumber)) {
            const tagName = `Layer${existingLayerNumber}`;
            if (CollectionService.HasTag(gameObject, tagName)) {
                CollectionService.RemoveTag(gameObject, tagName);
            }
        }
        
        // 添加新的layer标签
        const newTagName = `Layer${layerNumber}`;
        CollectionService.AddTag(gameObject, newTagName);
    }

    // 检查物体是否属于指定layer（对应Unity的 1 << layer 检查）
    public static CheckLayer(gameObject: Instance, expectedBitMask: number): boolean {
        if (!gameObject) return false;
        
        // 特殊处理Terrain
        if (gameObject === Workspace.Terrain) {
            return expectedBitMask === this.LayerConst.TRACK;
        }
        
        const layer = this.GetLayer(gameObject);
        const currentBitMask = this.LayerToBitMask(layer);
        
        return currentBitMask === expectedBitMask;
    }

    public static CheckLayerMask(layer: number, layerMask: number): boolean {
        return (layerMask & (1 << layer)) !== 0;
    }

    // 检查layer mask（用于射线检测等）
    public static CheckLayerMaskByNumber(layerNumber: number, layerMask: number): boolean {
        const bitMask = this.LayerToBitMask(layerNumber);
        return (layerMask & bitMask) !== 0;
    }

    public static GetMask(layerNames: string[]): number {
        let mask = 0;
        for (const name of layerNames) {
            mask |= (1 << this.NameToLayer(name));
        }
        return mask;
    }

    public static NameToLayer(layerName: string): number {
        // 实现层级名称到编号的映射
        const layerMap: Record<string, number> = {
            "Default": 0,
            "TransparentFX": 1,
            "IgnoreRaycast": 2,
            "Water": 4,
            "UI": 5,
            "Player": 8,
            "Ground": 9,
            "Wall": 10,
            "Item": 11
        };
        return layerMap[layerName] || 0;
    }
}

export default LayerMask;