//
//  RealityKitScene.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/04.
//

import RealityKit

public final class RealityKitScene {
    public let rootAnchor = AnchorEntity(world: .zero)
    public let gameMain = GameMain()
    
    init() {
        
    }
    
    public func AddToScene(_ gameObject: GameObject) {
        rootAnchor.addChild(gameObject.entity)
    }
    
    public func projectFromViewport(
        x: Float,
        y: Float,
        distance: Float,
        fov: Float
    ) -> Vector3 {

        // 仮実装（Z前方に押し出すだけ）
        // 次フェーズで本物に置き換える
        return Vector3(
            (x - 0.5) * 2.0,
            (y - 0.5) * 2.0,
            -distance
        )
    }
}
