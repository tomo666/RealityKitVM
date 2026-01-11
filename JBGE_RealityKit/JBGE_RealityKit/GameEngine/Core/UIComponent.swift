//
//  UIComponent.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/04.
//

import Foundation
import RealityKit
import AppKit

open class UIComponent {
    // NOTE: RealityKit's effective viewport height does not match the theoretical
    // perspective frustum due to SwiftUI + RealityView layout and safe margins.
    // This value was empirically calibrated to match full visible screen height
    // across common resolutions (640x480 .. 1920x1080).
    public let UIWorldUnitPerLogicalUnit: Float = 0.866

    // Generate a random ID for this object
    public var ID: Int = Int.random(in: 0..<Int.max)
    // Reference to the Game Engine
    public unowned let GE: GameEngine
    // Reference to this component's entity
    public var ThisObject: Entity = Entity()
    // Sort order of this layer
    public var SortOrder: Int = 0
        
    public init(
        _ GE: GameEngine,
        _ objectName: String? = nil,
        _ parentObj: UIComponent? = nil,
        _ isCreatePlaneForThisObject: Bool = false
    ) {
        self.GE = GE
        let name = objectName ?? String("UIComponent")
        // Create an empty UIPlane and set it in the hierarchy, if told to do so
        // Otherwise, just create an empty GameObject
        ThisObject = isCreatePlaneForThisObject ? CreateUIPlane(name, SIMD4<Float>(Float.random(in: 0..<1), Float.random(in: 0..<1), Float.random(in: 0..<1), 0.7)) : Entity()
        ThisObject.name = name
        // If we don't have any parent object specified, then this container will be the root
        ThisObject.setParent(parentObj?.ThisObject)
        // Reset all transform properties
        ResetTransform()
    }
    
    // Resets the transform properties of this object
    public func ResetTransform() {
        ThisObject.position = SIMD3(0, 0, 0)
        ThisObject.scale = SIMD3(1, 1, 1)
        let radians = SIMD3<Float>(0 * .pi / 180, 0 * .pi / 180, 0 * .pi / 180)
        let qx = simd_quatf(angle: radians.x, axis: SIMD3(1, 0, 0))
        let qy = simd_quatf(angle: radians.y, axis: SIMD3(0, 1, 0))
        let qz = simd_quatf(angle: radians.z, axis: SIMD3(0, 0, 1))
        ThisObject.orientation = qz * qx * qy
    }

    // Detaches and releases entities
    open func Destroy() {
        ThisObject.setParent(nil)
        ThisObject.removeFromParent()
        // Note: RealityKit entities are released automatically when no longer referenced
    }
    
    // Called every frame
    open func Update() {
    }
    
    /// <summary>
    /// Unity-compatible UI plane factory (RealityKit implementation).
    /// </summary>
    open func CreateUIPlane(
        _ objectName: String,
        _ bgColor: SIMD4<Float>? = .one
    ) -> Entity {
        var entity = Entity()
        
        // NOTE:
        // RealityKit's effective viewport height does not match the theoretical
        // perspective frustum due to SwiftUI + RealityView layout and safe margins.
        // This value was empirically calibrated to match full visible screen height
        // across common resolutions (640x480 .. 1920x1080).
        let height: Float = UIWorldUnitPerLogicalUnit
        let width: Float = height * (GE.ScreenWidth / GE.ScreenHeight)
        
        let vertices: [SIMD3<Float>] = [
            SIMD3(-width, -height, 0),
            SIMD3( width, -height, 0),
            SIMD3( width,  height, 0),
            SIMD3(-width,  height, 0)
        ]

        let indices: [UInt32] = [
            0, 1, 2,
            0, 2, 3
        ]

        var meshDesc = MeshDescriptor()
        meshDesc.positions = MeshBuffer(vertices)
        meshDesc.primitives = .triangles(indices)

        let mesh = try! MeshResource.generate(from: [meshDesc])
        
        // --- Material (simple unlit color, Unity placeholder equivalent) ---
        var material = UnlitMaterial()
        if let c = bgColor {
            material.color = .init(tint: .init(red: CGFloat(c.x), green: CGFloat(c.y), blue: CGFloat(c.z), alpha: 1.0))
        }

        let model = ModelEntity(mesh: mesh, materials: [material])
        model.name = "\(objectName)_Model"
        
        // Apply alpha if set
        model.components.set(OpacityComponent(opacity: Float(bgColor?.w ?? 1.0)))

        // Attach model under GameObject’s entity
        model.transform = .identity
        entity.addChild(model)

        // --- Debug outline (UI frame) ---
        
        let borderZ: Float = 0.001
        let thickness: Float = 0.02 as Float

        let halfW = width
        let halfH = height
        let t = thickness

        // Outer/Inner Rect
        let outerL = -halfW
        let outerR =  halfW
        let outerB = -halfH
        let outerT =  halfH

        let innerL = outerL + t
        let innerR = outerR - t
        let innerB = outerB + t
        let innerT = outerT - t

        // Polygon x4 Top, Bottom, Left, Right --> quad (2 triangles)
        // Each quad has a,b,c,d --> 4 vertices and triangles (0,1,2)(0,2,3)
        func addQuad(_ verts: inout [SIMD3<Float>],
                     _ indices: inout [UInt32],
                     _ a: SIMD3<Float>, _ b: SIMD3<Float>, _ c: SIMD3<Float>, _ d: SIMD3<Float>) {
            let base = UInt32(verts.count)
            verts.append(contentsOf: [a,b,c,d])
            indices.append(contentsOf: [
                base + 0, base + 1, base + 2,
                base + 0, base + 2, base + 3
            ])
        }

        var borderVerts: [SIMD3<Float>] = []
        var borderIndices: [UInt32] = []

        // Top strip
        addQuad(&borderVerts, &borderIndices,
                SIMD3(outerL, innerT, borderZ),
                SIMD3(outerR, innerT, borderZ),
                SIMD3(outerR, outerT, borderZ),
                SIMD3(outerL, outerT, borderZ))

        // Bottom strip
        addQuad(&borderVerts, &borderIndices,
                SIMD3(outerL, outerB, borderZ),
                SIMD3(outerR, outerB, borderZ),
                SIMD3(outerR, innerB, borderZ),
                SIMD3(outerL, innerB, borderZ))

        // Left strip
        addQuad(&borderVerts, &borderIndices,
                SIMD3(outerL, innerB, borderZ),
                SIMD3(innerL, innerB, borderZ),
                SIMD3(innerL, innerT, borderZ),
                SIMD3(outerL, innerT, borderZ))

        // Right strip
        addQuad(&borderVerts, &borderIndices,
                SIMD3(innerR, innerB, borderZ),
                SIMD3(outerR, innerB, borderZ),
                SIMD3(outerR, innerT, borderZ),
                SIMD3(innerR, innerT, borderZ))

        var borderDesc = MeshDescriptor()
        borderDesc.positions = MeshBuffer(borderVerts)
        borderDesc.primitives = .triangles(borderIndices)

        let borderMesh = try! MeshResource.generate(from: [borderDesc])

        var borderMaterial = UnlitMaterial()
        borderMaterial.color = .init(tint: .white)

        let borderEntity = ModelEntity(mesh: borderMesh, materials: [borderMaterial])
        borderEntity.name = "\(objectName)_Border"
         entity.addChild(borderEntity)
        // ----- Debug end -----

        // Unity RectTransform equivalent defaults
        entity.scale = SIMD3<Float>(width * 2.0, height * 2.0, 1.0)

        return entity
    }
    
