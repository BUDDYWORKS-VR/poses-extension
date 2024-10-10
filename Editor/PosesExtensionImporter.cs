using UnityEngine;
using UnityEditor;

namespace BUDDYWORKS.PosesExtension
{  
    public class PrefabSpawner : MonoBehaviour
    {
        // VRCF Prefab definitions
        static string prefabPE_Standalone_VRCF = "ff714a403f2fe944aa77358222a4be1c";
        static string prefabPE_GGL_VRCF = "09cb0fd5fac430446b22b023b90bd66d";

        static string VRCF_Path = "Packages/com.vrcfury.vrcfury";

        // GGL Prefab definitions
        static string prefabGGL_Beyond_Legacy = "10b55c87769faa544ae55a6de658bf86";
        static string prefabGGL_All_Legacy = "d1e19656881f0994b880e2ea7164e6bf";  
        static string prefabGGL_Beyond_Latest = "c901be5fd0387e04a86ff8d3496cfbcc";
        static string prefabGGL_All_Latest = "9f8fef6a5494d444c9165fb462015da2";
        static string prefabGGL_Beyond;
        static string prefabGGL_All;

        [InitializeOnLoadMethod]
        private static void GGL_Assoc()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGGL_Beyond_Latest)) != null)
            {
                prefabGGL_Beyond = prefabGGL_Beyond_Latest;
                prefabGGL_All = prefabGGL_All_Latest;
            }
            if (AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGGL_Beyond_Legacy)) != null)
            {
                prefabGGL_Beyond = prefabGGL_Beyond_Legacy;
                prefabGGL_All = prefabGGL_All_Legacy;
            }
        }
        
        // Toolbar Menu
        [MenuItem("BUDDYWORKS/Poses Extension/Spawn Standalone Prefab... [VRCFury] [Recommended]", false, 0)]
        [MenuItem("GameObject/BUDDYWORKS/Poses Extension/Spawn Standalone Prefab... [VRCFury] [Recommended]", false, 0)]
        private static void SpawnPE()
        {
            SpawnPrefab(prefabPE_Standalone_VRCF);
            NotifyOrder();
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn GGL-Variant Prefab... [VRCFury] [Lite]", false, 1)]
        [MenuItem("GameObject/BUDDYWORKS/Poses Extension/Spawn GGL-Variant Prefab... [VRCFury] [Lite]", false, 1)]
        private static void SpawnPEGGL()
        {
            SpawnPrefab(prefabPE_GGL_VRCF);
            NotifyOrder();
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn GoGo Loco Beyond Prefab...", false, 60)]
        [MenuItem("GameObject/BUDDYWORKS/Poses Extension/Spawn GoGo Loco Beyond Prefab...", false, 5)]
        private static void SpawnGGLBeyond()
        {
            SpawnPrefab(prefabGGL_Beyond);
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn GoGo Loco All Prefab...", false, 70)]
        [MenuItem("GameObject/BUDDYWORKS/Poses Extension/Spawn GoGo Loco All Prefab...", false, 6)]
        private static void SpawnGGLAll()
        {
            SpawnPrefab(prefabGGL_All);
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Prefab must go after Locomotion in Hierarchy!", false, 1000)]
        [MenuItem("GameObject/BUDDYWORKS/Poses Extension/Prefab must go after Locomotion in Hierarchy!", false, 1000)]
        [MenuItem("GameObject/BUDDYWORKS/Poses Extension/---------------", false, 4)]
        [MenuItem("GameObject/BUDDYWORKS/Poses Extension/----------------", false, 7)]
        private static void DISCLAIMER()
        {
            NotifyOrder();
        }

        // Enable or disable menu items dynamically

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn Standalone Prefab... [VRCFury] [Recommended]", true)]
        private static bool ValidateSpawnPE()
        {
            return AssetDatabase.IsValidFolder(VRCF_Path) != false;
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn GGL-Variant Prefab... [VRCFury] [Lite]", true)]
        private static bool ValidateSpawnPEGGL()
        {
            return AssetDatabase.IsValidFolder(VRCF_Path) != false;
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn GoGo Loco Beyond Prefab...", true)]
        [MenuItem("GameObject/BUDDYWORKS/Poses Extension/Spawn GoGo Loco Beyond Prefab...", true)]
        [MenuItem("GameObject/BUDDYWORKS/Poses Extension/---------------", true)]
        private static bool ValidateSpawnGGLBeyond()
        {
            GGL_Assoc();
            return AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGGL_Beyond)) != null;
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn GoGo Loco All Prefab...", true)]
        [MenuItem("GameObject/BUDDYWORKS/Poses Extension/Spawn GoGo Loco All Prefab...", true)]
        private static bool ValidateSpawnGGLAll()
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGGL_All)) != null;
        }

        // Called to notify about the thing.
        private static void NotifyOrder()
        {
            Debug.Log("Make sure the Poses Extension Prefab is placed after your Locomotion prefab in the Hierarchy, else you might encounter issues.");
            return;
        }

        // Prefab Spawner
        private static void SpawnPrefab(string guid)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);

            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError("Prefab with GUID " + guid + " not found.");
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject selectedObject = Selection.activeGameObject;

            if (prefab == null)
            {
                Debug.LogError("Failed to load prefab with GUID " + guid + " at path " + prefabPath);
                return;
            }

            GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            if (selectedObject != null)
            {
                instantiatedPrefab.transform.parent = selectedObject.transform;
            }

            if (instantiatedPrefab != null)
            {
                EditorGUIUtility.PingObject(instantiatedPrefab);
            }
            else
            {
                Debug.LogError("Failed to instantiate prefab with GUID " + guid);
            }
        }
    }
}