using UnityEngine;

namespace JBGE {
	/// <summary>
	/// Base class of all UI components
	/// - Canvas / RectTransform is intentionally NOT used
	/// - UI is rendered as a 3D plane in perspective space
	/// - All layout, pivot, scale, rotation are resolved mathematically
	/// </summary>
	public class UIComponent {
		// NOTE: RealityKit's effective viewport height does not match the theoretical
    // perspective frustum due to SwiftUI + RealityView layout and safe margins.
    // This value was empirically calibrated to match full visible screen height
    // across common resolutions (640x480 .. 1920x1080).
		public float UIWorldUnitPerLogicalUnit = 0.866f;

		// Generate a random ID for this object
		public int ID = new System.Random().Next();
		// Reference to the Game Engine
		protected GameEngine GE;
		// Reference to this component's GameObject
		public GameObject ThisObject;
		// Sort order of this layer
		public int SortOrder = 0;

		/// <summary>Constructor</summary>
		/// <param name="GE">Reference to the RQ Game Engine</param>
		/// <param name="objectName">The object name to be shown in the hierarchy</param>
		/// <param name="parentObj">The parent UIComponent object we attach this new UICanvas to</param>
		/// <param name="isCreatePlaneForThisObject">Create an empty plane mesh or not</param>
		public UIComponent(GameEngine GE, string objectName = null, UIComponent parentObj = null, bool isCreatePlaneForThisObject = false) {
			this.GE = GE;
			string name = objectName ?? this.GetType().Name;
			// Create an empty UIPlane and set it in the hierarchy, if told to do so
			ThisObject = isCreatePlaneForThisObject ? CreateUIPlane(name, new Color(0.5f,0.5f,0.5f,0.5f)) : new GameObject(name);
			ThisObject.layer = 5; // "UI" Layer
			// If we don't have any parent object specified, then this container will be the root
			ThisObject.transform.SetParent(parentObj?.ThisObject.transform);
			// Reset all transform properties
			ResetTransform();
		}

		// Resets the transform properties of this object
		public void ResetTransform() {
			// Reset to origin
			ThisObject.transform.localPosition = Vector3.zero;
			ThisObject.transform.localRotation = Quaternion.identity;
			ThisObject.transform.localScale    = Vector3.one;
		}

		// Detaches and releases entities
		public virtual void Destroy() {
			Object.Destroy(ThisObject);
			ThisObject = null;
		}

		// Called every frame
		public virtual void Update() {
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
			var width = height * (GE.ScreenWidth / GE.ScreenHeight);

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
			float aspectRatioX = UIWorldUnitPerLogicalUnit * (GE.ScreenWidth / GE.ScreenHeight);

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
	}
}
