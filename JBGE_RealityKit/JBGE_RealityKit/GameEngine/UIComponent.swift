//
//  UIComponent.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/04.
//

import Foundation
import RealityKit

open class UIComponent {

    public var ID: Int = Int.random(in: 0..<Int.max)
    public unowned let GE: GameEngine

    public let ThisObject: GameObject
    public var Controller: GameObject?
    /// <summary>Sort order of this layer</summary>
    public var SortOrder: Int = 0

    // --- Unity-compat state (Phase 1) ---
    // Unity-style viewport space: x,y are 0..1 where (0,0) is top-left.
    // We convert that into a local 3D coordinate space centered at (0,0).
    public var ScaleWidth: Float = 1.0
    public var ScaleHeight: Float = 1.0
    /// <summary>The scaled width of the UICamera's viewport width</summary>
    public var ScaleScreenWidth: Float = 0.0
    /// <summary>The scaled height of the UICamera's viewport height</summary>
    public var ScaleScreenHeight: Float = 0.0

    public var Pivot: Vector2 = Vector2(0.5, 0.5)
    public var Position: Vector3 = Vector3(0.5, 0.5, 0)
    public var Scale: Vector3 = Vector3(1, 1, 1)
    public var Rotation: Vector3 = Vector3(0, 0, 0) // degrees
    
    /// <summary>Unity-compatible visibility flag</summary>
    public var IsVisible: Bool {
        get {
            let go = Controller ?? ThisObject
            return go.isEnabled
        }
        set {
            let go = Controller ?? ThisObject
            go.isEnabled = newValue
        }
    }

    // --- Unity-compatible RectTransform-like accessors ---

    public var LocalWidth: Float {
        get {
            let go = Controller ?? ThisObject
            return go.localSize.x
        }
        set {
            let go = Controller ?? ThisObject
            go.localSize = Vector2(newValue, go.localSize.y)
        }
    }

    public var LocalHeight: Float {
        get {
            let go = Controller ?? ThisObject
            return go.localSize.y
        }
        set {
            let go = Controller ?? ThisObject
            go.localSize = Vector2(go.localSize.x, newValue)
        }
    }

    public var LocalPositionX: Float {
        get {
            let go = Controller ?? ThisObject
            return go.transform.localPosition.x
        }
        set {
            let go = Controller ?? ThisObject
            let p = go.transform.localPosition
            go.transform.localPosition = Vector3(newValue, p.y, p.z)
        }
    }

    public var LocalPositionY: Float {
        get {
            let go = Controller ?? ThisObject
            return go.transform.localPosition.y
        }
        set {
            let go = Controller ?? ThisObject
            let p = go.transform.localPosition
            go.transform.localPosition = Vector3(p.x, newValue, p.z)
        }
    }

    public var LocalPositionZ: Float {
        get {
            let go = Controller ?? ThisObject
            return go.transform.localPosition.z
        }
        set {
            let go = Controller ?? ThisObject
            let p = go.transform.localPosition
            go.transform.localPosition = Vector3(p.x, p.y, newValue)
        }
    }

    // --- Motion Tweening State ---
    public enum TargetProperty: Int {
        case Position = 0
        case Rotation = 1
        case Scale = 2
    }
    private var motionTweenWaitCount: Int = 0
    private var currentMotionTweenWaitCount: Int = 0

    private var motionDistanceFromPrevFrame: Int = 0
    private var motionTotalFrames: Int = 1

    // [0] = Position, [1] = Rotation, [2] = Scale
    private var motionTweenData: [MotionTweenData?] = [nil, nil, nil]
    
    /// <summary>Determines if we should update on each frames</summary>
    public var IsUpdate: Bool = true

    /// <summary>Stores the accumulated frame count of this object (based on the game's main FPS)</summary>
    public var Frames: Int = 0

