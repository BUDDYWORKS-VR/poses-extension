#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BUDDYWORKS.PosesExtension
{
    /// <summary>
    /// Provides utility methods for manipulating AnimationClips, specifically concerning RootQ curves.
    /// </summary>
    public static class AnimationClipUtility
    {
        /// <summary>
        /// Removes all RootQ curves (RootQ.x, RootQ.y, RootQ.z, RootQ.w) from the given AnimationClip.
        /// </summary>
        /// <param name="clip">The AnimationClip to modify.</param>
        public static void RemoveRootQCurves(AnimationClip clip)
        {
            if (clip == null) return;

            var bindings = AnimationUtility.GetCurveBindings(clip);
            foreach (var binding in bindings)
            {
                if (binding.propertyName.StartsWith("RootQ."))
                {
                    AnimationUtility.SetEditorCurve(clip, binding, null);
                }
            }
        }

        /// <summary>
        /// Copies all RootQ curves from a source AnimationClip to a target AnimationClip.
        /// Existing RootQ curves in the target clip will be overwritten.
        /// </summary>
        /// <param name="sourceClip">The AnimationClip to copy RootQ curves from.</param>
        /// <param name="targetClip">The AnimationClip to copy RootQ curves to.</param>
        public static void CopyRootQCurves(AnimationClip sourceClip, AnimationClip targetClip)
        {
            if (sourceClip == null || targetClip == null) return;

            var bindings = AnimationUtility.GetCurveBindings(sourceClip);
            foreach (var binding in bindings)
            {
                if (binding.propertyName.StartsWith("RootQ."))
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(sourceClip, binding);
                    AnimationUtility.SetEditorCurve(targetClip, binding, curve);
                }
            }
        }

        /// <summary>
        /// Checks if an AnimationClip contains any RootQ curves (RootQ.x, RootQ.y, RootQ.z, RootQ.w).
        /// </summary>
        /// <param name="clip">The AnimationClip to check.</param>
        /// <returns>True if the clip contains at least one RootQ curve, false otherwise.</returns>
        public static bool HasRootQCurves(AnimationClip clip)
        {
            if (clip == null) return false;
            return AnimationUtility.GetCurveBindings(clip)
                .Any(b => b.propertyName.StartsWith("RootQ."));
        }

        /// <summary>
        /// Compares RootQ curves between two AnimationClips.
        /// Returns true if both clips have identical RootQ curves (same properties, keyframes, and values)
        /// or if both entirely lack RootQ curves.
        /// Returns false if they differ or only one has RootQ curves.
        /// </summary>
        /// <param name="clipA">The first AnimationClip for comparison.</param>
        /// <param name="clipB">The second AnimationClip for comparison.</param>
        /// <returns>True if RootQ curves are identical or both are absent, false otherwise.</returns>
        public static bool AreRootQCurvesIdentical(AnimationClip clipA, AnimationClip clipB)
        {
            if (clipA == null || clipB == null) return false;

            var curvesA = AnimationUtility.GetCurveBindings(clipA)
                .Where(b => b.propertyName.StartsWith("RootQ."))
                .OrderBy(b => b.propertyName)
                .ToList();
            var curvesB = AnimationUtility.GetCurveBindings(clipB)
                .Where(b => b.propertyName.StartsWith("RootQ."))
                .OrderBy(b => b.propertyName)
                .ToList();

            if (curvesA.Count != curvesB.Count) return false;
            if (curvesA.Count == 0) return true; // Both have no RootQ curves

            for (int i = 0; i < curvesA.Count; i++)
            {
                if (curvesA[i].propertyName != curvesB[i].propertyName) return false;

                AnimationCurve curveA = AnimationUtility.GetEditorCurve(clipA, curvesA[i]);
                AnimationCurve curveB = AnimationUtility.GetEditorCurve(clipB, curvesB[i]);

                if (curveA == null && curveB == null) continue;
                if (curveA == null || curveB == null) return false; // One is null, other isn't

                if (curveA.keys.Length != curveB.keys.Length) return false;
                for (int j = 0; j < curveA.keys.Length; j++)
                {
                    if (!Mathf.Approximately(curveA.keys[j].time, curveB.keys[j].time) ||
                        !Mathf.Approximately(curveA.keys[j].value, curveB.keys[j].value))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
#endif