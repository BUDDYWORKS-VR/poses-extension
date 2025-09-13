#if UNITY_EDITOR
using UnityEngine;
using VRC.SDKBase;

namespace BUDDYWORKS.PosesExtension
{
    [AddComponentMenu("BUDDYWORKS/Poses Extension")]
    public class BWPosesExtension : MonoBehaviour, IEditorOnly
    {
        [Header("Sync Settings")]
        [Tooltip("Enable/Disable syncing of Mirror Pose parameter to other players.")]
        [SerializeField] private bool _syncMirror = true; 
        
        [Tooltip("Enable/Disable syncing of Height Adjust parameter to other players.")]
        [SerializeField] private bool _syncHeight = true; 

        [Tooltip("Enable/Disable syncing of View Adjust parameters to other players.")]
        [SerializeField] private bool _syncViewAdjustGroup = false; 
        
        [Tooltip("Enable/Disable syncing of Hand Adjust parameters to other players.")]
        [SerializeField] private bool _syncHandAdjustGroup = false;
        
        [Tooltip("Enable/Disable syncing of Head Adjust parameters to other players.")]
        [SerializeField] private bool _syncHeadAdjustGroup = false;
        
        [Tooltip("Enable/Disable syncing of Tilt Adjust parameters to other players.")]
        [SerializeField] private bool _syncTiltAdjustGroup = false;
        
        [Tooltip("Enable/Disable syncing of Offset Adjust parameters to other players.")]
        [SerializeField] private bool _syncOffsetAdjustGroup = false;
        
        public bool SyncViewAdjustGroup => _syncViewAdjustGroup;
        public bool SyncHandAdjustGroup => _syncHandAdjustGroup;
        public bool SyncHeadAdjustGroup => _syncHeadAdjustGroup;
        public bool SyncTiltAdjustGroup => _syncTiltAdjustGroup;
        public bool SyncOffsetAdjustGroup => _syncOffsetAdjustGroup;
        public bool SyncHeight => _syncHeight;
        public bool SyncMirror => _syncMirror;

        [Header("Custom Poses")]
        [Tooltip("Optional: Assign a custom AnimationClip for custom poses. If left empty, a fallback motion will be used.")]
        [SerializeField] private AnimationClip _customPose;
        
        [Header("Custom Dances")]
        [Tooltip("Optional: Assign a custom AnimationClip for Dance A. If left empty, a fallback motion will be used.")]
        [SerializeField] private AnimationClip _customDanceA;
        [Tooltip("Optional: Assign a custom AnimationClip for Dance B. If left empty, a fallback motion will be used.")]
        [SerializeField] private AnimationClip _customDanceB;
        [Tooltip("Optional: Assign a custom AnimationClip for Dance C. If left empty, a fallback motion will be used.")]
        [SerializeField] private AnimationClip _customDanceC;
        

        public AnimationClip CustomDanceA => _customDanceA;
        public AnimationClip CustomDanceB => _customDanceB;
        public AnimationClip CustomDanceC => _customDanceC;
        public AnimationClip CustomPose => _customPose;


        [Header("Adjustment Ranges")]
        [Tooltip("Multiplies the default height adjustment range. 1 = default, 2 = double range.")]
        [Range(1f, 2f)]
        [SerializeField] private float _heightAdjustMultiplier = 1f;
        
        [Tooltip("Controls the sensitivity of view adjustments. 0 = no movement, 1 = default, 2 = double sensitivity.")]
        [Range(0f, 2f)]
        [SerializeField] private float _viewAdjustSensitivity = 1f;

        public float HeightAdjustMultiplier => _heightAdjustMultiplier;
        public float ViewAdjustSensitivity => _viewAdjustSensitivity;

        // Constants for parameter costs of each function.
        private const int BaseCost = 16;
        private const int HeightCost = 8;
        private const int MirrorCost = 1;
        private const int ViewAdjustCost = 17;
        private const int HandAdjustCost = 17;
        private const int HeadAdjustCost = 25;
        private const int TiltAdjustCost = 9;
        private const int OffsetAdjustCost = 25;

        public int CalculateParameterCost()
        {
            int cost = BaseCost;
            if (_syncHeight) cost += HeightCost;
            if (_syncMirror) cost += MirrorCost;
            if (_syncViewAdjustGroup) cost += ViewAdjustCost;
            if (_syncHandAdjustGroup) cost += HandAdjustCost;
            if (_syncHeadAdjustGroup) cost += HeadAdjustCost;
            if (_syncTiltAdjustGroup) cost += TiltAdjustCost;
            if (_syncOffsetAdjustGroup) cost += OffsetAdjustCost;
            return cost;
        }
    }
}
#endif