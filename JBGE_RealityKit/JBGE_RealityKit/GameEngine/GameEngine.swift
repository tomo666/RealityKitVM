//
//  GameEngine.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/03.
//

// Pseudo-orthographic UI configuration struct
struct UIPseudoOrthoConfig {
    let uiDistance: Float
    let referenceFOV: Float
    let referenceHeight: Float
    let uiScale: Float
}

open class GameEngine {
    // RealityKit Specific: Root Scene to attach objects
    public let RootScene: RealityKitScene
    // RealityKit Specific: Root of the UI Component
    public let UIRoot: GameObject = GameObject("UIRoot")
    
    // For showing debug info (although this uses Unity's Canvas so currently is a placeholder)
    public var DebugDeviceInputCanvasObj: GameObject? = nil
    public var DebugPerformanceCanvasObj: GameObject? = nil

    // Main GameObject
    public var MainGameObject: GameObject
    
    // Reference to 2D Game Engine
    public var TopDownGameMain: Any? = nil

    // Manages the non-UI components
    public var SceneManager: GameObject
    // Manages the actors within the game
    public var ActorManager: GameObject
    // Manages the map within the game
    public var MapManager: GameObject
    
    // Flag to determine if DEBUG info should be shown on screen or not
    public var IsShowDebugInfo: Bool = false
    // Defines the target frame rate (defaults to 60)
    public var TargetFrameRate: Int = 60
    // Stores the average frame rate
    public var AverageFrameRate: Int = 0
    // Stores the frame count 0 to 60
    public var FrameCount: Int = 0
    // Amount of frame to wait until executing the next script
    public var WaitFrameCount: Int = 0
    // Determines if we are in wait state or not
    public var IsWaiting: Bool = false

    // Reference to the Main Camera object
    public var MainCamera: Any? = nil
    // Reference to the UICamera camera object
    public var UICamera: Any? = nil
    // Set to false if camera is in perspective mode, else true in orthographic mode
    public var IsUICameraOrthographic: Bool = false
    // If in perspective mode, the horizontal Field Of View Angle (in degrees)
    public var FOV: Float = 60.0
    // The Pixel Per Unit scale to lift up UI scaling
    public var PPUScaleUpUI: Float = 4.0
    // The Pixel Per Unit scale to lift up 2D Map scaling (Actually it's the ortho size so not affecting the actual object scales)
    public var PPUScaleUpWorld: Float = 5.0
    // Centralized configuration for UI pseudo-orthographic projection
    let uiConfig = UIPseudoOrthoConfig(
        uiDistance: 169.0,
        referenceFOV: 60.0,
        referenceHeight: 1080.0,
        uiScale: 1.0
    )

    // Reference to the Main Camera (or virtual camera) object
    public var CinemachineVirtualCamera: Any? = nil
    // Reference to the CinemachinePositionComposer
    public var CinemachinePositionComposer: Any? = nil

    // Manages all textmeshpro text used in game
    public var TextBlockManagerList: [Any] = []
    // TextBlockManager: current list index to operate on
    public var CurrentTextBlockManagerIndex: Int = 0
    // TextBlock: current list index to operate on
    public var CurrentTextBlockIndex: Int = 0

    // TextBlockManager: current list index to operate on
    public var CurrentBMPTextManagerIndex: Int = 0
    // TextBlock: current list index to operate on
    public var CurrentBMPTextIndex: Int = 0
    
    // The one and only Base UI Layer that always sits in front of the UICamera
    public var UIBaseLayer: UIComponent? = nil
    // UI layer that manages actors (or sprites)
    public var UIBackgroundLayer: UIComponent? = nil
    
    // Stores all the UILayer(s) that is placed on the UIBaseLayer UICanvas
    public var UILayers: [Int: UIComponent] = [:]
    // Stores all the 2D textures used in this game
    public var Textures: [Int: Any] = [:]
    // Stores all the Image object used in this game
    public var Images: [Int: Any] = [:]
    // Stores all the Font object used in this game
    public var Fonts: [Int: Any] = [:]
    // Stores all the BMPText object used in this game
    public var Texts: [Int: Any] = [:]
    // Stores all the Actor2D object used in this game
    public var Actor2Ds: [Int: Any] = [:]
    // Stores all the Map2D object used in this game
    public var Map2Ds: [Int: Any] = [:]

    // ImageManager: current list index to operate on
    public var CurrentImageManagerIndex: Int = 0
    // Image: current list index to operate on
    public var CurrentImageIndex: Int = 0

