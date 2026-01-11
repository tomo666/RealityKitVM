using System;

namespace JBGE {
	/*
	public enum GameResolutionType {
		RES_1920x1080xScale1, RES_1920x1080xScale5, RES_1280x800xScale1, RES_1280x800xScale5
	}*/
	// Image direction to search in the atlas for creating animations that are made up of multiple sprites
	public enum SheetOrientation { Horizontal, Vertical }
	// General horizontal alignment definitions
	public enum HorizontalAlignment { Left, Center, Right }
	// General vertical alignment definitions
	public enum VerticalAlignment { Top, Middle, Bottom }
	// General orientation definitions
	public enum Orientation { Horizontal, Vertical }
	// General speed identifiers
	public enum MovementSpeed { VerySlow, Slow, Normal, Fast, VeryFast }
	public enum Direction { South, North, East, West, NorthEast, SouthEast, SouthWest, NorthWest }
	public enum GamePad {
		DPadNorth, DPadSouth, DPadWest, DPadEast, DPadNorthEast, DPadSouthEast, DPadSouthWest, DPadNorthWest, ButtonNorth, ButtonSouth, ButtonWest, ButtonEast, ButtonLeftShoulder, ButtonRightShoulder, ButtonLeftTrigger, ButtonRightTrigger, ButtonLeftStick, ButtonRightStick, ButtonStart, ButtonSelect
	}
}
