import type { AnyEntity, World } from "@rbxts/matter";
import type { ComponentCtor } from "@rbxts/matter/lib/component";

import { TRIGGER_COMPONENTS } from "shared/ecs/components/trigger";
import type { ClientState } from "shared/ecs/constants/client-state";

import { Renderable, Transform } from "../components";
import { TAGGED_COMPONENTS } from "../components/tagged";

const Workspace = game.GetService("Workspace");
const CollectionService = game.GetService("CollectionService");
const isServer = game.GetService("RunService").IsServer();

/**
 * 设置标签系统，为带有标签的模型创建对应的 Matter 实体.
 *
 * @param world - The Matter world instance to insert and manage entities.
 * @param state - The client state object used for entity ID mapping.
 */
export function setupTags(world: World, state: ClientState): void {
	const entityKey = isServer ? "id" : "clientEntityId";
	/**
	 * 为绑定的模型生成实体和组件.
	 *
	 * @param model - The Roblox Model instance to bind and generate an entity
	 *   for.
	 * @param component - The Matter component constructor to attach to the
	 *   entity.
	 */
	function spawnBound(model: Model, component: ComponentCtor): void {
		const newComponent = component();
		if (model.GetAttribute(entityKey) !== undefined) {
			const atts: Record<string, unknown> = {};
			for (const [key, value] of pairs(model.GetAttributes())) {
				if (key !== "id" && key !== "clientEntityId") {
					atts[key] = value;
				}
			}

			const entity = model.GetAttribute(entityKey) as AnyEntity;
			// print("inserting into", entity, tostring(component));
			world.insert(entity, newComponent.patch(atts));
			return;
		}

		const id = world.spawn(
			newComponent.patch(model.GetAttributes()),
			Renderable({ model }),
			Transform({ cf: model.GetPivot() }),
		);
		if (!isServer) {
			const serverId = model.GetAttribute("id") as AnyEntity | undefined;
			if (serverId !== undefined) {
				state.entityIdMap.set(`${serverId}`, id);
			}
		}

		// print("spawned", id, tostring(component));
		model.SetAttribute(entityKey, id);
	}

	for (const newComponent of [...TAGGED_COMPONENTS, ...TRIGGER_COMPONENTS]) {
		const tagName = tostring(newComponent);
		for (const instance of CollectionService.GetTagged(tagName)) {
			// print("tagged", instance.Name, tagName);
			if (instance.IsDescendantOf(Workspace)) {
				spawnBound(instance as Model, newComponent);
			}
		}

		CollectionService.GetInstanceAddedSignal(tagName).Connect(instance => {
			if (instance.IsDescendantOf(Workspace)) {
				spawnBound(instance as Model, newComponent);
			}
		});

		CollectionService.GetInstanceRemovedSignal(tagName).Connect(instance => {
			const id = instance.GetAttribute(entityKey) as AnyEntity | undefined;

			if (id !== undefined && world.contains(id)) {
				world.despawn(id);
			}
		});
	}
}
