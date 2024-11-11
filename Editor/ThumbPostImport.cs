using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// auto process textures in specific path
/// </summary>

public class ThumbPostImport : AssetPostprocessor
{
    // assets affected with this path
    const string PATH_AFFECT = "Assets/UI/ItemThumbnail";

    void OnPreprocessTexture()
    {
        string lowercase_path = assetPath.ToLower();
        if(lowercase_path.IndexOf(PATH_AFFECT.ToLower()) == -1)
            return;

        TextureImporter ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Sprite;

        ti.textureCompression = TextureImporterCompression.Uncompressed;
    }
}
