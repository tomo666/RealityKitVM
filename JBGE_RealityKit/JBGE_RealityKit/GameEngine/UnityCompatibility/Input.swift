//
//  Input.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/04.
//

import Foundation

enum Input {

    // Unity Input.GetKey equivalent
    private static var PressedKeys = Set<Int>()

    // SwiftUI / AppKit から呼ばれる
    public static func OnKeyDown(_ keyCode: Int) {
        PressedKeys.insert(keyCode)
    }

    public static func OnKeyUp(_ keyCode: Int) {
        PressedKeys.remove(keyCode)
    }

    /// Unity-compatible
    public static func GetKey(_ keyCode: Int) -> Bool {
        return PressedKeys.contains(keyCode)
    }

    public static func AnyKey() -> Bool {
        return !PressedKeys.isEmpty
    }

    public static func FirstKey() -> Int {
        return PressedKeys.first ?? -1
    }
}
