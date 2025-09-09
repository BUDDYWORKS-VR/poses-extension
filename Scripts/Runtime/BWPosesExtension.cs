#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;
using VRC.SDK3.Avatars.ScriptableObjects;
using UnityEditor.Animations; // Needed for AnimatorController, AnimatorStateMachine, AnimatorState

namespace BUDDYWORKS.PosesExtension
{
    // BWPosesExtension.cs (No changes, remains as the component holding all settings)
    [AddComponentMenu("BUDDYWORKS/Poses Extension")]
    public class BWPosesExtension : MonoBehaviour, IEditorOnly
    {
        [Header("Sync Settings")]
        [Tooltip("Enable/Disable syncing of Mirror Pose parameter to other players.")]
        [SerializeField] private bool _syncMirror = true; 
        
        [Tooltip("Enable/Disable syncing of Height Adjust parameter to other players.")]
        [SerializeField] private bool _syncHeight = true; 

        [Tooltip("Enable/Disable syncing of View Adjust parameters to other players.")]
        [SerializeField] private bool _syncViewAdjustGroup = false; 
        
        [Tooltip("Enable/Disable syncing of Hand Adjust parameters to other players.")]
        [SerializeField] private bool _syncHandAdjustGroup = false;
        
        [Tooltip("Enable/Disable syncing of Head Adjust parameters to other players.")]
        [SerializeField] private bool _syncHeadAdjustGroup = false;
        
        [Tooltip("Enable/Disable syncing of Tilt Adjust parameters to other players.")]
        [SerializeField] private bool _syncTiltAdjustGroup = false;
        
        [Tooltip("Enable/Disable syncing of Offset Adjust parameters to other players.")]
        [SerializeField] private bool _syncOffsetAdjustGroup = false;
        
        public bool SyncViewAdjustGroup => _syncViewAdjustGroup;
        public bool SyncHandAdjustGroup => _syncHandAdjustGroup;
        public bool SyncHeadAdjustGroup => _syncHeadAdjustGroup;
        public bool SyncTiltAdjustGroup => _syncTiltAdjustGroup;
        public bool SyncOffsetAdjustGroup => _syncOffsetAdjustGroup;
        public bool SyncHeight => _syncHeight;
        public bool SyncMirror => _syncMirror;

        [Header("Modding Settings")]
        [Tooltip("Optional: Assign a custom AnimationClip for Dance A. If left empty, a fallback motion will be used.")]
        [SerializeField] private AnimationClip _customDanceA;
        [Tooltip("Optional: Assign a custom AnimationClip for Dance B. If left empty, a fallback motion will be used.")]
        [SerializeField] private AnimationClip _customDanceB;
        [Tooltip("Optional: Assign a custom AnimationClip for Dance C. If left empty, a fallback motion will be used.")]
        [SerializeField] private AnimationClip _customDanceC;

        public AnimationClip CustomDanceA => _customDanceA;
        public AnimationClip CustomDanceB => _customDanceB;
        public AnimationClip CustomDanceC => _customDanceC;


        [Header("Adjustment Ranges")]
        [Tooltip("Multiplies the default height adjustment range. 1 = default, 2 = double range.")]
        [Range(1f, 2f)]
        [SerializeField] private float _heightAdjustMultiplier = 1f;
        
        [Tooltip("Controls the sensitivity of view adjustments. 0 = no movement, 1 = default, 2 = double sensitivity.")]
        [Range(0f, 2f)]
        [SerializeField] private float _viewAdjustSensitivity = 1f;

        public float HeightAdjustMultiplier => _heightAdjustMultiplier;
        public float ViewAdjustSensitivity => _viewAdjustSensitivity;

        // Constants for parameter costs
        private const int BaseCost = 16;
        private const int HeightCost = 8;
        private const int MirrorCost = 1;
        private const int ViewAdjustCost = 17;
        private const int HandAdjustCost = 17;
        private const int HeadAdjustCost = 25;
        private const int TiltAdjustCost = 9;
        private const int OffsetAdjustCost = 25;

