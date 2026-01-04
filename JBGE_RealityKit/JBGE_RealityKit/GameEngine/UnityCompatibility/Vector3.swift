//
//  Vector3.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/04.
//

import Foundation
import simd

public struct Vector3 {

    public var x: Float
    public var y: Float
    public var z: Float

    public init(_ x: Float, _ y: Float, _ z: Float) {
        self.x = x
        self.y = y
        self.z = z
    }

    public static let zero = Vector3(0, 0, 0)
    public static let one  = Vector3(1, 1, 1)

    internal var simd: SIMD3<Float> {
        SIMD3<Float>(x, y, z)
    }

    internal init(simd: SIMD3<Float>) {
        self.x = simd.x
        self.y = simd.y
        self.z = simd.z
    }
}
