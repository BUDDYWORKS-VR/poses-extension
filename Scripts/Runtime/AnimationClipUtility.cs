#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BUDDYWORKS.PosesExtension
{
    public static class AnimationClipUtility
    {
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
        
        public static bool HasRootQCurves(AnimationClip clip)
        {
            if (clip == null) return false;
            return AnimationUtility.GetCurveBindings(clip)
                .Any(b => b.propertyName.StartsWith("RootQ."));
        }
        
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