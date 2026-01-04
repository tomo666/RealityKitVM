//
//  IDKeyboard.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/04.
//

import Foundation

public class IDKeyboard {
    /// <summary>Detects multiple key presses</summary>
    public func IsKeyCombinationPressed(_ keyCode1: Int, _ keyCode2: Int) -> Bool {
        return Input.GetKey(keyCode1) && Input.GetKey(keyCode2)
    }

    /// <summary>Detects if the specified key is pressed</summary>
    public func IsKeyPressed(_ keyCode: Int) -> Bool {
        return Input.GetKey(keyCode)
    }
    
    public var PressedKeyCode: Int {
        get {
            return Input.FirstKey()
        }
    }

    public var IsAnyKeyPressed: Bool {
        get {
            return Input.AnyKey()
        }
    }
}
