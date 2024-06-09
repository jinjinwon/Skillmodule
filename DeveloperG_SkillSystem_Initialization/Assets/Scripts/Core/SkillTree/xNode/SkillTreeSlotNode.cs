using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

// NodeWidth: Graph���� ���������� Node�� ����
// NodeTint: Node�� RGB(255, 255, 255) Color
[NodeWidth(300), NodeTint(60, 60, 60)]
public class SkillTreeSlotNode : Node
{
    // graph���� ���° �� or ������ ����
    [SerializeField]
    private int tier;
    // tier���� ���° Slot���� ����
    // ex. (tier=1, index=0), (tier=1, index=1), (tier=1, index=2) ...
    // tier�� index�� ���ļ� 2���� �迭 ������
    [SerializeField]
    private int index;

    // �� Node�� ������ �ִ� Skill
    [SerializeField]
    private Skill skill;

    // Skill�� ������ �������� �� �ڵ����� ������ ������ ����
    [SerializeField]
    private bool isSkillAutoAcquire;

    // Skill�� �����ϱ� ���� �ʿ��� ���� Skill��� Skill���� Level�� �޴� ����.
    // precedingLevels ��ü�� int�� �迭�̶� �ʿ��� Level ���� ���� �� ������,
    // CustomEditor�� ���ؼ� �߰��Ǵ� Element���� Input Port�� �Ҵ��ؼ� �ٸ� ���� Skill Node�� ����� �� �ֵ��� �� ����.
    // ��, Element�� �߰��ϸ� �ʿ��� Level�� �Է��ϰ� Element�� �Ҵ�� Port�� ���� Skill Node�� �����ؾ� ������ ���� �Է��� �Ϸ�� ����.
    [Input]
    [SerializeField]
    private int[] precedingLevels;

    // �ٸ� Node�� precdingLevels ������ ������ ���� Node(this)
    [Output(connectionType = ConnectionType.Override), HideInInspector]
    [SerializeField]
    private SkillTreeSlotNode thisNode;

    public int Tier => tier;
    public int Index => index;
    public Skill Skill => skill;
    public bool IsSkillAutoAcquire => isSkillAutoAcquire;

    protected override void Init()
    {
        thisNode = this;
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName != "thisNode")
            return null;
        return thisNode;
    }

    // �ʿ��� ���� Skill��
    // �� precdingLevels ������ ���� ã�ƿ�
    public SkillTreeSlotNode[] GetPrecedingSlotNodes()
        => precedingLevels.Select((value, index) => GetPrecedingSlotNode(index)).ToArray();

    // precedingLevels�� Element�� �Ҵ�� Port���� Port�� ����� �ٸ� Node�� ã�ƿ�
    // Element�� �Ҵ�� Port�� ã�ƿ��� Naming ��Ģ�� (���� �̸� + Element�� index)
    // ex. precedingLevels�� ù��° Element�� �Ҵ�� Port�� ã�ƿ��� �ʹٸ� precedingLevels0
    // Port�� ����� Node�� ���ٸ� null�� ��ȯ��
    private SkillTreeSlotNode GetPrecedingSlotNode(int index)
        => GetInputValue<SkillTreeSlotNode>("precedingLevels " + index);

    public bool IsSkillAcquirable(Entity entity)
    {
        // Skill ��ü�� ���� ���� ������ �����ߴ��� Ȯ��
        if (!skill.IsAcquirable(entity))
            return false;

        // Entity�� ���� SKill���� ������ �ְ�, ���� Skill���� Level�� �����ߴ��� Ȯ��
        for (int i = 0; i < precedingLevels.Length; i++)
        {
            var inputNode = GetPrecedingSlotNode(i);
            var entitySkill = entity.SkillSystem.Find(inputNode.Skill);

            if (entitySkill == null || entitySkill.Level < precedingLevels[i])
                return false;
        }

        return true;
    }

    public Skill AcquireSkill(Entity entity)
    {
        Debug.Assert(IsSkillAcquirable(entity), "SkillTreeNode::AcquireSkill - Skill ���� ������ �������� ���߽��ϴ�.");
        return entity.SkillSystem.Register(skill);
    }
}