using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq; // Still needed for Directory, File.Exists in processor, but not in editor UI

#if UNITY_EDITOR

namespace BUDDYWORKS.PosesExtension
{
    [CustomEditor(typeof(BWPosesExtension))]
    public class PosesExtensionEditor : Editor
    {
        // --- Properties for Sync Tab ---
        private SerializedProperty _propSyncViewAdjustGroup;
        private SerializedProperty _propSyncHandAdjustGroup;
        private SerializedProperty _propSyncHeadAdjustGroup;
        private SerializedProperty _propSyncTiltAdjustGroup;
        private SerializedProperty _propSyncOffsetAdjustGroup;
        private SerializedProperty _propSyncHeight;
        private SerializedProperty _propSyncMirror;

        // Reference to the target component (BWPosesExtension) for cost calculation
        private BWPosesExtension _targetComponent;

        // --- Properties for Multiplier/Sensitivity directly on BWPosesExtension ---
        private SerializedProperty _propHeightAdjustMultiplier;
        private SerializedProperty _propViewAdjustSensitivity;
        
        private int _selectedTab;
        private readonly string[] _tabNames = { "Sync", "Modding", "Other" };

        // Banner Utility constants
        private const float OriginalBannerWidth = 3840f;
        private const float OriginalBannerHeight = 1280f;
        private const float BannerAspectRatio = OriginalBannerWidth / OriginalBannerHeight; // Should be 3.0f

        private void OnEnable()
        {
            // --- Sync Tab Initialization ---
            _targetComponent = (BWPosesExtension)target;

            _propSyncViewAdjustGroup = serializedObject.FindProperty("_syncViewAdjustGroup");
            _propSyncHandAdjustGroup = serializedObject.FindProperty("_syncHandAdjustGroup");
            _propSyncHeadAdjustGroup = serializedObject.FindProperty("_syncHeadAdjustGroup");
            _propSyncTiltAdjustGroup = serializedObject.FindProperty("_syncTiltAdjustGroup");
            _propSyncOffsetAdjustGroup = serializedObject.FindProperty("_syncOffsetAdjustGroup");
            _propSyncHeight = serializedObject.FindProperty("_syncHeight");
            _propSyncMirror = serializedObject.FindProperty("_syncMirror");
            
            // --- Other Tab Property Initialization from BWPosesExtension ---
            _propHeightAdjustMultiplier = serializedObject.FindProperty("_heightAdjustMultiplier");
            _propViewAdjustSensitivity = serializedObject.FindProperty("_viewAdjustSensitivity");
        }

        private void OnDisable()
        {
            // No custom cleanup needed; Unity handles saving for SerializedProperties.
        }

        public override void OnInspectorGUI()
        {
            BannerUtility.DrawBanner();
            
            GUILayout.Space(4); 
            
            serializedObject.Update(); // Update SerializedObject for BWPosesExtension

            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);

            switch (_selectedTab)
            {
                case 0: // Sync Tab
                    DrawSyncTab(); 
                    break;
                case 1: // Modding Tab
                    DrawModdingTab();
                    break;
                case 2: // Other Tab
                    DrawOtherTab();
                    break;
            }

            serializedObject.ApplyModifiedProperties(); // Apply changes to BWPosesExtension
        }

        // --- DrawSyncTab() ---
        private void DrawSyncTab()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(4); 
            GUILayout.BeginVertical();

            GUILayout.Label("Select which features are synced to other players.", EditorStyles.boldLabel);
            Rect r = EditorGUILayout.GetControlRect(false, 1, new GUIStyle() { margin = new RectOffset(0, 0, 4, 4) });
            EditorGUI.DrawRect(r, Color.gray);

            GUIContent mirrorContent = new GUIContent("Mirror Pose", "Mirror Pose will flip your current pose, useful if you want to face a different direction.\n\nWe recommend leaving this core feature synced.");
            GUIContent heightContent = new GUIContent("Height Adjust", "Height Adjust allows you to adjust the height of your avatar during posing.\nUseful to account for colliders or the lack thereof.\n\nWe recommend leaving this core feature synced.");
            GUIContent viewContent = new GUIContent("View Adjust", "View Adjust lets you change your avatars viewing direction, giving more control.\n\nTakes quite a few parameters, sync if needed.");
            GUIContent handContent = new GUIContent("Hand Adjust", "Hand Adjust allows you to change the hand gestures of poses from a set of generic ones.\n\nTakes quite a few parameters, sync if needed.");
            GUIContent headContent = new GUIContent("Head Adjust", "Head Adjust lets you change your avatars head rotation and tilt, allowing for many new angles.\n\nThis costs many parameters, sync if needed.");
            GUIContent tiltContent = new GUIContent("Tilt Adjust", "Tilt Adjust allows for more angles for your photos.\n\nThis costs quite a few parameters, sync if needed.");
            GUIContent offsetContent = new GUIContent("Offset Adjust", "Offset Adjust lets you account for various body shapes.\n\nThis costs many parameters, sync if needed.");