    public func TransformAll(_ baseScale: SIMD3<Float>, _ position: SIMD3<Float>, _ scale: SIMD3<Float>, _ rotation: SIMD3<Float>, _ positionPivot: SIMD2<Float>, _ scalePivot: SIMD2<Float>, _ rotationPivot: SIMD2<Float>) {
        
        // Reset all transformations first
        ResetTransform()

        let aspectRatioY: Float = UIWorldUnitPerLogicalUnit
        let aspectRatioX: Float = UIWorldUnitPerLogicalUnit * (GE.ScreenWidth / GE.ScreenHeight)
        
        // Scale to the actual size first (at its center)
        let baseScaleX = baseScale.x //Image.Width / screenWidth;
        let baseScaleY = baseScale.y //Image.Height / screenHeight;
        let baseScaleZ = baseScale.z
        ThisObject.scale = SIMD3(baseScaleX, baseScaleY, baseScaleZ)
        
        // Then, whilst at its center position, move to the desired position
        let offsetX: Float = (position.x * (aspectRatioX * (2.0 as Float)) - aspectRatioX) + (aspectRatioX - positionPivot.x * (aspectRatioX * (2.0 as Float))) * baseScaleX
        let offsetY: Float = (aspectRatioY - position.y * (aspectRatioY * (2.0 as Float))) + ((positionPivot.y * (aspectRatioY * (2.0 as Float))) - aspectRatioY) * baseScaleY
        let offsetZ: Float = position.z * baseScaleZ
        
        ThisObject.position = SIMD3(offsetX, offsetY, offsetZ)

        // Actual size (world units) after baseScale applied
        let baseW: Float = aspectRatioX * baseScale.x * (2.0 as Float)
        let baseH: Float = aspectRatioY * baseScale.y * (2.0 as Float)

        let deltaW = baseW * (scale.x - (1.0 as Float))
        let deltaH = baseH * (scale.y - (1.0 as Float))

        // pivot 0..1 (where top-left is origin: 0,0)
        let pivotOffsetX = -deltaW * (scalePivot.x - (0.5 as Float))
        let pivotOffsetY =  deltaH * (scalePivot.y - (0.5 as Float))

        // Apply desired scale
        ThisObject.scale *= SIMD3(scale.x, scale.y, scale.z)
        // Fix difference amount after scaling
        ThisObject.position += SIMD3(
            pivotOffsetX,
            pivotOffsetY,
            0
        )
        
        // Rotation
        // Calculate final size (after scale)
        let finalW = baseW * scale.x
        let finalH = baseH * scale.y
        // Create rotation pivot inside local space
        let pivotLocal = SIMD3<Float>(
            (rotationPivot.x - (0.5 as Float)) * finalW,
            ((0.5 as Float) - rotationPivot.y) * finalH,
            0
        )
        // Create quaternion
        let rx = rotation.x * .pi / 180
        let ry = rotation.y * .pi / 180
        let rz = rotation.z * .pi / 180
        let q =
            simd_quatf(angle: ry, axis: SIMD3<Float>(0, 1, 0)) *
            simd_quatf(angle: rx, axis: SIMD3<Float>(1, 0, 0)) *
            simd_quatf(angle: rz, axis: SIMD3<Float>(0, 0, 1))
        // Fix difference amount after rotation
        let rotatedPivot = q.act(pivotLocal)
        let delta = rotatedPivot - pivotLocal
        // Apply rotation
        ThisObject.transform.rotation = q
        ThisObject.position -= delta
    }
}

