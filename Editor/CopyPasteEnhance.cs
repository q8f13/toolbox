using System.Collections;
using System.Collections.Generic;
using ICSharpCode.NRefactory.Ast;
using UnityEditor;
using UnityEngine;

public class CopyPasteEnhance : Editor
{
    static GameObject[] gos;

	[MenuItem("Tools/CutPaste/Cut GameObjects With Wpos")]
    static void DoCut()
    {
        gos = Selection.gameObjects;
    }

	[MenuItem("Tools/CutPaste/Paste GameObjects as child With Wpos")]
    static void DoPasteAsChild()
    {
        GameObject slot = Selection.gameObjects[0];
        if(slot == null)
            return;

        if(gos == null)
            return;

        foreach(GameObject g in gos)
        {
            g.transform.SetParent(slot.transform);
        }

        gos = null;
    }

}
