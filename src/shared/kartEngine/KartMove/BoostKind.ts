/**
 * BoostKind enum的TypeScript实现
 * 与Lua版本完全一致
 */

export enum BoostKind {
    NoBoost = 0,
    BoostNormal = 1,
    BoostTeam = 2,
    BoostDrift = 3,
    BoostPlay = 4,
    BoostZone = 5,
    BoostJumpZone = 6,
    BoostStart = 7,
    BoostAnimal = 8,
    BoostDelivery = 9
}

export default BoostKind;