    /// <summary>The maximum frame count that this object will accumulate (Once the frame count reaches this max value, frame count will reset to 0)</summary>
    public var MaxFrames: Int = 0
    
    
    public init(
        _ GE: GameEngine,
        _ objectName: String? = nil,
        _ parentObj: UIComponent? = nil,
        _ isControllerRequired: Bool = true,
        _ isCreatePlaneForThisObject: Bool = false
    ) {

        self.GE = GE
        let name = objectName ?? String(describing: type(of: self))
        self.ID = Int.random(in: 0..<Int.max)

        self.ThisObject = GameObject(name)

        // Phase 1 default: treat the UI space as a 2x2 plane centered at origin.
        // You can overwrite these from GameEngine later to match Unity camera/PPU behavior.
        self.ScaleWidth = 2.0
        self.ScaleHeight = 2.0

        // Phase 1 placeholder for Unity camera viewport sizing
        ScaleScreenWidth = ScaleWidth
        ScaleScreenHeight = ScaleHeight

        if isControllerRequired {
            let controller = GameObject("\(name)_Controller")
            self.Controller = controller
            controller.transform.SetParent(parentObj?.ThisObject.transform ?? GE.UIRoot.transform)
            ThisObject.transform.SetParent(controller.transform)
        } else {
            ThisObject.transform.SetParent(parentObj?.ThisObject.transform ?? GE.UIRoot.transform)
        }
    }

    open func Update() {

        if !IsUpdate { return }

        Frames = GE.FrameCount % GE.TargetFrameRate
        if Frames >= MaxFrames {
            Frames = 0
        }

        // --- Motion Tween Execution (Unity-compatible) ---
        if motionTotalFrames > 1 {

            if currentMotionTweenWaitCount > 0 {
                currentMotionTweenWaitCount -= 1
            }

            if currentMotionTweenWaitCount == 0 {

                currentMotionTweenWaitCount = motionTweenWaitCount

                // Order matters: Position → Scale → Rotation
                let order: [TargetProperty] = [.Position, .Scale, .Rotation]

                ResetTransform()

                let t = Float(motionDistanceFromPrevFrame) / Float(motionTotalFrames)

                for prop in order {
                    guard let mtd = motionTweenData[prop.rawValue] else { continue }

                    let curvePoint = BezierPoint(
                        t,
                        mtd.motionBezierPoints[0],
                        mtd.motionBezierPoints[1],
                        mtd.motionBezierPoints[2],
                        mtd.motionBezierPoints[3]
                    )

                    let lerp = curvePoint.y

                    let newX = mtd.originalTransformVector3.x +
                        (mtd.motionTargetVector3.x - mtd.originalTransformVector3.x) * lerp
                    let newY = mtd.originalTransformVector3.y +
                        (mtd.motionTargetVector3.y - mtd.originalTransformVector3.y) * lerp
                    let newZ = mtd.originalTransformVector3.z +
                        (mtd.motionTargetVector3.z - mtd.originalTransformVector3.z) * lerp

                    let newPivotX = mtd.originalPivotVector2.x +
                        (mtd.motionTargetPivot.x - mtd.originalPivotVector2.x) * lerp
                    let newPivotY = mtd.originalPivotVector2.y +
                        (mtd.motionTargetPivot.y - mtd.originalPivotVector2.y) * lerp

                    SetPivot(newPivotX, newPivotY)

                    if prop == .Position {
                        SetPosition(newX, newY, newZ)
                    } else if prop == .Rotation {
                        SetRotation(Int(newX), Int(newY), Int(newZ))
                    } else if prop == .Scale {
                        SetScale(newX, newY, newZ)
                    }
                }

                motionDistanceFromPrevFrame += 1
                if motionDistanceFromPrevFrame >= motionTotalFrames {
                    motionDistanceFromPrevFrame = 0
                }
            }
        }
    }

