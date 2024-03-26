using UnityEngine;
using UnityEditor;

namespace BUDDYWORKS.PosesExtension.AddToHierarchy
{
    public class InstantiateAndPingPrefabByGUID : MonoBehaviour
    {
        public static string prefabGUID = "ff714a403f2fe944aa77358222a4be1c";
        public static string prefabGUID2 = "09cb0fd5fac430446b22b023b90bd66d";
        public static string prefabGUID3 = "10b55c87769faa544ae55a6de658bf86";
        public static string prefabGUID4 = "d1e19656881f0994b880e2ea7164e6bf";

        // Toolbar Menu
        [MenuItem("BUDDYWORKS/Poses Extension/Spawn Prefab...", false, 0)]
        public static void SpawnPE()
        {
            SpawnPrefab(prefabGUID);
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn Prefab... (GGL Experimental)", false, 1)]
        public static void SpawnPEGGL()
        {
            SpawnPrefab(prefabGUID2);
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn GoGo Loco Beyond Prefab (If Imported)", false, 30)]
        public static void SpawnGGLBeyond()
        {
            SpawnPrefab(prefabGUID3);
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn GoGo Loco All Prefab (If Imported)", false, 40)]
        public static void SpawnGGLAll()
        {
            SpawnPrefab(prefabGUID4);
        }

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
