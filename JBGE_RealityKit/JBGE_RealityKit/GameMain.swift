//
//  GameMain.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/03.
//

open class GameMain {
    private let binPath = "Assets/Bin/"

    // Virtual Machine to run our game script
    private(set) var gameVM: RealityKitGameVM? = nil
    private(set) var MainScriptFile: String = ""
    private(set) var IsGameInitialized = false
    
    private var TimeToWaitBeforeSceneStart: Int = 2
    private var TimeToWaitCounter: Float = 0.0

    open func start(scene: RealityKitScene, gameObject: GameObject) {
        if gameVM == nil {
            let gameVM = RealityKitGameVM()
            gameVM.GE = GameEngine(gameObject, scene: scene)
        }

        // The very first script file that will be executed
        MainScriptFile = binPath + "Game.bytes"
        // Load script and initialize (script's inititiation routine is called)
        gameVM?.LoadBinFile(MainScriptFile)

        // Set to 60 FPS (as default, it will change dynamically afterwards in the main loop depending on the average FPS)
        //gameVM.GE.TargetFrameRate = 60;
        if(gameVM?.GE?.TargetFrameRate == -1) {
          // Set the target to constant value according to the current monitor display's refresh rate (i.e. 60 or 120)
          //gameVM.GE.TargetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
        }
        IsGameInitialized = true
        
        print("[GameMain] Start Completed.")
    }
    
    open func Update() {
        if(gameVM?.GE == nil) { return }
        if(!IsGameInitialized) { return }
        
        // TODO: To be implemented later
        
        
    }
}
