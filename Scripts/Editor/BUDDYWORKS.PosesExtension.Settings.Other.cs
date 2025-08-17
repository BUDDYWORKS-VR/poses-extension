using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

#if UNITY_EDITOR

namespace BUDDYWORKS.PosesExtension
{
    public class Other
    {
        private HeightAdjustSettings heightAdjustSettings;
        private ViewAdjustSettings viewAdjustSettings;
        private bool assetsWereDirty = false;

        public Other()
        {
            heightAdjustSettings =
                AssetDatabase.LoadAssetAtPath<HeightAdjustSettings>(
                    "Assets/HeightAdjustSettings.asset"
                );
            if (heightAdjustSettings == null)
            {
                heightAdjustSettings = ScriptableObject.CreateInstance<
                    HeightAdjustSettings
                >();
                AssetDatabase.CreateAsset(
                    heightAdjustSettings,
                    "Assets/HeightAdjustSettings.asset"
                );
                AssetDatabase.SaveAssets();
            }

            viewAdjustSettings =
                AssetDatabase.LoadAssetAtPath<ViewAdjustSettings>(
                    "Assets/ViewAdjustSettings.asset"
                );
            if (viewAdjustSettings == null)
            {
                viewAdjustSettings = ScriptableObject.CreateInstance<
                    ViewAdjustSettings
                >();
                AssetDatabase.CreateAsset(
                    viewAdjustSettings,
                    "Assets/ViewAdjustSettings.asset"
                );
                AssetDatabase.SaveAssets();
            }
        }

