/**
 * Vector3Helper class的TypeScript实现
 * 与Lua版本完全一致，提供Vector3辅助函数
 */

import { UnityVector3 } from "../KartShared/UnityMath";

export class Vector3Helper {
    public static SetVector3(v: UnityVector3, x: number, y?: number, z?: number): UnityVector3 {
        if (y === undefined && z === undefined) {
            // SetVector3(ref Vector3 v, float t) - 重载版本
            const t = x;
            return Vector3Helper.SetVector3(v, t, t, t);
        } else {
            // SetVector3(ref Vector3 v, float x, float y, float z)
            return new UnityVector3(x, y!, z!);
        }
    }

    public static Scale(scale: number, vector: UnityVector3): UnityVector3 {
        return new UnityVector3(vector.X * scale, vector.Y * scale, vector.Z * scale);
    }
}

export default Vector3Helper;