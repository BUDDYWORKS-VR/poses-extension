#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace BUDDYWORKS.PosesExtension
{
    public class PosesExtensionProcessor : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => -100000000;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            
            var poseExtension = avatarGameObject.GetComponentInChildren<BWPosesExtension>(true);
            if (poseExtension == null)
            {
                return true;
            }

            Debug.Log($"[PosesExtension] Starting Poses Extension preprocessing for avatar: {avatarGameObject.name}");
            
            PosesExtensionSyncProcessor.ApplySyncParameterChanges(poseExtension, avatarGameObject.name);
            PosesExtensionAdjustmentsProcessor.ApplyAnimationAdjustments(poseExtension.HeightAdjustMultiplier, poseExtension.ViewAdjustSensitivity);
            PosesExtensionModdingProcessor.ApplyCustomDances(poseExtension, avatarGameObject.name);
            PosesExtensionCustomPoseProcessor.ApplyCustomPose(poseExtension, avatarGameObject.name);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[PosesExtension] Finished Poses Extension preprocessing for avatar: {avatarGameObject.name}.");

            return true;
        }
    }
}
#endif