        public int CalculateParameterCost()
        {
            int cost = BaseCost;
            if (_syncHeight) cost += HeightCost;
            if (_syncMirror) cost += MirrorCost;
            if (_syncViewAdjustGroup) cost += ViewAdjustCost;
            if (_syncHandAdjustGroup) cost += HandAdjustCost;
            if (_syncHeadAdjustGroup) cost += HeadAdjustCost;
            if (_syncTiltAdjustGroup) cost += TiltAdjustCost;
            if (_syncOffsetAdjustGroup) cost += OffsetAdjustCost;
            return cost;
        }
    }

    // PosesExtensionProcessor.cs
    public class PosesExtensionProcessor : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => -100000000;

        private const string SyncParameterAssetGUID = "17cce2e1703370a41b2f584d6364944a";
        private const string AnimatorControllerGUID = "860fa19ea5c08d741ad8bace141a52d3";

        // Dance A
        private const long DanceALocalID = -4942040831587697338L;
        private const string DanceAFallbackGUID = "84c8e95c507fba643a7f8e30d7f8707b";

        // Dance B
        private const long DanceBLocalID = 7611955647017116060L;
        private const string DanceBFallbackGUID = "85536172e9c0d0740ab34a1e90fac770";

        // Dance C
        private const long DanceCLocalID = 8017281320110375518L;
        private const string DanceCFallbackGUID = "eaa4dd4845114f14186da152d65ef35f";

        private bool _processorAssetsWereDirty = false;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            _processorAssetsWereDirty = false;

            var poseExtension = avatarGameObject.GetComponentInChildren<BWPosesExtension>(true);
            if (poseExtension == null)
            {
                Debug.Log($"[PosesExtension] No BWPosesExtension component found on avatar {avatarGameObject.name}. Skipping all Poses Extension processing.");
                return true;
            }

            Debug.Log($"[PosesExtension] Starting Poses Extension preprocessing for avatar: {avatarGameObject.name}");

            // --- Step 1: Apply Sync Parameter Changes to VRCExpressionParameters Asset ---
            ApplySyncParameterChanges(poseExtension, avatarGameObject.name);

            // --- Step 2: Apply Height/View Adjustments to Animation Clips ---
            ApplyAnimationAdjustments(poseExtension.HeightAdjustMultiplier, poseExtension.ViewAdjustSensitivity);

            // --- Step 3: Apply Custom Dances to Animator Controller ---
            ApplyCustomDances(poseExtension, avatarGameObject.name);

            if (_processorAssetsWereDirty)
            {
                AssetDatabase.SaveAssets(); 
                AssetDatabase.Refresh(); 
                Debug.Log($"[PosesExtension] All modified Poses Extension assets saved and refreshed for {avatarGameObject.name}.");
            }
            else
            {
                Debug.Log($"[PosesExtension] No animation clip adjustments or controller changes applied for {avatarGameObject.name}.");
            }

            Debug.Log($"[PosesExtension] Finished Poses Extension preprocessing for avatar: {avatarGameObject.name}. Total Parameter Cost: {poseExtension.CalculateParameterCost()}");

