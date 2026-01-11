//
//  GameMain.swift
//  JBGE_RealityKit
//
//  Created by Tomohiro Kadono on 2026/01/03.
//

import RealityKit

open class GameMain {
    private let binPath = "Assets/Bin/"

    // Virtual Machine to run our game script
    private(set) var gameVM: RealityKitGameVM? = nil
    private(set) var MainScriptFile: String = ""
    public var IsGameInitialized = false
    
    private var TimeToWaitBeforeSceneStart: Int = 2
    private var TimeToWaitCounter: Float = 0.0
    
    open func start(_ gameObject: Entity, _ screenWidth: Float, _ screenHeight: Float) {
        if gameVM == nil {
            gameVM = RealityKitGameVM()
            gameVM?.GE = GameEngine(gameObject, screenWidth, screenHeight)
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
        
        print("[GameMain] Start Completed.")
    }
    
    open func Update(_ deltaTime: Float) {
        if(gameVM?.GE == nil) { return }
        if(!IsGameInitialized) { return }
        
        // TODO: To be implemented later
        
        gameVM?.GE?.Update(deltaTime)
    }
    
    open func FixedUpdate(_ deltaTime: Float) {
        //gameVM?.GE?.FixedUpdate(deltaTime: deltaTime)
    }
    
    open func UpdateScreenSize(_ screenWidth: Float, _ screenHeight: Float) {
        guard let ge = gameVM?.GE else { return }
        ge.UpdateScreenSize(screenWidth, screenHeight)
    }
}
