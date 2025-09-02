import type { World } from "@rbxts/matter";
import { ItemBox } from "shared/ecs/components/items";
import { RunService, Workspace } from "@rbxts/services";

/**
 * 创建道具盒子模型
 */
function createBoxModel(position: Vector3): Part {
    const part = new Instance("Part");
    part.Name = "ItemBox";
    part.Size = new Vector3(4, 4, 4);
    part.Position = position;
    part.Anchored = true;
    part.CanCollide = false;
    part.BrickColor = new BrickColor("Bright yellow");
    part.Material = Enum.Material.Neon;
    part.Transparency = 0.3;
    part.Parent = Workspace;
    return part;
}

// 存储已创建的模型，避免重复创建
const boxModels = new Map<number, Part>();

/**
 * 客户端道具盒子渲染系统 - 根据服务端状态更新视觉效果
 */
function itemBoxClientSystem(world: World): void {
    // 只在客户端运行
    if (!RunService.IsClient()) return;

    // 处理新增的道具盒子
    for (const [boxEntity, itemBox] of world.queryChanged(ItemBox)) {
        if (itemBox.new && !boxModels.has(boxEntity)) {
            // 创建新的模型
            const model = createBoxModel(itemBox.new.position);
            boxModels.set(boxEntity, model);

            // 根据初始状态设置透明度
            model.Transparency = itemBox.new.available ? 0.3 : 1;
        }
    }

    // 处理状态变化
    for (const [boxEntity, itemBox] of world.query(ItemBox)) {
        const model = boxModels.get(boxEntity);
        if (model) {
            // 更新透明度以反映可用状态
            const targetTransparency = itemBox.available ? 0.3 : 1;
            if (model.Transparency !== targetTransparency) {
                model.Transparency = targetTransparency;
            }
        }
    }

    // 清理已删除的道具盒子
    for (const [boxEntity] of boxModels) {
        if (!world.contains(boxEntity as any)) {
            const model = boxModels.get(boxEntity);
            if (model) {
                model.Destroy();
                boxModels.delete(boxEntity);
            }
        }
    }
}

export = {
    system: itemBoxClientSystem,
    priority: 30,
};