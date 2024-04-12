using UnityEngine;
using UnityEditor;

namespace BUDDYWORKS.PosesExtension
{
    public class PEPrefabSpawner : MonoBehaviour
    {
        //VRCF Prefab definitions
        public static string prefabPE_Standalone_VRCF = "ff714a403f2fe944aa77358222a4be1c";
        public static string prefabPE_GGL_VRCF = "09cb0fd5fac430446b22b023b90bd66d";
        public static string VRCF_Path = "Packages/com.vrcfury.vrcfury";

        //MA Prefab definitions
        public static string prefabPE_Standalone_MA = "095b224818f91be42a97b83442ee50de";
        public static string prefabPE_GGL_MA = "09cb0fd5fac430446b22b023b90bd66d";
        public static string MA_Path = "Packages/nadena.dev.modular-avatar";

        //GGL Prefab definitions
        public static string prefabGGL_Beyond = "10b55c87769faa544ae55a6de658bf86";
        public static string prefabGGL_All = "d1e19656881f0994b880e2ea7164e6bf";

        // Toolbar Menu
        [MenuItem("BUDDYWORKS/Poses Extension/Spawn Prefab... [VRCFury]", false, 0)]
        public static void SpawnPE()
        {
            SpawnPrefab(prefabPE_Standalone_VRCF);
            Debug.Log("Make sure the Poses Extension Prefab is placed after your Locomotion prefab in the Hierarchy, else you might encounter issues.");

        }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn GGL-Variant Prefab... [VRCFury]", false, 1)]
        public static void SpawnPEGGL()
        {
            SpawnPrefab(prefabPE_GGL_VRCF);
            Debug.Log("Make sure the Poses Extension Prefab is placed after your Locomotion prefab in the Hierarchy, else you might encounter issues.");
        }


        [MenuItem("BUDDYWORKS/Poses Extension/Spawn Prefab... [ModularAvatar] (Experimental)", false, 20)]
        public static void SpawnPE_MA()
        {
            SpawnPrefab(prefabPE_Standalone_MA);
            Debug.Log("Make sure the Poses Extension Prefab is placed after your Locomotion prefab in the Hierarchy, else you might encounter issues.");
        }

    //    [MenuItem("BUDDYWORKS/Poses Extension/Spawn GGL-Variant Prefab... [ModularAvatar] (Experimental)", false, 30)]
    //    public static void SpawnPEGGL_MA()
    //    {
    //        SpawnPrefab(prefabPE_Standalone_MA);
    //        Debug.Log("Make sure the Poses Extension Prefab is placed after your Locomotion prefab in the Hierarchy, else you might encounter issues.");
    //    }


        [MenuItem("BUDDYWORKS/Poses Extension/Spawn GoGo Loco Beyond Prefab...", false, 60)]
        public static void SpawnGGLBeyond()
        {
            SpawnPrefab(prefabGGL_Beyond);
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn GoGo Loco All Prefab...", false, 70)]
        public static void SpawnGGLAll()
        {
            SpawnPrefab(prefabGGL_All);
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Prefab must go after Locomotion in Hierarchy!", false, 1000)]
        public static void DISCLAIMER()
        {
            Debug.Log("Make sure the Poses Extension Prefab is placed after your Locomotion prefab in the Hierarchy, else you might encounter issues.");
        }


        // Enable or disable menu items dynamically


        [MenuItem("BUDDYWORKS/Poses Extension/Spawn Prefab... [VRCFury]", true)]
        public static bool ValidateSpawnPE()
        {
            return AssetDatabase.IsValidFolder(VRCF_Path) != false;
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn GGL-Variant Prefab... [VRCFury]", true)]
        public static bool ValidateSpawnPEGGL()
        {
            return AssetDatabase.IsValidFolder(VRCF_Path) != false;
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn Prefab... [ModularAvatar] (Experimental)", true)]
        public static bool ValidateSpawnPE_MA()
        {
            return AssetDatabase.IsValidFolder(MA_Path) != false;
        }

    //    [MenuItem("BUDDYWORKS/Poses Extension/Spawn GGL-Variant Prefab... [ModularAvatar] (Experimental)", true)]
    //    public static bool ValidateSpawnPEGGL_MA()
    //    {
    //        return AssetDatabase.IsValidFolder(MA_Path) != false;
    //    }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn GoGo Loco Beyond Prefab...", true)]
        public static bool ValidateSpawnGGLBeyond()
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGGL_Beyond)) != null;
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn GoGo Loco All Prefab...", true)]
        public static bool ValidateSpawnGGLAll()
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGGL_All)) != null;
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
