using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace JBGE {
	public class IDKeyboard {
    /// <summary>Detects multiple key presses</summary>
    /// <param name="keyCode1"></param>
    /// <param name="keyCode2"></param>
    /// <returns>true if specified keys are pressed together, else false</returns>
    public bool IsKeyCombinationPressed(int keyCode1, int keyCode2) {
      return Input.GetKey((KeyCode)keyCode1) && Input.GetKey((KeyCode)keyCode2);
    }

    /// <summary>Detects if the specified key is pressed</summary>
    /// <param name="keyCode"></param>
    /// <returns>true if specified key was pressed, else false</returns>
    public bool IsKeyPressed(int keyCode) {
      return Input.GetKey((KeyCode)keyCode);
    }

    public int PressedKeyCode {
			get {
          foreach(KeyControl kc in Keyboard.current.allKeys) {
            if(kc.isPressed) {
              return (int)kc.keyCode;
            }
          }
        return -1;
      }
		}

    public bool IsAnyKeyPressed {
      get {
        return Keyboard.current.anyKey.isPressed;
      }
    }
	}
}
