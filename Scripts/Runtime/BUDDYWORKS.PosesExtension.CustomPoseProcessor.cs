#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace BUDDYWORKS.PosesExtension
{
    public static class PosesExtensionCustomPoseProcessor
    {
        private const string BlendTreeAssetGUID = "39fbea230978ef64191b3f573024f4a3";
        private const long ExpectedFirstSlotMotionLocalID = 7400000L;
        private const string FallbackAnimationGUID = "1b0e592d46144124c994764898c7a9bc";

        private const string TargetAlignmentClipGUID = "89328a774b3eb2242be85e49abeb9599";
        
        public static bool ApplyCustomPose(BWPosesExtension poseExtension, string avatarName)
        {
            Debug.Log($"[PosesExtension] (CustomPose) Applying custom pose for {avatarName}.");

            bool assetsWereModified = false;

            // --- Step 1: Determine the target motion (user custom or fallback) ---
            Motion targetMotion = poseExtension.CustomPose;
            if (targetMotion == null)
            {
                string fallbackPath = AssetDatabase.GUIDToAssetPath(FallbackAnimationGUID);
                if (!string.IsNullOrEmpty(fallbackPath))
                {
                    targetMotion = AssetDatabase.LoadAssetAtPath<AnimationClip>(fallbackPath);
                }
            }

            if (targetMotion == null)
            {
                Debug.LogError(
                    $"[PosesExtension] (CustomPose) No custom pose or fallback animation found for GUID: {FallbackAnimationGUID}. Aborting custom pose application."
                );
                return false;
            }

            // Ensure the targetMotion is an AnimationClip for RootQ operations
            AnimationClip sourceAnimationClip = targetMotion as AnimationClip;
            if (sourceAnimationClip == null)
            {
                Debug.LogWarning($"[PosesExtension] (CustomPose) The selected custom pose motion '{targetMotion.name}' is not an AnimationClip. Skipping RootQ data application.");
            }


            // --- Step 2: Update the FIRST entry (index 0) of the BlendTree with the chosen motion ---
            string blendTreePath = AssetDatabase.GUIDToAssetPath(BlendTreeAssetGUID);
            if (string.IsNullOrEmpty(blendTreePath))
            {
                Debug.LogError(
                    $"[PosesExtension] (CustomPose) Failed to find BlendTree asset path for GUID: {BlendTreeAssetGUID}. Is the asset missing or corrupted? Cannot apply custom pose to BlendTree."
                );
            }
            else
            {
                BlendTree blendTree = AssetDatabase.LoadAssetAtPath<BlendTree>(blendTreePath);
                if (blendTree == null)
                {
                    Debug.LogError(
                        $"[PosesExtension] (CustomPose) Failed to load BlendTree asset at path: {blendTreePath} (GUID: {BlendTreeAssetGUID}). Cannot apply custom pose to BlendTree."
                    );
                }
                else
                {
                    bool blendTreeModified = false;
                    ChildMotion[] children = blendTree.children;

                    if (children.Length > 0)
                    {
                        // Target the first child motion explicitly
                        if (children[0].motion != targetMotion)
                        {
                            Motion currentMotionInSlot = children[0].motion;
                            string currentMotionName = (currentMotionInSlot != null) ? currentMotionInSlot.name : "None";

                            // Optional: Log if the local ID of the current motion in the first slot doesn't match expectations
                            if (currentMotionInSlot != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(currentMotionInSlot, out _, out long currentLocalId) && currentLocalId != ExpectedFirstSlotMotionLocalID)
                            {
                                Debug.LogWarning(
                                    $"[PosesExtension] (CustomPose) First blend tree slot currently contains motion '{currentMotionName}' (ID: {currentLocalId}) which does not match the expected ID: {ExpectedFirstSlotMotionLocalID}. Overwriting."
                                );
                            }

                            children[0].motion = targetMotion;
                            blendTreeModified = true;
                            Debug.Log(
                                $"[PosesExtension] (CustomPose) Changed motion in FIRST blend tree slot (index 0) of BlendTree '{blendTree.name}' to '{targetMotion.name}'."
                            );
                        }
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"[PosesExtension] (CustomPose) BlendTree '{blendTree.name}' has no child motions. Cannot apply custom pose to the first slot."
                        );
                    }

                    if (blendTreeModified)
                    {
                        blendTree.children = children;
                        EditorUtility.SetDirty(blendTree);
                        assetsWereModified = true;
                        Debug.Log(
                            $"[PosesExtension] (CustomPose) BlendTree {blendTree.name} modified and marked dirty."
                        );
                    }
                    else
                    {
                        Debug.Log(
                            $"[PosesExtension] (CustomPose) No changes made to BlendTree {blendTree.name}."
                        );
                    }
                }
            }


            // --- Step 3: Apply RootQ Data from the chosen AnimationClip to the specific Alignment clip ---
            if (sourceAnimationClip != null)
            {
                if (ApplyRootQDataToSingleAlignmentClip(sourceAnimationClip))
                {
                    assetsWereModified = true;
                }
            }


            if (!assetsWereModified)
            {
                Debug.Log($"[PosesExtension] (CustomPose) No assets were modified for {avatarName}.");
            }

            return assetsWereModified;
        }
        
        private static bool ApplyRootQDataToSingleAlignmentClip(AnimationClip sourceClip)
        {
            if (sourceClip == null)
            {
                Debug.LogError("[PosesExtension] (RootQ Apply) No source Animation Clip specified for RootQ data application. Aborting.");
                return false;
            }

            string targetAssetPath = AssetDatabase.GUIDToAssetPath(TargetAlignmentClipGUID);
            if (string.IsNullOrEmpty(targetAssetPath))
            {
                Debug.LogError($"[PosesExtension] (RootQ Apply) Failed to find target alignment clip path for GUID: {TargetAlignmentClipGUID}. Cannot apply RootQ data.");
                return false;
            }
            
            AnimationClip targetClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(targetAssetPath);
            if (targetClip == null)
            {
                Debug.LogError($"[PosesExtension] (RootQ Apply) Failed to load target alignment clip at path: {targetAssetPath} (GUID: {TargetAlignmentClipGUID}). Cannot apply RootQ data.");
                return false;
            }

            // Check if RootQ curves actually exist in the source
            bool sourceHasRootQ = AnimationClipUtility.HasRootQCurves(sourceClip);

            // Check if target clip already matches source clip's RootQ data
            if (AnimationClipUtility.AreRootQCurvesIdentical(sourceClip, targetClip))
            {
                Debug.Log($"[PosesExtension] (RootQ Apply) RootQ data for '{targetClip.name}' already matches '{sourceClip.name}'. Skipping.");
                return false;
            }

            bool modified = false;

            // If source has no RootQ, remove it from target if present
            if (!sourceHasRootQ)
            {
                if (AnimationClipUtility.HasRootQCurves(targetClip)) // Only remove if actually present
                {
                    AnimationClipUtility.RemoveRootQCurves(targetClip);
                    EditorUtility.SetDirty(targetClip);
                    Debug.Log($"[PosesExtension] (RootQ Apply) Removed RootQ data from '{targetClip.name}' as source '{sourceClip.name}' has none.");
                    modified = true;
                }
            }
            else // Source has RootQ, so copy it
            {
                AnimationClipUtility.RemoveRootQCurves(targetClip); // Ensure existing are cleared before copy
                AnimationClipUtility.CopyRootQCurves(sourceClip, targetClip);
                EditorUtility.SetDirty(targetClip);
                Debug.Log($"[PosesExtension] (RootQ Apply) Copied RootQ data from '{sourceClip.name}' to '{targetClip.name}'.");
                modified = true;
            }
            
            return modified;
        }
    }
}
#endif