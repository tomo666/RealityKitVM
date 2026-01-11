//
//  ContentView.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/03.
//

import SwiftUI
import RealityKit
import JBGE_RCP

final class GameLoopDriver {

    private var accumulator: Float = 0
    private let fixedDeltaTime: Float = 1.0 / 60.0

    func tick(gameMain: GameMain, deltaTime: Float) {
        // 可変フレーム更新
        gameMain.Update(deltaTime)

        // FixedUpdate 用に時間を貯める
        accumulator += deltaTime

        let maxSteps = 5
        var steps = 0

        while accumulator >= fixedDeltaTime && steps < maxSteps {
            gameMain.FixedUpdate(fixedDeltaTime)
            accumulator -= fixedDeltaTime
            steps += 1
        }
    }
}

struct ContentView: View {
    private let rootAnchor = AnchorEntity(world: .zero)

    private let loop = GameLoopDriver()
    private var gameMain = GameMain()
    private var gameObject: Entity = Entity()

    var body: some View {
        GeometryReader { geo in
            RealityView { content in
                if let sceneEntity = try? await Entity(named: "Scene", in: JBGE_RCPBundle) {
                    gameObject.name = "GameMain"
                    content.add(sceneEntity)
                    content.add(gameObject)
                    content.add(rootAnchor)
                    rootAnchor.addChild(gameObject)
                    
                    // Remove entities already staged from parent and add it to the gameObject
                    if let cube = sceneEntity.findEntity(named: "CubeTest") {
                        cube.removeFromParent()
                        gameObject.addChild(cube)
                    }
                    
                    // Unity: Start equivalent
                    gameMain.start(gameObject, Float(geo.size.width), Float(geo.size.height))

                    _ = content.subscribe(to: SceneEvents.Update.self) { event in
                        let deltaTime = Float(event.deltaTime)
                        loop.tick(gameMain: gameMain, deltaTime: deltaTime)
                    }
                }
            } update: { _ in
                // Update は SceneEvents.Update に一本化
                if gameMain.IsGameInitialized == false {
                    gameMain.UpdateScreenSize(
                        Float(geo.size.width),
                        Float(geo.size.height)
                    )
                    gameMain.IsGameInitialized = true
                }
            }
            .onChange(of: geo.size) { oldSize, newSize in
                guard oldSize != newSize else { return }
                gameMain.UpdateScreenSize(
                    Float(newSize.width),
                    Float(newSize.height)
                )
            }
        }
        //.frame(minWidth: 1280, maxWidth: .infinity, minHeight: 800, maxHeight: .infinity)
        .frame(minWidth: 960, maxWidth: .infinity, minHeight: 540, maxHeight: .infinity)
        //.frame(minWidth: 640, maxWidth: .infinity, minHeight: 480, maxHeight: .infinity)
        //.frame(minWidth: 1024, maxWidth: .infinity, minHeight: 768, maxHeight: .infinity)
        //.frame(minWidth: 1280, maxWidth: .infinity, minHeight: 720, maxHeight: .infinity)
        //.frame(minWidth: 1920, maxWidth: .infinity, minHeight: 1080, maxHeight: .infinity)
    }
}

#Preview {
    ContentView()
}
