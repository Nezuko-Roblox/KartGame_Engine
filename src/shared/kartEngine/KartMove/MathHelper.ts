/**
 * MathHelper class的TypeScript实现
 * 与Lua版本完全一致，提供数学辅助函数
 */

// Quaternion interface to match Lua implementation
interface QuaternionData {
    X: number;
    Y: number;
    Z: number;
    w: number;
}

export class MathHelper {
    // 静态构造函数的等效实现
    public static readonly next = [1, 2, 0];  // C#中的数组索引从0开始，这里调整为Lua的1开始

    public static CreateQuaternion(w: number, v: Vector3): QuaternionData {
        return { X: v.X, Y: v.Y, Z: v.Z, w: w };
    }

    public static QuaMulScala(q: QuaternionData, v: number): void {
        q.X = q.X * v;
        q.Y = q.Y * v;
        q.Z = q.Z * v;
        q.w = q.w * v;
    }

    public static QuaNormalize(q: QuaternionData): void {
        const magnitude = math.sqrt(q.w * q.w + q.X * q.X + q.Y * q.Y + q.Z * q.Z);
        MathHelper.QuaMulScala(q, 1.0 / magnitude);
    }

    public static QuaAdd(q1: QuaternionData, q2: QuaternionData): void {
        q1.w = q1.w + q2.w;
        q1.X = q1.X + q2.X;
        q1.Y = q1.Y + q2.Y;
        q1.Z = q1.Z + q2.Z;
    }

    public static IsBetweenII(value: number, min: number, max: number): boolean {
        return value >= min && value <= max;
    }

    public static IsBetweenIE(value: number, min: number, max: number): boolean {
        return value >= min && value < max;
    }
}

export default MathHelper;