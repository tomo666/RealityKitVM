//
//  GameEngine.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/03.
//

import RealityKit

open class GameEngine {
    // Main GameObject
    public var MainGameObject: Entity
    
    // Main Perspective Camera
    public var MainCamera: Entity

    // Manages the UI components
    public var UIManager: Entity? = nil
    // Manages the non-UI components
    public var SceneManager: Entity? = nil
    
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

    // The Pixel Per Unit scale to lift up UI scaling
    public var PPUScaleUpUI: Float = 4.0
    // The Pixel Per Unit scale to lift up 2D Map scaling (Actually it's the ortho size so not affecting the actual object scales)
    public var PPUScaleUpWorld: Float = 5.0
    // Screen width in pixels
    public var ScreenWidth: Float = 0
    // Screen height in pixels
    public var ScreenHeight: Float = 0
    // If in perspective mode, the horizontal Field Of View Angle (in degrees)
    public var FOV: Float = 60.0
    
    // The one and only Base UI Layer that always sits in front of the UICamera
    public var UIBaseLayer: UIComponent? = nil
    // UI layer that manages actors (or sprites)
    public var UIBackgroundLayer: UIComponent? = nil
    
    // Stores all the UILayer(s) that is placed on the UIBaseLayer UICanvas
    public var UILayers: [Int: UIComponent] = [:]

    // User gamepad device inputs
    public var IDGamePad: IDGamePad = JBGE_RealityKit.IDGamePad()
    // User mouse device inputs
    public var IDMouse: IDMouse = JBGE_RealityKit.IDMouse()
    // User keyboard device inputs
    public var IDKeyboard: IDKeyboard = JBGE_RealityKit.IDKeyboard()
    // Manages all user inputs
    public lazy var UserInput: IDUserInput = IDUserInput(self)
    
    public init(_ gameObject: Entity, _ screenWidth: Float, _ screenHeight: Float) {
        MainGameObject = gameObject
        
        // Setup main camera
        var cameraEntity = Entity()
        cameraEntity.name = "MainCamera"
        var camera = PerspectiveCameraComponent()
        camera.fieldOfViewInDegrees = FOV
        camera.near = 0.01
        camera.far  = 1000.0
        cameraEntity.components.set(camera)
        cameraEntity.transform.translation = SIMD3<Float>(0, 0, 1.5)
        MainCamera = cameraEntity
        MainGameObject.addChild(MainCamera)
        
        SceneManager = Entity()
        SceneManager?.name = "SceneManager"
        MainGameObject.addChild(SceneManager ?? Entity())
        UIManager = Entity()
        UIManager?.name = "UIManager"
        MainGameObject.addChild(UIManager ?? Entity())
        
        // TEST
        if let cube = MainGameObject.findEntity(named: "CubeTest") {
            cube.removeFromParent()
            SceneManager?.addChild(cube)
        }
        
        UpdateScreenSize(screenWidth, screenHeight)
        
        // Initialize UI
        InitializeUI()
    }

