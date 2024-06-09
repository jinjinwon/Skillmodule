using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNodeEditor;
using XNode;

[CustomEditor(typeof(SkillTree))]
public class SkillTreeEditor : IdentifiedObjectEditor
{
    private SerializedProperty graphProperty;

    protected override void OnEnable()
    {
        base.OnEnable();

        graphProperty = serializedObject.FindProperty("graph");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        // graph�� ������ �ڵ����� �������
        if (graphProperty.objectReferenceValue == null)
        {
            var targetObject = serializedObject.targetObject;
            var newGraph = CreateInstance<SkillTreeGraph>();
            newGraph.name = "Skill Tree Graph";

            // ���� ������ ����� ���� ��쿡 ���
            //newGraph.hideFlags = HideFlags.HideInHierarchy; 

            // NodeGraph�� ScriptableObject Type�̹Ƿ� �Ϲ����� �ڷ���ó�� Serialize�� �� �� ���⿡
            // IdendifiedObject�� ���� Asset���� ���� �ҷ����� ������� Serialize��
            AssetDatabase.AddObjectToAsset(newGraph, targetObject);
            AssetDatabase.SaveAssets();

            graphProperty.objectReferenceValue = newGraph;
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Open Window", GUILayout.Height(50f)))
            NodeEditorWindow.Open(graphProperty.objectReferenceValue as NodeGraph);

        serializedObject.ApplyModifiedProperties();
    }
}