    // Image direction to search in the atlas for creating animations that are made up of multiple sprites
    public enum SheetOrientation {
        case Horizontal
        case Vertical
    }
    // General horizontal alignment definitions
    public enum HorizontalAlignment {
        case Left
        case Center
        case Right
    }
    // General vertical alignment definitions
    public enum VerticalAlignment {
        case Top
        case Middle
        case Bottom
    }
    // General orientation definitions
    public enum Orientation {
        case Horizontal
        case Vertical
    }
    // General speed identifiers
    public enum MovementSpeed {
        case VerySlow
        case Slow
        case Normal
        case Fast
        case VeryFast
    }

    // User gamepad device inputs
    public var IDGamePad: IDGamePad = JBGE_RealityKit.IDGamePad()
    // User mouse device inputs
    public var IDMouse: IDMouse = JBGE_RealityKit.IDMouse()
    // User keyboard device inputs
    public var IDKeyboard: IDKeyboard = JBGE_RealityKit.IDKeyboard()
    // Manages all user inputs
    public lazy var UserInput: IDUserInput = IDUserInput(self)

    // The global pixel per unit applied to non-UI 2D graphic elements
    public var GlobalPixelPerUnit = 32
    
    public init(_ gameObject: GameObject, scene: RealityKitScene) {
        // RealityKit specific
        self.RootScene = scene
        
        self.MainGameObject = gameObject

        SceneManager = GameObject("SceneManager")
        ActorManager = GameObject("ActorManager")
        MapManager   = GameObject("MapManager")

        ActorManager.transform.SetParent(SceneManager.transform)
        MapManager.transform.SetParent(SceneManager.transform)

        // RealityKit specific
        scene.AddToScene(SceneManager)
        scene.AddToScene(ActorManager)
        scene.AddToScene(MapManager)
        
        // We need to call this in order to check for Addressable asset existence
        //Addressables.InitializeAsync();
        
        // Initialize UI
        InitializeUI()
    }

    private func InitializeUI() {
        // Create user input object
        //UserInput = IDUserInput(self)
        
        // RealityKit-specific: Cameras are injected via RealityKitScene
        UICamera = nil
        MainCamera = nil
        CinemachineVirtualCamera = nil
        CinemachinePositionComposer = nil

        // If perspective mode, set camera properties
        if !IsUICameraOrthographic {
            // TODO: configure perspective UI camera
        } else {
            // TODO: configure orthographic UI camera
        }

        // Create the one and utmost base layer that attaches to the UICamera
        UIBaseLayer = UIComponent(self, "UIBaseLayer", nil, true, false)

        // Reset and align base layer to center
        UIBaseLayer?.ResetTransform()
        UIBaseLayer?.SetPivot(0.5, 0.5)
        UIBaseLayer?.SetScale(1.0, 1.0, 1.0)
        UIBaseLayer?.SetPivot(0.5, 0.5)
        UIBaseLayer?.SetRotation(0, 0, 0)
        UIBaseLayer?.SetPivot(0.5, 0.5)
        UIBaseLayer?.SetPosition(0.5, 0.5, 0.0)
        UIBaseLayer?.IsVisible = true

        // Create UI Layers
        let UIBackgroundLayerID = CreateUILayer("UIBackgroundLayer")
        UIBackgroundLayer = UILayers[UIBackgroundLayerID]
        
        print("[GameEngine] Initialize UI Completed.")
    }

    /// Unity-compatible ViewportToWorldPoint replacement
    /// - Parameters:
    ///   - x: Viewport X (0.0 - 1.0)
    ///   - y: Viewport Y (0.0 - 1.0)
    ///   - z: Additional depth offset
    ///   - isUI: true if UI pseudo-orthographic space
    public func ViewportToWorldPoint(
        _ x: Float,
        _ y: Float,
        _ z: Float,
        isUI: Bool
    ) -> Vector3 {

        if isUI {
            let distance = uiConfig.uiDistance + z
            return RootScene.projectFromViewport(
                x: x,
                y: 1.0 - y,
                distance: distance,
                fov: uiConfig.referenceFOV
            )
        } else {
            return RootScene.projectFromViewport(
                x: x,
                y: 1.0 - y,
                distance: z,
                fov: FOV
            )
        }
    }
    
