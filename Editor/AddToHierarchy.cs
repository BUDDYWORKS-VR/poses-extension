using UnityEngine;
using UnityEditor;

namespace BUDDYWORKS.PosesExtension.AddToHierarchy
{
    public class InstantiateAndPingPrefabByGUID : MonoBehaviour
    {
        public static string prefabGUID = "10b55c87769faa544ae55a6de658bf86";
        public static string prefabGUID2 = "d1e19656881f0994b880e2ea7164e6bf";

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn Beyond Prefab... (recommended)")]
        public static void SpawnGGLBeyondPrefab()
        {
            SpawnPrefab(prefabGUID);
        }

        [MenuItem("BUDDYWORKS/Poses Extension/Spawn All Prefab...")]
        public static void SpawnGGLAllPrefab()
        {
            SpawnPrefab(prefabGUID2);
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
