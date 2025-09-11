#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace BUDDYWORKS.PosesExtension
{
    public static class PosesExtensionSyncProcessor
    {
        private const string SyncParameterAssetGUID = "17cce2e1703370a41b2f584d6364944a";

        /// <summary>
        /// Applies the network sync settings from the BWPosesExtension component to the VRCExpressionParameters asset.
        /// </summary>
        /// <param name="poseExtension">The BWPosesExtension component containing sync settings.</param>
        /// <param name="avatarName">The name of the avatar being processed (for logging).</param>
        /// <returns>True if the VRCExpressionParameters asset was modified, false otherwise.</returns>
        public static bool ApplySyncParameterChanges(
            BWPosesExtension poseExtension,
            string avatarName
        )
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(SyncParameterAssetGUID);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError(
                    $"[PosesExtension] (Sync) Failed to find asset path for GUID: {SyncParameterAssetGUID}. Is the asset missing or corrupted? Cannot modify sync parameters."
                );
                return false;
            }

            VRCExpressionParameters parametersAsset =
                AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(assetPath);
            if (parametersAsset == null)
            {
                Debug.LogError(
                    $"[PosesExtension] (Sync) Failed to load VRCExpressionParameters asset at path: {assetPath} (GUID: {SyncParameterAssetGUID}). Cannot modify sync parameters."
                );
                return false;
            }

            SerializedObject serializedParametersAsset = new SerializedObject(parametersAsset);
            SerializedProperty parametersProp =
                serializedParametersAsset.FindProperty("parameters");

            if (parametersProp == null || !parametersProp.isArray)
            {
                Debug.LogError(
                    $"[PosesExtension] (Sync) Parameters array property 'parameters' not found or is not an array on asset: {assetPath}. Cannot modify sync parameters."
                );
                return false;
            }

            bool syncParametersModified = false;

            for (int i = 0; i < parametersProp.arraySize; i++)
            {
                SerializedProperty parameterEntry = parametersProp.GetArrayElementAtIndex(i);
                SerializedProperty nameProp = parameterEntry.FindPropertyRelative("name");
                SerializedProperty networkSyncedProp =
                    parameterEntry.FindPropertyRelative("networkSynced");

                if (nameProp == null || networkSyncedProp == null) continue;

                string parameterName = nameProp.stringValue;
                bool currentNetworkSynced = (
                    networkSyncedProp.propertyType == SerializedPropertyType.Boolean
                )
                    ? networkSyncedProp.boolValue
                    : (networkSyncedProp.intValue == 1);
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
                    Debug.Log(
                        $"[PosesExtension] (Sync) Changed networkSynced for parameter '{parameterName}' to: {newNetworkSynced}"
                    );
                }
            }

            if (syncParametersModified)
            {
                serializedParametersAsset.ApplyModifiedProperties();
                EditorUtility.SetDirty(parametersAsset);
                Debug.Log(
                    $"[PosesExtension] (Sync) Parameters asset {assetPath} modified and marked dirty."
                );
                return true;
            }
            else
            {
                Debug.Log(
                    $"[PosesExtension] (Sync) No changes made to parameters asset {assetPath}."
                );
                return false;
            }
        }
    }
}
#endif