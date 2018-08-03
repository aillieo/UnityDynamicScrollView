using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace AillieoUtils
{

    [CustomEditor(typeof(ScrollView))]
    public class ScrollViewEditor : ScrollRectEditor
    {

        SerializedProperty itemTemplate;
        SerializedProperty poolSize;
        SerializedProperty maxShownCount;
        SerializedProperty defaultItemSize;
        SerializedProperty layoutType;


        protected override void OnEnable()
        {
            base.OnEnable();

            itemTemplate = serializedObject.FindProperty("itemTemplate");
            poolSize = serializedObject.FindProperty("poolSize");
            maxShownCount = serializedObject.FindProperty("maxShownCount");
            defaultItemSize = serializedObject.FindProperty("defaultItemSize");
            layoutType = serializedObject.FindProperty("m_layoutType");
        }
        

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("<color=#0000FF><b>Additional configs</b></color>", new GUIStyle { richText = true });

            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(itemTemplate);
            EditorGUILayout.PropertyField(poolSize);
            EditorGUILayout.PropertyField(maxShownCount);
            EditorGUILayout.PropertyField(defaultItemSize);
            layoutType.intValue = (int)(ScrollView.ItemLayoutType)EditorGUILayout.EnumPopup("layoutType", (ScrollView.ItemLayoutType)layoutType.intValue);


            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Separator();
            
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("<color=#0000FF><b>For original ScrollRect</b></color>", new GUIStyle { richText = true });

            EditorGUI.indentLevel++;

            base.OnInspectorGUI();

            EditorGUI.indentLevel--;

        }
    }
}