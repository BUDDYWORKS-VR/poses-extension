using UnityEngine;

namespace BUDDYWORKS.PosesExtension
{
    [CreateAssetMenu(
        fileName = "ViewAdjustSettings",
        menuName = "BUDDYWORKS/Poses Extension/View Adjust Settings"
    )]
    public class ViewAdjustSettings : ScriptableObject
    {
        [Range(0f, 2f)] // Slider range, adjust as needed
        public float viewAdjustMultiplier = 1f; // Default value
    }
}