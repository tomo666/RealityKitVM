using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using JBGE;

namespace JBGE {
	// The game engine's DSVM should have the same enum VMCID defined here
	public enum VMCID {
		// VMC call IDs: 0 to 255 are preserved by parent DSVM (which is defined inside DScript.cs), so we will use from 256
		SHOW_DEBUG_INFO = 256,
		WINDOW_SET_SCREEN_MODE,
		FRAME_WAIT,
		SET_UI_PPU_SCALE, SET_SCREEN_PPU_SCALE,

		// Input Device
		ID_IS_BUTTON_PRESSED, ID_GET_KEY_PRESSED,

		LAYER_CREATE, LAYER_DESTROY, LAYER_RESET_TRANSFORM,
		LAYER_SET_POSITION_PIVOT, LAYER_SET_ROTATION_PIVOT, LAYER_SET_SCALE_PIVOT,
		LAYER_SET_POSITION, LAYER_SET_ROTATION, LAYER_SET_SCALE, LAYER_SET_VISIBILITY,
		LAYER_GET_ACCUM_FRAME_COUNT, LAYER_SET_ACCUM_FRAME_MAX,
		LAYER_SET_MOTION_PATH_BEZIER, LAYER_SET_MOTION_TWEEN_WAIT_FRAME_COUNT,

		FONT_CREATE, FONT_DESTROY,

		TEXTURE_CREATE, TEXTURE_DESTROY,

		TEXT_CREATE, TEXT_DESTROY, TEXT_SET_TEXT, TEXT_RESET_TRANSFORM,
		TEXT_SET_BLIT_ORDER,
		TEXT_SET_POSITION_PIVOT, TEXT_SET_ROTATION_PIVOT, TEXT_SET_SCALE_PIVOT,
		TEXT_SET_POSITION, TEXT_SET_ROTATION, TEXT_SET_SCALE, TEXT_SET_VISIBILITY, TEXT_SET_CAPTION_SPEED,
		TEXT_FADEIN, TEXT_FADEOUT,
		TEXT_SET_COLOR, TEXT_SET_ALPHA, TEXT_GET_ALPHA,
		TEXT_GET_ACCUM_FRAME_COUNT, TEXT_SET_ACCUM_FRAME_MAX,
		TEXT_SET_MOTION_PATH_BEZIER, TEXT_SET_MOTION_TWEEN_WAIT_FRAME_COUNT,

		IMAGE_CREATE, IMAGE_DESTROY, IMAGE_RESET_TRANSFORM,
		IMAGE_SET_BLIT_ORDER,
		IMAGE_SET_POSITION_PIVOT, IMAGE_SET_ROTATION_PIVOT, IMAGE_SET_SCALE_PIVOT,
		IMAGE_SET_POSITION, IMAGE_SET_ROTATION, IMAGE_SET_SCALE, IMAGE_SET_VISIBILITY, IMAGE_SET_SHEET_ANIMATION_SPEED,
		IMAGE_FADEIN, IMAGE_FADEOUT,
		IMAGE_SET_COLOR, IMAGE_SET_ALPHA, IMAGE_GET_ALPHA,
		IMAGE_GET_ACCUM_FRAME_COUNT, IMAGE_SET_ACCUM_FRAME_MAX,
		IMAGE_SET_MOTION_PATH_BEZIER, IMAGE_SET_MOTION_TWEEN_WAIT_FRAME_COUNT,

		ACTOR2D_CREATE, ACTOR2D_DESTROY, ACTOR2D_SET_AS_PLAYER, ACTOR2D_SET_FACING_DIR, ACTOR2D_GET_FACING_DIR,
		ACTOR2D_IS_COLLISION_WITH_ACTOR, ACTOR2D_GET_TILE_POSITION_X, ACTOR2D_GET_TILE_POSITION_Y, ACTOR2D_GET_COLLISION_WIDTH, ACTOR2D_GET_COLLISION_HEIGHT,

		MAP2D_CREATE, MAP2D_DESTROY, MAP2D_SET_ACTOR_AT_TILE_POSITION, MAP2D_SET_VISIBILITY, MAP2D_GET_INSTANCE_OBJECT_REFID
	}

	public class UnityGameVM : VMBase {
		// The game engine object
		public GameEngine GE { get; set; }

		/// <summary>Constructor</summary>
		/// <param name="binFile">.bin file to be executed</param>
		/// <param name="bitMode">
		/// Size of maximum bit size allowed which should have the same size as specified on compilation
		/// (i.e. if -b32 was specified in the compile options, bitMode should be 32)
		/// </param>
		public UnityGameVM(int maxBitSize = 32) : base(maxBitSize) {}

