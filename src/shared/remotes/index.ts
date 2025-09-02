import { createRemotes } from "@rbxts/remo";

import { storeRemote } from "./namespaces";
import { mtxRemote } from "./namespaces/mtx";
import { itemsRemote } from "./namespaces/items";
import { kartRemote } from "./namespaces/kart";

export const remotes = createRemotes({
	mtx: mtxRemote,
	store: storeRemote,
	items: itemsRemote,
	kart: kartRemote,
});
