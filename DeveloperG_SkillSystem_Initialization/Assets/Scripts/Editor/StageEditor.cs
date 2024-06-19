using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using static UnityEditor.Rendering.FilterWindow;

[CustomEditor(typeof(Stage))]
public class StageEditor : IdentifiedObjectEditor
{
    private SerializedProperty maxFloorProperty;
    private SerializedProperty stageDatasProperty;
    private SerializedProperty isAllowFloorExceedDatasProperty;
    private SerializedProperty defaultFloorProperty;

    // Toolbar Button���� �̸�
    private readonly string[] customActionsToolbarList = new[] { "NextFloor"};
    // Skill Data���� ������ Toolbar Button�� Index ��
    private Dictionary<int, int> customActionToolbarIndexesByLevel = new();

    protected override void OnEnable()
    {
        base.OnEnable();

        maxFloorProperty = serializedObject.FindProperty("maxFloor");
        defaultFloorProperty = serializedObject.FindProperty("defaultFloor");
        isAllowFloorExceedDatasProperty = serializedObject.FindProperty("isAllowFloorExceedDatas");
        stageDatasProperty = serializedObject.FindProperty("stageDatas");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        float prevLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 220f;

        DrawSkillDatas();

        EditorGUIUtility.labelWidth = prevLabelWidth;

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSkillDatas()
    {
        // Skill�� Data�� �ƹ��͵� �������� ������ 1���� �ڵ������� �������
        if (stageDatasProperty.arraySize == 0)
        {
            // �迭 ���̸� �÷��� ���ο� Element�� ����
            stageDatasProperty.arraySize++;
            // �߰��� Data�� Level�� 1�� ����
            stageDatasProperty.GetArrayElementAtIndex(0).FindPropertyRelative("floor").intValue = 1;
        }

        if (!DrawFoldoutTitle("Data"))
            return;

        EditorGUILayout.PropertyField(isAllowFloorExceedDatasProperty);

        // Level ���� ������ ���ٸ� MaxLevel�� �״�� �׷��ְ�,
        // ���� ������ �ִٸ� MaxLevel�� �������� ���� ��Ű�� �۾��� ��
        if (isAllowFloorExceedDatasProperty.boolValue)
            EditorGUILayout.PropertyField(maxFloorProperty);
        else
        {
            // Property�� �������� ���ϰ� GUI Enable�� false�� �ٲ�
            GUI.enabled = false;
            var lastIndex = stageDatasProperty.arraySize - 1;
            // ������ SkillData(= ���� ���� Level�� Data)�� ������
            var lastSkillData = stageDatasProperty.GetArrayElementAtIndex(lastIndex);
            // maxFloor�� ������ Data�� Floor�� ����
            maxFloorProperty.intValue = lastSkillData.FindPropertyRelative("floor").intValue;
            // maxFloor Property�� �׷���
            EditorGUILayout.PropertyField(maxFloorProperty);
            GUI.enabled = true;
        }

        EditorGUILayout.PropertyField(defaultFloorProperty);

        for (int i = 0; i < stageDatasProperty.arraySize; i++)
        {
            var property = stageDatasProperty.GetArrayElementAtIndex(i);
            SerializedProperty bossRoundProperty = property.FindPropertyRelative("isBossRound");
            bool isBossRound = bossRoundProperty != null && bossRoundProperty.propertyType == SerializedPropertyType.Boolean && bossRoundProperty.boolValue;

            EditorGUILayout.BeginVertical("HelpBox");
            {
                // Data�� Level�� Data ������ ���� X Button�� �׷��ִ� Foldout Title�� �׷���
                // ��, ù��° Data(= index 0) ����� �ȵǱ� ������ X Button�� �׷����� ����
                // X Button�� ������ Data�� �������� true�� return��
                if (DrawRemovableLevelFoldout_Floor(stageDatasProperty, property, i, i != 0))
                {
                    // Data�� �����Ǿ����� �� �̻� GUI�� �׸��� �ʰ� �ٷ� ��������
                    // ���� Frame�� ó������ �ٽ� �׸��� ����
                    EditorGUILayout.EndVertical();
                    break;
                }

                EditorGUI.indentLevel += 1;

                if (property.isExpanded)
                {
                    // SkillData Property ���η� �� -> Property == level field;
                    property.NextVisible(true);

                    DrawAutoSortLevelProperty(stageDatasProperty, property, i, i != 0);

                    // Level Up
                    for (int j = 0; j < 7; j++)
                    {
                        property.NextVisible(false);

                        if (property.name == "bossMonsterGen" && !isBossRound)
                        {
                            continue; // ������ �����ϸ� �ʵ带 �׸��� ����
                        }
                        EditorGUILayout.PropertyField(property);
                    }

                    // Custom Action - UnderlineTitle
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("NextFloor Action", EditorStyles.boldLabel);
                    CustomEditorUtility.DrawUnderline();

                    // Custom Action - Toolbar
                    // �ѹ��� ��� Array ������ �� �׸��� ���� �����ϴ� Toolbar�� ���� ������ Array�� ������ �� �ְ���.
                    var customActionToolbarIndex = customActionToolbarIndexesByLevel.ContainsKey(i) ? customActionToolbarIndexesByLevel[i] : 0;
                    // Toolbar�� �ڵ� �鿩����(EditorGUI.indentLevel)�� ������ �ʾƼ� ���� �鿩���⸦ ����
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(12);
                        customActionToolbarIndex = GUILayout.Toolbar(customActionToolbarIndex, customActionsToolbarList);
                        customActionToolbarIndexesByLevel[i] = customActionToolbarIndex;
                    }
                    GUILayout.EndHorizontal();

                    property.NextVisible(false);
                    EditorGUILayout.PropertyField(property);
                }
                EditorGUI.indentLevel -= 1;
            }
            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Add New Floor"))
        {
            // Level Change
            var lastArraySize = stageDatasProperty.arraySize++;
            var prevElementalProperty = stageDatasProperty.GetArrayElementAtIndex(lastArraySize - 1);
            var newElementProperty = stageDatasProperty.GetArrayElementAtIndex(lastArraySize);
            var newElementLevel = prevElementalProperty.FindPropertyRelative("floor").intValue + 1;
            newElementProperty.FindPropertyRelative("floor").intValue = newElementLevel;
            newElementProperty.isExpanded = true;

            // �ϴ� ���� ������Ʈ�� ���� �ν����Ϳ� ������ �Ǵ� ������ ���� �� 
            // ���� Ŭ���� ��������� �׶� �۵��� ��;
            CustomEditorUtility.DeepCopyGameObjectArray(newElementProperty.FindPropertyRelative("monsters"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
