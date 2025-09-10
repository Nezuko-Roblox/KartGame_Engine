// Unity数学库的TypeScript等效实现

// Vector2类
export class Vector2 {
    public X: number;
    public Y: number;

    constructor(x: number = 0, y: number = 0) {
        this.X = x;
        this.Y = y;
    }

    public getMagnitude(): number {
        return math.sqrt(this.X * this.X + this.Y * this.Y);
    }

    public GetMagnitude(): number {
        return this.getMagnitude();
    }

    public getSqrMagnitude(): number {
        return this.X * this.X + this.Y * this.Y;
    }

    public GetSqrMagnitude(): number {
        return this.getSqrMagnitude();
    }

    public getNormalized(): Vector2 {
        const mag = math.sqrt(this.X * this.X + this.Y * this.Y);
        if (mag > 0) {
            return new Vector2(this.X / mag, this.Y / mag);
        }
        return new Vector2(0, 0);
    }

    public GetNormalized(): Vector2 {
        return this.getNormalized();
    }

    public getX(): number {
        return this.X;
    }

    public getY(): number {
        return this.Y;
    }

    public add(other: Vector2): Vector2 {
        return new Vector2(this.X + other.X, this.Y + other.Y);
    }

    public sub(other: Vector2): Vector2 {
        return new Vector2(this.X - other.X, this.Y - other.Y);
    }

    public mul(scalar: number): Vector2 {
        return new Vector2(this.X * scalar, this.Y * scalar);
    }

    public static readonly zero = new Vector2(0, 0);
    public static readonly one = new Vector2(1, 1);
}

// Unity风格Vector3类
export class UnityVector3 {
    public X: number;
    public Y: number;
    public Z: number;

    constructor(x: number = 0, y: number = 0, z: number = 0) {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }

    public getMagnitude(): number {
        return math.sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
    }

    public GetMagnitude(): number {
        return this.getMagnitude();
    }

    public getSqrMagnitude(): number {
        return this.X * this.X + this.Y * this.Y + this.Z * this.Z;
    }

    public GetSqrMagnitude(): number {
        return this.getSqrMagnitude();
    }

    public getNormalized(): UnityVector3 {
        const mag = math.sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
        if (mag > 0) {
            return new UnityVector3(this.X / mag, this.Y / mag, this.Z / mag);
        }
        return new UnityVector3(0, 0, 0);
    }

    public GetNormalized(): UnityVector3 {
        return this.getNormalized();
    }

    public getX(): number {
        return this.X;
    }

    public getY(): number {
        return this.Y;
    }

    public getZ(): number {
        return this.Z;
    }

    public add(other: UnityVector3): UnityVector3 {
        return new UnityVector3(this.X + other.X, this.Y + other.Y, this.Z + other.Z);
    }

    public sub(other: UnityVector3): UnityVector3 {
        return new UnityVector3(this.X - other.X, this.Y - other.Y, this.Z - other.Z);
    }

    public mul(scalar: number | UnityVector3): UnityVector3 {
        if (typeOf(scalar) === "number") {
            const s = scalar as number;
            return new UnityVector3(this.X * s, this.Y * s, this.Z * s);
        } else {
            const v = scalar as UnityVector3;
            return new UnityVector3(this.X * v.X, this.Y * v.Y, this.Z * v.Z);
        }
    }

    public div(scalar: number): UnityVector3 {
        return new UnityVector3(this.X / scalar, this.Y / scalar, this.Z / scalar);
    }

    public neg(): UnityVector3 {
        return new UnityVector3(-this.X, -this.Y, -this.Z);
    }

    public static Dot(a: UnityVector3, b: UnityVector3): number {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }

    public static Cross(a: UnityVector3, b: UnityVector3): UnityVector3 {
        return new UnityVector3(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }

    public static Scale(a: UnityVector3, b: UnityVector3): UnityVector3 {
        return new UnityVector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    }

    public static readonly zero = new UnityVector3(0, 0, 0);
    public static readonly one = new UnityVector3(1, 1, 1);
    public static readonly up = new UnityVector3(0, 1, 0);
    public static readonly down = new UnityVector3(0, -1, 0);
    public static readonly forward = new UnityVector3(0, 0, 1);
    public static readonly back = new UnityVector3(0, 0, -1);
}

// Mathf类
export class Mathf {
    public static Clamp(value: number, min: number, max: number): number {
        return math.max(min, math.min(max, value));
    }

    public static Min(...values: number[]): number {
        return math.min(...values);
    }

    public static Max(...values: number[]): number {
        return math.max(...values);
    }

    public static Abs(value: number): number {
        return math.abs(value);
    }

