using UnityEngine;
using UnityEditor;

namespace BUDDYWORKS.PosesExtension.AddToHierarchy
{
    public class InstantiateAndPingPrefabByGUID : MonoBehaviour
    {
        public static string prefabGUID = "ff714a403f2fe944aa77358222a4be1c";
        public static string prefabGUID2 = "09cb0fd5fac430446b22b023b90bd66d";

        // Toolbar Menu
        [MenuItem("BUDDYWORKS/Poses Extension/Spawn Prefab...")]
        public static void SpawnPE()
        {
            SpawnPrefab(prefabGUID);
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn Prefab... (GGL Experimental)")]
        public static void SpawnPEGGL()
        {
            SpawnPrefab(prefabGUID2);
        }

        public static void SpawnPrefab(string guid)
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
