using System.Diagnostics;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace BUDDYWORKS.PosesExtension
{
    public static class BannerUtility
    {
        private static Texture2D banner;
        private static Color backgroundColor = new Color(0.09411764705882353f, 0.09803921f, 0.09411764705882353f); // #181918

        static BannerUtility()
        {
            string bannerPath = AssetDatabase.GUIDToAssetPath("f229fdd4d9aee05439b7ffb66def3178");
            banner = AssetDatabase.LoadAssetAtPath<Texture2D>(bannerPath);
        }

        public static void DrawBanner()
        {
            if (banner == null) return;

            float inspectorWidth = EditorGUIUtility.currentViewWidth;

            // Calculate the aspect ratio of the banner
            float bannerAspectRatio = (float)banner.width / banner.height;

            // 1. Determine the maximum available width for the banner.
            // This is simply the current inspector width.
            float maxAvailableWidth = inspectorWidth;

            // 2. Determine the maximum available height for the banner.
            // We can define a reasonable maximum height to prevent the banner from
            // taking up too much vertical space in the inspector, especially on smaller screens.
            // If you don't want a vertical limit, set this to a very large number like float.MaxValue.
            float maxAvailableHeight = 300f; // Example: Cap banner height to 300px

            // Calculate the ideal dimensions if the banner were scaled to fit within
            // the maximum available width, preserving aspect ratio.
            float scaleByWidth = maxAvailableWidth;
            float heightWhenScaledByWidth = scaleByWidth / bannerAspectRatio;

            // Calculate the ideal dimensions if the banner were scaled to fit within
            // the maximum available height, preserving aspect ratio.
            float scaleByHeight = maxAvailableHeight;
            float widthWhenScaledByHeight = scaleByHeight * bannerAspectRatio;


            // Now, we need to choose the dimensions that fit BOTH constraints (max width and max height)
            // without exceeding either, while maintaining the aspect ratio.
            float finalBannerWidth;
            float finalBannerHeight;

            // If scaling to fit by width makes it too tall, then we must scale by height instead.
            if (heightWhenScaledByWidth > maxAvailableHeight)
            {
                finalBannerHeight = maxAvailableHeight;
                finalBannerWidth = widthWhenScaledByHeight;
            }
            else
            {
                // Otherwise, scaling by width works perfectly.
                finalBannerWidth = scaleByWidth;
                finalBannerHeight = heightWhenScaledByWidth;
            }
            
            // Ensure the banner doesn't exceed its original dimensions if the inspector is very large
            if (finalBannerWidth > banner.width)
            {
                finalBannerWidth = banner.width;
                finalBannerHeight = banner.height;
            }

            // Request layout space for the background. This rect determines how much vertical
            // space is reserved in the layout.
            // We use the 'finalBannerHeight' for this. The width should be the full inspector width
            // for the background fill.
            Rect bannerBackgroundRect = GUILayoutUtility.GetRect(
                inspectorWidth, // Request full inspector width for layout purposes
                finalBannerHeight, // Request the calculated final height
                GUILayout.ExpandWidth(true) // Allow it to expand with the inspector width
            );

            // Draw the background color. This rect covers the full width of the inspector
            // and the calculated final height.
            EditorGUI.DrawRect(
                new Rect(
                    bannerBackgroundRect.x,
                    bannerBackgroundRect.y,
                    inspectorWidth,
                    bannerBackgroundRect.height
                ),
                backgroundColor
            );

            // Calculate the position for the actual banner image to be drawn,
            // centering it horizontally within the allocated background space.
            Rect bannerImageRect = new Rect(
                bannerBackgroundRect.x + (inspectorWidth - finalBannerWidth) / 2,
                bannerBackgroundRect.y,
                finalBannerWidth,
                finalBannerHeight
            );

            // Draw the button/image with the calculated final dimensions.
            if (GUI.Button(bannerImageRect, banner, GUIStyle.none))
            {
                Process.Start(
                    new ProcessStartInfo("https://buddyworks.wtf") { UseShellExecute = true }
                );
            }
        }
    }
}
#endif