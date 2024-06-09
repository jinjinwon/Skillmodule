using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(SkillTreeSlotNode))]
public class SkillTreeSlotNodeEditor : NodeEditor
{
    // Foldout Title�� �׸������� Dictionary
    private Dictionary<string, bool> isFoldoutExpandedesByName = new Dictionary<string, bool>();

    // Node�� Title�� ��� �׸��� �����ϴ� �Լ�
    public override void OnHeaderGUI()
    {
        var targetAsSlotNode = target as SkillTreeSlotNode;

        // ������ �� ���� ������ �� �� �ֵ��� Header�� Node�� Tier�� Index, Node�� ���� Skill�� CodeName�� ������
        string header = $"Tier {targetAsSlotNode.Tier} - {targetAsSlotNode.Index} / " + (targetAsSlotNode.Skill?.CodeName ?? target.name);
        GUILayout.Label(header, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }

    // Node�� ���θ� ��� �׸��� �����ϴ� �Լ�
    public override void OnBodyGUI()
    {
        serializedObject.Update();

        float originLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 120f;

        // Node �ڽ��� return�ϴ� Outport�� ����
        NodePort output = target.GetPort("thisNode");
        NodeEditorGUILayout.PortField(GUIContent.none, output);

        DrawDefault();
        DrawSkill();
        DrawAcquisition();
        DrawPrecedingCondition();

        EditorGUIUtility.labelWidth = originLabelWidth;

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawDefault()
    {
        GUI.enabled = false;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("tier"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("index"));
        GUI.enabled = true;
    }

    private void DrawSkill()
    {
        if (!DrawFoldoutTitle("Skill"))
            return;

        var skillProperty = serializedObject.FindProperty("skill");
        var skill = skillProperty.objectReferenceValue as Skill;
        if (skill?.Icon)
        {
            EditorGUILayout.BeginHorizontal();
            {
                // Node�� ����(NodeWidth Attribute)�� ã�ƿ�
                var widthAttribute = typeof(SkillTreeSlotNode).GetCustomAttribute<Node.NodeWidthAttribute>();
                // �Ʒ� Icon Texture�� ����� �׷��� �� �ֵ��� Space�� ���� GUI�� �׷����� ��ġ�� ����� �̵� 
                GUILayout.Space((widthAttribute.width * 0.5f) - 50f);

                var preview = AssetPreview.GetAssetPreview(skill.Icon);
                GUILayout.Label(preview, GUILayout.Width(80), GUILayout.Height(80));
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.PropertyField(skillProperty);
    }

    private void DrawAcquisition()
    {
        if (!DrawFoldoutTitle("Acquisition"))
            return;
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isSkillAutoAcquire"));
    }

    private void DrawPrecedingCondition()
    {
        if (!DrawFoldoutTitle("Preceding Condition"))
            return;

        // List�� �� Element�� Port�� �޸� ���·� List�� �׷���
        // Port�� precedingLevels�� �� Element���� �Ҵ�Ǿ� �����ǹǷ� �迭�� ���̰� 10�̶�� 10���� Port�� ������.
        // ���⼭�� List�� Default ���·� �׸��� �ʰ�, ���¸� Customize�ϱ� ����
        // onCreation ������ OnCreateReorderableList Callback �Լ��� �Ѱ���
        NodeEditorGUILayout.DynamicPortList("precedingLevels", typeof(int), serializedObject,
            NodePort.IO.Input, Node.ConnectionType.Override, onCreation: OnCreatePrecedingLevels);
    }

    // precedingLevels ������ ReorderableList ���·� �׷��ִ� �Լ�
    private void OnCreatePrecedingLevels(ReorderableList list)
    {
        list.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "Preceding Skills");
        };

        list.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            // ���� index�� �ش��ϴ� Element�� ������
            var element = serializedObject.FindProperty("precedingLevels").GetArrayElementAtIndex(index); 
            // ������ Element�� �׷���, Need Level�̶�� Label�� ���� int Field�� �׷���
            EditorGUI.PropertyField(rect, element, new GUIContent("Need Level"));

            // Element�� �Ҵ�� Port�� ã�ƿ�, �� Port�� �ٸ� Node���� �����
            // GetPort ��Ģ�� (�迭 ���� �̸� + ã�ƿ� Port�� index)
            // ex. precedingLevels�� ù��° Element�� �Ҵ�� Port�� ã�ƿ����� target.GetPort("precedingLevels 0")
            var port = target.GetPort("precedingLevels " + index);
            // Port�� ����� Output Port�� ���� ��,
            // Output Port�� ��ȯ ���� SkillTreeSlotNode Type�� �ƴ϶�� ������ ����
            // Node.TypeConstraint.Strict�� ���� �������� ����
            if (port.Connection != null && port.Connection.GetOutputValue() is not SkillTreeSlotNode)
                port.Disconnect(port.Connection);

            // Port�� ����� Output Port�� ��ȯ���� SkillTreeSlotNode Type���� Casting�Ͽ� ������
            // GetInputValue() �Լ��� object type���� ������ ���� ����
            // Node�� ConnectionType�� Multiple�� ���,
            // GetInputValues �Լ��� ����� ��� Port�� Value�� ������ �� ����
            var inputSlot = port.GetInputValue<SkillTreeSlotNode>();
            // ����� Port�� �ְ�, �ش� Port�� Skill �Ҵ�Ǿ� �ִٸ�, ���� Node�� ���� Skill�� �ִ� Level�� ����
            if (inputSlot && inputSlot.Skill)
                element.intValue = Mathf.Clamp(element.intValue, 1, inputSlot.Skill.MaxLevel);

            // �� Code�� �Ʒ� Code�� �����ų �� ����
            //if (port.TryGetInputValue<SkillTreeSlotNode>(out var inputSlot))
            //    element.intValue = Mathf.Clamp(element.intValue, 1, inputSlot.Skill.MaxLevel);

            // Rect ��ǥ�� ���� �׸� int Field���� �������� �ű�
            var position = rect.position;
            position.x -= 37f;
            // Port�� �׷���
            NodeEditorGUILayout.PortField(position, port);
        };
    }

    private bool DrawFoldoutTitle(string title)
        => CustomEditorUtility.DrawFoldoutTitle(isFoldoutExpandedesByName, title);
}