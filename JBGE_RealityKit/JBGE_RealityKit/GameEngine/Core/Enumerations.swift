//
//  Enumerations.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/12.
//

// Image direction to search in the atlas for creating animations that are made up of multiple sprites
public enum SheetOrientation { case Horizontal, Vertical }
// General horizontal alignment definitions
public enum HorizontalAlignment { case Left, Center, Right }
// General vertical alignment definitions
public enum VerticalAlignment { case Top, Middle, Bottom }
// General orientation definitions
public enum Orientation { case Horizontal, Vertical }
// General speed identifiers
public enum MovementSpeed { case VerySlow, Slow, Normal, Fast, VeryFast }
// 8 Directions
public enum Direction { case South, North, East, West, NorthEast, SouthEast, SouthWest, NorthWest }
// Gamepad component definitions
public enum GamePad {
    case SDPadNorth, DPadSouth, DPadWest, DPadEast, DPadNorthEast, DPadSouthEast, DPadSouthWest, DPadNorthWest, ButtonNorth, ButtonSouth, ButtonWest, ButtonEast, ButtonLeftShoulder, ButtonRightShoulder, ButtonLeftTrigger, ButtonRightTrigger, ButtonLeftStick, ButtonRightStick, ButtonStart, ButtonSelect
}