    public static Sqrt(value: number): number {
        return math.sqrt(value);
    }

    public static Exp(value: number): number {
        return math.exp(value);
    }

    public static Approximately(a: number, b: number, epsilon: number = 0.0001): boolean {
        return math.abs(a - b) < epsilon;
    }

    public static Repeat(t: number, length: number): number {
        const result = t - math.floor(t / length) * length;
        // Unity的Clamp步骤：确保结果在[0, length)范围内
        return Mathf.Clamp(result, 0.0, length);
    }
}

// Quaternion类
export class Quaternion {
    public X: number;
    public Y: number;
    public Z: number;
    public w: number;

    constructor(x: number = 0, y: number = 0, z: number = 0, w: number = 1) {
        this.X = x;
        this.Y = y;
        this.Z = z;
        this.w = w;
    }

    public getEulerAngles(): UnityVector3 {
        // 将四元数转换为欧拉角
        const x = this.X;
        const y = this.Y;
        const z = this.Z;
        const w = this.w;
        
        // Roll (x-axis rotation)
        const sinr_cosp = 2 * (w * x + y * z);
        const cosr_cosp = 1 - 2 * (x * x + y * y);
        const roll = math.atan2(sinr_cosp, cosr_cosp);
        
        // Pitch (y-axis rotation)
        const sinp = 2 * (w * y - z * x);
        let pitch: number;
        if (math.abs(sinp) >= 1) {
            pitch = math.pi / 2 * (sinp > 0 ? 1 : -1); // use 90 degrees if out of range
        } else {
            pitch = math.asin(sinp);
        }
        
        // Yaw (z-axis rotation)
        const siny_cosp = 2 * (w * z + x * y);
        const cosy_cosp = 1 - 2 * (y * y + z * z);
        const yaw = math.atan2(siny_cosp, cosy_cosp);
        
        // 转换为度数并返回UnityVector3
        return new UnityVector3(math.deg(roll), math.deg(pitch), math.deg(yaw));
    }

    public mul(other: number | Quaternion): Quaternion {
        if (typeOf(other) === "number") {
            const s = other as number;
            return new Quaternion(this.X * s, this.Y * s, this.Z * s, this.w * s);
        } else {
            const q = other as Quaternion;
            return new Quaternion(
                this.w * q.X + this.X * q.w + this.Y * q.Z - this.Z * q.Y,
                this.w * q.Y + this.Y * q.w + this.Z * q.X - this.X * q.Z,
                this.w * q.Z + this.Z * q.w + this.X * q.Y - this.Y * q.X,
                this.w * q.w - this.X * q.X - this.Y * q.Y - this.Z * q.Z
            );
        }
    }

    public static Euler(x: number, y: number, z: number): Quaternion {
        const cx = math.cos(math.rad(x/2));
        const cy = math.cos(math.rad(y/2));
        const cz = math.cos(math.rad(z/2));
        const sx = math.sin(math.rad(x/2));
        const sy = math.sin(math.rad(y/2));
        const sz = math.sin(math.rad(z/2));
        
        return new Quaternion(
            sx * cy * cz + cx * sy * sz,  
            cx * sy * cz - sx * cy * sz, 
            cx * cy * sz + sx * sy * cz, 
            cx * cy * cz - sx * sy * sz   
        );
    }

    public static LookRotation(forward: UnityVector3, up: UnityVector3 = UnityVector3.up): Quaternion {
        // 标准化向量
        const f = forward.getNormalized();
        const u = up.getNormalized();
        
        // 构建正交基 (右手坐标系)
        const r = UnityVector3.Cross(u, f).getNormalized();  // right = up × forward
        const newUp = UnityVector3.Cross(f, r);         // newUp = forward × right
        
        // 构建旋转矩阵 (列矩阵, Unity标准)
        // 列0: right, 列1: newUp, 列2: forward
        const m00 = r.X; const m10 = r.Y; const m20 = r.Z;          // right列
        const m01 = newUp.X; const m11 = newUp.Y; const m21 = newUp.Z;  // up列  
        const m02 = f.X; const m12 = f.Y; const m22 = f.Z;          // forward列
        
        // Unity标准的旋转矩阵到四元数转换
        const trace = m00 + m11 + m22;
        const quaternion = new Quaternion();
        
        if (trace > 0) {
            const s = math.sqrt(trace + 1) * 2;  // s = 4 * qw
            quaternion.w = 0.25 * s;
            quaternion.X = (m21 - m12) / s;
            quaternion.Y = (m02 - m20) / s;  
            quaternion.Z = (m10 - m01) / s;
        } else if (m00 > m11 && m00 > m22) {
            const s = math.sqrt(1 + m00 - m11 - m22) * 2;  // s = 4 * qx
            quaternion.w = (m21 - m12) / s;
            quaternion.X = 0.25 * s;
            quaternion.Y = (m01 + m10) / s;
            quaternion.Z = (m02 + m20) / s;
        } else if (m11 > m22) {
            const s = math.sqrt(1 + m11 - m00 - m22) * 2;  // s = 4 * qy  
            quaternion.w = (m02 - m20) / s;
            quaternion.X = (m01 + m10) / s;
            quaternion.Y = 0.25 * s;
            quaternion.Z = (m12 + m21) / s;
        } else {
            const s = math.sqrt(1 + m22 - m00 - m11) * 2;  // s = 4 * qz
            quaternion.w = (m10 - m01) / s;
            quaternion.X = (m02 + m20) / s;
            quaternion.Y = (m12 + m21) / s;
            quaternion.Z = 0.25 * s;
        }
        
        return quaternion;
    }

