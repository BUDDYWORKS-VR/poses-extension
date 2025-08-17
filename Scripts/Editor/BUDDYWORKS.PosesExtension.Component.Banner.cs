using System.Diagnostics;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace BUDDYWORKS.PosesExtension
{
    public static class BannerUtility
    {
        private static Texture2D banner;
        private static Color backgroundColor = new Color(0.992f, 0.855f, 0.051f); // #FDDA0D

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

            // Determine the ideal width for the banner based on inspector width
            // We want the banner to take up as much width as possible, up to the inspector width.
            float targetBannerWidth = inspectorWidth;

            // Calculate the corresponding height to maintain aspect ratio
            float targetBannerHeight = targetBannerWidth / bannerAspectRatio;

            // --- Optional: Cap the height if it gets too large ---
            // Define a maximum desired height for the banner.
            // Adjust this value based on how tall you want the banner to be at its maximum.
            float maxAllowedHeight = 320f; // Example: A reasonable max height for a banner

            if (targetBannerHeight > maxAllowedHeight)
            {
                // If the calculated height exceeds the maximum, recalculate width based on max height
                targetBannerHeight = maxAllowedHeight;
                targetBannerWidth = targetBannerHeight * bannerAspectRatio;
            }
            // --- End Optional Cap ---


            // Calculate padding to center the banner horizontally
            float paddingX = (inspectorWidth - targetBannerWidth) / 2;

            // Request layout space for the entire area, including background, based on the *actual* scaled banner height
            // GUILayoutUtility.GetRect will advance the layout cursor
            Rect bannerAreaRect = GUILayoutUtility.GetRect(inspectorWidth, targetBannerHeight);

            // Draw the background color for the entire allocated area
            EditorGUI.DrawRect(new Rect(bannerAreaRect.x, bannerAreaRect.y, inspectorWidth, bannerAreaRect.height), backgroundColor);

            // Create the Rect for the clickable banner image
            // This Rect must precisely match the calculated targetBannerWidth and targetBannerHeight
            Rect clickableBannerRect = new Rect(bannerAreaRect.x + paddingX, bannerAreaRect.y, targetBannerWidth, targetBannerHeight);

            // Draw the button/image with the exact calculated dimensions
            // GUIStyle.none is important to avoid any default button styling that might interfere
            if (GUI.Button(clickableBannerRect, banner, GUIStyle.none))
            {
                Process.Start(new ProcessStartInfo("https://buddyworks.wtf") { UseShellExecute = true });
            }
        }
    }
}
#endif