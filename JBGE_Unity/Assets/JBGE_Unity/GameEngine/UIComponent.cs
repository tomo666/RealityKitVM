
using UnityEngine;

namespace JBGE {
	/// <summary>Base class of all UI components</summary>
	public class UIComponent {
		public float UIWorldUnitPerLogicalUnit = 0.866f;

		public int ID;
		/// <summary>Reference to the RQ Game Engine</summary>
		protected GameEngine GE;
		/// <summary>Reference to this object it self</summary>
		public GameObject ThisObject;
		/// <summary>Controller of this object, which is used to set pivots and transformations of this object</summary>
		public GameObject Controller;
		/// <summary>Sort order of this layer</summary>
		public int SortOrder;

		/// <summary>Stores the accumulated frame count of this object (based on the game's main FPS)</summary>
		public int Frames;
		/// <summary>The maximum frame count that this object will accumulate (Once the frame count reaches this max value, frame count will reset to 0)</summary>
		public int MaxFrames;

		public bool IsVisible {
			get { return ThisObject.activeSelf; }
			set { ThisObject.SetActive(value); }
		}
		public string Name {
			get { return ThisObject.name; }
			set { ThisObject.name = value; }
		}

		/// <summary>Constructor</summary>
		/// <param name="GE">Reference to the RQ Game Engine</param>
		/// <param name="objectName">The object name to be shown in the hierarchy</param>
		/// <param name="parentObj">The parent UIComponent object we attach this new UICanvas to</param>
		/// <param name="isCreatePlaneForThisObject">Create an empty plane mesh or not</param>
		public UIComponent(GameEngine GE, string objectName = null, UIComponent parentObj = null, bool isCreatePlaneForThisObject = false) {
			this.GE = GE;
			if(objectName == null) objectName = this.GetType().Name;

			// Generate a random ID for this object
			var rand = new System.Random();
			ID = rand.Next();

			// Create an empty UIPlane and set it in the hierarchy, if told to do so
			if(isCreatePlaneForThisObject) {
				ThisObject = CreateUIPlane(objectName, new Color(0.5f,0.5f,0.5f,0.5f));
			} else {
				// Otherwise, just create an empty GameObject
				ThisObject = new GameObject(objectName);
				ThisObject.layer = 5; // "UI" Layer
			}

			// If we don't have any parent object specified, then this container will be directly the child of the UICamera
			ThisObject.transform.SetParent(parentObj == null ? GE.UICamera.transform : parentObj.ThisObject.transform, false);
		}

		public virtual void Destroy() {
			UnityEngine.Object.Destroy(ThisObject);
			ThisObject = null;
			if(Controller) {
				UnityEngine.Object.Destroy(Controller);
				Controller = null;
			}
		}

		/// <summary>Called every frame</summary>
		public virtual void Update() {
			Frames = Time.frameCount % GE.TargetFrameRate;
			if(Frames >= MaxFrames) Frames = 0;
		}

		/// <summary>Resets the transform properties of this object</summary>
		public void ResetTransform() {
			GameObject go = Controller == null ? ThisObject : Controller;
			// Reset to origin
			var rectTransform = go.GetComponent<RectTransform>();
			rectTransform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
			rectTransform.localRotation = Quaternion.identity;
			rectTransform.localScale = Vector3.one;
		}

