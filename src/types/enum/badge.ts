import { GameId, getConfigValueForGame } from "shared/functions/game-config";

export const Badge = {
	Welcome: getConfigValueForGame({
		[GameId.Development]: "3630460038655754",
		[GameId.Production]: "1933841780815262",
		[GameId.Testing]: "3630460038655754",
	}),
} as const;

export type Badge = ValueOf<typeof Badge>;