		/// <summary>[Unity dependent] Load .bin file from resources folder to be executed</summary>
		/// <param name="binFile">.bin file to be executed</param>
		public override void LoadBinFile(string binFile) {
			// Try to load from Addressable assets as priority
			TextAsset asset = (TextAsset)GE.LoadAddressableAsset<TextAsset>(binFile);
			if(asset != null) {
				// Read all byte codes
				byte[] byteCodes = asset.bytes;
				BinCode.Clear();
				for(int i = 0; i < byteCodes.Length; i++) BinCode.Add(byteCodes[i]);
				// Initialize VM
				InitializeVM();
			} else {
				// Otherwise, try to load from local folder, but only load it if the current .bin file is newer than the corresponding DScript
				string scriptFile = Path.ChangeExtension(binFile, ".cs").Replace("Assets/Bin", "Assets/DScripts~");
				if(File.Exists(scriptFile)) {
					DateTime ftime1 = File.GetLastWriteTime(scriptFile);
					if(File.Exists(binFile)) {
						DateTime ftime2 = File.GetLastWriteTime(binFile);
						// If script file is newer, compile it to .bin file and load
						if(ftime1 > ftime2) {
							base.CompileScript(scriptFile, true);
							base.LoadBinFile(binFile);
						} else {
							// Load existing .bin file
							base.LoadBinFile(binFile);
						}
					} else {
						// Bin file does not exist, so compile it and load
						base.CompileScript(scriptFile, true);
						base.LoadBinFile(binFile);
					}
				} else {
					// DScript file does not exist, so just load the .bin file if it exists
					if(File.Exists(binFile)) {
						base.LoadBinFile(binFile);
					} else {
						// Both .bin and script file does not exist
						Debug.Log("Binary and DScript file is missing");
					}
				}
			}
		}

		/// <summary>Initialize VM</summary>
		protected override void InitializeVM() {
			// Execute base
			base.InitializeVM();
		}

