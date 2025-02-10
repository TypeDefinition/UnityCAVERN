using UnityEngine;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

namespace Spelunx
{
    public class Window : EditorWindow
    {
        private Object cavernSetup;
        private Object viveManager;
        private Object viveTracker;
        private GameObject newCavernSetup;
        private GameObject newViveManager;
        private GameObject newViveTracker;

        [MenuItem("CAVERN/Tools")]
        public static void ShowWindow()
        {
            GetWindow<Window>("CAVERN Tools");
        }

        void OnGUI()
        {
            GUILayout.Label("CAVERN Setup", EditorStyles.boldLabel);
            GUILayout.Label("Adds the CAVERN camera rig into your scene");
            GUILayout.Label("and creates a game object to hold additional CAVERN objects.");
            if (GUILayout.Button("Add"))
            {
                cavernSetup = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.toolkit/Runtime/Prefabs/CavernSetup.prefab", typeof(GameObject));
                Debug.Log("cavern setup: " + cavernSetup.name);
                newCavernSetup = (GameObject)PrefabUtility.InstantiatePrefab(cavernSetup as GameObject);
            }

            GUILayout.Label("Vive Tracker", EditorStyles.boldLabel);
            GUILayout.Label("Adds a new vive tracker and tracker manager to your scene.");
            GUILayout.Label("A Vive tracker can detect movement with 6 degrees of freedom.");

            if (GUILayout.Button("Add"))
            {
                // load from GUI input
                // objToSpawn = EditorGUILayout.ObjectField("Prefab", objToSpawn, typeof(Object), true);

                // load from path
                viveManager = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.toolkit/Runtime/Prefabs/ViveTrackerManager.prefab", typeof(GameObject));
                viveTracker = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.toolkit/Runtime/Prefabs/ViveTracker.prefab", typeof(GameObject));
                Debug.Log("vive setup: " + viveManager.name + " " + viveTracker.name);
                // Debug.Log("Got " + objToSpawn.name + objToSpawn.GetType());

                // instantiate prefab
                newViveManager = (GameObject)PrefabUtility.InstantiatePrefab(viveManager as GameObject);
                newViveTracker = (GameObject)PrefabUtility.InstantiatePrefab(viveTracker as GameObject);


                // set prefab's hierarcy in scene
                newViveManager.GetComponent<Transform>().parent = GameObject.Find("CavernSetup").GetComponent<Transform>();

                // TODO: find way to instantiate prefab at specific place in scene hierarchy
            }
        }
    }

}

