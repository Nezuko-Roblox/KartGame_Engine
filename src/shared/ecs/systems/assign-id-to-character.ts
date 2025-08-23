import type { World } from "@rbxts/matter";

import { PlayerModel } from "shared/ecs/components";

const isServer = game.GetService("RunService").IsServer();

const entityKey = isServer ? "id" : "clientEntityId";

/**
 * 为角色模型分配实体ID的系统 当玩家模型组件发生变化时，将对应的实体ID设置为角色模型的属性.
 *
 * @param world - Matter ECS 世界实例.
 */
function assignIdToCharacter(world: World) {
	for (const [id, changed] of world.queryChanged(PlayerModel)) {
		if (!world.contains(id) || changed.new?.character.PrimaryPart === undefined) {
			continue;
		}

		const { character } = changed.new;
		character.SetAttribute(entityKey, id);
	}
}

export = {
	system: assignIdToCharacter,
};
