using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace BUDDYWORKS.PosesExtension
{
    public class Options : EditorWindow
    {
        private const string AssetGUID = "67064e00373d93f448c3fc7565cebecc";
        private string assetPath;
        private SerializedObject serializedObject;
        private SerializedProperty parameters;

        private bool viewAdjustGroupSynced;
        private bool handAdjustGroupSynced;
        private bool heightSynced;
        private bool mirrorSynced;

        private const int BaseCost = 16;
        private const int HeightCost = 8;
        private const int MirrorCost = 1;
        private const int ViewAdjustCost = 17;
        private const int HandAdjustCost = 17;

        private static readonly HashSet<string> FilteredParameters = new HashSet<string>
        {
            "PE/Height",
            "PE/Mirror",
            "PE/ViewAdjust",
            "PE/HandAdjust"
        };

        [MenuItem("BUDDYWORKS/Poses Extension/Settings...")]
        public static void ShowWindow()
        {
            GetWindow<Options>("Poses Extension Settings");
        }

        private void OnEnable()
        {
            assetPath = AssetDatabase.GUIDToAssetPath(AssetGUID);

            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError($"Asset with GUID {AssetGUID} not found.");
                return;
            }

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset == null)
            {
                Debug.LogError($"Failed to load asset at path {assetPath}");
                return;
            }

            serializedObject = new SerializedObject(asset);
            parameters = serializedObject.FindProperty("parameters");

            if (parameters == null || !parameters.isArray)
            {
                Debug.LogError("'parameters' property not found or not an array.");
                return;
            }

            // Initialize group states
            viewAdjustGroupSynced = true;
            handAdjustGroupSynced = true;
            heightSynced = false;
            mirrorSynced = false;

            // Analyze relevant parameters
            for (int i = 0; i < parameters.arraySize; i++)
            {
                var entry = parameters.GetArrayElementAtIndex(i);
                var nameProp = entry.FindPropertyRelative("name");
                var networkSyncedProp = entry.FindPropertyRelative("networkSynced");

                if (nameProp == null || networkSyncedProp == null) continue;

                string name = nameProp.stringValue;
                bool networkSynced = networkSyncedProp.intValue == 1;

                if (name == "PE/Height")
                {
                    heightSynced = networkSynced;
                }
                else if (name == "PE/Mirror")
                {
                    mirrorSynced = networkSynced;
                }
                else if (name.StartsWith("PE/ViewAdjust"))
                {
                    viewAdjustGroupSynced &= networkSynced;
                }
                else if (name.StartsWith("PE/HandAdjust"))
                {
                    handAdjustGroupSynced &= networkSynced;
                }
            }
        }

        private void OnGUI()
        {
            if (serializedObject == null || parameters == null)
            {
                EditorGUILayout.LabelField("Asset not loaded or 'parameters' not found.");
                return;
            }

            serializedObject.Update();

            GUILayout.Label("Here you can select which features are synced to other players.", EditorStyles.boldLabel);
            GUILayout.Label("This is neat if you have someone else as photographer, but comes at a parameter cost.");
            GUILayout.Label("Note that you will have access to all features locally, regardless of your selection.");
            GUILayout.Label("This only applies to PE Standalone!");
            Rect r = EditorGUILayout.GetControlRect(false, 1, new GUIStyle() { margin = new RectOffset(0, 0, 4, 4) });
            EditorGUI.DrawRect(r, Color.gray);
            
            // Mirror checkbox
            bool newMirrorSynced = EditorGUILayout.Toggle("Mirror Pose", mirrorSynced);
            if (newMirrorSynced != mirrorSynced)
            {
                mirrorSynced = newMirrorSynced;
                UpdateAssetValue("PE/Mirror", mirrorSynced);
            }
            
            // Height checkbox
            bool newHeightSynced = EditorGUILayout.Toggle("HeightAdjust", heightSynced);
            if (newHeightSynced != heightSynced)
            {
                heightSynced = newHeightSynced;
                UpdateAssetValue("PE/Height", heightSynced);
            }

            // ViewAdjust group checkbox
            bool newViewAdjustGroupSynced = EditorGUILayout.Toggle("ViewAdjust", viewAdjustGroupSynced);
            if (newViewAdjustGroupSynced != viewAdjustGroupSynced)
            {
                viewAdjustGroupSynced = newViewAdjustGroupSynced;
                UpdateGroupedValues("PE/ViewAdjust", viewAdjustGroupSynced);
            }

            // HandAdjust group checkbox
            bool newHandAdjustGroupSynced = EditorGUILayout.Toggle("HandAdjust", handAdjustGroupSynced);
            if (newHandAdjustGroupSynced != handAdjustGroupSynced)
            {
                handAdjustGroupSynced = newHandAdjustGroupSynced;
                UpdateGroupedValues("PE/HandAdjust", handAdjustGroupSynced);
            }

            // Calculate cost
            int parameterCost = BaseCost;
            if (heightSynced) parameterCost += HeightCost;
            if (mirrorSynced) parameterCost += MirrorCost;
            if (viewAdjustGroupSynced) parameterCost += ViewAdjustCost;
            if (handAdjustGroupSynced) parameterCost += HandAdjustCost;

            // Display cost
            GUILayout.Space(10);
            EditorGUILayout.LabelField($"Parameter Cost: {parameterCost}");
            
            GUILayout.FlexibleSpace();
            GUILayout.Label("BUDDYWORKS Poses Extension", EditorStyles.boldLabel);
            Rect labelRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
            {
                Application.OpenURL("https://buddyworks.wtf");
            }
        }

        private void UpdateAssetValue(string name, bool value)
        {
            for (int i = 0; i < parameters.arraySize; i++)
            {
                var entry = parameters.GetArrayElementAtIndex(i);
                var nameProp = entry.FindPropertyRelative("name");
                var networkSyncedProp = entry.FindPropertyRelative("networkSynced");

                if (nameProp != null && networkSyncedProp != null && nameProp.stringValue == name)
                {
                    networkSyncedProp.intValue = value ? 1 : 0;
                    SaveChanges();
                }
            }
        }

        private void UpdateGroupedValues(string prefix, bool value)
        {
            for (int i = 0; i < parameters.arraySize; i++)
            {
                var entry = parameters.GetArrayElementAtIndex(i);
                var nameProp = entry.FindPropertyRelative("name");
                var networkSyncedProp = entry.FindPropertyRelative("networkSynced");

                if (nameProp != null && networkSyncedProp != null && nameProp.stringValue.StartsWith(prefix))
                {
                    networkSyncedProp.intValue = value ? 1 : 0;
                }
            }
            SaveChanges();
        }

        private new void SaveChanges()
        {
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
