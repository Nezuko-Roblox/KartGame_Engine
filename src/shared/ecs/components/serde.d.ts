import { GenericOfComponent } from "@rbxts/matter";
import * as Components from "shared/ecs/components";

/** 组件名称类型 */
declare type ComponentNames = keyof typeof Components;
/** 将组件映射到名称的类型 */
declare type MappedComponentToName<T extends ComponentNames> = GenericOfComponent<ReturnType<(typeof Components)[T]>>;

/** 组件映射类型 */
declare type ComponentsMap<T extends ComponentNames> = T extends []
	? T
	: T extends [infer F, ...infer B]
		? F extends keyof T
			? B extends ComponentNames
				? [MappedComponentToName<T>, ...ComponentsMap<B>]
				: never
			: never
		: never;

/** 联合组件映射类型 */
declare type UnionComponentsMap = ComponentsMap<ComponentNames>;
