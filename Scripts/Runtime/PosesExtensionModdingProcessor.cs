#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace BUDDYWORKS.PosesExtension
{
    public static class PosesExtensionModdingProcessor
    {
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

        /// <summary>
        /// Applies custom dance AnimationClips to the main Animator Controller.
        /// </summary>
        /// <param name="poseExtension">The BWPosesExtension component containing custom dance settings.</param>
        /// <param name="avatarName">The name of the avatar being processed (for logging).</param>
        /// <returns>True if the AnimatorController was modified, false otherwise.</returns>
        public static bool ApplyCustomDances(BWPosesExtension poseExtension, string avatarName)
        {
            Debug.Log(
                $"[PosesExtension] (Modding) Applying custom dance animations for {avatarName}."
            );

            string controllerPath = AssetDatabase.GUIDToAssetPath(AnimatorControllerGUID);
            if (string.IsNullOrEmpty(controllerPath))
            {
                Debug.LogError(
                    $"[PosesExtension] (Modding) Failed to find AnimatorController asset path for GUID: {AnimatorControllerGUID}. Is the asset missing or corrupted? Cannot apply custom dances."
                );
                return false;
            }

            AnimatorController animatorController =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (animatorController == null)
            {
                Debug.LogError(
                    $"[PosesExtension] (Modding) Failed to load AnimatorController asset at path: {controllerPath} (GUID: {AnimatorControllerGUID}). Cannot apply custom dances."
                );
                return false;
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
            Dictionary<long, AnimatorState> statesByLocalId =
                new Dictionary<long, AnimatorState>();

            foreach (Object obj in allAssets)
            {
                if (obj is AnimatorState state)
                {
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                        state,
                        out _,
                        out long localId
                    ))
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
                    Debug.Log(
                        $"[PosesExtension] (Modding) Changed motion for state '{danceAState.name}' (ID: {DanceALocalID}) to '{targetMotion.name}' in controller '{animatorController.name}'."
                    );
                }
                else if (targetMotion == null)
                {
                    Debug.LogWarning(
                        $"[PosesExtension] (Modding) No motion found for Dance A state (ID: {DanceALocalID}). This state might be empty or missing its fallback. Skipping."
                    );
                }
            }
            else
            {
                Debug.LogWarning(
                    $"[PosesExtension] (Modding) AnimatorState with Local ID {DanceALocalID} (Dance A) not found in controller '{animatorController.name}'. Skipping dance A modification."
                );
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
                    Debug.Log(
                        $"[PosesExtension] (Modding) Changed motion for state '{danceBState.name}' (ID: {DanceBLocalID}) to '{targetMotion.name}' in controller '{animatorController.name}'."
                    );
                }
                else if (targetMotion == null)
                {
                    Debug.LogWarning(
                        $"[PosesExtension] (Modding) No motion found for Dance B state (ID: {DanceBLocalID}). This state might be empty or missing its fallback. Skipping."
                    );
                }
            }
            else
            {
                Debug.LogWarning(
                    $"[PosesExtension] (Modding) AnimatorState with Local ID {DanceBLocalID} (Dance B) not found in controller '{animatorController.name}'. Skipping dance B modification."
                );
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
                    Debug.Log(
                        $"[PosesExtension] (Modding) Changed motion for state '{danceCState.name}' (ID: {DanceCLocalID}) to '{targetMotion.name}' in controller '{animatorController.name}'."
                    );
                }
                else if (targetMotion == null)
                {
                    Debug.LogWarning(
                        $"[PosesExtension] (Modding) No motion found for Dance C state (ID: {DanceCLocalID}). This state might be empty or missing its fallback. Skipping."
                    );
                }
            }
            else
            {
                Debug.LogWarning(
                    $"[PosesExtension] (Modding) AnimatorState with Local ID {DanceCLocalID} (Dance C) not found in controller '{animatorController.name}'. Skipping dance C modification."
                );
            }

            if (controllerModified)
            {
                EditorUtility.SetDirty(animatorController);
                Debug.Log(
                    $"[PosesExtension] (Modding) AnimatorController {animatorController.name} modified and marked dirty."
                );
                return true;
            }
            else
            {
                Debug.Log(
                    $"[PosesExtension] (Modding) No changes made to AnimatorController {animatorController.name}."
                );
                return false;
            }
        }
    }
}
#endif