            if (_propSyncMirror != null) EditorGUILayout.PropertyField(_propSyncMirror, mirrorContent);
            if (_propSyncHeight != null) EditorGUILayout.PropertyField(_propSyncHeight, heightContent);
            if (_propSyncViewAdjustGroup != null) EditorGUILayout.PropertyField(_propSyncViewAdjustGroup, viewContent);
            if (_propSyncHandAdjustGroup != null) EditorGUILayout.PropertyField(_propSyncHandAdjustGroup, handContent);
            if (_propSyncHeadAdjustGroup != null) EditorGUILayout.PropertyField(_propSyncHeadAdjustGroup, headContent);
            if (_propSyncTiltAdjustGroup != null) EditorGUILayout.PropertyField(_propSyncTiltAdjustGroup, tiltContent);
            if (_propSyncOffsetAdjustGroup != null) EditorGUILayout.PropertyField(_propSyncOffsetAdjustGroup, offsetContent);

            int parameterCost = _targetComponent.CalculateParameterCost();
            GUIContent parameterCostContent = new GUIContent($"Parameter Cost: {parameterCost}", "This value represents how much syncing Poses Extension with your current selection will cost by itself. It does not consider your current avatar's synced parameters.");
            GUILayout.Space(10);
            EditorGUILayout.LabelField(parameterCostContent);

            GUILayout.FlexibleSpace();

            EditorGUILayout.HelpBox("Useful if you have a photographer, though it does incur a parameter cost.\nYou'll still have access to all features locally, regardless of your selection.", MessageType.Info);
            
            GUILayout.EndVertical();
            GUILayout.Space(4); 
            GUILayout.EndHorizontal();
        }

        // --- DrawModdingTab() ---
        private void DrawModdingTab()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(4); 
            GUILayout.BeginVertical();

            GUILayout.Label("Add your own pose and dance data.", EditorStyles.boldLabel);
            Rect r = EditorGUILayout.GetControlRect(false, 1, new GUIStyle() { margin = new RectOffset(0, 0, 4, 4) });
            EditorGUI.DrawRect(r, Color.gray);

            GUIContent customPosebankA = new GUIContent("Custom Pose Bank", "Insert the posebank animationclip here.");
            GUIContent customPosebankB = new GUIContent("Custom Pose Bank Mirror",
                "Slot in an animation here if you want to use the mirror slot for an additional pose bank. Note that this replaces the mirror option for the custom pose bank.");
            GUIContent customDanceA = new GUIContent("Custom Dance A", "Insert the dance animationclip here.");
            GUIContent customDanceB = new GUIContent("Custom Dance B", "Insert the dance animationclip here.");
            GUIContent customDanceC = new GUIContent("Custom Dance C", "Insert the dance animationclip here.");
            
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.HelpBox("Introduce your own dances and pose banks here.\nPlease review the docs for proper setup of those animation clips.", MessageType.Info);
            
            GUILayout.EndVertical();
            GUILayout.Space(4); 
            GUILayout.EndHorizontal();
        }
        
        // --- DrawOtherTab() ---
        private void DrawOtherTab()
        {
            if (_propHeightAdjustMultiplier == null || _propViewAdjustSensitivity == null)
            {
                EditorGUILayout.LabelField("Other tab settings failed to load properties. See console for errors.");
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(4); 
            GUILayout.BeginVertical();

            GUILayout.Label("Modify various aspects of Poses Extension.", EditorStyles.boldLabel);
            Rect r = EditorGUILayout.GetControlRect(false, 1, new GUIStyle() { margin = new RectOffset(0, 0, 4, 4) });
            EditorGUI.DrawRect(r, Color.gray);

            if (_propHeightAdjustMultiplier != null)
            {
                EditorGUILayout.PropertyField(_propHeightAdjustMultiplier, new GUIContent("Height Adjust Multiplier"));
            }

            if (_propViewAdjustSensitivity != null)
            {
                EditorGUILayout.PropertyField(_propViewAdjustSensitivity, new GUIContent("View Adjust Sensitivity"));
            }
            
            GUILayout.FlexibleSpace();

            EditorGUILayout.HelpBox("These settings allow you to change some ranges of PE adjustment features.\nThey are meant to offset some specific avatar setups, so use with care.", MessageType.Info);
            
            GUILayout.EndVertical();
            GUILayout.Space(4); 
            GUILayout.EndHorizontal();
        }
    }
}
#endif