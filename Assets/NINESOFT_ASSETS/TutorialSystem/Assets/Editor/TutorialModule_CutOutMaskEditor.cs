using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace NINESOFT.TUTORIAL_SYSTEM
{
    [CustomEditor(typeof(TutorialModule_CutOutMask))]

    public class TutorialModule_CutOutMaskEditor : Editor
    {
        public SerializedProperty HoleScale;
        public SerializedProperty TargetUI;

        public SerializedProperty MaskColor;

        public override void OnInspectorGUI()
        {
            GUI.backgroundColor = NSEditorData.Purple2;

            FindProperties();
            DrawProperties();
        }

        private void FindProperties()
        {
            HoleScale = serializedObject.FindProperty("HoleScale");
            TargetUI = serializedObject.FindProperty("TargetUI");

            MaskColor = serializedObject.FindProperty("MaskColor");
        }

        private void DrawProperties()
        {
            TutorialModule_CutOutMask myObject = (TutorialModule_CutOutMask)target;

            NSEditorData.DrawComponentTitleBox("CUT OUT MASK",NSEditorData.EditorScriptType.ModuleUI);

            if (NSEditorData.DrawTitleBox("CUT OUT CONFIG", 0))
            {
                EditorGUILayout.PropertyField(TargetUI);
                EditorGUILayout.Space(20);
                EditorGUILayout.PropertyField(HoleScale);
                EditorGUILayout.PropertyField(MaskColor);         

            }
          
            serializedObject.ApplyModifiedProperties();
        }

    }
}
