#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;

// BUDDYWORKS Posebank Creator: merges single-keyframe pose clips into one
// 60 fps posebank. One key per union binding per pose, gaps default-filled,
// linear tangents, optional loop pose appended. Overwrites preserve GUID,
// wipe stale events, set stopTime explicitly.

namespace BUDDYWORKS.PosesExtension
{
    public class AnimationMergerWindow : EditorWindow
    {
        private const string LoopTooltip =
            "If enabled, the first pose is appended again at the end of the merged clip, " +
            "so the posebank loops seamlessly back to its start.\n\n" +
            "Default: enabled.";

        [SerializeField] private List<AnimationClip> clipsToMerge = new List<AnimationClip>();
        [SerializeField] private bool loopPosebank = true;

        private SerializedObject serializedObj;
        private SerializedProperty clipsProp;
        private SerializedProperty loopPosebankProp;
        private ReorderableList reorderableList;
        private Vector2 scrollPos;
        private GUIContent loopToggleContent;

        [MenuItem("BUDDYWORKS/Poses Extension/Create Custom Posebank")]
        public static void ShowWindow()
        {
            var window = GetWindow<AnimationMergerWindow>("Posebank Creator");
            window.minSize = new Vector2(400, 300);
        }

        private void OnEnable()
        {
            serializedObj = new SerializedObject(this);
            clipsProp = serializedObj.FindProperty("clipsToMerge");
            loopPosebankProp = serializedObj.FindProperty("loopPosebank");
            loopToggleContent = new GUIContent("Loop Posebank", LoopTooltip);

            reorderableList = new ReorderableList(serializedObj,
                clipsProp,
                true, true, true, true);

            reorderableList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Drag your loose pose clips into this window.");
            };

            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    element, GUIContent.none);
            };
        }

        private void OnGUI()
        {
            serializedObj.Update();
            HandleDragAndDrop();

            EditorGUILayout.HelpBox(
                "Merges loose pose clips into a single posebank animation. " +
                "Each clip's first keyframe is placed one frame after the previous pose.",
                MessageType.Info);

            EditorGUILayout.Space(8);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            reorderableList.DoLayoutList();
            if (clipsToMerge.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Drop AnimationClips here to get started.",
                    MessageType.None);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(8);

            EditorGUILayout.PropertyField(loopPosebankProp, loopToggleContent);

            EditorGUILayout.Space(8);

            bool hasAnyClip = clipsToMerge.Exists(c => c != null);
            EditorGUI.BeginDisabledGroup(!hasAnyClip);
            if (GUILayout.Button("Merge Poses & Save as...", GUILayout.Height(30)))
            {
                MergeAndSave();
            }
            EditorGUI.EndDisabledGroup();

            serializedObj.ApplyModifiedProperties();
        }

        private void HandleDragAndDrop()
        {
            Event evt = Event.current;
            Rect dropArea = new Rect(0, 0, position.width, position.height);

            if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
                && dropArea.Contains(evt.mousePosition))
            {
                bool hasClips = System.Array.Exists(
                    DragAndDrop.objectReferences, o => o is AnimationClip);
                if (!hasClips) return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    int added = 0;
                    var batchSeen = new HashSet<AnimationClip>();
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is AnimationClip clip
                            && !clipsToMerge.Contains(clip)
                            && batchSeen.Add(clip))
                        {
                            int idx = clipsProp.arraySize;
                            clipsProp.InsertArrayElementAtIndex(idx);
                            clipsProp.GetArrayElementAtIndex(idx).objectReferenceValue = clip;
                            added++;
                        }
                    }

                    if (added < DragAndDrop.objectReferences.Length)
                    {
                        Debug.LogWarning(
                            "Some dragged items were skipped: they were either not " +
                            "AnimationClips or already in the list.");
                    }

                    evt.Use();
                }
            }
        }

        private void MergeAndSave()
        {
            var processClips = new List<AnimationClip>();
            var seen = new HashSet<AnimationClip>();
            foreach (AnimationClip clip in clipsToMerge)
            {
                if (clip == null || !seen.Add(clip)) continue;
                processClips.Add(clip);
            }

            int poseCount = processClips.Count;
            if (poseCount == 0) return;

            int skipped = clipsToMerge.Count - poseCount;
            if (skipped > 0)
            {
                Debug.Log($"Merge skipped {skipped} null or duplicate list entr"
                    + $"{(skipped == 1 ? "y" : "ies")}; remaining poses are packed "
                    + "one frame apart.");
            }

            bool loop = loopPosebankProp.boolValue;
            if (loop) processClips.Add(processClips[0]);

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Merged Animation", "MergedPoses", "anim", "Select save location");

            if (string.IsNullOrEmpty(path)) return;

            AnimationClip existingClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            Object existingAsset = AssetDatabase.LoadMainAssetAtPath(path);
            if (existingClip == null && existingAsset != null)
            {
                EditorUtility.DisplayDialog(
                    "Cannot Save",
                    "The selected path already contains a non-AnimationClip asset. " +
                    "Choose a different name or location.",
                    "OK");
                return;
            }

            AnimationClip newClip = existingClip != null ? existingClip : new AnimationClip();

            if (existingClip != null)
            {
                Undo.RegisterCompleteObjectUndo(newClip, "Merge Poses");
                newClip.ClearCurves();
            }

            float targetFrameRate = 60f;
            newClip.frameRate = targetFrameRate;

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(newClip);
            settings.loopTime = false;
            settings.mirror = false;
            settings.stopTime = processClips.Count / targetFrameRate;

            settings.loopBlendOrientation = true;
            settings.keepOriginalOrientation = true;
            settings.orientationOffsetY = 0f;

            settings.loopBlendPositionY = true;
            settings.keepOriginalPositionY = true;
            settings.level = 0f;

            settings.loopBlendPositionXZ = true;
            settings.keepOriginalPositionXZ = true;

            AnimationUtility.SetAnimationClipSettings(newClip, settings);
            AnimationUtility.SetAnimationEvents(newClip, new AnimationEvent[0]);

            var floatCurves = new Dictionary<EditorCurveBinding, AnimationCurve>();
            var objectCurves = new Dictionary<EditorCurveBinding, List<ObjectReferenceKeyframe>>();

            var allFloatBindings = new List<EditorCurveBinding>();
            var floatSeen = new HashSet<EditorCurveBinding>();
            var floatReference = new Dictionary<EditorCurveBinding, float>();

            var allObjectBindings = new List<EditorCurveBinding>();
            var objectSeen = new HashSet<EditorCurveBinding>();
            var objectReference = new Dictionary<EditorCurveBinding, Object>();

            foreach (AnimationClip sourceClip in processClips)
            {
                if (sourceClip == null) continue;

                foreach (var binding in AnimationUtility.GetCurveBindings(sourceClip))
                {
                    if (!floatSeen.Add(binding)) continue;
                    allFloatBindings.Add(binding);

                    AnimationCurve curve = AnimationUtility.GetEditorCurve(sourceClip, binding);
                    if (curve != null && curve.length > 0)
                    {
                        floatReference[binding] = curve.keys[0].value;
                    }
                }

                foreach (var binding in
                    AnimationUtility.GetObjectReferenceCurveBindings(sourceClip))
                {
                    ObjectReferenceKeyframe[] keys =
                        AnimationUtility.GetObjectReferenceCurve(sourceClip, binding);
                    if (keys == null || keys.Length == 0) continue;

                    if (!objectSeen.Add(binding)) continue;
                    allObjectBindings.Add(binding);
                    objectReference[binding] = keys[0].value;
                }
            }

            for (int i = 0; i < processClips.Count; i++)
            {
                AnimationClip sourceClip = processClips[i];
                if (sourceClip == null) continue;

                float targetTime = i / targetFrameRate;

                var thisPoseFloats = new Dictionary<EditorCurveBinding, float>();
                var thisPoseObjects = new Dictionary<EditorCurveBinding, ObjectReferenceKeyframe>();
                var multiKeyProps = new List<string>();

                foreach (var binding in AnimationUtility.GetCurveBindings(sourceClip))
                {
                    AnimationCurve sourceCurve =
                        AnimationUtility.GetEditorCurve(sourceClip, binding);
                    if (sourceCurve == null || sourceCurve.length == 0) continue;

                    if (sourceCurve.length > 1 || CurveValuesDiffer(sourceCurve))
                    {
                        multiKeyProps.Add(binding.propertyName);
                    }
                    thisPoseFloats[binding] = sourceCurve.keys[0].value;
                }

                foreach (var binding in
                    AnimationUtility.GetObjectReferenceCurveBindings(sourceClip))
                {
                    ObjectReferenceKeyframe[] sourceKeys =
                        AnimationUtility.GetObjectReferenceCurve(sourceClip, binding);
                    if (sourceKeys == null || sourceKeys.Length == 0) continue;

                    if (sourceKeys.Length > 1 || ObjectReferenceValuesDiffer(sourceKeys))
                    {
                        multiKeyProps.Add(binding.propertyName + " (object)");
                    }
                    ObjectReferenceKeyframe key = sourceKeys[0];
                    key.time = targetTime;
                    thisPoseObjects[binding] = key;
                }

                if (multiKeyProps.Count > 0)
                {
                    Debug.LogWarning(
                        $"Clip '{sourceClip.name}': {multiKeyProps.Count} curve"
                        + $"{(multiKeyProps.Count == 1 ? "" : "s")} had multiple "
                        + "keyframes or animated values. Only the first keyframe is "
                        + $"used; the rest are discarded. Affected: "
                        + string.Join(", ", multiKeyProps));
                }

                foreach (var binding in allFloatBindings)
                {
                    float value = thisPoseFloats.TryGetValue(binding, out float animated)
                        ? animated
                        : GetDefaultFloatValue(binding, floatReference);

                    if (!floatCurves.TryGetValue(binding, out AnimationCurve merged))
                    {
                        merged = new AnimationCurve();
                        floatCurves[binding] = merged;
                    }
                    merged.AddKey(new Keyframe(targetTime, value));
                }

                foreach (var binding in allObjectBindings)
                {
                    if (!objectReference.TryGetValue(binding, out Object defaultValue))
                    {
                        continue;
                    }

                    ObjectReferenceKeyframe key =
                        thisPoseObjects.TryGetValue(binding, out ObjectReferenceKeyframe animated)
                            ? animated
                            : new ObjectReferenceKeyframe { time = targetTime, value = defaultValue };

                    if (!objectCurves.TryGetValue(binding,
                            out List<ObjectReferenceKeyframe> merged))
                    {
                        merged = new List<ObjectReferenceKeyframe>();
                        objectCurves[binding] = merged;
                    }
                    merged.Add(key);
                }
            }

            foreach (var kvp in floatCurves)
            {
                AnimationCurve curve = kvp.Value;
                ApplyLinearTangents(curve);
                AnimationUtility.SetEditorCurve(newClip, kvp.Key, curve);
            }

            foreach (var kvp in objectCurves)
            {
                AnimationUtility.SetObjectReferenceCurve(newClip, kvp.Key, kvp.Value.ToArray());
            }

            if (existingClip == null)
            {
                AssetDatabase.CreateAsset(newClip, path);
            }

            AssetDatabase.SaveAssets();

            Debug.Log(
                $"Merged {poseCount} poses into {path}. "
                + "GUID was preserved if the file existed.");
        }

        private static void ApplyLinearTangents(AnimationCurve curve)
        {
            Keyframe[] keys = curve.keys;
            int last = keys.Length - 1;
            for (int i = 0; i <= last; i++)
            {
                float inSlope = i > 0
                    ? (keys[i].value - keys[i - 1].value) / (keys[i].time - keys[i - 1].time)
                    : 0f;
                float outSlope = i < last
                    ? (keys[i + 1].value - keys[i].value) / (keys[i + 1].time - keys[i].time)
                    : 0f;

                keys[i].inTangent = inSlope;
                keys[i].outTangent = outSlope;
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            }
            curve.keys = keys;
        }

        private static float GetDefaultFloatValue(
            EditorCurveBinding binding, Dictionary<EditorCurveBinding, float> references)
        {
            if (binding.type == typeof(Animator) && string.IsNullOrEmpty(binding.path))
            {
                return 0f;
            }

            return references.TryGetValue(binding, out float value) ? value : 0f;
        }

        private static bool CurveValuesDiffer(AnimationCurve curve)
        {
            float first = curve.keys[0].value;
            for (int i = 1; i < curve.length; i++)
            {
                if (!Mathf.Approximately(curve.keys[i].value, first)) return true;
            }
            return false;
        }

        private static bool ObjectReferenceValuesDiffer(ObjectReferenceKeyframe[] keys)
        {
            Object first = keys[0].value;
            for (int i = 1; i < keys.Length; i++)
            {
                if (keys[i].value != first) return true;
            }
            return false;
        }
    }
}
#endif