using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.UIElements;
using System.Collections;
using UnityEngine.SceneManagement;

namespace JBGE {
	public class GameEngine {
    // Main GameObject
    public GameObject MainGameObject;

    // Main Perspective Camera
    public Camera MainCamera;

    // Manages the Ui components
    public GameObject UIManager;
    // Manages the non-UI components
    public GameObject SceneManager;

    // Flag to determine if DEBUG info should be shown on screen or not
    public bool IsShowDebugInfo { get; set; } = false;
    // Defines the target frame rate (defaults to 60)
    public int TargetFrameRate { get; set; } = 60;
    // Stores the average frame rate
    public int AverageFrameRate { get; set; }
    // Stores the frame count 0 to 60
    public int FrameCount { get; set; }
    // Amount of frame to wait until executing the next script
    public int WaitFrameCount { get; set; }
    // Determines if we are in wait state or not
    public bool IsWaiting { get; set; }

    // The Pixel Per Unit scale to lift up UI scaling
    public float PPUScaleUpUI = 4f;
		// The Pixel Per Unit scale to lift up 2D Map scaling (Actually it's the ortho size so not affecting the actual object scales)
		public float PPUScaleUpWorld = 5f;
    // Screen width in pixels
    public float ScreenWidth = 0f;
    // Screen height in pixels
    public float ScreenHeight = 0f;
    // If in perspective mode, the horizontal Field Of View Angle (in degrees)
    public float FOV = 60f;

    // The one and only Base UI Layer that always sits in front of the UICamera
    public UIComponent UIBaseLayer;
    // UI layer that manages actors (or sprites)
    public UIComponent UIBackgroundLayer;

		// Stores all the UILayer(s) that is placed on the UIBaseLayer UICanvas
		public Dictionary<int, UIComponent> UILayers = new Dictionary<int, UIComponent>();

    // User gamepad device inputs
		public IDGamePad IDGamePad = new IDGamePad();
    // User mouse device inputs
    public IDMouse IDMouse = new IDMouse();
    // User keyboard device inputs
    public IDKeyboard IDKeyboard = new IDKeyboard();
    // Manages all user inputs
    public IDUserInput UserInput;

    public GameEngine(GameObject gameObject, float screenWidth, float screenHeight) {
      MainGameObject = gameObject;

			// Setup main camera
			MainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
			MainCamera.orthographic = false;
      MainCamera.farClipPlane = 1000;
      MainCamera.nearClipPlane = 0.01f;
			MainCamera.usePhysicalProperties = false;
			MainCamera.fieldOfView = FOV;

      SceneManager = new GameObject("SceneManager");
      SceneManager.transform.SetParent(MainGameObject.transform);
      UIManager = new GameObject("UIManager");
      UIManager.transform.SetParent(MainGameObject.transform);

			// We need to call this in order to check for Addressable asset existence
			Addressables.InitializeAsync();

      UpdateScreenSize(screenWidth, screenHeight);

			// Initialize UI
			InitializeUI();
		}

    private void InitializeUI() {
			// Create user input object
			UserInput = new IDUserInput(this);

			// Create the one and utmost base layer that attaches to the UICamera so we can place UILayer(s) and its child UI components accordingly
			UIBaseLayer = new UIComponent(this, "UIBaseLayer", null, true);
      // Add UIBaseLayer to UIManager as root layer
      UIBaseLayer.ThisObject.transform.SetParent(UIManager.transform);

      // Test
      /*
      UIBaseLayer.TransformAll(
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.5f, 0.5f, 1.0f),
            new Vector3(0, 0, 45),
            new Vector2(0.0f, 0.0f),
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 1.0f));*/

			// Create UI Layers
			//int UIBackgroundLayerID = CreateUILayer("UIBackgroundLayer");
      //UIBackgroundLayer = UILayers[UIBackgroundLayerID];
		}

    // Called when screen size changes
    public void UpdateScreenSize(float width, float height) {
        if(height <= 0) return;
        ScreenWidth = width;
        ScreenHeight = height;
        Debug.Log($"[GameEngine] Screen Size Changed: {width} x {height} --> aspect: {width / height}");
    }

    public void Update() {
      UserInput.Update();
    }

		/// <summary>
		/// Check if asset exists in addressable or not
		/// IMPORTANT: Before this method can work, we need to call: Addressables.InitializeAsync();
		/// at the very start of app initialization in order to check for Addressable asset existence
		/// </summary>
		/// <param name="key">The Addressable key path to check for existence</param>
		public static bool IsAddressableAssetExists(object key) {
      if(Application.isPlaying) {
        foreach(var l in Addressables.ResourceLocators) {
          IList<IResourceLocation> locs;
          if(l.Locate(key, null, out locs)) return true;
        }
        return false;
      }
      return false;
    }

    /// <summary>Load asset from addressables</summary>
    /// <typeparam name="T">The asset type to load</typeparam>
    /// <param name="address">The address path of the asset</param>
    /// <returns>Loaded asset as object</returns>
    public object LoadAddressableAsset<T>(string address) {
      // If asset does not exist in the addressables, return null
      if(!IsAddressableAssetExists(address)) return null;
      // For Unity, if asset is PNG, we want to use the ".bytes" extension (unless, it converts to Texture2D which we don't want to)
      if(Path.GetExtension(address) == ".png") {
        address = Path.ChangeExtension(address, ".bytes");
      }
      try {
        var handle = Addressables.LoadAssetAsync<T>(address);
        var asset = handle.WaitForCompletion();
        Addressables.Release(handle);
        return asset;
      } catch {
        Debug.Log("Failed to load asset from Addressables: " + address);
			}
      return null;
    }


    /// <summary>Creates a new Layer under the base layer</summary>
    /// <returns>ID is generated that can be used to identify the newly created object (if creation fails, returns -1)</returns>
    public int CreateUILayer(string layerName = "Layer") {
      UIComponent layer = new UIComponent(this, layerName, UIBaseLayer);
			// Sort order for each custom UI layer is incremented by 1000, where the UI BaseLayer is 0
			layer.SortOrder = (UILayers.Count + 1) * 1000;
      // Check if same ID already exists in our list
      while(UILayers.ContainsKey(layer.ID)) {
        // Re-generate a random ID for this object
        layer.ID = new System.Random().Next();
      }
      UILayers.Add(layer.ID, layer);
      return layer.ID;
    }

    /// <summary>Destroys the UILayer from our list</summary>
    /// <param name="id">The handle ID of the object to be deleted</param>
    public void DestroyUILayer(int id) {
      if(!UILayers.ContainsKey(id)) return;
      UILayers[id].Destroy();
      UILayers[id] = null;
      UILayers.Remove(id);
    }
	}
}