    /// Unity-compatible: x,y are viewport coords (0..1), (0,0)=top-left.
    /// RealityKit-compatible: local space centered at (0,0), +Y up.
    public func SetPosition(_ x: Float, _ y: Float, _ z: Float = 0) {
        // Store Unity-style values
        Position = Vector3(x, y, z)

        // Convert viewport -> centered local space
        let localX = (x - 0.5) * ScaleWidth
        let localY = (0.5 - y) * ScaleHeight
        let localZ = z

        (Controller ?? ThisObject).transform.localPosition = Vector3(localX, localY, localZ)
    }

    public func SetScale(_ x: Float, _ y: Float, _ z: Float = 1) {
        Scale = Vector3(x, y, z)
        (Controller ?? ThisObject).transform.localScale = Vector3(x, y, z)
    }

    public func SetRotation(_ x: Int, _ y: Int, _ z: Int) {
        let radians = Vector3(
            Float(x) * .pi / 180,
            Float(y) * .pi / 180,
            Float(z) * .pi / 180
        )

        let qx = simd_quatf(angle: radians.x, axis: Vector3(1,0,0).simd)
        let qy = simd_quatf(angle: radians.y, axis: Vector3(0,1,0).simd)
        let qz = simd_quatf(angle: radians.z, axis: Vector3(0,0,1).simd)

        Rotation = Vector3(Float(x), Float(y), Float(z))

        // Unity Quaternion.Euler is effectively Z, X, Y order (ZXY)
        // Quaternion multiplication applies right-to-left.
        (Controller ?? ThisObject).transform.localRotation = qy * qx * qz
    }

    /// Phase 1: store pivot (Phase 2 will apply proper pivot offset via Controller).
    public func SetPivot(_ x: Float, _ y: Float) {
        Pivot = Vector2(x, y)
    }

    /// Unity-like reset (local).
    public func ResetTransform() {
        let target = (Controller ?? ThisObject)
        target.transform.localPosition = Vector3(0, 0, 0)
        target.transform.SetLocalRotation(0, 0, 0)
        target.transform.localScale = Vector3(1, 1, 1)

        Pivot = Vector2(0.5, 0.5)
        Position = Vector3(0.5, 0.5, 0)
        Scale = Vector3(1, 1, 1)
        Rotation = Vector3(0, 0, 0)
    }


    /// <summary>
    /// Sets the time frame to wait until this object applies transformations during motion tween.
    /// By specifiying 0, it will apply motion tween per frame as normally according to the game's main FPS.
    /// By specifying more than 0, the motion tween will not trigger until the frame elapses - so for example:
    /// If you have 60 FPS, and you set the frames to 2, the motion tween will be applied after 2 frames step, which results in this object being applied motion tweens as if it is running in 30 FPS
    /// </summary>
    /// <param name="frames">Number of frames to wait</param>
    public func SetMotionTweenWaitCount(_ frames: Int) {
        motionTweenWaitCount = frames
        currentMotionTweenWaitCount = frames
    }

