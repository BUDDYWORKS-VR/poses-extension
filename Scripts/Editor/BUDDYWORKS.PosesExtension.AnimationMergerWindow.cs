#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;

namespace BUDDYWORKS.PosesExtension
{
    public class AnimationMergerWindow : EditorWindow
    {
        [SerializeField] private List<AnimationClip> clipsToMerge = new List<AnimationClip>();
        [SerializeField] private bool loopPosebank = true;
        
        private SerializedObject serializedObj;
        private ReorderableList reorderableList;
        private Vector2 scrollPos;

        [MenuItem("BUDDYWORKS/Poses Extension/Create Custom Posebank")]
        public static void ShowWindow()
        {
            GetWindow<AnimationMergerWindow>("Posebank Creator");
        }

        private void OnEnable()
        {
            serializedObj = new SerializedObject(this);
            reorderableList = new ReorderableList(serializedObj, 
                serializedObj.FindProperty("clipsToMerge"), 
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

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            reorderableList.DoLayoutList();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            
            loopPosebank = EditorGUILayout.Toggle("Loop Posebank", loopPosebank);

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(clipsToMerge.Count == 0);
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

            if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) && dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is AnimationClip clip) clipsToMerge.Add(clip);
                    }
                    evt.Use();
                }
            }
        }

        private void MergeAndSave()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Merged Animation", "MergedPoses", "anim", "Select save location");

            if (string.IsNullOrEmpty(path)) return;

            // Check if file exists to preserve GUID
            AnimationClip existingClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            AnimationClip newClip = existingClip != null ? existingClip : new AnimationClip();
            
            // Clear existing curves if we are overwriting to avoid ghost data
            if (existingClip != null)
            {
                newClip.ClearCurves();
            }

            float targetFrameRate = 60f; 
            newClip.frameRate = targetFrameRate;

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(newClip);
            settings.loopTime = false;
            settings.mirror = false;

            settings.loopBlendOrientation = true;  
            settings.keepOriginalOrientation = true; 
            settings.orientationOffsetY = 0f;

            settings.loopBlendPositionY = true;    
            settings.keepOriginalPositionY = true;   
            settings.level = 0f;

            settings.loopBlendPositionXZ = true;   
            settings.keepOriginalPositionXZ = true;  

            AnimationUtility.SetAnimationClipSettings(newClip, settings);
            
            var floatCurves = new Dictionary<EditorCurveBinding, AnimationCurve>();
            var objectCurves = new Dictionary<EditorCurveBinding, List<ObjectReferenceKeyframe>>();
            
            List<AnimationClip> processClips = new List<AnimationClip>(clipsToMerge);
            if (loopPosebank && processClips.Count > 0 && processClips[0] != null)
            {
                processClips.Add(processClips[0]);
            }

            for (int i = 0; i < processClips.Count; i++)
            {
                AnimationClip sourceClip = processClips[i];
                if (sourceClip == null) continue;

                float targetTime = i / targetFrameRate;

                foreach (var binding in AnimationUtility.GetCurveBindings(sourceClip))
                {
                    if (!floatCurves.ContainsKey(binding)) floatCurves[binding] = new AnimationCurve();
                    
                    AnimationCurve sourceCurve = AnimationUtility.GetEditorCurve(sourceClip, binding);
                    if (sourceCurve.length > 0)
                    {
                        floatCurves[binding].AddKey(new Keyframe(targetTime, sourceCurve.keys[0].value));
                    }
                }

                foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(sourceClip))
                {
                    if (!objectCurves.ContainsKey(binding)) objectCurves[binding] = new List<ObjectReferenceKeyframe>();
                    
                    ObjectReferenceKeyframe[] sourceKeys = AnimationUtility.GetObjectReferenceCurve(sourceClip, binding);
                    if (sourceKeys.Length > 0)
                    {
                        ObjectReferenceKeyframe newKey = sourceKeys[0];
                        newKey.time = targetTime;
                        objectCurves[binding].Add(newKey);
                    }
                }
            }

            foreach (var kvp in floatCurves)
            {
                AnimationCurve curve = kvp.Value;
                for (int j = 0; j < curve.length; j++)
                {
                    AnimationUtility.SetKeyLeftTangentMode(curve, j, AnimationUtility.TangentMode.Linear);
                    AnimationUtility.SetKeyRightTangentMode(curve, j, AnimationUtility.TangentMode.Linear);
                }
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
            AssetDatabase.Refresh();
            
            Debug.Log($"Merged {processClips.Count} poses. GUID was preserved if file existed.");
        }
    }
}
#endif