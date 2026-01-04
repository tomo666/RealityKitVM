//
//  MotionTweenData.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/04.
//

import Foundation

/// Direct Swift write-through of Unity's MotionTweenData
/// This class is intentionally Unity-style for cross-engine portability.
public class MotionTweenData {

    public var motionTargetVector3: Vector3
    public var motionTargetPivot: Vector2
    public var motionBezierPoints: [Vector3] = Array(repeating: Vector3(0, 0, 0), count: 4)
    public var originalTransformVector3: Vector3
    public var originalPivotVector2: Vector2

    public init(
        _ motionTargetVector3: Vector3,
        _ motionTargetPivot: Vector2,
        _ motionBezierPoints: [Vector3],
        _ originalTransformVector3: Vector3,
        _ originalPivotVector2: Vector2
    ) {
        self.motionTargetVector3 = motionTargetVector3
        self.motionTargetPivot = motionTargetPivot
        self.motionBezierPoints = motionBezierPoints
        self.originalTransformVector3 = originalTransformVector3
        self.originalPivotVector2 = originalPivotVector2
    }
}
