using UnityEditor;
using UnityEngine;

namespace BuildR2
{
    [CustomEditor(typeof(Readme))]
    public class ReadmeEditor : Editor
    {
        private Readme _readme;

        private void OnEnable()
        {
            _readme = (Readme)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(_readme.content, MessageType.Info);
//            EditorStyles.label.wordWrap = false;
//            EditorStyles.label.richText = true;
//            GUILayout.Label(_readme.content, GUILayout.Width(250), GUILayout.Height(400));
        }
    }
}