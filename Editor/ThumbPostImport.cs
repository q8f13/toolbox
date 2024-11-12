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
    static string[] PATH_AFFECT = {
        "Assets/UI/ItemThumbnail",
        "Assets/Gizmos"
    };

    void OnPreprocessTexture()
    {
        string lowercase_path = assetPath.ToLower();
        bool ignore = true;
        foreach(string s in PATH_AFFECT)
        {
            if(lowercase_path.IndexOf(s.ToLower()) >= 0)
            {
                ignore = false;
                break;
            }
        }

        if(ignore)
            return;

        TextureImporter ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Sprite;

        ti.textureCompression = TextureImporterCompression.Uncompressed;
    }
}
