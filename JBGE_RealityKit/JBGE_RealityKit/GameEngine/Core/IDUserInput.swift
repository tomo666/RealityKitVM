//
//  IDUserInput.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/04.
//

import Foundation

public class IDUserInput {

    private var GE: GameEngine

    public var KeyArrowNorth: Int = KeyCode.UpArrow
    public var KeyArrowSouth: Int = KeyCode.DownArrow
    public var KeyArrowWest:  Int = KeyCode.LeftArrow
    public var KeyArrowEast:  Int = KeyCode.RightArrow

    public var KeyW: Int = KeyCode.W
    public var KeyS: Int = KeyCode.S
    public var KeyA: Int = KeyCode.A
    public var KeyD: Int = KeyCode.D

    // Define the minimum interval between key presses (in seconds)
    public var KeyPressInterval: Float = 0.01
    internal var keyPressTimer: Float = 0.0

    public init(_ GE: GameEngine) {
        self.GE = GE
        KeyPressInterval = 1.0 / Float(GE.TargetFrameRate)
    }

    public func Update(deltaTime: Float) {
        // Increment the timer
        keyPressTimer += deltaTime
        if keyPressTimer > 10000 { keyPressTimer = 0 }
    }

    /// <summary>Check for key input only if the timer exceeds the interval</summary>
    /// <returns>true, if input is allowed, else false</returns>
    internal func CheckInputPermitted() -> Bool {
        // Check for key input only if the timer exceeds the interval
        return keyPressTimer >= KeyPressInterval
    }

    public func CheckUserInputNorth() -> Bool {
        if !CheckInputPermitted() { return false }
        var isInput = false

        if GE.IDKeyboard.IsKeyPressed(KeyArrowNorth) {
            isInput = true
        } else if GE.IDKeyboard.IsKeyPressed(KeyW) {
            isInput = true
        } else {
            isInput = GE.IDGamePad.IsDPadNorthPressed
        }

        if isInput { keyPressTimer = 0 }
        return isInput
    }

    public func CheckUserInputSouth() -> Bool {
        if !CheckInputPermitted() { return false }
        var isInput = false

        if GE.IDKeyboard.IsKeyPressed(KeyArrowSouth) {
            isInput = true
        } else if GE.IDKeyboard.IsKeyPressed(KeyS) {
            isInput = true
        } else {
            isInput = GE.IDGamePad.IsDPadSouthPressed
        }

        if isInput { keyPressTimer = 0 }
        return isInput
    }

    public func CheckUserInputWest() -> Bool {
        if !CheckInputPermitted() { return false }
        var isInput = false

        if GE.IDKeyboard.IsKeyPressed(KeyArrowWest) {
            isInput = true
        } else if GE.IDKeyboard.IsKeyPressed(KeyA) {
            isInput = true
        } else {
            isInput = GE.IDGamePad.IsDPadWestPressed
        }

        if isInput { keyPressTimer = 0 }
        return isInput
    }

    public func CheckUserInputEast() -> Bool {
        if !CheckInputPermitted() { return false }
        var isInput = false

        if GE.IDKeyboard.IsKeyPressed(KeyArrowEast) {
            isInput = true
        } else if GE.IDKeyboard.IsKeyPressed(KeyD) {
            isInput = true
        } else {
            isInput = GE.IDGamePad.IsDPadEastPressed
        }

        if isInput { keyPressTimer = 0 }
        return isInput
    }

    public func CheckUserInputNorthEast() -> Bool {
        if !CheckInputPermitted() { return false }
        let isInput =
            GE.IDGamePad.IsDPadNorthEastPressed ||
            GE.IDKeyboard.IsKeyCombinationPressed(KeyArrowNorth, KeyArrowEast) ||
            GE.IDKeyboard.IsKeyCombinationPressed(KeyW, KeyD)

        if isInput { keyPressTimer = 0 }
        return isInput
    }

    public func CheckUserInputSouthEast() -> Bool {
        if !CheckInputPermitted() { return false }
        let isInput =
            GE.IDGamePad.IsDPadSouthEastPressed ||
            GE.IDKeyboard.IsKeyCombinationPressed(KeyArrowSouth, KeyArrowEast) ||
            GE.IDKeyboard.IsKeyCombinationPressed(KeyS, KeyD)

        if isInput { keyPressTimer = 0 }
        return isInput
    }

    public func CheckUserInputSouthWest() -> Bool {
        if !CheckInputPermitted() { return false }
        let isInput =
            GE.IDGamePad.IsDPadSouthWestPressed ||
            GE.IDKeyboard.IsKeyCombinationPressed(KeyArrowSouth, KeyArrowWest) ||
            GE.IDKeyboard.IsKeyCombinationPressed(KeyS, KeyA)

        if isInput { keyPressTimer = 0 }
        return isInput
    }

    public func CheckUserInputNorthWest() -> Bool {
        if !CheckInputPermitted() { return false }
        let isInput =
            GE.IDGamePad.IsDPadNorthWestPressed ||
            GE.IDKeyboard.IsKeyCombinationPressed(KeyArrowNorth, KeyArrowWest) ||
            GE.IDKeyboard.IsKeyCombinationPressed(KeyW, KeyA)

        if isInput { keyPressTimer = 0 }
        return isInput
    }
}
