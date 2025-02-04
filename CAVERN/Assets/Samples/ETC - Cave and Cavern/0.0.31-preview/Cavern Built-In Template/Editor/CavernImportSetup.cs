using UnityEngine;
using UnityEditor;
// Executes once on import to verify settings assets are correctly configured
// (Maybe URP checks too, in the future)
namespace ETC.CaveCavern {
    [InitializeOnLoad]
    public static class CavernSampleImportSetup {
        // Note: On domain reload, is not re-checking if should be validated.
        // Can cause confusing behaviour on delete and re-import of sample scene.
        // Setting to do this once per session to enable easier debugging for end users trying to get unpredicatable behaviour resolved.
        private static bool HasSampleSceneBeenImported {
            get => SessionState.GetBool("Cavern_SampleSceneImported", false);
            set => SessionState.SetBool("Cavern_SampleSceneImported", value);
        }
        // Check for any instances of scriptable objects that inherit from SettingsSOSingleton<T> in the project and call ValidateInPreloadedAssets on them
        static CavernSampleImportSetup()
        {
            if (HasSampleSceneBeenImported)
            {
                return;
            }
            string[] guids = AssetDatabase.FindAssets("t:SettingsSOSingleton");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject instance = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (instance != null)
                {
                    dynamic dynamicInstance = instance;
                    dynamicInstance.ValidateInPreloadedAssets();
                }
            }
        }
    }
}
