/**
 * Matrix3 struct的TypeScript实现
 * 与Lua版本完全一致，提供3x3矩阵操作
 */

import { UnityVector3 } from "../KartShared/UnityMath";

export class Matrix3 {
    public m: number[][] = [
        [0, 0, 0],
        [0, 0, 0],
        [0, 0, 0]
    ];

    constructor() {
        this.m = [
            [0, 0, 0],
            [0, 0, 0],
            [0, 0, 0]
        ];
    }

    public static CreateMtxIdentity(): Matrix3 {
        return Matrix3.CreateMtx(1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0);
    }

    public static CreateMtx(_11: number, _12: number, _13: number, _21: number, _22: number, _23: number, _31: number, _32: number, _33: number): Matrix3 {
        const matrix = new Matrix3();
        matrix.m[0]![0] = _11;
        matrix.m[0]![1] = _12;
        matrix.m[0]![2] = _13;
        matrix.m[1]![0] = _21;
        matrix.m[1]![1] = _22;
        matrix.m[1]![2] = _23;
        matrix.m[2]![0] = _31;
        matrix.m[2]![1] = _32;
        matrix.m[2]![2] = _33;
        return matrix;
    }

    // setCol(int col, UnityVector3 v) - 设置单个列
    private setColSingle(col: number, v: UnityVector3): void {
        for (let i = 0; i < 3; i++) {
            let vIndex: number;
            if (i === 0) {
                vIndex = v.X;
            } else if (i === 1) {
                vIndex = v.Y;
            } else { // i === 2
                vIndex = v.Z;
            }
            this.m[i]![col] = vIndex;
        }
    }

    // setCol - 支持两种调用方式
    // 1. setCol(int col, UnityVector3 v) - 设置单个列
    // 2. setCol(UnityVector3 v1, UnityVector3 v2, UnityVector3 v3) - 设置三个列
    public setCol(v1: number | UnityVector3, v2?: UnityVector3, v3?: UnityVector3): void {
        if (typeIs(v1, "number") && v2 && !v3) {
            // 第一种情况：setCol(int col, UnityVector3 v)
            const col = v1 as number;
            const v = v2 as UnityVector3;
            this.setColSingle(col, v);
        } else if (v2 && v3 && !typeIs(v1, "number")) {
            // 第二种情况：setCol(UnityVector3 v1, UnityVector3 v2, UnityVector3 v3)
            this.setColSingle(0, v1 as UnityVector3);
            this.setColSingle(1, v2 as UnityVector3);
            this.setColSingle(2, v3 as UnityVector3);
        } else {
            error("Matrix3:setCol invalid arguments");
        }
    }

    public mul(other: UnityVector3): UnityVector3;
    public mul(other: Matrix3): Matrix3;
    public mul(other: UnityVector3 | Matrix3): UnityVector3 | Matrix3 {
        if ('X' in other && 'Y' in other && 'Z' in other) {
            // UnityVector3 case - check for UnityVector3 properties
            const vector = other as UnityVector3;
            return new UnityVector3(
                this.m[0]![0]! * vector.X + this.m[0]![1]! * vector.Y + this.m[0]![2]! * vector.Z,
                this.m[1]![0]! * vector.X + this.m[1]![1]! * vector.Y + this.m[1]![2]! * vector.Z,
                this.m[2]![0]! * vector.X + this.m[2]![1]! * vector.Y + this.m[2]![2]! * vector.Z
            );
        } else {
            // Matrix3 case
            const matrix = other as Matrix3;
            return Matrix3.CreateMtx(
                this.m[0]![0]! * matrix.m[0]![0]! + this.m[0]![1]! * matrix.m[1]![0]! + this.m[0]![2]! * matrix.m[2]![0]!,
                this.m[0]![0]! * matrix.m[0]![1]! + this.m[0]![1]! * matrix.m[1]![1]! + this.m[0]![2]! * matrix.m[2]![1]!,
                this.m[0]![0]! * matrix.m[0]![2]! + this.m[0]![1]! * matrix.m[1]![2]! + this.m[0]![2]! * matrix.m[2]![2]!,
                this.m[1]![0]! * matrix.m[0]![0]! + this.m[1]![1]! * matrix.m[1]![0]! + this.m[1]![2]! * matrix.m[2]![0]!,
                this.m[1]![0]! * matrix.m[0]![1]! + this.m[1]![1]! * matrix.m[1]![1]! + this.m[1]![2]! * matrix.m[2]![1]!,
                this.m[1]![0]! * matrix.m[0]![2]! + this.m[1]![1]! * matrix.m[1]![2]! + this.m[1]![2]! * matrix.m[2]![2]!,
                this.m[2]![0]! * matrix.m[0]![0]! + this.m[2]![1]! * matrix.m[1]![0]! + this.m[2]![2]! * matrix.m[2]![0]!,
                this.m[2]![0]! * matrix.m[0]![1]! + this.m[2]![1]! * matrix.m[1]![1]! + this.m[2]![2]! * matrix.m[2]![1]!,
                this.m[2]![0]! * matrix.m[0]![2]! + this.m[2]![1]! * matrix.m[1]![2]! + this.m[2]![2]! * matrix.m[2]![2]!
            );
        }
    }
    
    public transformVector(vector: UnityVector3): UnityVector3 {
        // transformVector 就是 Matrix3 * UnityVector3 的操作
        return this.mul(vector) as UnityVector3;
    }

    // 静态工厂方法
    static create(): Matrix3 {
        return new Matrix3();
    }
}

export default Matrix3;