using System;
using System.Collections;
using JBGE;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;


// REFERENCE:
// Creating UI elements from scripting: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/HOWTO-UICreateFromScripting.html
// Addressable Asset System: https://blog.applibot.co.jp/2020/06/15/introduce-addressable-assets-system/

public class GameMain : MonoBehaviour {
  public const string assetPath = "Assets/";
  public const string binPath = assetPath + "Bin/";
  
  // Virtual Machine to run our game script
  protected UnityGameVM gameVM = null;

  protected string MainScriptFile;
  protected bool IsGameInitialized = false;
  private int TimeToWaitBeforeSceneStart = 2;
	private float TimeToWaitBeforeSceneStartCounter = 0;

	void Awake() {
		int logicalW = 1280;
		int logicalH = 800;
		//int logicalW = 640;
		//int logicalH = 480;
		Screen.SetResolution(logicalW, logicalH, FullScreenMode.Windowed);
	}

	// Start is called before the first frame update
	protected virtual void Start() {
		// If we already have gameVM instantiated, ignore (i.e. the inherited child class may have already instantiated it)
		if(gameVM == null) {
      // Initialize VM and run the initial game script
      gameVM = new UnityGameVM();
      // Instantiate the game engine object
      gameVM.GE = new GameEngine(gameObject);

		}
    // Get the scene name (this is the main script file that always gets loaded and executed)
    //MainScriptFile = binPath + SceneManager.GetActiveScene().name + ".bytes";
    // The very first script file that will be executed
    MainScriptFile = binPath + "Game.bytes";
    // Load script and initialize (script's inititiation routine is called)
    gameVM.LoadBinFile(MainScriptFile);
    // Set to 60 FPS (as default, it will change dynamically afterwards in the main loop depending on the average FPS)
    //gameVM.GE.TargetFrameRate = 60;
    if(gameVM.GE.TargetFrameRate == -1) {
      // Set the target to constant value according to the current monitor display's refresh rate (i.e. 60 or 120)
      gameVM.GE.TargetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
    }

    // Disable vSync
    //QualitySettings.vSyncCount = 0;
    // Make the game run as fast as possible
    Application.targetFrameRate = -1;
    gameVM.GE.TargetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;


		// Set the target frame rate
		//Application.targetFrameRate = gameVM.GE.TargetFrameRate;

		// Debug
		//Application.targetFrameRate = 120; gameVM.GE.TargetFrameRate = 120;

		//FPSToWaitBeforeSceneStart = gameVM.GE.TargetFrameRate - (gameVM.GE.TargetFrameRate / 20);

		//IsGameInitialized = true;
	}

	// Update is called once per frame
	protected virtual void Update() {
    if(gameVM.GE == null) return;
		if(!IsGameInitialized) return;

    // Wait for a while to remove screen updating glitches before fading in
    if(TimeToWaitBeforeSceneStartCounter < TimeToWaitBeforeSceneStart) {
      TimeToWaitBeforeSceneStartCounter += Time.deltaTime;
      return;
		}

		gameVM.GE.AverageFrameRate = (int)(Time.frameCount / Time.time);
    gameVM.GE.FrameCount = Time.frameCount % gameVM.GE.TargetFrameRate;
    //gameVM.GE.TargetFrameRate = gameVM.GE.AverageFrameRate;

    // Run the script's main loop
    if(gameVM.GE.IsWaiting) {
      gameVM.GE.WaitFrameCount--;
      if(gameVM.GE.WaitFrameCount <= 0) {
        gameVM.GE.WaitFrameCount = 0;
        gameVM.GE.IsWaiting = false;
        gameVM.Run(-1, 0);
        //gameVM.GE.Update();
      }
    } else {
      gameVM.Run(-1, 0);
    }
    gameVM.GE.Update();

    //gameVM.PerformGarbageCollection();

    // DEBUG (execute if debug mode is ON)
    ShowDebugInfo();
  }

  // Show debug info
  void ShowDebugInfo() {
    // DEBUG (execute if debug mode is ON)
    if(gameVM.GE.IsShowDebugInfo) {
      /*
      TextMeshProUGUI textFPS = gameVM.GE.DebugPerformanceCanvasObj.GetComponent<TextMeshProUGUI>();

      textFPS.SetText("BUILD 1.30.11.49\n" + "AVG FPS " + gameVM.GE.AverageFrameRate + " / FRAME " + gameVM.GE.FrameCount.ToString() + "\n" +
        "VM ADDR CNT = " + gameVM.VirtualMemory.Count.ToString() + "\n" +
        "VAR ADDR TABLE CNT = " + gameVM.VarAddrTable.Count.ToString() + "\n" +
        "VAR MEM SIZE TABLE CNT = " + gameVM.VarMemSizeTable.Count.ToString() + "\n" +
        "MALLOC ADDR SIZE TABLE CNT = " + gameVM.MallocAddressSizeTable.Count.ToString() + "\n" +
        "STATIC INSTANCE OBJ CNT = " + gameVM.StaticInstanceObjectList.Count.ToString() + "\n" +
        "UI LAYERS = " + gameVM.GE.UILayers.Count.ToString() + "\n" +
        "TEXTURES = " + gameVM.GE.Textures.Count.ToString() + "\n" +
        "FONTS = " + gameVM.GE.Fonts.Count.ToString() + "\n" +
        //"TEXTS = " + gameVM.GE.Texts.Count.ToString() + "\n" +
        "IMAGES = " + gameVM.GE.Images.Count.ToString() + "\n" +
        "FRAME WAIT = " + gameVM.GE.WaitFrameCount.ToString() + "\n" +
        "FRAME ON WAIT = " + gameVM.GE.IsWaiting.ToString() + "\n"
        );
        */
    }
  }
}
