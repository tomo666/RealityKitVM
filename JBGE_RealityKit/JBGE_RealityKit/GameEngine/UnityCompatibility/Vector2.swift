//
//  Vector2.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/04.
//

import Foundation
import simd

public struct Vector2 {

    public var x: Float
    public var y: Float

    public init(_ x: Float, _ y: Float) {
        self.x = x
        self.y = y
    }

    // Unity-style constants
    public static let zero = Vector2(0, 0)
    public static let one  = Vector2(1, 1)

    // Internal bridge (not for public API)
    internal var simd: SIMD2<Float> {
        SIMD2<Float>(x, y)
    }

    internal init(simd: SIMD2<Float>) {
        self.x = simd.x
        self.y = simd.y
    }
}
