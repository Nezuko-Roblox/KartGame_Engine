import type { AnyEntity, World } from "@rbxts/matter";

import { BoundEntities } from "shared/ecs/components";

const isServer = game.GetService("RunService").IsServer();

const entityKey = isServer ? "id" : "clientEntityId";

/**
 * 销毁绑定实体的系统 负责管理和清理与其他实体绑定的模型和实体，当绑定关系发生变化时自动处理销毁逻辑.
 *
 * @param world - Matter ECS 世界实例.
 */
function destroyBoundEntities(world: World) {
	for (const [id, boundEntities] of world.queryChanged(BoundEntities)) {
		if (!world.contains(id)) {
			continue;
		}

		if (boundEntities.new === undefined && boundEntities.old !== undefined) {
			for (const model of boundEntities.old.models) {
				model.Destroy();
			}

			for (const entity of boundEntities.old.entities) {
				if (world.contains(entity)) {
					world.despawn(entity);
				}
			}
		}
	}

	for (const [id, boundEntities] of world.query(BoundEntities)) {
		if (boundEntities.models.size() > 0) {
			const toRemove: Array<Model> = [];
			const toAdd: Array<AnyEntity> = [];
			for (const model of boundEntities.models) {
				if (model.Parent === undefined) {
					toRemove.push(model);
				} else {
					const entityId = model.GetAttribute(entityKey) as AnyEntity | undefined;
					if (entityId !== undefined && world.contains(entityId)) {
						toAdd.push(entityId);
						toRemove.push(model);
					}
				}
			}

			if (toRemove.size() > 0 || toAdd.size() > 0) {
				world.insert(
					id,
					boundEntities.patch({
						entities: [...boundEntities.entities, ...toAdd],
						models: boundEntities.models.filter(model => !toRemove.includes(model)),
					}),
				);
			}
		}
	}
}

export = {
	system: destroyBoundEntities,
};