		/// <summary>Transform this object in UI local space where transform order is: translation, scale, rotation</summary>
		/// <param name="baseScale">The base scale is this object's pixel {width|height} / screen pixel {width|height}</param>
		/// <param name="position">Position (X,Y,Z) in UI space, starting origin from the top-left as 0,0</param>
		/// <param name="scale">Scale (X,Y,Z) in UI space - note that Z is omitted</param>
		/// <param name="rotation">Rotation (X,Y,Z) in UI space, in degrees</param>
		/// <param name="positionPivot">The pivot for position in UI space specified between 0.0 to 1.0 (top-left is origin: 0,0)</param>
		/// <param name="scalePivot">The pivot for scaling based on this objects current size, specified between 0.0 to 1.0 (top-left of object is origin: 0,0)</param>
		/// <param name="rotationPivot">The pivot for rotation based on this objects current size, specified between 0.0 to 1.0 (top-left of object is origin: 0,0)</param>
		public void TransformAll(
		Vector3 baseScale,
		Vector3 position,
		Vector3 scale,
		Vector3 rotation,
		Vector2 positionPivot,
		Vector2 scalePivot,
		Vector2 rotationPivot
) {
			// --- Reset ---
			Transform t = ThisObject.transform;
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;

			float aspectRatioY = UIWorldUnitPerLogicalUnit;
			float aspectRatioX = UIWorldUnitPerLogicalUnit * GE.UICamera.aspect;

			// --- Base scale (actual object size) ---
			float baseScaleX = baseScale.x;
			float baseScaleY = baseScale.y;
			float baseScaleZ = baseScale.z;
			t.localScale = new Vector3(baseScaleX, baseScaleY, baseScaleZ);

			float offsetX =
					position.x * (aspectRatioX * 2f) - aspectRatioX +
					(0.5f - positionPivot.x) * (aspectRatioX * 2f);

			float offsetY =
					aspectRatioY - position.y * (aspectRatioY * 2f) +
					(positionPivot.y - 0.5f) * (aspectRatioY * 2f);

			float offsetZ = position.z * baseScaleZ;

			t.localPosition = new Vector3(offsetX, offsetY, offsetZ);

			// --- Actual size after base scale ---
			float baseW = aspectRatioX * baseScale.x * 2f;
			float baseH = aspectRatioY * baseScale.y * 2f;

			float deltaW = baseW * (scale.x - 1f);
			float deltaH = baseH * (scale.y - 1f);

			// pivot 0..1 (top-left = 0,0)
			float pivotOffsetX = -deltaW * (scalePivot.x - 0.5f);
			float pivotOffsetY = deltaH * (scalePivot.y - 0.5f);

			// --- Apply scale ---
			t.localScale = Vector3.Scale(t.localScale, scale);
			t.localPosition += new Vector3(pivotOffsetX, pivotOffsetY, 0f);

			// --- Rotation ---
			float finalW = baseW * scale.x;
			float finalH = baseH * scale.y;

			Vector3 pivotLocal = new Vector3(
					(rotationPivot.x - 0.5f) * finalW,
					(0.5f - rotationPivot.y) * finalH,
					0f
			);

			Quaternion q =
					Quaternion.AngleAxis(rotation.y, Vector3.up) *
					Quaternion.AngleAxis(rotation.x, Vector3.right) *
					Quaternion.AngleAxis(rotation.z, Vector3.forward);

			Vector3 rotatedPivot = q * pivotLocal;
			Vector3 delta = rotatedPivot - pivotLocal;

			t.localRotation = q;
			t.localPosition -= delta;
		}
		
		/// <summary>Creates a UI Plane that can act as a placeholder game object</summary>
		/// <param name="objectName">Object name to be shown in the hierarchy</param>
		/// <param name="width">The scaled width of the object</param>
		/// <param name="height"></param>
		/// <param name="bgColor"></param>
		/// <returns></returns>
		protected GameObject CreateUIPlane(string objectName, Color? bgColor = null) {
			GameObject go = new GameObject(objectName);
			go.layer = 5; // "UI" Layer
			RectTransform rect = go.AddComponent<RectTransform>();

			
			MeshFilter mf = go.AddComponent(typeof(MeshFilter)) as MeshFilter;

			if(bgColor != null) {
				MeshRenderer mr = go.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
				mr.material.EnableKeyword("_NORMALMAP");
				mr.material.EnableKeyword("_METALLICGLOSSMAP");
				// Create a blank 2x2 white texture on the fly
				Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
				// Set the pixel values
				texture.SetPixel(0, 0, (Color)bgColor);
				texture.SetPixel(1, 0, (Color)bgColor);
				texture.SetPixel(0, 1, (Color)bgColor);
				texture.SetPixel(1, 1, (Color)bgColor);
				texture.Apply();
				mr.material.SetTexture("_MainTex", texture);
			}

			var height = UIWorldUnitPerLogicalUnit;
			var width = height * GE.UICamera.aspect;

			Mesh m = new Mesh();
			m.vertices = new Vector3[] {
				new Vector3(-width, -height, 0),
				new Vector3(width, -height, 0),
				new Vector3(width, height, 0),
				new Vector3(-width, height, 0)
			};
			m.uv = new Vector2[] {
				new Vector2(0, 0),
				new Vector2(0, 1),
				new Vector2(1, 1),
				new Vector2(1, 0)
			};
			m.triangles = new int[] { 2, 1, 0, 3, 2, 0 };
			//m.triangles = m.triangles.Reverse().ToArray();
			
			mf.mesh = m;
			m.RecalculateBounds();
			m.RecalculateNormals();
			
			return go;
		}
	}
}
