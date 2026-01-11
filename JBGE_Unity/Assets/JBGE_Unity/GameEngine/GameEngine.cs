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
    public GameObject MainGameObject { get; set; }

    // Reference to 2D Game Engine
    //public TopDownGameMain TopDownGameMain;

    // Manages the non-UI components
    public GameObject SceneManager;
    // Manages the actors within the game
    public GameObject ActorManager;
    // Manages the map within the game
    public GameObject MapManager;

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

    // Reference to the Main Camera object
    public Camera MainCamera;
    // Reference to the UICamera camera object
    public Camera UICamera;
    // The Pixel Per Unit scale to lift up UI scaling
    public float PPUScaleUpUI = 4f;
		// The Pixel Per Unit scale to lift up 2D Map scaling (Actually it's the ortho size so not affecting the actual object scales)
		public float PPUScaleUpWorld = 5f;

    /*
		// Manages all textmeshpro text used in game
		public List<TextBlockManager> TextBlockManagerList = new List<TextBlockManager>();
    // TextBlockManager: current list index to operate on
    public int CurrentTextBlockManagerIndex { get; set; } = 0;
    // TextBlock: current list index to operate on
    public int CurrentTextBlockIndex { get; set; } = 0;
    */

    // TextBlockManager: current list index to operate on
    public int CurrentBMPTextManagerIndex { get; set; } = 0;
    // TextBlock: current list index to operate on
    public int CurrentBMPTextIndex { get; set; } = 0;

    // The one and only Base UI Layer that always sits in front of the UICamera
    public UIComponent UIBaseLayer;
    // UI layer that manages actors (or sprites)
    public UIComponent UIBackgroundLayer;

		// Stores all the UILayer(s) that is placed on the UIBaseLayer UICanvas
		public Dictionary<int, UIComponent> UILayers = new Dictionary<int, UIComponent>();
    // Stores all the 2D textures used in this game
    public Dictionary<int, Texture> Textures = new Dictionary<int, Texture>();
    // Stores all the Image object used in this game
    public Dictionary<int, Image> Images = new Dictionary<int, Image>();
    // Stores all the Font object used in this game
    public Dictionary<int, Font> Fonts = new Dictionary<int, Font>();

    /*
    // Stores all the BMPText object used in this game
    public Dictionary<int, BMPText> Texts = new Dictionary<int, BMPText>();
    // Stores all the Actor2D object used in this game
    public Dictionary<int, Actor2D> Actor2Ds = new Dictionary<int, Actor2D>();
    // Stores all the Map2D object used in this game
    public Dictionary<int, Map2D> Map2Ds = new Dictionary<int, Map2D>();
    */

    // ImageManager: current list index to operate on
    public int CurrentImageManagerIndex { get; set; } = 0;
    // Image: current list index to operate on
    public int CurrentImageIndex { get; set; } = 0;

    // Image direction to search in the atlas for creating animations that are made up of multiple sprites
    public enum SheetOrientation { Horizontal, Vertical }
    // General horizontal alignment definitions
    public enum HorizontalAlignment { Left, Center, Right }
    // General vertical alignment definitions
    public enum VerticalAlignment { Top, Middle, Bottom }
    // General orientation definitions
    public enum Orientation { Horizontal, Vertical }
    // General speed identifiers
    public enum MovementSpeed { VerySlow, Slow, Normal, Fast, VeryFast }

    // User gamepad device inputs
		public IDGamePad IDGamePad = new IDGamePad();
    // User mouse device inputs
    public IDMouse IDMouse = new IDMouse();
    // User keyboard device inputs
    public IDKeyboard IDKeyboard = new IDKeyboard();
    // Manages all user inputs
    public IDUserInput UserInput;

    // The global pixel per unit applied to non-UI 2D graphic elements
    public int GlobalPixelPerUnit = 32;

    public GameEngine(GameObject gameObject) {
      this.MainGameObject = gameObject;
      SceneManager = new GameObject("SceneManager");
      ActorManager = new GameObject("ActorManager");
      MapManager = new GameObject("MapManager");

      ActorManager.transform.SetParent(SceneManager.transform);
      MapManager.transform.SetParent(SceneManager.transform);

			// We need to call this in order to check for Addressable asset existence
			Addressables.InitializeAsync();

			// Initialize UI
			InitializeUI();
		}

    public void Update() {

      UserInput.Update();

      /*
      foreach(KeyValuePair<int, Actor2D> kvp in Actor2Ds) {
        kvp.Value.Update();
      }
      foreach(KeyValuePair<int, Map2D> kvp in Map2Ds) {
        kvp.Value.Update();
      }
      foreach(KeyValuePair<int, Image> kvp in Images) {
        kvp.Value.Update();
      }
      foreach(KeyValuePair<int, BMPText> kvp in Texts) {
        kvp.Value.Update();
      }*/

      // Get user input states
      if(Gamepad.current != null) {
        IDGamePad.IsNorthPressed = Gamepad.current.buttonNorth.isPressed;
        IDGamePad.IsSouthPressed = Gamepad.current.buttonSouth.isPressed;
        IDGamePad.IsWestPressed = Gamepad.current.buttonWest.isPressed;
        IDGamePad.IsEastPressed = Gamepad.current.buttonEast.isPressed;
        IDGamePad.LeftStickValue = Gamepad.current.leftStick.ReadValue();
        IDGamePad.RightStickValue = Gamepad.current.rightStick.ReadValue();
        IDGamePad.IsLeftShoulderPressed = Gamepad.current.leftShoulder.ReadValue() == 1 ? true : false;
        IDGamePad.IsRightShoulderPressed = Gamepad.current.rightShoulder.ReadValue() == 1 ? true : false;
        IDGamePad.LeftTriggerValue = Gamepad.current.leftTrigger.ReadValue();
        IDGamePad.RightTriggerValue = Gamepad.current.rightTrigger.ReadValue();
        IDGamePad.DPadValue = Gamepad.current.dpad.ReadValue();
        IDGamePad.IsLeftStickPressed = Gamepad.current.leftStickButton.IsPressed();
        IDGamePad.IsRightStickPressed = Gamepad.current.rightStickButton.IsPressed();
        IDGamePad.IsSelectPressed = Gamepad.current.selectButton.IsPressed();
        IDGamePad.IsStartPressed = Gamepad.current.startButton.IsPressed();
      }

      if(Mouse.current != null) {
        IDMouse.IsMouseLeftPressed = Mouse.current.leftButton.isPressed;
        IDMouse.IsMouseRightPressed = Mouse.current.rightButton.isPressed;
        IDMouse.IsMouseMiddlePressed = Mouse.current.middleButton.isPressed;
        IDMouse.IsMouseForwardPressed = Mouse.current.forwardButton.isPressed;
        IDMouse.IsMouseBackPressed = Mouse.current.backButton.isPressed;
        IDMouse.ScrollAmount = (int)Mouse.current.scroll.y.ReadValue();
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        IDMouse.MousePosition = new Vector2((float)Math.Round(mousePosition.x), (float)Math.Round(Screen.height - mousePosition.y));
      }

      // B Key to toggle debug info display
      if(IDKeyboard.IsKeyPressed((int)KeyCode.B) && IsWaiting == false) {
        IsShowDebugInfo = !IsShowDebugInfo;
        WaitFrameCount = 10;
        IsWaiting = true;
      }

      // Show input status
      if(IsShowDebugInfo) {
        string strInputStates = "GAMEPAD STATES\n";
        strInputStates += "BTN NORTH [△]: " + IDGamePad.IsNorthPressed + "\n";
        strInputStates += "BTN EAST [〇]: " + IDGamePad.IsEastPressed + "\n";
        strInputStates += "BTN SOUTH [×]: " + IDGamePad.IsSouthPressed + "\n";
        strInputStates += "BTN WEST [□]: " + IDGamePad.IsWestPressed + "\n";
        strInputStates += "BTN LEFT1 [L1]: " + IDGamePad.IsLeftShoulderPressed + "\n";
        strInputStates += "BTN RIGHT1 [R1]: " + IDGamePad.IsRightShoulderPressed + "\n";
        strInputStates += "BTN Trigger Left [L2]: " + IDGamePad.IsLeftTriggerPressed + "\n";
        strInputStates += "BTN Trigger Right [R2]: " + IDGamePad.IsRightTriggerPressed + "\n";
        strInputStates += "BTN LEFT STICK [L3]: " + IDGamePad.IsLeftStickPressed + "\n";
        strInputStates += "BTN RIGHT STICK [R3]: " + IDGamePad.IsRightStickPressed + "\n";
        strInputStates += "BTN START [Start]: " + IDGamePad.IsStartPressed + "\n";
        strInputStates += "BTN Select [Select]: " + IDGamePad.IsSelectPressed + "\n";
        strInputStates += "DPAD Value (X,Y):" + IDGamePad.DPadValue + "\n";
        strInputStates += "DPAD [↑]: " + IDGamePad.IsDPadNorthPressed + "\n";
        strInputStates += "DPAD [↑→]: " + IDGamePad.IsDPadNorthEastPressed + "\n";
        strInputStates += "DPAD [→]: " + IDGamePad.IsDPadEastPressed + "\n";
        strInputStates += "DPAD [→↓]: " + IDGamePad.IsDPadSouthEastPressed + "\n";
        strInputStates += "DPAD [↓]: " + IDGamePad.IsDPadSouthPressed + "\n";
        strInputStates += "DPAD [↓←]: " + IDGamePad.IsDPadSouthWestPressed + "\n";
        strInputStates += "DPAD [←]: " + IDGamePad.IsDPadWestPressed + "\n";
        strInputStates += "DPAD [←↑]: " + IDGamePad.IsDPadNorthWestPressed + "\n";
        strInputStates += "Left Stick Value (X,Y): " + IDGamePad.LeftStickValue + "\n";
        strInputStates += "Right Stick Value (X,Y): " + IDGamePad.RightStickValue + "\n";
        strInputStates += "Left Trigger Value: " + IDGamePad.LeftTriggerValue + "\n";
        strInputStates += "Right Trigger Value: " + IDGamePad.RightTriggerValue + "\n";
        strInputStates += "--------------------------\n";
        strInputStates += "MOUSE STATES\n";
        strInputStates += "BTN LEFT PRESSED: " + IDMouse.IsMouseLeftPressed + "\n";
        strInputStates += "BTN RIGHT PRESSED: " + IDMouse.IsMouseRightPressed + "\n";
        strInputStates += "BTN MIDDLE PRESSED: " + IDMouse.IsMouseMiddlePressed + "\n";
        strInputStates += "BTN FORWARD PRESSED: " + IDMouse.IsMouseForwardPressed + "\n";
        strInputStates += "BTN BACK PRESSED: " + IDMouse.IsMouseBackPressed + "\n";
        strInputStates += "V-SCROLL: " + IDMouse.ScrollAmount + "\n";
        strInputStates += "V-SCROLL UP: " + IDMouse.IsScrolledUp + "\n";
        strInputStates += "V-SCROLL DOWN: " + IDMouse.IsScrolledDown + "\n";
        strInputStates += "POSITION: " + IDMouse.MousePosition + "\n";
        strInputStates += "--------------------------\n";
        strInputStates += "KEYBOARD STATES\n";
        strInputStates += "KEY PRESSED: " + IDKeyboard.IsAnyKeyPressed + "\n";
        strInputStates += "KEY CODE: " + IDKeyboard.PressedKeyCode + "\n";

        //TextMeshProUGUI textInput = DebugDeviceInputCanvasObj.GetComponent<TextMeshProUGUI>();
        //textInput.SetText(strInputStates);
      }
    }

    private void InitializeUI() {
			// Create user input object
			UserInput = new IDUserInput(this);

			// Get the reference to the Cameras
			MainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
			
			// Set UI camera properties
			MainCamera.orthographic = false;
      MainCamera.farClipPlane = 1000;
      MainCamera.nearClipPlane = 0.01f;
			MainCamera.usePhysicalProperties = false;
			MainCamera.fieldOfView = 60f;

			// Create the one and utmost base layer that attaches to the UICamera so we can place UILayer(s) and its child UI components accordingly
			UIBaseLayer = new UIComponent(this, "UIBaseLayer", null, true);
			// We need to set the pivot and position of this layer to center of our UICarmera so the sub layers will be contained correctly when created
			UIBaseLayer.ResetTransform();
			UIBaseLayer.TransformAll(new Vector3(0.5f, 0.5f, 1.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1f, 1f, 1f), new Vector3(0, 0, 0),
	new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f));
			UIBaseLayer.IsVisible = true; 

			// Create UI Layers
			int UIBackgroundLayerID = CreateUILayer("UIBackgroundLayer");
      UIBackgroundLayer = UILayers[UIBackgroundLayerID];
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
      // We need to set the pivot and position of this layer to center of our UICarmera so the sub layers will be contained correctly when created
			layer.ResetTransform();
      /*layer.SetPivot(0.5f, 0.5f);
			layer.SetScale(1.0f, 1.0f, 1.0f);
			layer.SetPivot(0.5f, 0.5f);
			layer.SetRotation(0, 0, 0);
			layer.SetPivot(0.5f, 0.5f);
			layer.SetPosition(0.5f, 0.5f, 0.0f);*/
      /*
      layer.TransformScale(new Vector3(1.0f, 1.0f, 1.0f), new Vector2(0.5f, 0.5f));
			layer.TransformRotation(new Vector3(0, 0, 0), new Vector2(0.5f, 0.5f));
			layer.TransformPosition(new Vector3(0.5f, 0.5f, 0.0f), new Vector2(0.5f, 0.5f));

			layer.LocalWidth *= 2;
      layer.LocalHeight *= 2;
      */

			var controllerRectTransform = layer.Controller.GetComponent<RectTransform>();
      layer.ThisObject.GetComponent<RectTransform>().sizeDelta = controllerRectTransform.sizeDelta;

			layer.IsVisible = true;

			// Sort order for each custom UI layer is incremented by 1000, where the UI BaseLayer is 0
			layer.SortOrder = (UILayers.Count + 1) * 1000;
      // Check if same ID already exists in our list
      while(UILayers.ContainsKey(layer.ID)) {
        // Re-generate a random ID for this object
        var rand = new System.Random();
        layer.ID = rand.Next();
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

    /*
		/// <summary>Creates a new Actor2D object and instantiate the Actor</summary>
		/// <param name="mapFile">Tiled map file (.tmx)</param>
		/// <returns>ID is generated that can be used to identify the newly created object (if creation fails, returns -1)</returns>
		//public int CreateActor2D(int imageID, string componentName = "Actor2D") {
		public int CreateActor2D(string mapFile, bool isUI = false) {
      Actor2D actor2D = new Actor2D(this, mapFile);
      actor2D.Create(isUI);

      // Check if same ID already exists in our list
      while(Actor2Ds.ContainsKey(actor2D.ID)) {
        // Re-generate a random ID for this object
        var rand = new System.Random();
        actor2D.ID = rand.Next();
      }
      // Add our Actor2D object into the list
      Actor2Ds.Add(actor2D.ID, actor2D);
      return actor2D.ID;
    }

		/// <summary>Destroys the Actor2D object from our list</summary>
		/// <param name="id">The handle ID of the object to be deleted</param>
		public void DestroyActor2D(int id) {
			if(!Actor2Ds.ContainsKey(id)) return;
			Actor2Ds[id].Destroy();
			Actor2Ds[id] = null;
			Actor2Ds.Remove(id);
		}
    */

    /*
		/// <summary>Creates a new Map2D object and instantiate the Map</summary>
		/// <param name="mapFile">Tiled map file (.tmx)</param>
		/// <param name="layerID">The parent UIComponent identifier (which is an UILayer identifier) this object will be attached to</param>
		/// <returns>ID is generated that can be used to identify the newly created object (if creation fails, returns -1)</returns> 
		public int CreateMap2D(string mapFile, bool isUI = false) {
      Map2D map2D = new Map2D(this, mapFile);
      map2D.Create(isUI);

      // Check if same ID already exists in our list
      while(Map2Ds.ContainsKey(map2D.ID)) {
        // Re-generate a random ID for this object
        var rand = new System.Random();
        map2D.ID = rand.Next();
      }
      // Add our Actor2D object into the list
      Map2Ds.Add(map2D.ID, map2D);
      return map2D.ID;
    }

    /// <summary>Destroys the Map2D object from our list</summary>
    /// <param name="id">The handle ID of the object to be deleted</param>
    public void DestroyMap2D(int id) {
      if(!Map2Ds.ContainsKey(id)) return;
      Map2Ds[id].Destroy();
      Map2Ds[id] = null;
      Map2Ds.Remove(id);
    }

    /// <summary>Creates a new Bitmap Text object</summary>
    /// <param name="text">The actual text to generate</param>
    /// <param name="fontID">The font identifier to be used for this text object</param>
    /// <param name="layerID">The parent UIComponent identifier (which is an UILayer identifier) this text object will be attached to</param>
    /// <param name="horizontalAlignment">Horizontal alignemnt (0=Left, 1=Center, 2=Right)</param>
    /// <param name="lineHeight">Line-height amount of each line of text</param>
    /// <param name="charSpacing">Character spacing amount between each character</param>
    /// <returns>ID is generated that can be used to identify the newly created object (if creation fails, returns -1)</returns>
    public int CreateText(string text, int fontID, int layerID, GameEngine.HorizontalAlignment horizontalAlignment = GameEngine.HorizontalAlignment.Left,
      int lineHeight = 0, int charSpacing = 0, int textBlockPixelWidth = -1, int textBlockPixelHeight = -1) {
      BMPText bmpText = new BMPText(this, fontID, layerID);
      bmpText.Create(text, horizontalAlignment, lineHeight, charSpacing, false, textBlockPixelWidth, textBlockPixelHeight);
      // Check if same ID already exists in our list
      while(Texts.ContainsKey(bmpText.ID)) {
        // Re-generate a random ID for this object
        var rand = new System.Random();
        bmpText.ID = rand.Next();
      }
      // Add our BMPText object into the list
      Texts.Add(bmpText.ID, bmpText);
      return bmpText.ID;
    }

    /// <summary>Destroys the BMPText from our list</summary>
    /// <param name="id">The handle ID of the object to be deleted</param>
    public void DestroyText(int id) {
      if(!Texts.ContainsKey(id)) return;
      Texts[id].Destroy();
      Texts[id] = null;
      Texts.Remove(id);
    }

    /// <summary>Creates a new Image object and instantiate a sprite</summary>
    /// <param name="textureID">The texture ID of the Texture2D object to be used as base texxture</param>
    /// <param name="layerID">The parent UIComponent (which is an UILayer identifier) this text object will be attached to</param>
    /// <param name="rX">Rect position X of texture to use</param>
    /// <param name="rY">Rect position Y of texture to use</param>
    /// <param name="rW">Rect width to use in texture: Set to -1 if using the full texture length</param>
    /// <param name="rH">Rect height to use in texture: Set to -1 if using the full texture length</param>
    /// <param name="maxSheets">for animation purposes, users can specify how many number of spirte sheets to be used of the same width and height, starting from the original position specified with x and y</param>
    /// <param name="sheetOrientation">Horizontal(0) = Sheet animation is read horizontally, Vertical(1) = Sheet animation is read vertically</param>
    /// <returns>ID is generated that can be used to identify the newly created object (if creation fails, returns -1)</returns>
    public int CreateImage(int textureID, int layerID, int rX, int rY, int rW, int rH, int maxSheets = 1, SheetOrientation sheetOrientation = SheetOrientation.Horizontal, float pixelPerUnit = -1) {
      if(!Textures.ContainsKey(textureID)) return -1;
      UIComponent parentObj = (layerID == -1 ? null : UILayers[layerID]);
      // Create image object
      Image image = new Image(this, textureID, "Image", layerID, parentObj, true);
      image.Create(rX, rY, rW, rH, maxSheets, (SheetOrientation)sheetOrientation, null, pixelPerUnit);
      // Check if same ID already exists in our list
      while(Images.ContainsKey(image.ID)) {
        // Re-generate a random ID for this object
        var rand = new System.Random();
        image.ID = rand.Next();
      }
      // Add our image into the image list
      Images.Add(image.ID, image);
      return image.ID;
    }

    /// <summary>Creates a new Image object but don't instantiate a sprite</summary>
    /// <param name="textureID">The texture ID of the Texture2D object to be used as base texxture</param>
    /// <param name="parentObj">The parent UIComponent this text object will be attached to</param>
    /// <param name="isControllerRequired">Whether a transformation controller is required for this image</param>
    /// <returns>ID is generated that can be used to identify the newly created object (if creation fails, returns -1)</returns>
    public int CreateImageWithoutSprite(int textureID, UIComponent parentObj, bool isControllerRequired = true) {
      if(!Textures.ContainsKey(textureID)) return -1;
      // Create image object
      Image image = new Image(this, textureID, "Image", -1, parentObj, isControllerRequired);
      // Check if same ID already exists in our list
      while(Images.ContainsKey(image.ID)) {
        // Re-generate a random ID for this object
        var rand = new System.Random();
        image.ID = rand.Next();
      }
      // Add our image into the image list
      Images.Add(image.ID, image);
      return image.ID;
    }

    /// <summary>Destroys the image from our list</summary>
    /// <param name="id">The handle ID of the object to be deleted</param>
    public void DestroyImage(int id) {
      if(!Images.ContainsKey(id)) return;
      Images[id].Destroy();
      Images[id] = null;
      Images.Remove(id);
    }
    */
    /// <summary>Creates texture from local file or from addressables (NOTES: if file exists in addressables, then it will be loaded as priority)</summary>
    /// <param name="textureFileName">Relative path to the texture file (e.g. Assets/Images/myimage.png)</param>
    /// <returns>ID is generated that can be used to identify the newly created object (if creation fails, returns -1)</returns>
    public int CreateTexture(string textureFileName) {
      Texture texture = new Texture(this);
      int textureID = texture.Create(textureFileName);
      if(textureID == -1) return -1;
      Textures.Add(textureID, texture);
      return textureID;
    }

    /// <summary>Destroys the texture from our list</summary>
    /// <param name="id">The handle ID of the object to be deleted</param>
    public void DestroyTexture(int id) {
      if(!Textures.ContainsKey(id)) return;
      Textures[id].Destroy();
      Textures[id] = null;
      Textures.Remove(id);
		}
    /*
    /// <summary>Creates a new Font object</summary>
    /// <param name="fontPath">The path to the .fnt file</param>
    /// <returns>ID is generated that can be used to identify the newly created object (if creation fails, returns -1)</returns>
    public int CreateFont(string fontPath) {
      Font font = new Font(this);
      int fontID = font.Create(fontPath);
      Fonts.Add(fontID, font);
      return fontID;
    }

    /// <summary>Destroys the Font from our list</summary>
    /// <param name="id">The handle ID of the object to be deleted</param>
    public void DestroyFont(int id) {
      if(!Fonts.ContainsKey(id)) return;
      Fonts[id].Destroy();
      Fonts[id] = null;
      Fonts.Remove(id);
    }
    */

    /*
    /// <summary>Creates a mesh that can act as a placeholder game object</summary>
    /// <param name="objectName">Object name to be shown in the hierarchy</param>
		/// <param name="layerID">The Layer in which this collider mesh should be included</param>
		/// <param name="polygonPoints">Points that make up the polygon - this is the value used in the "polygon" element's "points" attribute value use in the TMX file of Tiled map editor</param>
    /// <returns></returns>
    public GameObject CreateMeshCollider(string objectName, int layerID, Vector3[] polygonPoints, float scale = 0.0313f) {
      GameObject go = new GameObject(objectName);
      go.layer = layerID; // 5 is "UI" Layer
      RectTransform rect = go.AddComponent<RectTransform>();
      MeshCollider mc = go.AddComponent(typeof(MeshCollider)) as MeshCollider;

      Mesh m = PathToMeshBuilder.GenerateMesh(polygonPoints);
      m.name = "Mesh - " + objectName;

      mc.sharedMesh = m;

      // Rotate by 180 degrees because currently it is upside-down
      UICamera.orthographic = true;
      go.transform.rotation = Quaternion.Euler(new Vector3(-180, 0, 0));
      UICamera.orthographic = false;
      go.transform.localScale = new Vector3(scale, scale, scale);
      //go.transform.localScale = new Vector3(0.102f, 0.102f, 1f);

      return go;
    }

		public GameObject CreateCapsuleCollider(string objectName, int layerID, float width, float height, float scale = 0.0313f) {
			GameObject go = new GameObject(objectName);
			go.layer = layerID; // 5 is "UI" Layer
			RectTransform rect = go.AddComponent<RectTransform>();
			CapsuleCollider cc = go.AddComponent(typeof(CapsuleCollider)) as CapsuleCollider;
      if(width >= height) {
        cc.direction = 0;
        cc.radius = height / 2;
        cc.height = width;
      } else {
				cc.direction = 1;
        cc.radius = width / 2;
				cc.height = height;
			}

			cc.center = new Vector3(width / 2, -height / 2);

			go.transform.localScale = new Vector3(scale, scale, scale);

			return go;
		}
    */
	}
}