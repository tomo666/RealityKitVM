//
//  IDGamePad.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/04.
//


//
//  IDGamePad.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/04.
//

import Foundation

public class IDGamePad {

    public var IsNorthPressed: Bool = false
    public var IsSouthPressed: Bool = false
    public var IsWestPressed: Bool = false
    public var IsEastPressed: Bool = false
    public var IsLeftShoulderPressed: Bool = false
    public var IsRightShoulderPressed: Bool = false
    public var IsLeftStickPressed: Bool = false
    public var IsRightStickPressed: Bool = false
    public var IsSelectPressed: Bool = false
    public var IsStartPressed: Bool = false

    public var LeftStickValue: SIMD2<Float> = SIMD2(0, 0)
    public var LeftTriggerValue: Float = 0.0
    public var RightStickValue: SIMD2<Float> = SIMD2(0, 0)
    public var RightTriggerValue: Float = 0.0
    public var DPadValue: SIMD2<Float> = SIMD2(0, 0)
    
    public var IsDPadNorthPressed: Bool {
        return DPadValue.x == 0 && DPadValue.y == 1
    }

    public var IsDPadEastPressed: Bool {
        return DPadValue.x == 1 && DPadValue.y == 0
    }

    public var IsDPadSouthPressed: Bool {
        return DPadValue.x == 0 && DPadValue.y == -1
    }

    public var IsDPadWestPressed: Bool {
        return DPadValue.x == -1 && DPadValue.y == 0
    }

    public var IsDPadNorthEastPressed: Bool {
        return round(DPadValue.x * 100) / 100 == 0.71 &&
               round(DPadValue.y * 100) / 100 == 0.71
    }

    public var IsDPadSouthEastPressed: Bool {
        return round(DPadValue.x * 100) / 100 == 0.71 &&
               round(DPadValue.y * 100) / 100 == -0.71
    }

    public var IsDPadSouthWestPressed: Bool {
        return round(DPadValue.x * 100) / 100 == -0.71 &&
               round(DPadValue.y * 100) / 100 == -0.71
    }

    public var IsDPadNorthWestPressed: Bool {
        return round(DPadValue.x * 100) / 100 == -0.71 &&
               round(DPadValue.y * 100) / 100 == 0.71
    }

    public var IsLeftTriggerPressed: Bool {
        return LeftTriggerValue == 1
    }

    public var IsRightTriggerPressed: Bool {
        return RightTriggerValue == 1
    }
}
