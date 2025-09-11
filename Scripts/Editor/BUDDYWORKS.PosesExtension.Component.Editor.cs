#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;


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

        // --- Properties for Modding Tab ---
        private SerializedProperty _propCustomDanceA;
        private SerializedProperty _propCustomDanceB;
        private SerializedProperty _propCustomDanceC;
        private SerializedProperty _propCustomPose; // ADDED: New Custom Pose field

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
            // --- Target Component Reference ---
            _targetComponent = (BWPosesExtension)target;

            // --- Sync Tab Initialization ---
            _propSyncViewAdjustGroup = serializedObject.FindProperty("_syncViewAdjustGroup");
            _propSyncHandAdjustGroup = serializedObject.FindProperty("_syncHandAdjustGroup");
            _propSyncHeadAdjustGroup = serializedObject.FindProperty("_syncHeadAdjustGroup");
            _propSyncTiltAdjustGroup = serializedObject.FindProperty("_syncTiltAdjustGroup");
            _propSyncOffsetAdjustGroup = serializedObject.FindProperty("_syncOffsetAdjustGroup");
            _propSyncHeight = serializedObject.FindProperty("_syncHeight");
            _propSyncMirror = serializedObject.FindProperty("_syncMirror");
            
            // --- Modding Tab Property Initialization ---
            _propCustomDanceA = serializedObject.FindProperty("_customDanceA");
            _propCustomDanceB = serializedObject.FindProperty("_customDanceB");
            _propCustomDanceC = serializedObject.FindProperty("_customDanceC");
            _propCustomPose = serializedObject.FindProperty("_customPose"); // ADDED: Initialize new custom pose property

            // --- Other Tab Property Initialization from BWPosesExtension ---
            _propHeightAdjustMultiplier = serializedObject.FindProperty("_heightAdjustMultiplier");
            _propViewAdjustSensitivity = serializedObject.FindProperty("_viewAdjustSensitivity");
        }

        public override void OnInspectorGUI()
        {
            // IMPORTANT: This line remains as per your original provided code.
            // If DrawBanner() needs a specific implementation, it should be provided by you.
            // I will not alter or uncomment this line or assume its implementation.
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
            
            // Custom Dance Slots
            GUIContent customDanceA = new GUIContent("Custom Dance A", "Insert the dance animation clip for slot A here. If empty, a fallback animation will be used.");
            GUIContent customDanceB = new GUIContent("Custom Dance B", "Insert the dance animation clip for slot B here. If empty, a fallback animation will be used.");
            GUIContent customDanceC = new GUIContent("Custom Dance C", "Insert the dance animation clip for slot C here. If empty, a fallback animation will be used.");
            
            // Custom Pose Slot
            GUIContent customPoseContent = new GUIContent("Custom Posebank", "Insert a custom animation clip here for the Custom Pose. If empty, a fallback animation will be used.");
            
            // Draw the property fields for custom dances
            if (_propCustomDanceA != null) EditorGUILayout.PropertyField(_propCustomDanceA, customDanceA);
            if (_propCustomDanceB != null) EditorGUILayout.PropertyField(_propCustomDanceB, customDanceB);
            if (_propCustomDanceC != null) EditorGUILayout.PropertyField(_propCustomDanceC, customDanceC);
            
            // ADDED: Draw the new property field for the custom pose
            if (_propCustomPose != null) EditorGUILayout.PropertyField(_propCustomPose, customPoseContent);


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