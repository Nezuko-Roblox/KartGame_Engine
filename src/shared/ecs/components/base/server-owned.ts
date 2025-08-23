import { component } from "@rbxts/matter";

/** 使实体在网络上由服务器拥有. */
export const ServerOwned = component<{}>("ServerOwned");
export type ServerOwned = ReturnType<typeof ServerOwned>;
