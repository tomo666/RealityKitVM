//
//  IDMouse.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/04.
//

import Foundation

public class IDMouse {

    public var IsMouseLeftPressed: Bool = false
    public var IsMouseRightPressed: Bool = false
    public var IsMouseMiddlePressed: Bool = false
    public var IsMouseForwardPressed: Bool = false
    public var IsMouseBackPressed: Bool = false

    public var ScrollAmount: Int = 0

    public var IsScrolledUp: Bool {
        if ScrollAmount > 0 { return true }
        return false
    }

    public var IsScrolledDown: Bool {
        if ScrollAmount < 0 { return true }
        return false
    }

    public var MousePosition: SIMD2<Int> = SIMD2(0, 0)
}
