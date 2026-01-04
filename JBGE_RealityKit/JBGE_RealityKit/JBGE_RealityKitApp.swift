//
//  JBGE_RealityKitApp.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/03.
//

import SwiftUI

@main
struct JBGE_RealityKitApp: App {
    var body: some Scene {
        WindowGroup {
            ContentView()
                .focusable()
                // phases -> .down, .up, .repeat
                .onKeyPress(phases: [.down, .up]) { press in
                    let char = press.key.character
                    if let keyCode = KeyCode.GetKeyCode(String(char)) {

                        if press.phase == .down {
                            Input.OnKeyDown(keyCode)
                        } else {
                            Input.OnKeyUp(keyCode)
                        }
                    }
                    return .handled
                }
        }
    }
}
