using UnityEngine;
using UnityEditor;

namespace BUDDYWORKS.PosesExtension
{  
    public class PrefabSpawner : MonoBehaviour
    {
        // VRCF Prefab definitions
        #if UNITY_2022_3_OR_NEWER
        static string prefabPE_Standalone_VRCF = "ff714a403f2fe944aa77358222a4be1c";
        static string prefabPE_GGL_VRCF = "09cb0fd5fac430446b22b023b90bd66d";
        #else
        static string prefabPE_Standalone2019_VRCF = "c935052f7e5696b418d71df05cae7163";
        static string prefabPE_GGL2019_VRCF = "208c3816e6a08c744bd98bcde7d8f09e";
        #endif
        static string VRCF_Path = "Packages/com.vrcfury.vrcfury";

        // MA Prefab definitions
        static string prefabPE_Standalone_MA = "095b224818f91be42a97b83442ee50de";
        // public static string prefabPE_GGL_MA = "09cb0fd5fac430446b22b023b90bd66d";
        static string MA_Path = "Packages/nadena.dev.modular-avatar";

        // GGL Prefab definitions
        static string prefabGGL_Beyond = "10b55c87769faa544ae55a6de658bf86";
        static string prefabGGL_All = "d1e19656881f0994b880e2ea7164e6bf";  

        // Toolbar Menu
        [MenuItem("BUDDYWORKS/Poses Extension/Spawn Standalone Prefab... [VRCFury] [Recommended]", false, 0)]
        [MenuItem("GameObject/BUDDYWORKS/Poses Extension/Spawn Standalone Prefab... [VRCFury] [Recommended]", false, 0)]
        private static void SpawnPE()
        {
            #if UNITY_2022_3_OR_NEWER
            SpawnPrefab(prefabPE_Standalone_VRCF);
            #else
            SpawnPrefab(prefabPE_Standalone2019_VRCF);
            #endif
            NotifyOrder();
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn GGL-Variant Prefab... [VRCFury] [Lite]", false, 1)]
        [MenuItem("GameObject/BUDDYWORKS/Poses Extension/Spawn GGL-Variant Prefab... [VRCFury] [Lite]", false, 1)]
        private static void SpawnPEGGL()
        {
            #if UNITY_2022_3_OR_NEWER
            SpawnPrefab(prefabPE_GGL_VRCF);
            #else
            SpawnPrefab(prefabPE_GGL2019_VRCF);
            #endif
            NotifyOrder();
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn Prefab... [ModularAvatar] (Experimental)", false, 20)]
        [MenuItem("GameObject/BUDDYWORKS/Poses Extension/Spawn Prefab... [ModularAvatar] (Experimental)", false, 2)]
        private static void SpawnPE_MA()
        {
            SpawnPrefab(prefabPE_Standalone_MA);
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

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn Prefab... [ModularAvatar] (Experimental)", true)]
        [MenuItem("GameObject/BUDDYWORKS/Poses Extension/Spawn Prefab... [ModularAvatar] (Experimental)", true)]
        private static bool ValidateSpawnPE_MA()
        {
            return AssetDatabase.IsValidFolder(MA_Path) != false;
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn GoGo Loco Beyond Prefab...", true)]
        [MenuItem("GameObject/BUDDYWORKS/Poses Extension/Spawn GoGo Loco Beyond Prefab...", true)]
        [MenuItem("GameObject/BUDDYWORKS/Poses Extension/---------------", true)]
        private static bool ValidateSpawnGGLBeyond()
        {
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