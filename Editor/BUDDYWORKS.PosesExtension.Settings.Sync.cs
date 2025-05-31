using UnityEditor;
using UnityEngine;

namespace BUDDYWORKS.PosesExtension
{
    public class Sync
    {
        private bool _viewAdjustGroupSynced = true;
        private bool _handAdjustGroupSynced = true;
        private bool _headAdjustGroupSynced = true;
        private bool _tiltAdjustGroupSynced = true;
        private bool _offsetAdjustGroupSynced = true;
        private bool _heightSynced;
        private bool _mirrorSynced;

        private const int BaseCost = 16;
        private const int HeightCost = 8;
        private const int MirrorCost = 1;
        private const int ViewAdjustCost = 17;
        private const int HandAdjustCost = 17;
        private const int HeadAdjustCost = 25;
        private const int TiltAdjustCost = 9;
        private const int OffsetAdjustCost = 25;

        private SerializedObject _serializedObject;
        private SerializedProperty _parameters;

        public Sync(SerializedObject serializedObject, SerializedProperty parameters)
        {
            _serializedObject = serializedObject;
            _parameters = parameters;

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
                if (parameterName.StartsWith("PE/HeadAdjust") && !parameterName.Contains("Reset"))
                {
                    _headAdjustGroupSynced &= networkSynced;
                }
                if (parameterName.StartsWith("PE/TiltAdjust") && !parameterName.Contains("Reset"))
                {
                    _tiltAdjustGroupSynced &= networkSynced;
                }
                if (parameterName.StartsWith("PE/OffsetAdjust") && !parameterName.Contains("Reset"))
                {
                    _offsetAdjustGroupSynced &= networkSynced;
                }
            }
        }

