using System;
using UnityEngine;

namespace JBGE {
	public class IDUserInput {
		private GameEngine GE;

		/*
		public int KeyArrowNorth = 63;
		public int KeyArrowSouth = 64;
		public int KeyArrowWest = 61;
		public int KeyArrowEast = 62;

		public int KeyW = 37;
		public int KeyS = 33;
		public int KeyA = 15;
		public int KeyD = 18;
		*/

		public int KeyArrowNorth = (int)KeyCode.UpArrow;
		public int KeyArrowSouth = (int)KeyCode.DownArrow;
		public int KeyArrowWest = (int)KeyCode.LeftArrow;
		public int KeyArrowEast = (int)KeyCode.RightArrow;

		public int KeyW = (int)KeyCode.W;
		public int KeyS = (int)KeyCode.S;
		public int KeyA = (int)KeyCode.A;
		public int KeyD = (int)KeyCode.D;

		// Define the minimum interval between key presses (in seconds)
		public float KeyPressInterval = 0.01f;
		protected float keyPressTimer = 0f;

		public IDUserInput(GameEngine GE) {
			this.GE = GE;
			KeyPressInterval = 1 / GE.TargetFrameRate;
		}

		public void Update() {
			// Increment the timer
			keyPressTimer += Time.deltaTime;
			if(keyPressTimer > 10000) keyPressTimer = 0;
		}

		/// <summary>Check for key input only if the timer exceeds the interval</summary>
		/// <returns>true, if input is allowed, else false</returns>
		protected bool CheckInputPermitted() {
			// Check for key input only if the timer exceeds the interval
			return keyPressTimer >= KeyPressInterval;
		}

		public bool CheckUserInputNorth() {
			if(!CheckInputPermitted()) return false;
			bool isInput = false;
			// If multiple keys pressed, determine if it includes the target key
			if(GE.IDKeyboard.IsKeyPressed(KeyArrowNorth)) {
				isInput = true;
			} else if(GE.IDKeyboard.IsKeyPressed(KeyW)) {
				isInput = true;
			} else {
				// Otherwise, check the pressed keys
				isInput = (GE.IDGamePad.IsDPadNorthPressed) ? true : false;
			}
			// If a key was pressed, reset the timer
			if(isInput) keyPressTimer = 0f;
			return isInput;
		}

		public bool CheckUserInputSouth() {
			if(!CheckInputPermitted()) return false;
			bool isInput = false;
			// If multiple keys pressed, determine if it includes the target key
			if(GE.IDKeyboard.IsKeyPressed(KeyArrowSouth)) {
				isInput = true;
			} else if(GE.IDKeyboard.IsKeyPressed(KeyS)) {
				isInput = true;
			} else {
				// Otherwise, check the pressed keys
				isInput = (GE.IDGamePad.IsDPadSouthPressed) ? true : false;
			}
			// If a key was pressed, reset the timer
			if(isInput) keyPressTimer = 0f;
			return isInput;
		}

		public bool CheckUserInputWest() {
			if(!CheckInputPermitted()) return false;
			bool isInput = false;
			// If multiple keys pressed, determine if it includes the target key
			if(GE.IDKeyboard.IsKeyPressed(KeyArrowWest)) {
				isInput = true;
			} else if(GE.IDKeyboard.IsKeyPressed(KeyA)) {
				isInput = true;
			} else {
				// Otherwise, check the pressed keys
				isInput = (GE.IDGamePad.IsDPadWestPressed) ? true : false;
			}
			// If a key was pressed, reset the timer
			if(isInput) keyPressTimer = 0f;
			return isInput;
		}
		public bool CheckUserInputEast() {
			if(!CheckInputPermitted()) return false;
			bool isInput = false;
			// If multiple keys pressed, determine if it includes the target key
			if(GE.IDKeyboard.IsKeyPressed(KeyArrowEast)) {
				isInput = true;
			} else if(GE.IDKeyboard.IsKeyPressed(KeyD)) {
				isInput = true;
			} else {
				// Otherwise, check the pressed keys
				isInput = (GE.IDGamePad.IsDPadEastPressed) ? true : false;
			}
			// If a key was pressed, reset the timer
			if(isInput) keyPressTimer = 0f;
			return isInput;
		}
		public bool CheckUserInputNorthEast() {
			if(!CheckInputPermitted()) return false;
			bool isInput = (GE.IDGamePad.IsDPadNorthEastPressed || GE.IDKeyboard.IsKeyCombinationPressed(KeyArrowNorth, KeyArrowEast) ||
				GE.IDKeyboard.IsKeyCombinationPressed(KeyW, KeyD)) ? true : false;
			// If a key was pressed, reset the timer
			if(isInput) keyPressTimer = 0f;
			return isInput;
		}
		public bool CheckUserInputSouthEast() {
			if(!CheckInputPermitted()) return false;
			bool isInput = (GE.IDGamePad.IsDPadSouthEastPressed || GE.IDKeyboard.IsKeyCombinationPressed(KeyArrowSouth, KeyArrowEast) ||
				GE.IDKeyboard.IsKeyCombinationPressed(KeyS, KeyD)) ? true : false;
			// If a key was pressed, reset the timer
			if(isInput) keyPressTimer = 0f;
			return isInput;
		}
		public bool CheckUserInputSouthWest() {
			if(!CheckInputPermitted()) return false;
			bool isInput = (GE.IDGamePad.IsDPadSouthWestPressed || GE.IDKeyboard.IsKeyCombinationPressed(KeyArrowSouth, KeyArrowWest) ||
				GE.IDKeyboard.IsKeyCombinationPressed(KeyS, KeyA)) ? true : false;
			// If a key was pressed, reset the timer
			if(isInput) keyPressTimer = 0f;
			return isInput;
		}
		public bool CheckUserInputNorthWest() {
			if(!CheckInputPermitted()) return false;
			bool isInput = (GE.IDGamePad.IsDPadNorthWestPressed || GE.IDKeyboard.IsKeyCombinationPressed(KeyArrowNorth, KeyArrowWest) ||
				GE.IDKeyboard.IsKeyCombinationPressed(KeyW, KeyA)) ? true : false;
			// If a key was pressed, reset the timer
			if(isInput) keyPressTimer = 0f;
			return isInput;
		}
	}
}