    /// <summary>Creates a new Layer under the base layer</summary>
    /// <returns>ID is generated that can be used to identify the newly created object (if creation fails, returns -1)</returns>
    @discardableResult
    public func CreateUILayer(_ layerName: String = "Layer") -> Int {
        guard let baseLayer = UIBaseLayer else { return -1 }

        let layer = UIComponent(self, layerName, baseLayer)

        // We need to set the pivot and position of this layer to center of our UICamera
        layer.ResetTransform()
        layer.SetPivot(0.5, 0.5)
        layer.SetScale(1.0, 1.0, 1.0)
        layer.SetPivot(0.5, 0.5)
        layer.SetRotation(0, 0, 0)
        layer.SetPivot(0.5, 0.5)
        layer.SetPosition(0.5, 0.5, 0.0)

        layer.LocalWidth *= 2
        layer.LocalHeight *= 2

        if let controller = layer.Controller {
            layer.ThisObject.SetSizeFrom(controller)
        }

        layer.IsVisible = true

        // Sort order for each custom UI layer is incremented by 1000, where the UI BaseLayer is 0
        layer.SortOrder = (UILayers.count + 1) * 1000

        // Ensure unique ID
        while UILayers[layer.ID] != nil {
            layer.ID = Int.random(in: Int.min...Int.max)
        }

        UILayers[layer.ID] = layer
        return layer.ID
    }

    /// <summary>Destroys the UILayer from our list</summary>
    /// <param name="id">The handle ID of the object to be deleted</param>
    public func DestroyUILayer(_ id: Int) {
        guard let layer = UILayers[id] else { return }
        layer.Destroy()
        UILayers.removeValue(forKey: id)
    }
    