    private func InitializeUI() {
        // Create user input object
        UserInput = IDUserInput(self);
        
        // Create the one and utmost base layer
        UIBaseLayer = UIComponent(self, "UIBaseLayer", nil, true)
        // Add UIBaseLayer to UIManager as root layer
        UIManager?.addChild(UIBaseLayer?.ThisObject ?? Entity())
        // Test 0:
        /*UIBaseLayer?.TransformAll(
            SIMD3<Float>(0.5, 0.5, 1),
            SIMD3<Float>(0.0, 0.0, 0.0),
            SIMD3<Float>(0.5, 0.5, 1.0),
            SIMD3<Float>(0, 0, 45),
            SIMD2<Float>(0.0, 0.0),
            SIMD2<Float>(0.0, 0.0),
            SIMD2<Float>(1.0, 1.0))
        // Test 1:
        UIBaseLayer?.TransformAll(
            SIMD3<Float>(1, 1, 1),
            SIMD3<Float>(0, 0, 0),
            SIMD3<Float>(1, 1, 1),
            SIMD3<Float>(0, 0, 0),
            SIMD2<Float>(0, 0),
            SIMD2<Float>(0.5, 0.5),
            SIMD2<Float>(0.5, 0.5)
        );
        // Test 2:
        UIBaseLayer?.TransformAll(
            SIMD3<Float>(0.5, 0.5, 1),
            SIMD3<Float>(0.5, 0.5, 0),
            SIMD3<Float>(1, 1, 1),
            SIMD3<Float>(0, 0, 0),
            SIMD2<Float>(0.5, 0.5),
            SIMD2<Float>(0.5, 0.5),
            SIMD2<Float>(0.5, 0.5)
        );
        // Test 3:
        UIBaseLayer?.TransformAll(
            SIMD3<Float>(0.5, 0.5, 1),
            SIMD3<Float>(1, 1, 0),
            SIMD3<Float>(1, 1, 1),
            SIMD3<Float>(0, 0, 0),
            SIMD2<Float>(1, 1),
            SIMD2<Float>(0.5, 0.5),
            SIMD2<Float>(0.5, 0.5)
        );
        // Test 4:
        UIBaseLayer?.TransformAll(
            SIMD3<Float>(1, 1, 1),
            SIMD3<Float>(0, 0, 0),
            SIMD3<Float>(0.5, 0.5, 1),
            SIMD3<Float>(0, 0, 0),
            SIMD2<Float>(0, 0),
            SIMD2<Float>(0, 0),
            SIMD2<Float>(0.5, 0.5)
        );
        // Test 5:
        UIBaseLayer?.TransformAll(
            SIMD3<Float>(0.6, 0.6, 1),
            SIMD3<Float>(0.5, 0.5, 0),
            SIMD3<Float>(1, 1, 1),
            SIMD3<Float>(0, 0, 30),
            SIMD2<Float>(0.5, 0.5),
            SIMD2<Float>(0.5, 0.5),
            SIMD2<Float>(0.5, 0.5)
        );
        // Test 6:
        UIBaseLayer?.TransformAll(
            SIMD3<Float>(0.5, 0.5, 1),
            SIMD3<Float>(0, 0, 0),
            SIMD3<Float>(0.5, 0.5, 1),
            SIMD3<Float>(0, 0, 45),
            SIMD2<Float>(0, 0),
            SIMD2<Float>(0, 0),
            SIMD2<Float>(1, 1)
        );
        // Test 7:
        UIBaseLayer?.TransformAll(
            SIMD3<Float>(0.75, 0.3, 1),
            SIMD3<Float>(0.4, 0.1, 0),
            SIMD3<Float>(0.5, 0.5, 1),
            SIMD3<Float>(0, 0, 0),
            SIMD2<Float>(0.25, 0.25),
            SIMD2<Float>(0.8, 0.8),
            SIMD2<Float>(0.5, 0.5)
        );*/
        // Test 8:
        UIBaseLayer?.TransformAll(
            SIMD3<Float>(0.75, 0.3, 1),
            SIMD3<Float>(0.4, 0.1, 0),
            SIMD3<Float>(0.5, 0.5, 1),
            SIMD3<Float>(20, 10, 80),
            SIMD2<Float>(0.25, 0.25),
            SIMD2<Float>(0.8, 0.8),
            SIMD2<Float>(0.16, 0.23)
        );
        
        // Create UI Layers
        //let UIBackgroundLayerID = CreateUILayer("UIBackgroundLayer")
        //UIBackgroundLayer = UILayers[UIBackgroundLayerID]

        print("[GameEngine] Initialize UI Completed.")
        print("===== ENTITY HIERARCHY DUMP =====")
        dumpEntityTree(MainGameObject)
        print("================================")
    }
    
    // Called when screen size changes
    public func UpdateScreenSize(_ width: Float, _ height: Float) {
        guard height > 0 else { return }
        ScreenWidth = width
        ScreenHeight = height
        print("[GameEngine] Screen Size Changed: \(width) x \(height) --> aspect: \(width / height)")
    }

    /// <summary>Creates a new Layer under the base layer</summary>
    /// <returns>ID is generated that can be used to identify the newly created object (if creation fails, returns -1)</returns>
    @discardableResult
    public func CreateUILayer(_ layerName: String = "Layer") -> Int {
        guard let baseLayer = UIBaseLayer else { return -1 }
        let layer = UIComponent(self, layerName, baseLayer, true)
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
    
    private var time: Float = 0

    public func Update(_ deltaTime: Float) {
        time += deltaTime

        guard let scene = SceneManager else { return }

        let r: Float = 2.0
        let x = cos(time) * r
        let z = sin(time) * r

        // ワールドを動かす（＝カメラが周回して見える）
        scene.position = SIMD3<Float>(-x, 0, -z)
    }
    
    // Debug: Dumps the entire scene's hierarchy
    private func dumpEntityTree(
        _ entity: Entity,
        indent: String = "",
        isLast: Bool = true
    ) {
        let marker = isLast ? "└─" : "├─"
        let name = entity.name.isEmpty ? "(unnamed)" : entity.name
        let typeName = String(describing: type(of: entity))

        print("\(indent)\(marker) \(name) [\(typeName)]")

        let nextIndent = indent + (isLast ? "   " : "│  ")
        let children = entity.children

        for i in 0..<children.count {
            dumpEntityTree(
                children[i],
                indent: nextIndent,
                isLast: i == children.count - 1
            )
        }
    }
}

