using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace qbfox
{
    public class TerrainUtils : MonoBehaviour
    {
        // remove all missing TerrainLayer in selected Terrain
        [MenuItem("Custom/Terrain/RemoveInvalidTerrainLayer")]
        static void CheckAndRmInvalidTerrainLayer(){
            GameObject selected = Selection.activeGameObject;
            Terrain[] ts = selected.GetComponentsInChildren<Terrain>();
            foreach(Terrain t in ts)
            {
                List<TerrainLayer> detected = new List<TerrainLayer>();
                for(int i=0;i<t.terrainData.terrainLayers.Length;i++)
                {
                    TerrainLayer tl = t.terrainData.terrainLayers[i];
                    if(tl != null)
                        detected.Add(tl);
                }

                t.terrainData.SetTerrainLayersRegisterUndo(detected.ToArray(), "filter valid terrain layer");
            }
        }

    }
}