		/// <summary>
		/// Calls a platform dependent function
		/// Here, you will write your own functions and call them to perform various s
		/// NOTE: callID from 0 to 255 are preserved by the VM
		/// </summary>
		/// <param name="callID">The ID to identify which function you want to call</param>
		protected override void VirtualMachineCall(long callID) {
			// Execute base VMC Call
			base.VirtualMachineCall(callID);
			// Write you own VMC call IDs here:
			if(callID == (int)VMCID.SHOW_DEBUG_INFO) {
				GE.IsShowDebugInfo = ((int)PopValueFromStack() == 1) ? true : false;
			} else if(callID == (int)VMCID.WINDOW_SET_SCREEN_MODE) {
				/*
				int fov = (int)PopValueFromStack();
				bool isOrthographicMode = ((int)PopValueFromStack() == 1) ? true : false;
				int fps = (int)PopValueFromStack();
				bool isFullScreen = ((int)PopValueFromStack() == 1) ? true : false;
				int screenHeight = (int)PopValueFromStack();
				int screenWidth = (int)PopValueFromStack();
				GE.FOV = fov;
				GE.IsUICameraOrthographic = isOrthographicMode;
				Screen.SetResolution(screenWidth, screenHeight, isFullScreen);
				GE.TargetFrameRate = fps;
				// Initialize UI
				GE.InitializeUI();
				*/
			} else if(callID == (int)VMCID.FRAME_WAIT) {
				float frameWaitRate = (float)((double)PopValueFromStack() / DecPtMltFactor);
				if(!GE.IsWaiting) {
					GE.WaitFrameCount = (int)((float)GE.TargetFrameRate * frameWaitRate);
					GE.IsWaiting = true;
				}
			} else if(callID == (int)VMCID.SET_UI_PPU_SCALE) {
				GE.PPUScaleUpUI = (float)((double)PopValueFromStack() / DecPtMltFactor);
			} else if(callID == (int)VMCID.SET_SCREEN_PPU_SCALE) {
				GE.PPUScaleUpWorld = (float)((double)PopValueFromStack() / DecPtMltFactor);
			}

			// ------------------------ Input Device ------------------------
			if(callID == (int)VMCID.ID_IS_BUTTON_PRESSED) {
				int buttonID = (int)PopValueFromStack();
				bool results = false;
				switch(buttonID) {
					case (int)GamePad.DPadNorth: results = GE.IDGamePad.IsDPadNorthPressed; break;
					case (int)GamePad.DPadSouth: results = GE.IDGamePad.IsDPadSouthPressed; break;
					case (int)GamePad.DPadWest: results = GE.IDGamePad.IsDPadWestPressed; break;
					case (int)GamePad.DPadEast: results = GE.IDGamePad.IsDPadEastPressed; break;
					case (int)GamePad.DPadNorthEast: results = GE.IDGamePad.IsDPadNorthEastPressed; break;
					case (int)GamePad.DPadSouthEast: results = GE.IDGamePad.IsDPadSouthEastPressed; break;
					case (int)GamePad.DPadSouthWest: results = GE.IDGamePad.IsDPadSouthWestPressed; break;
					case (int)GamePad.DPadNorthWest: results = GE.IDGamePad.IsDPadNorthWestPressed; break;
					case (int)GamePad.ButtonNorth: results = GE.IDGamePad.IsNorthPressed; break;
					case (int)GamePad.ButtonSouth: results = GE.IDGamePad.IsSouthPressed; break;
					case (int)GamePad.ButtonWest: results = GE.IDGamePad.IsWestPressed; break;
					case (int)GamePad.ButtonEast: results = GE.IDGamePad.IsEastPressed; break;
					case (int)GamePad.ButtonLeftShoulder: results = GE.IDGamePad.IsLeftShoulderPressed; break;
					case (int)GamePad.ButtonRightShoulder: results = GE.IDGamePad.IsRightShoulderPressed; break;
					case (int)GamePad.ButtonLeftTrigger: results = GE.IDGamePad.IsLeftTriggerPressed; break;
					case (int)GamePad.ButtonRightTrigger: results = GE.IDGamePad.IsRightTriggerPressed; break;
					case (int)GamePad.ButtonLeftStick: results = GE.IDGamePad.IsLeftStickPressed; break;
					case (int)GamePad.ButtonRightStick: results = GE.IDGamePad.IsRightStickPressed; break;
					case (int)GamePad.ButtonStart: results = GE.IDGamePad.IsSelectPressed; break;
					case (int)GamePad.ButtonSelect: results = GE.IDGamePad.IsStartPressed; break;
				}
				PushValueToStack((long)(results ? 1 : 0));
			} else if(callID == (int)VMCID.ID_GET_KEY_PRESSED) {
				PushValueToStack((long)GE.IDKeyboard.PressedKeyCode);
			}


			/*
			// ------------------------ 2D Engine ------------------------
			// ------------------------ ACTOR 2D ------------------------
			if(callID == (int)VMCID.ACTOR2D_CREATE) {
				int textStartAddress = (int)PopValueFromStack();
				PushValueToStack((long)GE.CreateActor2D(GenerateStringFromAddress(textStartAddress)));
			} else if(callID == (int)VMCID.ACTOR2D_DESTROY) {
				GE.DestroyActor2D((int)PopValueFromStack());
			} else if(callID == (int)VMCID.ACTOR2D_SET_AS_PLAYER) {
				foreach(KeyValuePair<int, Actor2D> kvp in GE.Actor2Ds) kvp.Value.IsPlayerActor = false;
				GE.TopDownGameMain.PlayerActor = GE.Actor2Ds[(int)PopValueFromStack()];
				GE.TopDownGameMain.PlayerActor.IsPlayerActor = true;
			} else if(callID == (int)VMCID.ACTOR2D_IS_COLLISION_WITH_ACTOR) {
				int targetActorID = (int)PopValueFromStack();
				int sourceActorID = (int)PopValueFromStack();
				bool results = GE.TopDownGameMain.IsActorCollidedWithActor(sourceActorID, targetActorID);
				PushValueToStack((long)(results ? 1 : 0));
			} else if(callID == (int)VMCID.ACTOR2D_SET_FACING_DIR) {
				int direction = (int)PopValueFromStack();
				GE.Actor2Ds[(int)PopValueFromStack()].FacingDirection = (Direction)direction;
			} else if(callID == (int)VMCID.ACTOR2D_GET_FACING_DIR) {
				PushValueToStack((long)(GE.Actor2Ds[(int)PopValueFromStack()].FacingDirection));
			} else if(callID == (int)VMCID.ACTOR2D_GET_TILE_POSITION_X) {
				PushValueToStack((long)GE.Actor2Ds[(int)PopValueFromStack()].TileMapPositionX);
			} else if(callID == (int)VMCID.ACTOR2D_GET_TILE_POSITION_Y) {
				PushValueToStack((long)GE.Actor2Ds[(int)PopValueFromStack()].TileMapPositionY);
			} else if(callID == (int)VMCID.ACTOR2D_GET_COLLISION_WIDTH) {
				PushValueToStack((long)GE.Actor2Ds[(int)PopValueFromStack()].GetCurrentCollisionMeshPixelDimension().x);
			} else if(callID == (int)VMCID.ACTOR2D_GET_COLLISION_HEIGHT) {
				PushValueToStack((long)GE.Actor2Ds[(int)PopValueFromStack()].GetCurrentCollisionMeshPixelDimension().y);
			}
			*/
			/*
			// ------------------------ MAP 2D ------------------------
			if(callID == (int)VMCID.MAP2D_CREATE) {
				bool isUI = (int)PopValueFromStack() == 1 ? true : false;
				int textStartAddress = (int)PopValueFromStack();
				string mapFile = GenerateStringFromAddress(textStartAddress);
				PushValueToStack((long)GE.CreateMap2D(mapFile, isUI));
			} else if(callID == (int)VMCID.MAP2D_DESTROY) {
				GE.DestroyMap2D((int)PopValueFromStack());
			} else if(callID == (int)VMCID.MAP2D_SET_ACTOR_AT_TILE_POSITION) {
				int z = (int)PopValueFromStack();
				int y = (int)PopValueFromStack();
				int x = (int)PopValueFromStack();
				int actorID = (int)PopValueFromStack();
				GE.Map2Ds[(int)PopValueFromStack()].SetActorAtTilePosition(GE.Actor2Ds[actorID], x, y, z);
			} else if(callID == (int)VMCID.MAP2D_SET_VISIBILITY) {
				bool isVisible = (int)PopValueFromStack() == 1 ? true : false;
				GE.Map2Ds[(int)PopValueFromStack()].IsVisible = isVisible;
			} else if(callID == (int)VMCID.MAP2D_GET_INSTANCE_OBJECT_REFID) {
				bool isUI = (int)PopValueFromStack() == 1 ? true : false;
				int textStartAddress = (int)PopValueFromStack();
				string objectNameToSearch = GenerateStringFromAddress(textStartAddress);
				GameObject gameObject = null;
				int objID = -1;
				if(isUI) {
					foreach(GameObject go in GameObject.FindObjectsOfType(typeof(GameObject))) {
						if(go.name.Contains(objectNameToSearch + "_")) {
							gameObject = GameObject.Find("UIBaseLayer/UIComponentController/UIBackgroundLayer/Map2DController/Map2D/" + go.name);
							if(gameObject != null) break;
						}
					}
				} else {
					foreach(GameObject go in GameObject.FindObjectsOfType(typeof(GameObject))) {
						if(go.name.Contains(objectNameToSearch + "_")) {
							gameObject = GameObject.Find("SceneManager/MapManager/Map2DController/Map2D/" + go.name);
							if(gameObject != null) break;
						}
					}
				}
				if(gameObject != null) {
					string[] strSplit = gameObject.name.Split('_');
					objID = int.Parse(strSplit[strSplit.Length - 1]);
				}
				PushValueToStack((long)objID);
			}
			*/

			// ------------------------ UI Layer ------------------------
			if(callID == (int)VMCID.LAYER_CREATE) {
				PushValueToStack((long)GE.CreateUILayer());
			} else if(callID == (int)VMCID.LAYER_DESTROY) {
				GE.DestroyUILayer((int)PopValueFromStack());
			} else if(callID == (int)VMCID.LAYER_RESET_TRANSFORM) {
				GE.UILayers[(int)PopValueFromStack()].ResetTransform();
			}/* else if(callID == (int)VMCID.LAYER_SET_POSITION_PIVOT) {
				float y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float x = (float)((double)PopValueFromStack() / DecPtMltFactor);
				GE.UILayers[(int)PopValueFromStack()].PositionPivot = new Vector2(x, y);
			} else if(callID == (int)VMCID.LAYER_SET_ROTATION_PIVOT) {
				float y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float x = (float)((double)PopValueFromStack() / DecPtMltFactor);
				GE.UILayers[(int)PopValueFromStack()].RotationPivot = new Vector2(x, y);
			} else if(callID == (int)VMCID.LAYER_SET_SCALE_PIVOT) {
				float y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float x = (float)((double)PopValueFromStack() / DecPtMltFactor);
				GE.UILayers[(int)PopValueFromStack()].ScalePivot = new Vector2(x, y);
			} else if(callID == (int)VMCID.LAYER_SET_POSITION) {
				float z = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float x = (float)((double)PopValueFromStack() / DecPtMltFactor);
				//GE.UILayers[(int)PopValueFromStack()].SetPosition(x, y, z);
				GE.UILayers[(int)PopValueFromStack()].TransformPosition(new Vector3(x, y, z));
			} else if(callID == (int)VMCID.LAYER_SET_ROTATION) {
				int z = (int)PopValueFromStack();
				int y = (int)PopValueFromStack();
				int x = (int)PopValueFromStack();
				//GE.UILayers[(int)PopValueFromStack()].SetRotation(x, y, z);
				GE.UILayers[(int)PopValueFromStack()].TransformRotation(new Vector3(x, y, z));
			} else if(callID == (int)VMCID.LAYER_SET_SCALE) {
				float z = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float x = (float)((double)PopValueFromStack() / DecPtMltFactor);
				//GE.UILayers[(int)PopValueFromStack()].SetScale(x, y, z);
				GE.UILayers[(int)PopValueFromStack()].TransformScale(new Vector3(x, y, z));
			}*/
			else if(callID == (int)VMCID.LAYER_SET_VISIBILITY) {
				bool isVisible = (int)PopValueFromStack() == 1 ? true : false;
				GE.UILayers[(int)PopValueFromStack()].ThisObject.SetActive(isVisible);
			} /*else if(callID == (int)VMCID.LAYER_SET_ACCUM_FRAME_MAX) {
				int maxFrames = (int)PopValueFromStack();
				GE.UILayers[(int)PopValueFromStack()].MaxFrames = maxFrames;
			} else if(callID == (int)VMCID.LAYER_GET_ACCUM_FRAME_COUNT) {
				PushValueToStack((long)(GE.UILayers[(int)PopValueFromStack()].Frames));
			} else if(callID == (int)VMCID.LAYER_SET_MOTION_PATH_BEZIER) {
				int targetProperty = (int)PopValueFromStack();
				float P3Y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P3X = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P2Y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P2X = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P1Y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P1X = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P0Y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P0X = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float targetPivotY = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float targetPivotX = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float targetZ = 0;
				float targetY = 0;
				float targetX = 0;
				// When target property is 1, it is a rotation, but we cannot have float values for rotation so convert it as integer instead
				if(targetProperty == 1) {
					targetZ = (int)PopValueFromStack();
					targetY = (int)PopValueFromStack();
					targetX = (int)PopValueFromStack();
				} else {
					// Otherwise, other properties are parsed as floats
					targetZ = (float)((double)PopValueFromStack() / DecPtMltFactor);
					targetY = (float)((double)PopValueFromStack() / DecPtMltFactor);
					targetX = (float)((double)PopValueFromStack() / DecPtMltFactor);
				}

				int totalFrames = (int)PopValueFromStack();

				//GE.UILayers[(int)PopValueFromStack()].SetMotionPathBezier(targetProperty, totalFrames, targetX, targetY, targetZ, targetPivotX, targetPivotY, P0X, P0Y, P1X, P1Y, P2X, P2Y, P3X, P3Y);
			} else if(callID == (int)VMCID.LAYER_SET_MOTION_TWEEN_WAIT_FRAME_COUNT) {
				int frames = (int)PopValueFromStack();
				//GE.UILayers[(int)PopValueFromStack()].SetMotionTweenWaitCount(frames);
			}
*/
			/*
			// ------------------------ Font ------------------------
			if(callID == (int)VMCID.FONT_CREATE) {
				int textStartAddress = (int)PopValueFromStack();
				PushValueToStack((long)GE.CreateFont(GenerateStringFromAddress(textStartAddress)));
			} else if(callID == (int)VMCID.FONT_DESTROY) {
				GE.DestroyFont((int)PopValueFromStack());
			}*/

/*
			// ------------------------ Texture ------------------------
			if(callID == (int)VMCID.TEXTURE_CREATE) {
				int textStartAddress = (int)PopValueFromStack();
				PushValueToStack((long)GE.CreateTexture(GenerateStringFromAddress(textStartAddress)));
			} else if(callID == (int)VMCID.TEXTURE_DESTROY) {
				GE.DestroyTexture((int)PopValueFromStack());
			}
*/
			/*
			// ------------------------ Text ------------------------
			if(callID == (int)VMCID.TEXT_CREATE) {
				int charSpacing = (int)PopValueFromStack();
				int lineHeight = (int)PopValueFromStack();
				int horizontalAlignment = (int)PopValueFromStack();
				int height = (int)PopValueFromStack();
				int width = (int)PopValueFromStack();
				int layerID = (int)PopValueFromStack();
				int fontID = (int)PopValueFromStack();
				PushValueToStack((long)GE.CreateText("", fontID, layerID, (GameEngine.HorizontalAlignment)horizontalAlignment,
					(int)((float)lineHeight * GE.PPUScaleUpUI), (int)((float)charSpacing * GE.PPUScaleUpUI),
					(int)((float)width * GE.PPUScaleUpUI), (int)((float)height * GE.PPUScaleUpUI)));
			} else if(callID == (int)VMCID.TEXT_DESTROY) {
				GE.DestroyText((int)PopValueFromStack());
			} else if(callID == (int)VMCID.TEXT_RESET_TRANSFORM) {
				GE.Texts[(int)PopValueFromStack()].ResetTransform();
			} else if(callID == (int)VMCID.TEXT_SET_TEXT) {
				int textStartAddress = (int)PopValueFromStack();
				GE.Texts[(int)PopValueFromStack()].Text = GenerateStringFromAddress(textStartAddress);
			} else if(callID == (int)VMCID.TEXT_SET_BLIT_ORDER) {
				int order = (int)PopValueFromStack();
				GE.Texts[(int)PopValueFromStack()].SetBlitOrder(order);
			} else if(callID == (int)VMCID.TEXT_SET_POSITION_PIVOT) {
				float y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float x = (float)((double)PopValueFromStack() / DecPtMltFactor);
				GE.Texts[(int)PopValueFromStack()].PositionPivot = new Vector2(x, y);
			} else if(callID == (int)VMCID.TEXT_SET_ROTATION_PIVOT) {
				float y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float x = (float)((double)PopValueFromStack() / DecPtMltFactor);
				GE.Texts[(int)PopValueFromStack()].RotationPivot = new Vector2(x, y);
			} else if(callID == (int)VMCID.TEXT_SET_SCALE_PIVOT) {
				float y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float x = (float)((double)PopValueFromStack() / DecPtMltFactor);
				GE.Texts[(int)PopValueFromStack()].ScalePivot = new Vector2(x, y);
			} else if(callID == (int)VMCID.TEXT_SET_POSITION) {
				float z = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float x = (float)((double)PopValueFromStack() / DecPtMltFactor);
				//GE.Texts[(int)PopValueFromStack()].SetPosition(x, y, z);
				GE.Texts[(int)PopValueFromStack()].TransformPosition(new Vector3(x, y, z));
			} else if(callID == (int)VMCID.TEXT_SET_ROTATION) {
				int z = (int)PopValueFromStack();
				int y = (int)PopValueFromStack();
				int x = (int)PopValueFromStack();
				//GE.Texts[(int)PopValueFromStack()].SetRotation(x, y, z);
				GE.Texts[(int)PopValueFromStack()].TransformRotation(new Vector3(x, y, z));
			} else if(callID == (int)VMCID.TEXT_SET_SCALE) {
				float z = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float x = (float)((double)PopValueFromStack() / DecPtMltFactor);
				//GE.Texts[(int)PopValueFromStack()].SetScale(x, y, z);
				GE.Texts[(int)PopValueFromStack()].TransformScale(new Vector3(x, y, z));
			} else if(callID == (int)VMCID.TEXT_SET_VISIBILITY) {
				bool isVisible = (int)PopValueFromStack() == 1 ? true : false;
				GE.Texts[(int)PopValueFromStack()].IsVisible = isVisible;
			} else if(callID == (int)VMCID.TEXT_SET_CAPTION_SPEED) {
				float rate = (float)((double)PopValueFromStack() / DecPtMltFactor);
				GE.Texts[(int)PopValueFromStack()].SetCaptionSpeed(rate);
			} else if(callID == (int)VMCID.TEXT_FADEIN) {
				float rate = (float)((double)PopValueFromStack() / DecPtMltFactor);
				GE.Texts[(int)PopValueFromStack()].StartFadeIn(rate);
			} else if(callID == (int)VMCID.TEXT_FADEOUT) {
				float rate = (float)((double)PopValueFromStack() / DecPtMltFactor);
				GE.Texts[(int)PopValueFromStack()].StartFadeOut(rate);
			} else if(callID == (int)VMCID.TEXT_SET_COLOR) {
				int range = (int)PopValueFromStack();
				int charIndex = (int)PopValueFromStack();
				float b = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float g = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float r = (float)((double)PopValueFromStack() / DecPtMltFactor);
				GE.Texts[(int)PopValueFromStack()].SetColor(r, g, b, charIndex, range);
			} else if(callID == (int)VMCID.TEXT_SET_ALPHA) {
				float alpha = (float)((double)PopValueFromStack() / DecPtMltFactor);
				GE.Texts[(int)PopValueFromStack()].SetAlpha(alpha);
			} else if(callID == (int)VMCID.TEXT_GET_ALPHA) {
				PushValueToStack((long)(GE.Texts[(int)PopValueFromStack()].GetAlpha() * DecPtMltFactor));
			} else if(callID == (int)VMCID.TEXT_SET_ACCUM_FRAME_MAX) {
				int maxFrames = (int)PopValueFromStack();
				GE.Texts[(int)PopValueFromStack()].MaxFrames = maxFrames;
			} else if(callID == (int)VMCID.TEXT_GET_ACCUM_FRAME_COUNT) {
				PushValueToStack((long)(GE.Texts[(int)PopValueFromStack()].Frames));
			} else if(callID == (int)VMCID.TEXT_SET_MOTION_PATH_BEZIER) {
				int targetProperty = (int)PopValueFromStack();
				float P3Y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P3X = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P2Y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P2X = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P1Y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P1X = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P0Y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P0X = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float targetPivotY = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float targetPivotX = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float targetZ = 0;
				float targetY = 0;
				float targetX = 0;
				// When target property is 1, it is a rotation, but we cannot have float values for rotation so convert it as integer instead
				if(targetProperty == 1) {
					targetZ = (int)PopValueFromStack();
					targetY = (int)PopValueFromStack();
					targetX = (int)PopValueFromStack();
				} else {
					// Otherwise, other properties are parsed as floats
					targetZ = (float)((double)PopValueFromStack() / DecPtMltFactor);
					targetY = (float)((double)PopValueFromStack() / DecPtMltFactor);
					targetX = (float)((double)PopValueFromStack() / DecPtMltFactor);
				}

				int totalFrames = (int)PopValueFromStack();

				//GE.Texts[(int)PopValueFromStack()].SetMotionPathBezier(targetProperty, totalFrames, targetX, targetY, targetZ, targetPivotX, targetPivotY, P0X, P0Y, P1X, P1Y, P2X, P2Y, P3X, P3Y);
			} else if(callID == (int)VMCID.TEXT_SET_MOTION_TWEEN_WAIT_FRAME_COUNT) {
				int frames = (int)PopValueFromStack();
				//GE.Texts[(int)PopValueFromStack()].SetMotionTweenWaitCount(frames);
			}
			*/
			/*
			// ------------------------ Image ------------------------
			if(callID == (int)VMCID.IMAGE_CREATE) {
				int sheetOrientation = (int)PopValueFromStack();
				int maxSheets = (int)PopValueFromStack();
				int rH = (int)PopValueFromStack();
				int rW = (int)PopValueFromStack();
				int rY = (int)PopValueFromStack();
				int rX = (int)PopValueFromStack();
				int layerID = (int)PopValueFromStack();
				int textureID = (int)PopValueFromStack();
				PushValueToStack((long)GE.CreateImage(textureID, layerID, rX, rY, rW, rH, maxSheets, (GameEngine.SheetOrientation)sheetOrientation));
			} else if(callID == (int)VMCID.IMAGE_DESTROY) {
				GE.DestroyImage((int)PopValueFromStack());
			} else if(callID == (int)VMCID.IMAGE_RESET_TRANSFORM) {
				GE.Images[(int)PopValueFromStack()].ResetTransform();
			} else if(callID == (int)VMCID.IMAGE_SET_BLIT_ORDER) {
				int order = (int)PopValueFromStack();
				GE.Images[(int)PopValueFromStack()].SetBlitOrder(order);
			} else if(callID == (int)VMCID.IMAGE_SET_POSITION_PIVOT) {
				float y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float x = (float)((double)PopValueFromStack() / DecPtMltFactor);
				GE.Images[(int)PopValueFromStack()].PositionPivot = new Vector2(x, y);
			} else if(callID == (int)VMCID.IMAGE_SET_ROTATION_PIVOT) {
				float y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float x = (float)((double)PopValueFromStack() / DecPtMltFactor);
				GE.Images[(int)PopValueFromStack()].RotationPivot = new Vector2(x, y);
			} else if(callID == (int)VMCID.IMAGE_SET_SCALE_PIVOT) {
				float y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float x = (float)((double)PopValueFromStack() / DecPtMltFactor);
				GE.Images[(int)PopValueFromStack()].ScalePivot = new Vector2(x, y);
			} else if(callID == (int)VMCID.IMAGE_SET_POSITION) {
				float z = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float x = (float)((double)PopValueFromStack() / DecPtMltFactor);
				//GE.Images[(int)PopValueFromStack()].SetPosition(x, y, z);
				GE.Images[(int)PopValueFromStack()].TransformPosition(new Vector3(x, y, z));
			} else if(callID == (int)VMCID.IMAGE_SET_ROTATION) {
				int z = (int)PopValueFromStack();
				int y = (int)PopValueFromStack();
				int x = (int)PopValueFromStack();
				//GE.Images[(int)PopValueFromStack()].SetRotation(x, y, z);
				GE.Images[(int)PopValueFromStack()].TransformRotation(new Vector3(x, y, z));
			} else if(callID == (int)VMCID.IMAGE_SET_SCALE) {
				float z = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float x = (float)((double)PopValueFromStack() / DecPtMltFactor);
				//GE.Images[(int)PopValueFromStack()].SetScale(x * GE.PPUScaleUpUI, y * GE.PPUScaleUpUI, z * GE.PPUScaleUpUI);
				GE.Images[(int)PopValueFromStack()].TransformScale(new Vector3(x * GE.PPUScaleUpUI, y * GE.PPUScaleUpUI, z * GE.PPUScaleUpUI));
			}
			else if(callID == (int)VMCID.IMAGE_SET_VISIBILITY) {
				bool isVisible = (int)PopValueFromStack() == 1 ? true : false;
				GE.Images[(int)PopValueFromStack()].IsVisible = isVisible;
			} else if(callID == (int)VMCID.IMAGE_SET_SHEET_ANIMATION_SPEED) {
				float rate = (float)((double)PopValueFromStack() / DecPtMltFactor);
				GE.Images[(int)PopValueFromStack()].SetAnimationDurationRate(rate);
			} else if(callID == (int)VMCID.IMAGE_FADEIN) {
				float rate = (float)((double)PopValueFromStack() / DecPtMltFactor);
				GE.Images[(int)PopValueFromStack()].StartFadeIn(rate);
			} else if(callID == (int)VMCID.IMAGE_FADEOUT) {
				float rate = (float)((double)PopValueFromStack() / DecPtMltFactor);
				GE.Images[(int)PopValueFromStack()].StartFadeOut(rate);
			} else if(callID == (int)VMCID.IMAGE_SET_COLOR) {
				float b = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float g = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float r = (float)((double)PopValueFromStack() / DecPtMltFactor);
				GE.Images[(int)PopValueFromStack()].SetColor(r, g, b);
			} else if(callID == (int)VMCID.IMAGE_SET_ALPHA) {
				float alpha = (float)((double)PopValueFromStack() / DecPtMltFactor);
				GE.Images[(int)PopValueFromStack()].SetAlpha(alpha);
			} else if(callID == (int)VMCID.IMAGE_GET_ALPHA) {
				PushValueToStack((long)(GE.Images[(int)PopValueFromStack()].GetAlpha() * DecPtMltFactor));
			} else if(callID == (int)VMCID.IMAGE_SET_ACCUM_FRAME_MAX) {
				int maxFrames = (int)PopValueFromStack();
				GE.Images[(int)PopValueFromStack()].MaxFrames = maxFrames;
			} else if(callID == (int)VMCID.IMAGE_GET_ACCUM_FRAME_COUNT) {
				PushValueToStack((long)(GE.Images[(int)PopValueFromStack()].Frames));
			} else if(callID == (int)VMCID.IMAGE_SET_MOTION_PATH_BEZIER) {
				int targetProperty = (int)PopValueFromStack();
				float P3Y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P3X = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P2Y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P2X = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P1Y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P1X = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P0Y = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float P0X = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float targetPivotY = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float targetPivotX = (float)((double)PopValueFromStack() / DecPtMltFactor);
				float targetZ = 0;
				float targetY = 0;
				float targetX = 0;
				// When target property is 1, it is a rotation, but we cannot have float values for rotation so convert it as integer instead
				if(targetProperty == 1) {
					targetZ = (int)PopValueFromStack();
					targetY = (int)PopValueFromStack();
					targetX = (int)PopValueFromStack();
				} else {
					// Otherwise, other properties are parsed as floats
					targetZ = (float)((double)PopValueFromStack() / DecPtMltFactor);
					targetY = (float)((double)PopValueFromStack() / DecPtMltFactor);
					targetX = (float)((double)PopValueFromStack() / DecPtMltFactor);
				}

				int totalFrames = (int)PopValueFromStack();

				//GE.Images[(int)PopValueFromStack()].SetMotionPathBezier(targetProperty, totalFrames, targetX, targetY, targetZ, targetPivotX, targetPivotY, P0X, P0Y, P1X, P1Y, P2X, P2Y, P3X, P3Y);
			} else if(callID == (int)VMCID.IMAGE_SET_MOTION_TWEEN_WAIT_FRAME_COUNT) {
				int frames = (int)PopValueFromStack();
				//GE.Images[(int)PopValueFromStack()].SetMotionTweenWaitCount(frames);
			}
			*/
		}
	}
}
