#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BUDDYWORKS.PosesExtension
{
    public static class PosesExtensionAdjustmentsProcessor
    {
        /// <summary>
        /// Applies height and view adjustments to the relevant animation clips.
        /// </summary>
        /// <param name="heightMultiplier">Multiplier for height adjustment range.</param>
        /// <param name="viewSensitivity">Sensitivity for view adjustments.</param>
        /// <returns>True if any animation clips were modified, false otherwise.</returns>
        public static bool ApplyAnimationAdjustments(
            float heightMultiplier,
            float viewSensitivity
        )
        {
            bool anyClipModified = false;

            Debug.Log(
                $"[PosesExtension] (Adjustments) Applying height adjustment multiplier: {heightMultiplier}"
            );
            if (ChangeHeightAdjustRange(multiplier: heightMultiplier))
            {
                anyClipModified = true;
            }

            Debug.Log(
                $"[PosesExtension] (Adjustments) Applying view adjustment sensitivity: {viewSensitivity}"
            );
            if (ChangeViewAdjustRange(multiplier: viewSensitivity))
            {
                anyClipModified = true;
            }

            return anyClipModified;
        }

        private static bool ChangeHeightAdjustRange(float multiplier)
        {
            bool modified = false;
            string rootPath = "Packages/wtf.buddyworks.posesextension/Data/AnimationClips/Adjustments/HeightAdjust";
            if (!Directory.Exists(rootPath))
            {
                Debug.LogError($"[PosesExtension] (HeightAdjust) Folder not found: {rootPath}");
                return false;
            }

            string lowClipPath =
                Directory.GetFiles(rootPath, "*-Low.anim", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault();
            string highClipPath =
                Directory.GetFiles(rootPath, "*-High.anim", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault();

            if (string.IsNullOrEmpty(lowClipPath) || string.IsNullOrEmpty(highClipPath))
            {
                Debug.LogError(
                    $"[PosesExtension] (HeightAdjust) Could not find animation clips in {rootPath}"
                );
                return false;
            }

            AnimationClip lowClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(lowClipPath);
            AnimationClip highClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(highClipPath);

            if (lowClip != null && highClip != null)
            {
                if (ApplyHeightAdjust(lowClip, -0.5f, multiplier))
                    modified = true;
                if (ApplyHeightAdjust(highClip, 2f, multiplier))
                    modified = true;
            }
            else
            {
                Debug.LogError(
                    $"[PosesExtension] (HeightAdjust) Could not load animation clips in {rootPath}"
                );
            }
            return modified;
        }

        private static bool ChangeViewAdjustRange(float multiplier)
        {
            bool modified = false;
            string rootPath = "Packages/wtf.buddyworks.posesextension/Data/AnimationClips/Adjustments/ViewAdjust";
            if (!Directory.Exists(rootPath))
            {
                Debug.LogError($"[PosesExtension] (ViewAdjust) Folder not found: {rootPath}");
                return false;
            }

            string xPlusClipPath =
                Directory.GetFiles(rootPath, "X+.anim", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault();
            string xMinusClipPath =
                Directory.GetFiles(rootPath, "X-.anim", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault();
            string yPlusClipPath =
                Directory.GetFiles(rootPath, "Y+.anim", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault();
            string yMinusClipPath =
                Directory.GetFiles(rootPath, "Y-.anim", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault();

            if (
                string.IsNullOrEmpty(xPlusClipPath)
                || string.IsNullOrEmpty(xMinusClipPath)
                || string.IsNullOrEmpty(yPlusClipPath)
                || string.IsNullOrEmpty(yMinusClipPath)
            )
            {
                Debug.LogError(
                    $"[PosesExtension] (ViewAdjust) Could not find animation clips in {rootPath}"
                );
                return false;
            }

            AnimationClip xPlusClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(xPlusClipPath);
            AnimationClip xMinusClip =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(xMinusClipPath);
            AnimationClip yPlusClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(yPlusClipPath);
            AnimationClip yMinusClip =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(yMinusClipPath);

            if (
                xPlusClip != null
                && xMinusClip != null
                && yPlusClip != null
                && yMinusClip != null
            )
            {
                float xPlusLeftEyeBase = 2f;
                float xPlusRightEyeBase = -2f;
                float xMinusLeftEyeBase = -2f;
                float xMinusRightEyeBase = 2f;
                float yPlusLeftEyeBase = 4f;
                float yPlusRightEyeBase = 4f;
                float yMinusLeftEyeBase = -4f;
                float yMinusRightEyeBase = -4f;

                if (ApplyViewAdjust(xPlusClip, "Left Eye In-Out", xPlusLeftEyeBase, multiplier))
                    modified = true;
                if (ApplyViewAdjust(xPlusClip, "Right Eye In-Out", xPlusRightEyeBase, multiplier))
                    modified = true;

                if (ApplyViewAdjust(xMinusClip, "Left Eye In-Out", xMinusLeftEyeBase, multiplier))
                    modified = true;
                if (ApplyViewAdjust(xMinusClip, "Right Eye In-Out", xMinusRightEyeBase, multiplier))
                    modified = true;

                if (ApplyViewAdjust(yPlusClip, "Left Eye Down-Up", yPlusLeftEyeBase, multiplier))
                    modified = true;
                if (ApplyViewAdjust(yPlusClip, "Right Eye Down-Up", yPlusRightEyeBase, multiplier))
                    modified = true;

                if (ApplyViewAdjust(yMinusClip, "Left Eye Down-Up", yMinusLeftEyeBase, multiplier))
                    modified = true;
                if (ApplyViewAdjust(yMinusClip, "Right Eye Down-Up", yMinusRightEyeBase, multiplier))
                    modified = true;
            }
            else
            {
                Debug.LogError(
                    $"[PosesExtension] (ViewAdjust) Could not load animation clips in {rootPath}"
                );
            }
            return modified;
        }

        private static bool ApplyHeightAdjust(AnimationClip clip, float baseValue, float multiplier)
        {
            float newValue = baseValue * multiplier;
            bool clipModified = false;

            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(
                "",
                typeof(Animator),
                "RootT.y"
            ); // ClassID 95 for Animator
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
            }
            return clipModified;
        }

        private static bool ApplyViewAdjust(
            AnimationClip clip,
            string propertyName,
            float baseValue,
            float multiplier
        )
        {
            float newValue = baseValue * multiplier;
            bool clipModified = false;

            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(
                "", // path: empty string for Animator component directly on avatar root
                typeof(Animator), // classID 95 is Animator
                propertyName // The custom float property name (e.g., "Left Eye In-Out")
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
            }
            return clipModified;
        }
    }
}
#endif