        public void DrawOtherTab()
        {
            // Add padding to the left
            GUILayout.BeginHorizontal();
            GUILayout.Space(4); // Adjust the value for more or less padding

            GUILayout.BeginVertical();
            GUILayout.Label(
                "Modify various aspects of Poses Extension.",
                EditorStyles.boldLabel
            );
            EditorGUILayout.HelpBox(
                "These settings allow you to change some ranges of PE adjustment features.\nThey are meant to offset some specific avatar setups, so use with care.\n\nNote that these settings are reset after updating or reimporting the Poses Extension Package.",
                MessageType.Info
            );

            Rect r = EditorGUILayout.GetControlRect(
                false,
                1,
                new GUIStyle() { margin = new RectOffset(0, 0, 4, 4) }
            );
            EditorGUI.DrawRect(r, Color.gray);

            // Height Adjust Slider
            GUILayout.Label("Height Adjust Multiplier (Default Value: 1)", EditorStyles.boldLabel);
            heightAdjustSettings.heightAdjustMultiplier =
                EditorGUILayout.Slider(
                    heightAdjustSettings.heightAdjustMultiplier,
                    1f,
                    2f
                );

            // View Adjust Slider
            GUILayout.Label("View Adjust Sensitivity (Default Value: 1)", EditorStyles.boldLabel);
            viewAdjustSettings.viewAdjustMultiplier = EditorGUILayout.Slider(
                viewAdjustSettings.viewAdjustMultiplier,
                0f,
                2f
            );

            // Unified Apply Button
            if (GUILayout.Button("Apply"))
            {
                assetsWereDirty = false; // Reset the flag
                ChangeHeightAdjustRange(heightAdjustSettings.heightAdjustMultiplier);
                ChangeViewAdjustRange(viewAdjustSettings.viewAdjustMultiplier);
                SaveAndRefreshAssets(); // Call the save function
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label("BUDDYWORKS Poses Extension", EditorStyles.boldLabel);
            Rect labelRect = GUILayoutUtility.GetLastRect();
            if (
                Event.current.type == EventType.MouseDown &&
                labelRect.Contains(Event.current.mousePosition)
            )
            {
                Application.OpenURL("https://buddyworks.wtf");
            }

            GUILayout.EndVertical();
            GUILayout.Space(4); // Right padding
            GUILayout.EndHorizontal();
        }

        private void ChangeHeightAdjustRange(float multiplier)
        {
            string rootPath =
                "Packages/wtf.buddyworks.posesextension/Data/AnimationClips/Adjustments/HeightAdjust";

            string heightAdjustPath = rootPath;

            if (Directory.Exists(heightAdjustPath))
            {
                string lowClipPath =
                    Directory.GetFiles(
                        heightAdjustPath,
                        "*-Low.anim",
                        SearchOption.TopDirectoryOnly
                    )
                    .FirstOrDefault();
                string highClipPath =
                    Directory.GetFiles(
                        heightAdjustPath,
                        "*-High.anim",
                        SearchOption.TopDirectoryOnly
                    )
                    .FirstOrDefault();

                if (
                    !string.IsNullOrEmpty(lowClipPath) &&
                    !string.IsNullOrEmpty(highClipPath)
                )
                {
                    AnimationClip lowClip = AssetDatabase.LoadAssetAtPath<
                        AnimationClip
                    >(lowClipPath);
                    AnimationClip highClip = AssetDatabase.LoadAssetAtPath<
                        AnimationClip
                    >(highClipPath);

                    if (lowClip != null && highClip != null)
                    {
                        ApplyHeightAdjust(lowClip, -0.5f, multiplier);
                        ApplyHeightAdjust(highClip, 2f, multiplier);
                    }
                    else
                    {
                        Debug.LogError(
                            "Could not load animation clips in " +
                            heightAdjustPath
                        );
                    }
                }
                else
                {
                    Debug.LogError(
                        "Could not find animation clips in " +
                        heightAdjustPath
                    );
                }
            }
            else
            {
                Debug.LogError(
                    "HeightAdjust folder not found in " + rootPath
                );
            }
        }

        private void ChangeViewAdjustRange(float multiplier)
        {
            string rootPath =
                "Packages/wtf.buddyworks.posesextension/Data/AnimationClips/Adjustments/ViewAdjust";

            if (Directory.Exists(rootPath))
            {
                string xPlusClipPath =
                    Directory.GetFiles(
                        rootPath,
                        "X+.anim",
                        SearchOption.TopDirectoryOnly
                    )
                    .FirstOrDefault();
                string xMinusClipPath =
                    Directory.GetFiles(
                        rootPath,
                        "X-.anim",
                        SearchOption.TopDirectoryOnly
                    )
                    .FirstOrDefault();
                string yPlusClipPath =
                    Directory.GetFiles(
                        rootPath,
                        "Y+.anim",
                        SearchOption.TopDirectoryOnly
                    )
                    .FirstOrDefault();
                string yMinusClipPath =
                    Directory.GetFiles(
                        rootPath,
                        "Y-.anim",
                        SearchOption.TopDirectoryOnly
                    )
                    .FirstOrDefault();

                if (
                    !string.IsNullOrEmpty(xPlusClipPath) &&
                    !string.IsNullOrEmpty(xMinusClipPath) &&
                    !string.IsNullOrEmpty(yPlusClipPath) &&
                    !string.IsNullOrEmpty(yMinusClipPath)
                )
                {
                    AnimationClip xPlusClip = AssetDatabase.LoadAssetAtPath<
                        AnimationClip
                    >(xPlusClipPath);
                    AnimationClip xMinusClip = AssetDatabase.LoadAssetAtPath<
                        AnimationClip
                    >(xMinusClipPath);
                    AnimationClip yPlusClip = AssetDatabase.LoadAssetAtPath<
                        AnimationClip
                    >(yPlusClipPath);
                    AnimationClip yMinusClip = AssetDatabase.LoadAssetAtPath<
                        AnimationClip
                    >(yMinusClipPath);

                    if (
                        xPlusClip != null &&
                        xMinusClip != null &&
                        yPlusClip != null &&
                        yMinusClip != null
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

                        ApplyViewAdjust(
                            xPlusClip,
                            "Left Eye In-Out",
                            xPlusLeftEyeBase,
                            multiplier
                        );
                        ApplyViewAdjust(
                            xPlusClip,
                            "Right Eye In-Out",
                            xPlusRightEyeBase,
                            multiplier
                        );

                        ApplyViewAdjust(
                            xMinusClip,
                            "Left Eye In-Out",
                            xMinusLeftEyeBase,
                            multiplier
                        );
                        ApplyViewAdjust(
                            xMinusClip,
                            "Right Eye In-Out",
                            xMinusRightEyeBase,
                            multiplier
                        );

                        ApplyViewAdjust(
                            yPlusClip,
                            "Left Eye Down-Up",
                            yPlusLeftEyeBase,
                            multiplier
                        );
                        ApplyViewAdjust(
                            yPlusClip,
                            "Right Eye Down-Up",
                            yPlusRightEyeBase,
                            multiplier
                        );

                        ApplyViewAdjust(
                            yMinusClip,
                            "Left Eye Down-Up",
                            yMinusLeftEyeBase,
                            multiplier
                        );
                        ApplyViewAdjust(
                            yMinusClip,
                            "Right Eye Down-Up",
                            yMinusRightEyeBase,
                            multiplier
                        );
                    }
                    else
                    {
                        Debug.LogError(
                            "Could not load animation clips in " +
                            rootPath
                        );
                    }
                }
                else
                {
                    Debug.LogError(
                        "Could not find animation clips in " +
                        rootPath
                    );
                }
            }
            else
            {
                Debug.LogError(
                    "ViewAdjust folder not found in " + rootPath
                );
            }
            
        }

        private void ApplyHeightAdjust(
            AnimationClip clip,
            float baseValue,
            float multiplier
        )
        {
            // Calculate the new value
            float newValue = baseValue * multiplier;

            // Iterate through the animation clip's bindings
            foreach (
                EditorCurveBinding binding in AnimationUtility.GetCurveBindings(
                    clip
                )
            )
            {
                if (binding.propertyName == "RootT.y")
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(
                        clip,
                        binding
                    );
                    if (curve != null)
                    {
                        // Modify the curve values
                        Keyframe[] keys = curve.keys; // Get the keys array
                        for (int i = 0; i < keys.Length; i++)
                        {
                            keys[i].value = newValue; // Directly modify the value
                        }
                        curve.keys = keys; // Assign the modified keys back to the curve

                        // Apply the modified curve back to the animation clip
                        AnimationUtility.SetEditorCurve(clip, binding, curve);

                        EditorUtility.SetDirty(clip); // Mark the clip as dirty
                        assetsWereDirty = true;
                    }
                }
            }
        }

        private void ApplyViewAdjust(
            AnimationClip clip,
            string propertyName,
            float baseValue,
            float multiplier
        )
        {
            // Calculate the new value
            float newValue = baseValue * multiplier;

            // Iterate through the animation clip's bindings
            foreach (
                EditorCurveBinding binding in AnimationUtility.GetCurveBindings(
                    clip
                )
            )
            {
                if (binding.propertyName == propertyName)
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(
                        clip,
                        binding
                    );
                    if (curve != null)
                    {
                        // Modify the curve values
                        Keyframe[] keys = curve.keys; // Get the keys array
                        for (int i = 0; i < keys.Length; i++)
                        {
                            keys[i].value = newValue; // Directly modify the value
                        }
                        curve.keys = keys; // Assign the modified keys back to the curve

                        // Apply the modified curve back to the animation clip
                        AnimationUtility.SetEditorCurve(clip, binding, curve);

                        EditorUtility.SetDirty(clip); // Mark the clip as dirty
                        assetsWereDirty = true;
                    }
                }
            }
        }

        private void SaveAndRefreshAssets()
        {
            if (assetsWereDirty)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                assetsWereDirty = false;
            }
        }
    }
}
#endif