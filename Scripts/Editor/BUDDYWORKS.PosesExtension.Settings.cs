using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace BUDDYWORKS.PosesExtension
{
    public class Settings : EditorWindow
    {
        private const string AssetGuid = "17cce2e1703370a41b2f584d6364944a";
        private string _assetPath;
        private SerializedObject _serializedObject;
        private SerializedProperty _parameters;

        private Sync _syncTab;
        private Other _otherTab;

        private int _selectedTab;
        private readonly string[] _tabNames = { "Sync", "Other" };

        [MenuItem("BUDDYWORKS/Poses Extension/Settings...")]
        public static void ShowWindow()
        {
            Settings window = GetWindow<Settings>("Poses Extension Settings");
            Texture2D icon = EditorGUIUtility.IconContent("Settings").image as Texture2D;
            if (icon != null)
            {
                window.titleContent = new GUIContent("Poses Extension Settings", icon);
            }
        }

        private void OnEnable()
        {
            _assetPath = AssetDatabase.GUIDToAssetPath(AssetGuid);

            if (string.IsNullOrEmpty(_assetPath))
            {
                Debug.LogError($"Asset with GUID {AssetGuid} not found.");
                return;
            }

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(_assetPath);
            if (asset == null)
            {
                Debug.LogError($"Failed to load asset at path {_assetPath}");
                return;
            }

            _serializedObject = new SerializedObject(asset);
            _parameters = _serializedObject.FindProperty("parameters");

            if (_parameters == null || !_parameters.isArray)
            {
                Debug.LogError("'parameters' property not found or not an array.");
                return;
            }

            _syncTab = new Sync(_serializedObject, _parameters);
            _otherTab = new Other();
        }

        private void OnGUI()
        {
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);

            switch (_selectedTab)
            {
                case 0:
                    _syncTab.DrawSyncTab();
                    break;
                case 1:
                    _otherTab.DrawOtherTab();
                    break;
            }
        }
    }
}
#endif