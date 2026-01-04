//
//  ContentView.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/03.
//

import SwiftUI
import RealityKit

struct ContentView: View {

    @State private var scene: RealityKitScene? = nil
    private var gameObject: GameObject = GameObject("GameMain")
    
    var body: some View {
        RealityView { content in
            // 初回のみ
            if scene == nil {
                let s = RealityKitScene()
                scene = s
                scene?.rootAnchor.addChild(gameObject.entity)

                content.add(s.rootAnchor)

                // Unity: Start equivalent
                s.gameMain.start(
                    scene: s,
                    gameObject: gameObject
                )
            }
        } update: { _ in
            // Unity: Update equivalent
            scene?.gameMain.Update()
        }
    }
}

#Preview {
    ContentView()
}