    public static Slerp(a: Quaternion, b: Quaternion, t: number): Quaternion {
        let dot = a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.w * b.w;
        let bQuaternion = b;
        
        if (dot < 0) {
            bQuaternion = new Quaternion(-b.X, -b.Y, -b.Z, -b.w);
            dot = -dot;
        }
        
        if (dot > 0.9995) {
            return new Quaternion(
                a.X + (bQuaternion.X - a.X) * t,
                a.Y + (bQuaternion.Y - a.Y) * t,
                a.Z + (bQuaternion.Z - a.Z) * t,
                a.w + (bQuaternion.w - a.w) * t
            );
        }
        
        const theta0 = math.acos(dot);
        const theta = theta0 * t;
        const sinTheta = math.sin(theta);
        const sinTheta0 = math.sin(theta0);
        
        const s0 = math.cos(theta) - dot * sinTheta / sinTheta0;
        const s1 = sinTheta / sinTheta0;
        
        return new Quaternion(
            s0 * a.X + s1 * bQuaternion.X,
            s0 * a.Y + s1 * bQuaternion.Y,
            s0 * a.Z + s1 * bQuaternion.Z,
            s0 * a.w + s1 * bQuaternion.w
        );
    }

    // 将Quaternion转换为Roblox CFrame
    public toCFrame(): CFrame {
        const x = this.X;
        const y = this.Y;
        const z = this.Z;
        const w = this.w;
        
        // 转换四元数到旋转矩阵
        const xx = x*x; const yy = y*y; const zz = z*z;
        const xy = x*y; const xz = x*z; const yz = y*z;
        const wx = w*x; const wy = w*y; const wz = w*z;
        
        const m00 = 1 - 2*(yy + zz);
        const m01 = 2*(xy - wz);
        const m02 = 2*(xz + wy);
        
        const m10 = 2*(xy + wz);
        const m11 = 1 - 2*(xx + zz);
        const m12 = 2*(yz - wx);
        
        const m20 = 2*(xz - wy);
        const m21 = 2*(yz + wx);
        const m22 = 1 - 2*(xx + yy);
        
        return new CFrame(0, 0, 0, m00, m01, m02, m10, m11, m12, m20, m21, m22);
    }

    // 将Roblox CFrame转换为Quaternion
    public static fromCFrame(cf: CFrame): Quaternion {
        const [, , , xx, yx, zx, xy, yy, zy, xz, yz, zz] = cf.GetComponents();
        // CFrame到Quaternion的标准转换
        const trace = xx + yy + zz;
        if (trace > 0) {
            const s = math.sqrt(trace + 1) * 2;
            return new Quaternion(
                (zy - yz) / s,
                (xz - zx) / s, 
                (yx - xy) / s,
                0.25 * s
            );
        } else if (xx > yy && xx > zz) {
            const s = math.sqrt(1 + xx - yy - zz) * 2;
            return new Quaternion(
                0.25 * s,
                (xy + yx) / s,
                (xz + zx) / s,
                (zy - yz) / s
            );
        } else if (yy > zz) {
            const s = math.sqrt(1 + yy - xx - zz) * 2;
            return new Quaternion(
                (xy + yx) / s,
                0.25 * s,
                (yz + zy) / s,
                (xz - zx) / s
            );
        } else {
            const s = math.sqrt(1 + zz - xx - yy) * 2;
            return new Quaternion(
                (xz + zx) / s,
                (yz + zy) / s,
                0.25 * s,
                (yx - xy) / s
            );
        }
    }

    public static readonly identity = new Quaternion(0, 0, 0, 1);
}

// 导出UnityMath命名空间，作为默认导出
const UnityMath = {
    Vector2,
    UnityVector3,
    Quaternion,
    Mathf
};

export default UnityMath;