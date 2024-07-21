using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SplineTrack))]
public class RebuildEditorButton : Editor {

    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        SplineTrack rebuild = (SplineTrack) target;
        if(GUILayout.Button("Rebuild Mesh")) {
            rebuild.Rebuild();
        }
    }
}