    /// <summary>Sets the motion path bezier of this object</summary>
    /// <param name="targetProperty">The target property ID: [0] = PositionXYZ, [1] = RotateXYZ, [2] = ScaleXYZ</param>
    /// <param name="totalFrames">The number of frames (moving forward in the timeline) to apply the motion path bezier</param>
    /// <param name="targetX">The destination target position/rotation/scale X-axis value when the object reaches the "totalFrames"</param>
    /// <param name="targetY">The destination target position/rotation/scale Y-axis value when the object reaches the "totalFrames"</param>
    /// <param name="targetZ">The destination target position/rotation/scale Z-axis value when the object reaches the "totalFrames"</param>
    /// <param name="targetPivotX">The destination target pivot X-axis value when the object reaches the "totalFrames"</param>
    /// <param name="targetPivotY">The destination target pivot Y-axis value when the object reaches the "totalFrames"</param>
    /// <param name="P0X">The bezier P0 point X-axis</param>
    /// <param name="P0Y">The bezier P0 point Y-axis</param>
    /// <param name="P1X">The bezier P1 point X-axis</param>
    /// <param name="P1Y">The bezier P1 point Y-axis</param>
    /// <param name="P2X">The bezier P2 point X-axis</param>
    /// <param name="P2Y">The bezier P2 point Y-axis</param>
    /// <param name="P3X">The bezier P3 point X-axis</param>
    /// <param name="P3Y">The bezier P3 point Y-axis</param>
    public func SetMotionPathBezier(
        _ targetProperty: Int,
        _ totalFrames: Int,
        _ targetX: Float,
        _ targetY: Float,
        _ targetZ: Float,
        _ targetPivotX: Float,
        _ targetPivotY: Float,
        _ P0X: Float,
        _ P0Y: Float,
        _ P1X: Float,
        _ P1Y: Float,
        _ P2X: Float,
        _ P2Y: Float,
        _ P3X: Float,
        _ P3Y: Float
    ) {
        motionTotalFrames = totalFrames
        motionDistanceFromPrevFrame = 0

        var currentTransformVector3 = Vector3(0, 0, 0)
        if targetProperty == 0 {
            currentTransformVector3 = Position
        } else if targetProperty == 1 {
            currentTransformVector3 = Rotation
        } else if targetProperty == 2 {
            currentTransformVector3 = Scale
        }

        motionTweenData[targetProperty] = MotionTweenData(
            Vector3(targetX, targetY, targetZ),
            Vector2(targetPivotX, targetPivotY),
            [
                Vector3(P0X, P0Y, 0),
                Vector3(P1X, P1Y, 0),
                Vector3(P2X, P2Y, 0),
                Vector3(P3X, P3Y, 0)
            ],
            Vector3(currentTransformVector3.x, currentTransformVector3.y, currentTransformVector3.z),
            Vector2(Pivot.x, Pivot.y)
        )
    }

    /// <summary>
    /// Gets a point in a Cubic Bezier Curve
    /// (You can visualize the points better by using bezier curve simulators: https://www.desmos.com/calculator/d1ofwre0fr?lang=en)
    /// </summary>
    /// <param name="t">Time from range 0.0 to 1.0</param>
    /// <param name="p0">Point 0</param>
    /// <param name="p1">Point 1</param>
    /// <param name="p2">Point 2</param>
    /// <param name="p3">Point 3</param>
    /// <returns>Point on the bezier curve at a specified time</returns>
    private func BezierPoint(
        _ t: Float,
        _ p0: Vector3,
        _ p1: Vector3,
        _ p2: Vector3,
        _ p3: Vector3
    ) -> Vector3 {
        let u: Float = 1 - t
        let tt: Float = t * t
        let uu: Float = u * u
        let uuu: Float = uu * u
        let ttt: Float = tt * t

        let x =
            p0.x * uuu +
            p1.x * (3 * uu * t) +
            p2.x * (3 * u * tt) +
            p3.x * ttt

        let y =
            p0.y * uuu +
            p1.y * (3 * uu * t) +
            p2.y * (3 * u * tt) +
            p3.y * ttt

        let z =
            p0.z * uuu +
            p1.z * (3 * uu * t) +
            p2.z * (3 * u * tt) +
            p3.z * ttt

        return Vector3(x, y, z)
    }
    
    public func SetName(_ name: String) {
        let go = Controller ?? ThisObject
        go.name = name
    }

    public func GetName() -> String {
        let go = Controller ?? ThisObject
        return go.name
    }

    /// <summary>
    /// Unity-compatible destroy.
    /// Detaches and releases RealityKit entities.
    /// </summary>
    open func Destroy() {
        // Destroy child first (Unity-style safety)
        if let controller = Controller {
            controller.transform.SetParent(nil)
            controller.Destroy()
            Controller = nil
        }

        ThisObject.transform.SetParent(nil)
        ThisObject.Destroy()
        // Note: RealityKit entities are released automatically when no longer referenced
    }

    /// <summary>
    /// Unity-compatible UI plane factory (Phase 1 stub).
    /// </summary>
    open func CreateUIPlane(_ objectName: String) -> GameObject {
        let go = GameObject(objectName)
        return go
    }
}
