using System.IO;
using UnityEditor;
using UnityEngine;

namespace BUDDYWORKS.PosesExtension
{
    public class RootQ
    {
        private static string sourceFolderPath = "Packages/wtf.buddyworks.posesextension/Data/AnimationClips/Poses";
        private static string targetRootPath = "Packages/wtf.buddyworks.posesextension/Data/AnimationClips/Adjustments";
        
        private static string devPath = "Packages/wtf.buddyworks.posesextension/isDeveloper";

        [MenuItem("BUDDYWORKS/Development/Animation Clip Root.Q Transfer")]
        public static void RunTransfer()
        {
            TransferRootQData();
        }

        [MenuItem("BUDDYWORKS/Development/Animation Clip Root.Q Transfer", true)]
        public static bool ValidateRunTransfer()
        {
            return AssetDatabase.IsValidFolder(devPath);
        }

        private static void TransferRootQData()
        {
            string[] sourceFiles = Directory.GetFiles(sourceFolderPath, "*.anim", SearchOption.TopDirectoryOnly);

            foreach (string sourceFilePath in sourceFiles)
            {
                string sourceFileName = Path.GetFileNameWithoutExtension(sourceFilePath);
                string[] targetFolders =
                    Directory.GetDirectories(targetRootPath, sourceFileName, SearchOption.TopDirectoryOnly);

                foreach (string targetFolder in targetFolders)
                {
                    string[] targetFiles = Directory.GetFiles(targetFolder, "*.anim", SearchOption.AllDirectories);

                    foreach (string targetFilePath in targetFiles)
                    {
                        CopyRootQData(sourceFilePath, targetFilePath);
                    }
                }
            }

            Debug.Log("Root.Q data transfer completed.");
        }

        private static void CopyRootQData(string sourceFilePath, string targetFilePath)
        {
            string sourceAssetPath = sourceFilePath.Replace("\\", "/");
            string targetAssetPath = targetFilePath.Replace("\\", "/");

            // Log paths for debugging
            //Debug.Log($"Attempting to load source animation clip from: {sourceAssetPath}");
            //Debug.Log($"Attempting to load target animation clip from: {targetAssetPath}");

            AnimationClip sourceClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(sourceAssetPath);
            AnimationClip targetClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(targetAssetPath);

            if (sourceClip == null)
            {
                Debug.LogError($"Failed to load source animation clip at path: {sourceAssetPath}");
                return;
            }

            if (targetClip == null)
            {
                Debug.LogError($"Failed to load target animation clip at path: {targetAssetPath}");
                return;
            }

            AnimationClipUtility.RemoveRootQCurves(targetClip);
            AnimationClipUtility.CopyRootQCurves(sourceClip, targetClip);

            EditorUtility.SetDirty(targetClip);
            AssetDatabase.SaveAssets();
        }
    }

    public static class AnimationClipUtility
    {
        public static void RemoveRootQCurves(AnimationClip clip)
        {
            if (clip == null) return;

            var bindings = AnimationUtility.GetCurveBindings(clip);
            foreach (var binding in bindings)
            {
                if (binding.propertyName == "RootQ.x" || binding.propertyName == "RootQ.y" ||
                    binding.propertyName == "RootQ.z" || binding.propertyName == "RootQ.w")
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
                if (binding.propertyName == "RootQ.x" || binding.propertyName == "RootQ.y" ||
                    binding.propertyName == "RootQ.z" || binding.propertyName == "RootQ.w")
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(sourceClip, binding);
                    AnimationUtility.SetEditorCurve(targetClip, binding, curve);
                }
            }
        }
    }
}