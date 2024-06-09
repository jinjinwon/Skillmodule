using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeGraphEditor(typeof(SkillTreeGraph))]
public class SkillTreeGraphEditor : NodeGraphEditor
{
    // Graph�� �����ϴ� Node���� ��ġ�� �����ϴ� �迭.
    // Node���� ����� ��ġ�� ���� ��ġ�� �ٸ��ٸ� Node�� Update�� ���� ����.
    private Vector2[] nodePositions;

    private readonly int spacingBetweenTier = 320;

    // Graph Editor�� �� �� ����Ǵ� �Լ�
    public override void OnOpen()
    {
        target.nodes.Remove(null);
        nodePositions = target.nodes.Select(x => x.position).ToArray();
    }

    // Graph Editor�� GUI�� �׷��ִ� �Լ�
    // �����ؾ��ϴ� ������ �ְų� �׷��ְ� ���� GUI�� �ִٸ� Custom Editor�� �ۼ��ϴ� �Ͱ� �Ȱ��� Code�� �߰����ָ� ��
    public override void OnGUI()
    {
        if (CheckNodePositionUpdate())
            UpdateNodePositionsAndTiers();
    }

    // Graph�� Node�� ���� �����ϴ� �Լ� 
    public override Node CopyNode(Node original)
    {
        // base.CopyNode �Լ��� �����ؼ� ���ڷ� �Ѿ�� Copy ��� Node�� ���� ������
        var newNode = base.CopyNode(original);
        // ���ο� Node�� �߰� �Ǿ����� Node���� Position�� Tier�� Update ��
        UpdateNodePositionsAndTiers();

        return newNode;
    }

    // Graph�� Node�� �����ϴ� �Լ�
    public override Node CreateNode(Type type, Vector2 position)
    {
        var node = base.CreateNode(type, position);
        UpdateNodePositionsAndTiers();
        return node;
    }

    // Graph���� Node�� �����ϴ� �Լ�
    public override void RemoveNode(Node node)
    {
        base.RemoveNode(node);

        if (target.nodes.Count == 0)
            nodePositions = Array.Empty<Vector2>();
        else
            UpdateNodePositionsAndTiers();
    }

    // SkillTreeGraph���� ����� Node�� ��ȯ�ϴ� �Լ�
    // ���ڷδ� Project�� ���ǵǾ��ִ� ��� Node Type�� �Ѿ��
    public override string GetNodeMenuName(Type type)
    {
        if (type.Name == "SkillTreeSlotNode")
            return base.GetNodeMenuName(type);
        else
            return null;
    }

    private bool CheckNodePositionUpdate()
    {
        for (int i = 0; i < nodePositions.Length; i++)
        {
            if (nodePositions[i] != target.nodes[i].position)
                return true;
        }
        return false;
    }
    
    private void UpdateNodePositionsAndTiers()
    {
        if (target.nodes.Count == 0)
            return;

        // x��ǥ�� �������� node���� �������� ����
        // => Graph���� �������� ������ x��ǥ�� �۾����Ƿ� ���ʿ� �ִ� Node���� �����ʿ� �ִ� Node������ ���ĵ�
        target.nodes = target.nodes.OrderBy(x => x.position.x).ToList();
        // ������ Node���� ��ǥ�� ������
        nodePositions = target.nodes.Select(x => x.position).ToArray();

        int tier = 0;
        var nodes = target.nodes;
        
        var tierField = typeof(SkillTreeSlotNode).GetField("tier", BindingFlags.Instance | BindingFlags.NonPublic);
        tierField.SetValue(nodes[0], tier);

        var firstNodePosition = nodes[0].position;

        for (int i = 1; i < nodes.Count; i++)
        {
            // index�� �ش��ϴ� Node�� ù��° Node���� �Ÿ��� spacingBetweenTier�� ���� ���� �ش� Node�� Tier�� �� 
            tier = (int)(Mathf.Abs(nodes[i].position.x - firstNodePosition.x) / spacingBetweenTier);
            tierField.SetValue(nodes[i], tier);
        }

        var index = 0;
        // y��ǥ�� �������� node���� �������� ����
        // => Graph���� ���� ������ y��ǥ�� �۾����Ƿ� �Ʒ��� �ִ� Node���� ���� �ִ� Node������ ���ĵ�
        var nodesByY = nodes.OrderByDescending(x => x.position.y).ToArray();

        var indexField = typeof(SkillTreeSlotNode).GetField("index", BindingFlags.Instance | BindingFlags.NonPublic);
        indexField.SetValue(nodesByY[0], index);

        for (int i = 1; i < nodes.Count; i++)
        {
            // ���� ������ �ٸ� ������ ��ų Ʈ������ Tier�� �߰��� ����־ �̻����� ������
            // index�� �߰��� ��������� �̻��غ��̱� ������ index�� ������ ������� �������ϱ� ����
            // ���� Node�� ���� Node�� �Ÿ� ���̰� spacingBetweenTier��ŭ ���ٸ� index ���� ������Ŵ
            if (nodesByY[i - 1].position.y - nodesByY[i].position.y >= spacingBetweenTier)
                index++;

            indexField.SetValue(nodesByY[i], index);
        }
    }
}
