using UnityEngine;

namespace BUDDYWORKS.PosesExtension
{
    [CreateAssetMenu(
        fileName = "HeightAdjustSettings",
        menuName = "BUDDYWORKS/Poses Extension/Height Adjust Settings"
    )]
    public class HeightAdjustSettings : ScriptableObject
    {
        [Range(1f, 2f)] // Slider range, adjust as needed
        public float heightAdjustMultiplier = 1f; // Default value
    }
}