using UnityEngine;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

namespace Spelunx
{
    public class Window : EditorWindow
    {
        // private string str = "Hello World";
        private Color color;
        private Object objToSpawn;
        private Object newObj;

        [MenuItem("CAVERN/Tools")]
        public static void ShowWindow()
        {
            GetWindow<Window>("CAVERN Tools");
        }

        void OnGUI()
        {
            // GUILayout.Label("Change Colors", EditorStyles.boldLabel);
            // // str = EditorGUILayout.TextField("Name Var", str);

            // color = EditorGUILayout.ColorField("Color", color);

            // if (GUILayout.Button("Change Color"))
            // {
            //     ChangeColor();
            //     Debug.Log("color change button pressed");
            // }
            // GUILayout.Space(20);

            GUILayout.Label("Vive Tracker", EditorStyles.boldLabel);
            GUILayout.Label("Adds a new vive tracker to your scene.");
            GUILayout.Label("A Vive tracker can detect movement with 6 degrees of freedom.");

            if (GUILayout.Button("Add"))
            {
                // load from GUI input
                // objToSpawn = EditorGUILayout.ObjectField("Prefab", objToSpawn, typeof(Object), true);

                // load from path
                objToSpawn = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/tealCube.prefab", typeof(GameObject));
                // Debug.Log("Got " + objToSpawn.name + objToSpawn.GetType());

                // instantiate prefab
                newObj = PrefabUtility.InstantiatePrefab(objToSpawn as GameObject);

                // set prefab's hierarcy in scene
                // newObj.GetComponent<Transform>().parent = GameObject.Find("NewObjs").GetComponent<Transform>();

                // TODO: find way to instantiate prefab at specific place in scene hierarchy
            }


            // void ChangeColor()
            // {
            //     foreach (GameObject obj in Selection.gameObjects)
            //     {
            //         Debug.Log("Selected: " + obj.name);
            //         Renderer objRenderer = obj.GetComponent<Renderer>();
            //         if (objRenderer != null)
            //         {
            //             objRenderer.sharedMaterial.SetColor("_BaseColor", color);
            //         }
            //     }
            // }
        }
    }

}

