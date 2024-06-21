using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using static UnityEditor.Rendering.FilterWindow;
using UnityEditorInternal;

[CustomEditor(typeof(Monster))]
public class MonsterEditor : IdentifiedObjectEditor
{
    private SerializedProperty typeProperty;
    private SerializedProperty patternProperty;
    private SerializedProperty animatorProperty;
    private SerializedProperty attackRangeProperty;
    private SerializedProperty statOverrideProperty;
    private SerializedProperty appearActionProperty;

    private ReorderableList statOverridesList;

    protected override void OnEnable()
    {
        base.OnEnable();

        typeProperty = serializedObject.FindProperty("type");
        patternProperty = serializedObject.FindProperty("pattern");
        animatorProperty = serializedObject.FindProperty("animatorController");
        attackRangeProperty = serializedObject.FindProperty("attackRange");
        statOverrideProperty = serializedObject.FindProperty("statOverrides");
        appearActionProperty = serializedObject.FindProperty("customActionsOnAppear");

        #region ∏ÛΩ∫≈Õ Ω∫≈›
        statOverridesList = new ReorderableList(serializedObject, statOverrideProperty, true, true, true, true)
        {
            drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Stat Overrides");
            },
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = statOverrideProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                var statRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(statRect, element.FindPropertyRelative("stat"), GUIContent.none);

                var isUseOverrideRect = new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 2, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(isUseOverrideRect, element.FindPropertyRelative("isUseOverride"), new GUIContent("Use Override"));

                if (element.FindPropertyRelative("isUseOverride").boolValue)
                {
                    var overrideDefaultValueRect = new Rect(rect.x, rect.y + (EditorGUIUtility.singleLineHeight + 2) * 2, rect.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(overrideDefaultValueRect, element.FindPropertyRelative("overrideDefaultValue"), new GUIContent("Override Default Value"));
                }
            },
            elementHeightCallback = (int index) =>
            {
                var element = statOverrideProperty.GetArrayElementAtIndex(index);
                var baseHeight = EditorGUIUtility.singleLineHeight + 4;
                if (element.FindPropertyRelative("isUseOverride").boolValue)
                {
                    baseHeight += EditorGUIUtility.singleLineHeight + 2;
                }
                return baseHeight + EditorGUIUtility.singleLineHeight + 2;
            },
            onAddCallback = (ReorderableList list) =>
            {
                statOverrideProperty.arraySize++;
                var newElement = statOverrideProperty.GetArrayElementAtIndex(statOverrideProperty.arraySize - 1);
                newElement.boxedValue = new StatMonsterOverride(null);
            }
        };
        #endregion
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        float prevLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 220f;

        DrawSettings();

        EditorGUIUtility.labelWidth = prevLabelWidth;

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSettings()
    {
        if (!DrawFoldoutTitle("Setting"))
            return;

        CustomEditorUtility.DrawEnumToolbar(typeProperty);
        CustomEditorUtility.DrawEnumToolbar(patternProperty);

        EditorGUILayout.PropertyField(animatorProperty);
        EditorGUILayout.PropertyField(attackRangeProperty);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("∏ÛΩ∫≈Õ Ω∫∆Â", EditorStyles.boldLabel);
        CustomEditorUtility.DrawUnderline();
        statOverridesList.DoLayoutList();

        EditorGUILayout.PropertyField(appearActionProperty, new GUIContent("CustomActionsOnAppear"), true);
    }
}
