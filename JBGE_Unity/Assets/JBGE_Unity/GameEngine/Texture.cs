using System.IO;
using UnityEngine;

namespace JBGE {
	public class Texture {
    public GameEngine GE;
    public Texture2D Texture2D;

    public Texture(GameEngine GE) {
      this.GE = GE;
    }

    /// <summary>Creates texture from local file or from addressables (NOTES: if file exists in addressables path, then it will be loaded as priority)</summary>
    /// <param name="textureFileName">Relative path to the texture file (e.g. Assets/Images/myimage.png)</param>
    /// <returns>ID is generated that can be used to identify the newly created object (if creation fails, returns -1)</returns>
    public int Create(string textureFileName) {
      // Create texture 32 bit with alpha, no mipmaps (size does not matter when loading from file)
      Texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
      bool results = false;


      // Try to load from Addressable assets as priority
      TextAsset asset = (TextAsset)GE.LoadAddressableAsset<TextAsset>(textureFileName);
      if(asset != null) {
        results = Texture2D.LoadImage(asset.bytes);
      } else {
        // Otherwise, try to load from local file
        if(File.Exists(textureFileName)) {
          var rawImgData = File.ReadAllBytes(textureFileName);
          results = Texture2D.LoadImage(rawImgData);
        }
      }

      if(!results) return -1;

      // Generate a random ID for this object
      var rand = new System.Random();
      int randomID = rand.Next();
      // Check if same ID already exists in our list
      while(GE.Textures.ContainsKey(randomID)) {
        // Re-generate a random ID for this object
        randomID = rand.Next();
      }
      return randomID;
    }

    /// <summary>Creates blank texture )</summary>
    /// <param name="textureFileName">Relative path to the texture file (e.g. Assets/Images/myimage.png)</param>
    /// <returns>ID is generated that can be used to identify the newly created object (if creation fails, returns -1)</returns>
    public int CreateBlankTexture(int width, int height) {
      // Create texture 32 bit with alpha, no mipmaps (size does not matter when loading from file)
      Texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);

      // Generate a random ID for this object
      var rand = new System.Random();
      int randomID = rand.Next();
      // Check if same ID already exists in our list
      while(GE.Textures.ContainsKey(randomID)) {
        // Re-generate a random ID for this object
        randomID = rand.Next();
      }
      return randomID;
    }

    public void Destroy() {
      Object.Destroy(Texture2D);
      Texture2D = null;
    }
  }
}