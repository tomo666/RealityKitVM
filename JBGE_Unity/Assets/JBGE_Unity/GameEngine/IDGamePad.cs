using System;
using UnityEngine;

namespace JBGE {
	public class IDGamePad {
		public bool IsNorthPressed = false;
		public bool IsSouthPressed = false;
		public bool IsWestPressed = false;
		public bool IsEastPressed = false;
		public bool IsLeftShoulderPressed = false;
		public bool IsRightShoulderPressed = false;
		public bool IsLeftStickPressed = false;
		public bool IsRightStickPressed = false;
		public bool IsSelectPressed = false;
		public bool IsStartPressed = false;

		public Vector2 LeftStickValue;
		public float LeftTriggerValue;

		public Vector2 RightStickValue;
		public float RightTriggerValue;

		public Vector2 DPadValue;
		public bool IsDPadNorthPressed { get { return DPadValue.x == 0 && DPadValue.y == 1; } }
		public bool IsDPadEastPressed { get { return DPadValue.x == 1 && DPadValue.y == 0; } }
		public bool IsDPadSouthPressed { get { return DPadValue.x == 0 && DPadValue.y == -1; } }
		public bool IsDPadWestPressed { get { return DPadValue.x == -1 && DPadValue.y == 0; } }
		public bool IsDPadNorthEastPressed { get { return Math.Round(DPadValue.x, 2) == 0.71 && Math.Round(DPadValue.y, 2) == 0.71; } }
		public bool IsDPadSouthEastPressed { get { return Math.Round(DPadValue.x, 2) == 0.71 && Math.Round(DPadValue.y, 2) == -0.71; } }
		public bool IsDPadSouthWestPressed { get { return Math.Round(DPadValue.x, 2) == -0.71 && Math.Round(DPadValue.y, 2) == -0.71; } }
		public bool IsDPadNorthWestPressed { get { return Math.Round(DPadValue.x, 2) == -0.71 && Math.Round(DPadValue.y, 2) == 0.71; } }

		public bool IsLeftTriggerPressed { get { return LeftTriggerValue == 1; } }
		public bool IsRightTriggerPressed { get { return RightTriggerValue == 1; } }
	}
}