            return true;
        }

        private void ApplySyncParameterChanges(BWPosesExtension poseExtension, string avatarName)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(SyncParameterAssetGUID);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError($"[PosesExtension] (Sync) Failed to find asset path for GUID: {SyncParameterAssetGUID}. Is the asset missing or corrupted? Cannot modify sync parameters.");
                return;
            }
            
            VRCExpressionParameters parametersAsset = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(assetPath);
            if (parametersAsset == null)
            {
                Debug.LogError($"[PosesExtension] (Sync) Failed to load VRCExpressionParameters asset at path: {assetPath} (GUID: {SyncParameterAssetGUID}). Cannot modify sync parameters.");
                return;
            }

            SerializedObject serializedParametersAsset = new SerializedObject(parametersAsset);
            SerializedProperty parametersProp = serializedParametersAsset.FindProperty("parameters"); 

            if (parametersProp == null || !parametersProp.isArray)
            {
                Debug.LogError($"[PosesExtension] (Sync) Parameters array property 'parameters' not found or is not an array on asset: {assetPath}. Cannot modify sync parameters.");
                return;
            }

            bool syncParametersModified = false;

            for (int i = 0; i < parametersProp.arraySize; i++)
            {
                SerializedProperty parameterEntry = parametersProp.GetArrayElementAtIndex(i);
                SerializedProperty nameProp = parameterEntry.FindPropertyRelative("name");
                SerializedProperty networkSyncedProp = parameterEntry.FindPropertyRelative("networkSynced");

                if (nameProp == null || networkSyncedProp == null) continue;

                string parameterName = nameProp.stringValue;
                bool currentNetworkSynced = (networkSyncedProp.propertyType == SerializedPropertyType.Boolean) ? networkSyncedProp.boolValue : (networkSyncedProp.intValue == 1);
                bool newNetworkSynced = currentNetworkSynced;

                if (parameterName == "PE/Mirror")
                {
                    newNetworkSynced = poseExtension.SyncMirror;
                }
                else if (parameterName == "PE/Height")
                {
                    newNetworkSynced = poseExtension.SyncHeight;
                }
                else if (parameterName.StartsWith("PE/ViewAdjust"))
                {
                    newNetworkSynced = poseExtension.SyncViewAdjustGroup;
                }
                else if (parameterName.StartsWith("PE/HandAdjust"))
                {
                    newNetworkSynced = poseExtension.SyncHandAdjustGroup;
                }
                else if (parameterName.StartsWith("PE/HeadAdjust"))
                {
                    if (!parameterName.Contains("Reset"))
                    {
                        newNetworkSynced = poseExtension.SyncHeadAdjustGroup;
                    }
                }
                else if (parameterName.StartsWith("PE/TiltAdjust"))
                {
                    if (!parameterName.Contains("Reset"))
                    {
                        newNetworkSynced = poseExtension.SyncTiltAdjustGroup;
                    }
                }
                else if (parameterName.StartsWith("PE/OffsetAdjust"))
                {
                    if (!parameterName.Contains("Reset"))
                    {
                        newNetworkSynced = poseExtension.SyncOffsetAdjustGroup;
                    }
                }

                if (newNetworkSynced != currentNetworkSynced)
                {
                    if (networkSyncedProp.propertyType == SerializedPropertyType.Boolean)
                    {
                        networkSyncedProp.boolValue = newNetworkSynced;
                    }
                    else
                    {
                        networkSyncedProp.intValue = newNetworkSynced ? 1 : 0;
                    }
                    syncParametersModified = true;
                    Debug.Log($"[PosesExtension] (Sync) Changed networkSynced for parameter '{parameterName}' to: {newNetworkSynced}");
                }
            }

            if (syncParametersModified)
            {
                serializedParametersAsset.ApplyModifiedProperties();
                EditorUtility.SetDirty(parametersAsset);
                _processorAssetsWereDirty = true;
                Debug.Log($"[PosesExtension] (Sync) Parameters asset {assetPath} modified and marked dirty.");
            }
            else
            {
                Debug.Log($"[PosesExtension] (Sync) No changes made to parameters asset {assetPath}.");
            }
        }

        private void ApplyAnimationAdjustments(float heightMultiplier, float viewSensitivity)
        {
            Debug.Log($"[PosesExtension] (Adjustments) Applying height adjustment multiplier: {heightMultiplier}");
            ChangeHeightAdjustRange(multiplier: heightMultiplier);

            Debug.Log($"[PosesExtension] (Adjustments) Applying view adjustment sensitivity: {viewSensitivity}");
            ChangeViewAdjustRange(multiplier: viewSensitivity);
        }

        private void ApplyCustomDances(BWPosesExtension poseExtension, string avatarName)
        {
            Debug.Log($"[PosesExtension] (Modding) Applying custom dance animations for {avatarName}.");

            string controllerPath = AssetDatabase.GUIDToAssetPath(AnimatorControllerGUID);
            if (string.IsNullOrEmpty(controllerPath))
            {
                Debug.LogError($"[PosesExtension] (Modding) Failed to find AnimatorController asset path for GUID: {AnimatorControllerGUID}. Is the asset missing or corrupted? Cannot apply custom dances.");
                return;
            }

            AnimatorController animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (animatorController == null)
            {
                Debug.LogError($"[PosesExtension] (Modding) Failed to load AnimatorController asset at path: {controllerPath} (GUID: {AnimatorControllerGUID}). Cannot apply custom dances.");
                return;
            }

            bool controllerModified = false;

            // Helper to get motion by GUID
            Motion GetMotionByGUID(string guid)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path))
                {
                    return AssetDatabase.LoadAssetAtPath<Motion>(path);
                }
                Debug.LogError($"[PosesExtension] (Modding) Failed to load Motion asset for GUID: {guid}.");
                return null;
            }

            // Load all sub-assets of the Animator Controller to find states by local ID
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(controllerPath);
            Dictionary<long, AnimatorState> statesByLocalId = new Dictionary<long, AnimatorState>();

            foreach (Object obj in allAssets)
            {
                if (obj is AnimatorState state)
                {
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(state, out _, out long localId))
                    {
                        statesByLocalId[localId] = state;
                    }
                }
            }

            // Apply custom dance A
            if (statesByLocalId.TryGetValue(DanceALocalID, out AnimatorState danceAState))
            {
                Motion targetMotion = poseExtension.CustomDanceA;
                if (targetMotion == null) targetMotion = GetMotionByGUID(DanceAFallbackGUID);

                if (targetMotion != null && danceAState.motion != targetMotion)
                {
                    danceAState.motion = targetMotion;
                    controllerModified = true;
                    Debug.Log($"[PosesExtension] (Modding) Changed motion for state '{danceAState.name}' (ID: {DanceALocalID}) to '{targetMotion.name}' in controller '{animatorController.name}'.");
                }
                else if (targetMotion == null)
                {
                    Debug.LogWarning($"[PosesExtension] (Modding) No motion found for Dance A state (ID: {DanceALocalID}). This state might be empty or missing its fallback. Skipping.");
                }
            }
            else
            {
                Debug.LogWarning($"[PosesExtension] (Modding) AnimatorState with Local ID {DanceALocalID} (Dance A) not found in controller '{animatorController.name}'. Skipping dance A modification.");
            }

            // Apply custom dance B
            if (statesByLocalId.TryGetValue(DanceBLocalID, out AnimatorState danceBState))
            {
                Motion targetMotion = poseExtension.CustomDanceB;
                if (targetMotion == null) targetMotion = GetMotionByGUID(DanceBFallbackGUID);

                if (targetMotion != null && danceBState.motion != targetMotion)
                {
                    danceBState.motion = targetMotion;
                    controllerModified = true;
                    Debug.Log($"[PosesExtension] (Modding) Changed motion for state '{danceBState.name}' (ID: {DanceBLocalID}) to '{targetMotion.name}' in controller '{animatorController.name}'.");
                }
                else if (targetMotion == null)
                {
                    Debug.LogWarning($"[PosesExtension] (Modding) No motion found for Dance B state (ID: {DanceBLocalID}). This state might be empty or missing its fallback. Skipping.");
                }
            }
            else
            {
                Debug.LogWarning($"[PosesExtension] (Modding) AnimatorState with Local ID {DanceBLocalID} (Dance B) not found in controller '{animatorController.name}'. Skipping dance B modification.");
            }

            // Apply custom dance C
            if (statesByLocalId.TryGetValue(DanceCLocalID, out AnimatorState danceCState))
            {
                Motion targetMotion = poseExtension.CustomDanceC;
                if (targetMotion == null) targetMotion = GetMotionByGUID(DanceCFallbackGUID);

                if (targetMotion != null && danceCState.motion != targetMotion)
                {
                    danceCState.motion = targetMotion;
                    controllerModified = true;
                    Debug.Log($"[PosesExtension] (Modding) Changed motion for state '{danceCState.name}' (ID: {DanceCLocalID}) to '{targetMotion.name}' in controller '{animatorController.name}'.");
                }
                else if (targetMotion == null)
                {
                    Debug.LogWarning($"[PosesExtension] (Modding) No motion found for Dance C state (ID: {DanceCLocalID}). This state might be empty or missing its fallback. Skipping.");
                }
            }
            else
            {
                Debug.LogWarning($"[PosesExtension] (Modding) AnimatorState with Local ID {DanceCLocalID} (Dance C) not found in controller '{animatorController.name}'. Skipping dance C modification.");
            }

            if (controllerModified)
            {
                EditorUtility.SetDirty(animatorController);
                _processorAssetsWereDirty = true;
                Debug.Log($"[PosesExtension] (Modding) AnimatorController {animatorController.name} modified and marked dirty.");
            }
            else
            {
                Debug.Log($"[PosesExtension] (Modding) No changes made to AnimatorController {animatorController.name}.");
            }
        }

        // The GetAllStates method is no longer strictly needed for this specific local ID approach
        // as we are loading all sub-assets and querying them directly.
        // I'll keep it commented out for now, as it might be useful for other scenarios.
        /*
        private IEnumerable<AnimatorState> GetAllStates(AnimatorStateMachine stateMachine)
        {
            foreach (AnimatorState state in stateMachine.states.Select(s => s.state))
            {
                yield return state;
            }

            foreach (ChildAnimatorStateMachine subStateMachine in stateMachine.stateMachine)
            {
                foreach (AnimatorState state in GetAllStates(subStateMachine.stateMachine))
                {
                    yield return state;
                }
            }
        }
        */

        private void ChangeHeightAdjustRange(float multiplier)
        {
            string rootPath = "Packages/wtf.buddyworks.posesextension/Data/AnimationClips/Adjustments/HeightAdjust";
            if (!Directory.Exists(rootPath))
            {
                Debug.LogError($"[PosesExtension] (HeightAdjust) Folder not found: {rootPath}");
                return;
            }

            string lowClipPath = Directory.GetFiles(rootPath, "*-Low.anim", SearchOption.TopDirectoryOnly).FirstOrDefault();
            string highClipPath = Directory.GetFiles(rootPath, "*-High.anim", SearchOption.TopDirectoryOnly).FirstOrDefault();

            if (string.IsNullOrEmpty(lowClipPath) || string.IsNullOrEmpty(highClipPath))
            {
                Debug.LogError($"[PosesExtension] (HeightAdjust) Could not find animation clips in {rootPath}");
                return;
            }

            AnimationClip lowClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(lowClipPath);
            AnimationClip highClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(highClipPath);

            if (lowClip != null && highClip != null)
            {
                ApplyHeightAdjust(lowClip, -0.5f, multiplier);
                ApplyHeightAdjust(highClip, 2f, multiplier);
            }
            else
            {
                Debug.LogError($"[PosesExtension] (HeightAdjust) Could not load animation clips in {rootPath}");
            }
        }

        private void ChangeViewAdjustRange(float multiplier)
        {
            string rootPath = "Packages/wtf.buddyworks.posesextension/Data/AnimationClips/Adjustments/ViewAdjust";
            if (!Directory.Exists(rootPath))
            {
                Debug.LogError($"[PosesExtension] (ViewAdjust) Folder not found: {rootPath}");
                return;
            }

            string xPlusClipPath = Directory.GetFiles(rootPath, "X+.anim", SearchOption.TopDirectoryOnly).FirstOrDefault();
            string xMinusClipPath = Directory.GetFiles(rootPath, "X-.anim", SearchOption.TopDirectoryOnly).FirstOrDefault();
            string yPlusClipPath = Directory.GetFiles(rootPath, "Y+.anim", SearchOption.TopDirectoryOnly).FirstOrDefault();
            string yMinusClipPath = Directory.GetFiles(rootPath, "Y-.anim", SearchOption.TopDirectoryOnly).FirstOrDefault();

            if (string.IsNullOrEmpty(xPlusClipPath) || string.IsNullOrEmpty(xMinusClipPath) ||
                string.IsNullOrEmpty(yPlusClipPath) || string.IsNullOrEmpty(yMinusClipPath))
            {
                Debug.LogError($"[PosesExtension] (ViewAdjust) Could not find animation clips in {rootPath}");
                return;
            }

            AnimationClip xPlusClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(xPlusClipPath);
            AnimationClip xMinusClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(xMinusClipPath);
            AnimationClip yPlusClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(yPlusClipPath);
            AnimationClip yMinusClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(yMinusClipPath);

            if (xPlusClip != null && xMinusClip != null && yPlusClip != null && yMinusClip != null)
            {
                float xPlusLeftEyeBase = 2f;
                float xPlusRightEyeBase = -2f;
                float xMinusLeftEyeBase = -2f;
                float xMinusRightEyeBase = 2f;
                float yPlusLeftEyeBase = 4f;
                float yPlusRightEyeBase = 4f;
                float yMinusLeftEyeBase = -4f;
                float yMinusRightEyeBase = -4f;

                ApplyViewAdjust(xPlusClip, "Left Eye In-Out", xPlusLeftEyeBase, multiplier);
                ApplyViewAdjust(xPlusClip, "Right Eye In-Out", xPlusRightEyeBase, multiplier);

                ApplyViewAdjust(xMinusClip, "Left Eye In-Out", xMinusLeftEyeBase, multiplier);
                ApplyViewAdjust(xMinusClip, "Right Eye In-Out", xMinusRightEyeBase, multiplier);

                ApplyViewAdjust(yPlusClip, "Left Eye Down-Up", yPlusLeftEyeBase, multiplier);
                ApplyViewAdjust(yPlusClip, "Right Eye Down-Up", yPlusRightEyeBase, multiplier);

                ApplyViewAdjust(yMinusClip, "Left Eye Down-Up", yMinusLeftEyeBase, multiplier);
                ApplyViewAdjust(yMinusClip, "Right Eye Down-Up", yMinusRightEyeBase, multiplier);
            }
            else
            {
                Debug.LogError($"[PosesExtension] (ViewAdjust) Could not load animation clips in {rootPath}");
            }
        }

        private void ApplyHeightAdjust(AnimationClip clip, float baseValue, float multiplier)
        {
            float newValue = baseValue * multiplier;
            bool clipModified = false;

            // CORRECTED: Target Animator for RootT.y
            EditorCurveBinding binding = EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT.y"); // ClassID 95 for Animator
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);

            if (curve != null)
            {
                Keyframe[] keys = curve.keys;
                if (keys.Length > 0 && !Mathf.Approximately(keys[0].value, newValue))
                {
                    for (int i = 0; i < keys.Length; i++)
                    {
                        keys[i].value = newValue;
                    }
                    curve.keys = keys;
                    AnimationUtility.SetEditorCurve(clip, binding, curve);
                    clipModified = true;
                }
            }
            else if (newValue != 0f)
            {
                curve = new AnimationCurve(new Keyframe(0f, newValue));
                AnimationUtility.SetEditorCurve(clip, binding, curve);
                clipModified = true;
            }

            if (clipModified)
            {
                EditorUtility.SetDirty(clip);
                _processorAssetsWereDirty = true;
            }
        }

        private void ApplyViewAdjust(AnimationClip clip, string propertyName, float baseValue, float multiplier)
        {
            float newValue = baseValue * multiplier;
            bool clipModified = false;

            // CORRECTED BINDING TO TARGET ANIMATOR CUSTOM FLOAT PROPERTIES
            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(
                "",                 // path: empty string for Animator component directly on avatar root
                typeof(Animator),   // classID 95 is Animator
                propertyName        // The custom float property name (e.g., "Left Eye In-Out")
            );
            
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);

            if (curve != null)
            {
                Keyframe[] keys = curve.keys;
                if (keys.Length > 0 && !Mathf.Approximately(keys[0].value, newValue))
                {
                    for (int i = 0; i < keys.Length; i++)
                    {
                        keys[i].value = newValue;
                    }
                    curve.keys = keys;
                    AnimationUtility.SetEditorCurve(clip, binding, curve);
                    clipModified = true;
                }
            }
            else if (newValue != 0f)
            {
                curve = new AnimationCurve(new Keyframe(0f, newValue));
                AnimationUtility.SetEditorCurve(clip, binding, curve);
                clipModified = true;
            }

            if (clipModified)
            {
                EditorUtility.SetDirty(clip);
                _processorAssetsWereDirty = true;
            }
        }
    }
}
#endif