    /*
    public void Update() {

      UserInput.Update();

      foreach(KeyValuePair<int, Actor2D> kvp in Actor2Ds) {
        kvp.Value.Update();
      }
      foreach(KeyValuePair<int, Map2D> kvp in Map2Ds) {
        kvp.Value.Update();
      }
      foreach(KeyValuePair<int, Image> kvp in Images) {
        kvp.Value.Update();
      }
      foreach(KeyValuePair<int, BMPText> kvp in Texts) {
        kvp.Value.Update();
      }

      // Get user input states
      if(Gamepad.current != null) {
        IDGamePad.IsNorthPressed = Gamepad.current.buttonNorth.isPressed;
        IDGamePad.IsSouthPressed = Gamepad.current.buttonSouth.isPressed;
        IDGamePad.IsWestPressed = Gamepad.current.buttonWest.isPressed;
        IDGamePad.IsEastPressed = Gamepad.current.buttonEast.isPressed;
        IDGamePad.LeftStickValue = Gamepad.current.leftStick.ReadValue();
        IDGamePad.RightStickValue = Gamepad.current.rightStick.ReadValue();
        IDGamePad.IsLeftShoulderPressed = Gamepad.current.leftShoulder.ReadValue() == 1 ? true : false;
        IDGamePad.IsRightShoulderPressed = Gamepad.current.rightShoulder.ReadValue() == 1 ? true : false;
        IDGamePad.LeftTriggerValue = Gamepad.current.leftTrigger.ReadValue();
        IDGamePad.RightTriggerValue = Gamepad.current.rightTrigger.ReadValue();
        IDGamePad.DPadValue = Gamepad.current.dpad.ReadValue();
        IDGamePad.IsLeftStickPressed = Gamepad.current.leftStickButton.IsPressed();
        IDGamePad.IsRightStickPressed = Gamepad.current.rightStickButton.IsPressed();
        IDGamePad.IsSelectPressed = Gamepad.current.selectButton.IsPressed();
        IDGamePad.IsStartPressed = Gamepad.current.startButton.IsPressed();
      }

      if(Mouse.current != null) {
        IDMouse.IsMouseLeftPressed = Mouse.current.leftButton.isPressed;
        IDMouse.IsMouseRightPressed = Mouse.current.rightButton.isPressed;
        IDMouse.IsMouseMiddlePressed = Mouse.current.middleButton.isPressed;
        IDMouse.IsMouseForwardPressed = Mouse.current.forwardButton.isPressed;
        IDMouse.IsMouseBackPressed = Mouse.current.backButton.isPressed;
        IDMouse.ScrollAmount = (int)Mouse.current.scroll.y.ReadValue();
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        IDMouse.MousePosition = new Vector2((float)Math.Round(mousePosition.x), (float)Math.Round(Screen.height - mousePosition.y));
      }

      // B Key to toggle debug info display
      if(IDKeyboard.IsKeyPressed((int)KeyCode.B) && IsWaiting == false) {
        IsShowDebugInfo = !IsShowDebugInfo;
        DebugDeviceInputCanvasObj.SetActive(IsShowDebugInfo);
        DebugPerformanceCanvasObj.SetActive(IsShowDebugInfo);
        WaitFrameCount = 10;
        IsWaiting = true;
      }

      // Show input status
      if(IsShowDebugInfo) {
        string strInputStates = "GAMEPAD STATES\n";
        strInputStates += "BTN NORTH [△]: " + IDGamePad.IsNorthPressed + "\n";
        strInputStates += "BTN EAST [〇]: " + IDGamePad.IsEastPressed + "\n";
        strInputStates += "BTN SOUTH [×]: " + IDGamePad.IsSouthPressed + "\n";
        strInputStates += "BTN WEST [□]: " + IDGamePad.IsWestPressed + "\n";
        strInputStates += "BTN LEFT1 [L1]: " + IDGamePad.IsLeftShoulderPressed + "\n";
        strInputStates += "BTN RIGHT1 [R1]: " + IDGamePad.IsRightShoulderPressed + "\n";
        strInputStates += "BTN Trigger Left [L2]: " + IDGamePad.IsLeftTriggerPressed + "\n";
        strInputStates += "BTN Trigger Right [R2]: " + IDGamePad.IsRightTriggerPressed + "\n";
        strInputStates += "BTN LEFT STICK [L3]: " + IDGamePad.IsLeftStickPressed + "\n";
        strInputStates += "BTN RIGHT STICK [R3]: " + IDGamePad.IsRightStickPressed + "\n";
        strInputStates += "BTN START [Start]: " + IDGamePad.IsStartPressed + "\n";
        strInputStates += "BTN Select [Select]: " + IDGamePad.IsSelectPressed + "\n";
        strInputStates += "DPAD Value (X,Y):" + IDGamePad.DPadValue + "\n";
        strInputStates += "DPAD [↑]: " + IDGamePad.IsDPadNorthPressed + "\n";
        strInputStates += "DPAD [↑→]: " + IDGamePad.IsDPadNorthEastPressed + "\n";
        strInputStates += "DPAD [→]: " + IDGamePad.IsDPadEastPressed + "\n";
        strInputStates += "DPAD [→↓]: " + IDGamePad.IsDPadSouthEastPressed + "\n";
        strInputStates += "DPAD [↓]: " + IDGamePad.IsDPadSouthPressed + "\n";
        strInputStates += "DPAD [↓←]: " + IDGamePad.IsDPadSouthWestPressed + "\n";
        strInputStates += "DPAD [←]: " + IDGamePad.IsDPadWestPressed + "\n";
        strInputStates += "DPAD [←↑]: " + IDGamePad.IsDPadNorthWestPressed + "\n";
        strInputStates += "Left Stick Value (X,Y): " + IDGamePad.LeftStickValue + "\n";
        strInputStates += "Right Stick Value (X,Y): " + IDGamePad.RightStickValue + "\n";
        strInputStates += "Left Trigger Value: " + IDGamePad.LeftTriggerValue + "\n";
        strInputStates += "Right Trigger Value: " + IDGamePad.RightTriggerValue + "\n";
        strInputStates += "--------------------------\n";
        strInputStates += "MOUSE STATES\n";
        strInputStates += "BTN LEFT PRESSED: " + IDMouse.IsMouseLeftPressed + "\n";
        strInputStates += "BTN RIGHT PRESSED: " + IDMouse.IsMouseRightPressed + "\n";
        strInputStates += "BTN MIDDLE PRESSED: " + IDMouse.IsMouseMiddlePressed + "\n";
        strInputStates += "BTN FORWARD PRESSED: " + IDMouse.IsMouseForwardPressed + "\n";
        strInputStates += "BTN BACK PRESSED: " + IDMouse.IsMouseBackPressed + "\n";
        strInputStates += "V-SCROLL: " + IDMouse.ScrollAmount + "\n";
        strInputStates += "V-SCROLL UP: " + IDMouse.IsScrolledUp + "\n";
        strInputStates += "V-SCROLL DOWN: " + IDMouse.IsScrolledDown + "\n";
        strInputStates += "POSITION: " + IDMouse.MousePosition + "\n";
        strInputStates += "--------------------------\n";
        strInputStates += "KEYBOARD STATES\n";
        strInputStates += "KEY PRESSED: " + IDKeyboard.IsAnyKeyPressed + "\n";
        strInputStates += "KEY CODE: " + IDKeyboard.PressedKeyCode + "\n";

        TextMeshProUGUI textInput = DebugDeviceInputCanvasObj.GetComponent<TextMeshProUGUI>();
        textInput.SetText(strInputStates);
      }
    }*/
}

