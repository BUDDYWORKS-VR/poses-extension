using UnityEditor;
using UnityEngine;

namespace BUDDYWORKS.PosesExtension
{
    public class Options : EditorWindow
    {
        private const string AssetGuid = "17cce2e1703370a41b2f584d6364944a";
        private string _assetPath;
        private SerializedObject _serializedObject;
        private SerializedProperty _parameters;

        private bool _viewAdjustGroupSynced = true;
        private bool _handAdjustGroupSynced = true;
        private bool _headAdjustGroupSynced = true;
        private bool _heightSynced;
        private bool _mirrorSynced;

        private const int BaseCost = 16;
        private const int HeightCost = 8;
        private const int MirrorCost = 1;
        private const int ViewAdjustCost = 17;
        private const int HandAdjustCost = 17;
        private const int HeadAdjustCost = 25;

        [MenuItem("BUDDYWORKS/Poses Extension/Settings...")]
        public static void ShowWindow()
        {
            GetWindow<Options>("Poses Extension Settings");
        }

        private void OnEnable()
        {
            _assetPath = AssetDatabase.GUIDToAssetPath(AssetGuid);

            if (string.IsNullOrEmpty(_assetPath))
            {
                Debug.LogError($"Asset with GUID {AssetGuid} not found.");
                return;
            }

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(_assetPath);
            if (asset == null)
            {
                Debug.LogError($"Failed to load asset at path {_assetPath}");
                return;
            }

            _serializedObject = new SerializedObject(asset);
            _parameters = _serializedObject.FindProperty("parameters");

            if (_parameters == null || !_parameters.isArray)
            {
                Debug.LogError("'parameters' property not found or not an array.");
                return;
            }

            // Initialize group states
            //_viewAdjustGroupSynced = true;
            //_handAdjustGroupSynced = true;
            //_headAdjustGroupSynced = false;
            //_heightSynced = false;
            //_mirrorSynced = false;

            // Analyze relevant parameters
            for (int i = 0; i < _parameters.arraySize; i++)
            {
                var entry = _parameters.GetArrayElementAtIndex(i);
                var nameProp = entry.FindPropertyRelative("name");
                var networkSyncedProp = entry.FindPropertyRelative("networkSynced");

                if (nameProp == null || networkSyncedProp == null) continue;

                string parameterName = nameProp.stringValue;
                bool networkSynced = networkSyncedProp.intValue == 1;

                if (parameterName == "PE/Height")
                {
                    _heightSynced = networkSynced;
                }
                if (parameterName == "PE/Mirror")
                {
                    _mirrorSynced = networkSynced;
                }
                if (parameterName.StartsWith("PE/ViewAdjust"))
                {
                    _viewAdjustGroupSynced &= networkSynced;
                }
                if (parameterName.StartsWith("PE/HandAdjust"))
                {
                    _handAdjustGroupSynced &= networkSynced;
                }
                if (parameterName.StartsWith("PE/HeadAdjust"))
                {
                    _headAdjustGroupSynced &= networkSynced;
                }
            }
        }

        private void OnGUI()
        {
            if (_serializedObject == null || _parameters == null)
            {
                EditorGUILayout.LabelField("Unable to access the parameter file, please report this!");
                return;
            }

            _serializedObject.Update();

            GUILayout.Label("Here you can select which features are synced to other players.", EditorStyles.boldLabel);
            GUILayout.Label("This is neat if you have someone else as photographer, but comes at a parameter cost.");
            GUILayout.Label("Note that you will have access to all features locally, regardless of your selection.");
            GUILayout.Label("This only applies to PE Standalone!");
            Rect r = EditorGUILayout.GetControlRect(false, 1, new GUIStyle() { margin = new RectOffset(0, 0, 4, 4) });
            EditorGUI.DrawRect(r, Color.gray);
            
            // Mirror checkbox
            bool newMirrorSynced = EditorGUILayout.Toggle("Mirror Pose", _mirrorSynced);
            if (newMirrorSynced != _mirrorSynced)
            {
                _mirrorSynced = newMirrorSynced;
                UpdateAssetValue("PE/Mirror", _mirrorSynced);
            }
            
            // Height checkbox
            bool newHeightSynced = EditorGUILayout.Toggle("Height Adjust", _heightSynced);
            if (newHeightSynced != _heightSynced)
            {
                _heightSynced = newHeightSynced;
                UpdateAssetValue("PE/Height", _heightSynced);
            }

            // ViewAdjust group checkbox
            bool newViewAdjustGroupSynced = EditorGUILayout.Toggle("View Adjust", _viewAdjustGroupSynced);
            if (newViewAdjustGroupSynced != _viewAdjustGroupSynced)
            {
                _viewAdjustGroupSynced = newViewAdjustGroupSynced;
                UpdateGroupedValues("PE/ViewAdjust", _viewAdjustGroupSynced);
            }

            // HandAdjust group checkbox
            bool newHandAdjustGroupSynced = EditorGUILayout.Toggle("Hand Adjust", _handAdjustGroupSynced);
            if (newHandAdjustGroupSynced != _handAdjustGroupSynced)
            {
                _handAdjustGroupSynced = newHandAdjustGroupSynced;
                UpdateGroupedValues("PE/HandAdjust", _handAdjustGroupSynced);
            }
            
            // HeadAdjust group checkbox
            bool newHeadAdjustGroupSynced = EditorGUILayout.Toggle("Head Adjust", _headAdjustGroupSynced);
            if (newHeadAdjustGroupSynced != _headAdjustGroupSynced)
            {
                _headAdjustGroupSynced = newHeadAdjustGroupSynced;
                UpdateGroupedValues("PE/HeadAdjust", _headAdjustGroupSynced);
            }

            // Calculate cost
            int parameterCost = BaseCost;
            if (_heightSynced) parameterCost += HeightCost;
            if (_mirrorSynced) parameterCost += MirrorCost;
            if (_viewAdjustGroupSynced) parameterCost += ViewAdjustCost;
            if (_handAdjustGroupSynced) parameterCost += HandAdjustCost;
            if (_headAdjustGroupSynced) parameterCost += HeadAdjustCost;

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

        private void UpdateAssetValue(string parameterName, bool value)
        {
            for (int i = 0; i < _parameters.arraySize; i++)
            {
                var entry = _parameters.GetArrayElementAtIndex(i);
                var nameProp = entry.FindPropertyRelative("name");
                var networkSyncedProp = entry.FindPropertyRelative("networkSynced");

                if (nameProp != null && networkSyncedProp != null && nameProp.stringValue == parameterName)
                {
                    networkSyncedProp.intValue = value ? 1 : 0;
                    SaveChanges();
                }
            }
        }

        private void UpdateGroupedValues(string prefix, bool value)
        {
            for (int i = 0; i < _parameters.arraySize; i++)
            {
                var entry = _parameters.GetArrayElementAtIndex(i);
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
            _serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