        public void DrawSyncTab()
        {
            if (_serializedObject == null || _parameters == null)
            {
                EditorGUILayout.LabelField(
                    "Unable to access the parameter file, please report this!"
                );
                return;
            }

            _serializedObject.Update();

            // Add padding to the left
            GUILayout.BeginHorizontal();
            GUILayout.Space(4); // Adjust the value for more or less padding

            GUILayout.BeginVertical();
            GUILayout.Label(
                "Select which features are synced to other players.",
                EditorStyles.boldLabel
            );
            EditorGUILayout.HelpBox(
                "Useful if you have a photographer, though it does incur a " +
                "parameter cost.\nYou'll still have access to all features " +
                "locally, regardless of your selection.\nThis applies only to PE " +
                "Standalone!",
                MessageType.Info
            );
            Rect r = EditorGUILayout.GetControlRect(
                false,
                1,
                new GUIStyle() { margin = new RectOffset(0, 0, 4, 4) }
            );
            EditorGUI.DrawRect(r, Color.gray);

            // Tooltips and Labels
            GUIContent mirrorContent = new GUIContent(
                "Mirror Pose",
                "Mirror Pose will flip your current pose, useful if you want to " +
                "face a different direction.\n\nWe recommend leaving this core " +
                "feature synced."
            );
            GUIContent heightContent = new GUIContent(
                "Height Adjust",
                "Height Adjust allows you to adjust the height of your avatar " +
                "during posing.\nUseful to account for colliders or the lack " +
                "thereof.\n\nWe recommend leaving this core feature synced."
            );
            GUIContent viewContent = new GUIContent(
                "View Adjust",
                "View Adjust lets you change your avatars viewing direction, " +
                "giving more control.\n\nTakes quite a few parameters, sync if " +
                "needed."
            );
            GUIContent handContent = new GUIContent(
                "Hand Adjust",
                "Hand Adjust allows you to change the hand gestures of poses " +
                "from a set of generic ones.\n\nTakes quite a few parameters, " +
                "sync if needed."
            );
            GUIContent headContent = new GUIContent(
                "Head Adjust",
                "Head Adjust lets you change your avatars head rotation and tilt, " +
                "allowing for many new angles.\n\nThis costs many parameters, sync " +
                "if needed."
            );
            GUIContent tiltContent = new GUIContent(
                "Tilt Adjust",
                "Tilt Adjust allows for more angles for your photos.\n\nThis costs quite a few parameters, " +
                "sync if needed."
            );
            GUIContent offsetContent = new GUIContent(
                "Offset Adjust",
                "Offset Adjust lets you account for various body shapes.\n\nThis costs many parameters, sync " +
                "if needed."
            );

            // Mirror checkbox
            bool newMirrorSynced = EditorGUILayout.Toggle(mirrorContent, _mirrorSynced);
            if (newMirrorSynced != _mirrorSynced)
            {
                _mirrorSynced = newMirrorSynced;
                UpdateAssetValue("PE/Mirror", _mirrorSynced);
            }

            // Height checkbox
            bool newHeightSynced = EditorGUILayout.Toggle(heightContent, _heightSynced);
            if (newHeightSynced != _heightSynced)
            {
                _heightSynced = newHeightSynced;
                UpdateAssetValue("PE/Height", _heightSynced);
            }

            // ViewAdjust group checkbox
            bool newViewAdjustGroupSynced = EditorGUILayout.Toggle(
                viewContent,
                _viewAdjustGroupSynced
            );
            if (newViewAdjustGroupSynced != _viewAdjustGroupSynced)
            {
                _viewAdjustGroupSynced = newViewAdjustGroupSynced;
                UpdateGroupedValues("PE/ViewAdjust", _viewAdjustGroupSynced, false); // No "Reset" for ViewAdjust
            }

            // HandAdjust group checkbox
            bool newHandAdjustGroupSynced = EditorGUILayout.Toggle(
                handContent,
                _handAdjustGroupSynced
            );
            if (newHandAdjustGroupSynced != _handAdjustGroupSynced)
            {
                _handAdjustGroupSynced = newHandAdjustGroupSynced;
                UpdateGroupedValues("PE/HandAdjust", _handAdjustGroupSynced, false); // No "Reset" for HandAdjust
            }

            // HeadAdjust group checkbox
            bool newHeadAdjustGroupSynced = EditorGUILayout.Toggle(
                headContent,
                _headAdjustGroupSynced
            );
            if (newHeadAdjustGroupSynced != _headAdjustGroupSynced)
            {
                _headAdjustGroupSynced = newHeadAdjustGroupSynced;
                UpdateGroupedValues("PE/HeadAdjust", _headAdjustGroupSynced, true); // Exclude "Reset"
            }
            
            // TiltAdjust group checkbox
            bool newTiltAdjustGroupSynced = EditorGUILayout.Toggle(
                tiltContent,
                _tiltAdjustGroupSynced
                );
            if (newTiltAdjustGroupSynced != _tiltAdjustGroupSynced)
            {
                _tiltAdjustGroupSynced = newTiltAdjustGroupSynced;
                UpdateGroupedValues("PE/TiltAdjust", _tiltAdjustGroupSynced, true); // Exclude "Reset"
            }
            
            // OffsetAdjust group checkbox
            bool newOffsetAdjustGroupSynced = EditorGUILayout.Toggle(
                offsetContent,
                _offsetAdjustGroupSynced
                );
            if (newOffsetAdjustGroupSynced != _offsetAdjustGroupSynced)
            {
                _offsetAdjustGroupSynced = newOffsetAdjustGroupSynced;
                UpdateGroupedValues("PE/OffsetAdjust", _offsetAdjustGroupSynced, true); // Exclude "Reset"
            }

            // Calculate cost
            int parameterCost = BaseCost;
            if (_heightSynced) parameterCost += HeightCost;
            if (_mirrorSynced) parameterCost += MirrorCost;
            if (_viewAdjustGroupSynced) parameterCost += ViewAdjustCost;
            if (_handAdjustGroupSynced) parameterCost += HandAdjustCost;
            if (_headAdjustGroupSynced) parameterCost += HeadAdjustCost;
            if (_tiltAdjustGroupSynced) parameterCost += TiltAdjustCost;
            if (_offsetAdjustGroupSynced) parameterCost += OffsetAdjustCost;

            // Display cost
            GUIContent parameterCostContent = new GUIContent(
                $"Parameter Cost: {parameterCost}",
                "This value represents how much syncing Poses Extension with you " +
                "current selection will cost by itself. It does not consider your " +
                "current avatars synced parameters."
            );
            GUILayout.Space(10);
            EditorGUILayout.LabelField(parameterCostContent);

            GUILayout.FlexibleSpace();
            GUILayout.Label("BUDDYWORKS Poses Extension", EditorStyles.boldLabel);
            Rect labelRect = GUILayoutUtility.GetLastRect();
            if (
                Event.current.type == EventType.MouseDown &&
                labelRect.Contains(Event.current.mousePosition)
            )
            {
                Application.OpenURL("https://buddyworks.wtf");
            }

            GUILayout.EndVertical();
            GUILayout.Space(4); // Right padding
            GUILayout.EndHorizontal();
        }

        private void UpdateAssetValue(string parameterName, bool value)
        {
            for (int i = 0; i < _parameters.arraySize; i++)
            {
                var entry = _parameters.GetArrayElementAtIndex(i);
                var nameProp = entry.FindPropertyRelative("name");
                var networkSyncedProp = entry.FindPropertyRelative("networkSynced");

                if (
                    nameProp != null &&
                    networkSyncedProp != null &&
                    nameProp.stringValue == parameterName
                )
                {
                    networkSyncedProp.intValue = value ? 1 : 0;
                    SaveChanges();
                }
            }
        }

        // Modified UpdateGroupedValues to include an 'excludeReset' parameter
        private void UpdateGroupedValues(string prefix, bool value, bool excludeReset)
        {
            for (int i = 0; i < _parameters.arraySize; i++)
            {
                var entry = _parameters.GetArrayElementAtIndex(i);
                var nameProp = entry.FindPropertyRelative("name");
                var networkSyncedProp = entry.FindPropertyRelative("networkSynced");

                if (nameProp == null || networkSyncedProp == null) continue;

                string parameterName = nameProp.stringValue;

                if (parameterName.StartsWith(prefix))
                {
                    // Add the "Reset" exclusion logic here
                    if (excludeReset && parameterName.Contains("Reset"))
                    {
                        continue; // Skip parameters that contain "Reset" if excludeReset is true
                    }

                    networkSyncedProp.intValue = value ? 1 : 0;
                }
            }
            SaveChanges();
        }

        private void SaveChanges()
        {
            _serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}