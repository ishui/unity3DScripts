using UnityEngine;
using System.Collections;

namespace BuildR2 {
    public interface ICustomIconAsset {
        string customIconPath {get; set;}
        Texture2D previewTexture {get;}
    }
}