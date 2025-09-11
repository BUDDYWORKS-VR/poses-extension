#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase.Editor.BuildPipeline;
// No longer needed: using VRC.SDK3.Avatars.ScriptableObjects;
// No longer needed: using UnityEditor.Animations;
// No longer needed: using System.IO;
// No longer needed: using System.Linq;
// No longer needed: using System.Collections.Generic;

namespace BUDDYWORKS.PosesExtension
{
    // PosesExtensionProcessor.cs - The orchestrator
    public class PosesExtensionProcessor : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => -100000000;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            var poseExtension = avatarGameObject.GetComponentInChildren<BWPosesExtension>(true);
            if (poseExtension == null)
            {
                Debug.Log(
                    $"[PosesExtension] No BWPosesExtension component found on avatar {avatarGameObject.name}. Skipping all Poses Extension processing."
                );
                return true;
            }

            Debug.Log(
                $"[PosesExtension] Starting Poses Extension preprocessing for avatar: {avatarGameObject.name}"
            );

            bool assetsWereDirty = false;

            // --- Step 1: Apply Sync Parameter Changes to VRCExpressionParameters Asset ---
            if (
                PosesExtensionSyncProcessor.ApplySyncParameterChanges(
                    poseExtension,
                    avatarGameObject.name
                )
            )
            {
                assetsWereDirty = true;
            }

            // --- Step 2: Apply Height/View Adjustments to Animation Clips ---
            if (
                PosesExtensionAdjustmentsProcessor.ApplyAnimationAdjustments(
                    poseExtension.HeightAdjustMultiplier,
                    poseExtension.ViewAdjustSensitivity
                )
            )
            {
                assetsWereDirty = true;
            }

            // --- Step 3: Apply Custom Dances to Animator Controller ---
            if (
                PosesExtensionModdingProcessor.ApplyCustomDances(
                    poseExtension,
                    avatarGameObject.name
                )
            )
            {
                assetsWereDirty = true;
            }

            // --- Step 4: Apply Custom Pose to Blend Tree ---
            if (
                PosesExtensionCustomPoseProcessor.ApplyCustomPose(
                    poseExtension,
                    avatarGameObject.name
                )
            )
            {
                assetsWereDirty = true;
            }

            if (assetsWereDirty)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log(
                    $"[PosesExtension] All modified Poses Extension assets saved and refreshed for {avatarGameObject.name}."
                );
            }
            else
            {
                Debug.Log(
                    $"[PosesExtension] No animation clip adjustments or controller changes applied for {avatarGameObject.name}."
                );
            }

            Debug.Log(
                $"[PosesExtension] Finished Poses Extension preprocessing for avatar: {avatarGameObject.name}. Total Parameter Cost: {poseExtension.CalculateParameterCost()}"
            );

            return true;
        }
    }
}
#endif