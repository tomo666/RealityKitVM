//
//  GameObject.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/04.
//

import RealityKit
import simd

public final class GameObject {
    public var isEnabled: Bool = true {
        didSet { entity.isEnabled = isEnabled }
    }
    public var name: String {
        didSet { entity.name = name }
    }

    // RealityKit実体
    internal let entity: Entity

    // Unity互換: RectTransform.sizeDelta 相当
    // UI用途の論理サイズ（ワールドスケールとは独立）
    public var localSize: Vector2 = Vector2(0, 0)

    // Unity互換Transform
    public lazy var transform: Transform = Transform(owner: self)

    public init(_ name: String) {
        self.name = name
        self.entity = Entity()
        self.entity.name = name
    }

    /// Unity互換: 親からこのオブジェクトを削除する
    public func Destroy() {
        entity.removeFromParent()
    }

    /// Unity互換: 他のGameObjectのlocalSizeをコピーする
    /// 主に Controller → ThisObject へのサイズ同期用途
    public func SetSizeFrom(_ other: GameObject) {
        self.localSize = other.localSize
    }
}
