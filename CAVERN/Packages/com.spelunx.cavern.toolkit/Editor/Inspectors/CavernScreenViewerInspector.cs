using UnityEngine;
using UnityEditor;

namespace Spelunx {
    [CustomEditor(typeof(CavernScreenViewer))]
    public class CavernScreenViewerInspector : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            CavernScreenViewer cavernScreenViewer = (CavernScreenViewer)target;
            if (GUILayout.Button("Generate Mesh")) {
                cavernScreenViewer.GenerateMesh();
            }

            if (GUILayout.Button("Clear Mesh")) {
                cavernScreenViewer.ClearMesh();
            }
        }
    }
}