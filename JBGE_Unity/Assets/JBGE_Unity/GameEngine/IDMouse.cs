using UnityEngine;

namespace JBGE {
	public class IDMouse {
		public bool IsMouseLeftPressed = false;
		public bool IsMouseRightPressed = false;
		public bool IsMouseMiddlePressed = false;
		public bool IsMouseForwardPressed = false;
		public bool IsMouseBackPressed = false;
		public int ScrollAmount = 0;
		public bool IsScrolledUp {
			get {
				if(ScrollAmount > 0) return true;
				return false;
			}
		}
		public bool IsScrolledDown {
			get {
				if(ScrollAmount < 0) return true;
				return false;
			}
		}
		public Vector2 MousePosition;
	}
}
