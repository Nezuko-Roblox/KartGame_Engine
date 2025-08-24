import { component } from "@rbxts/matter";

/** 标记实体为敌人的组件. */
export const Enemy = component<{
	enemyId: string;
}>("Enemy");
export type Enemy = ReturnType<typeof Enemy>;
