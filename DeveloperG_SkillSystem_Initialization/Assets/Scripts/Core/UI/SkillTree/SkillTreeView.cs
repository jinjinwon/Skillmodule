using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillTreeView : MonoSingleton<SkillTreeView>
{
    // Slot UI���� �θ� Object
    [SerializeField]
    private Transform root;
    [SerializeField]
    private TextMeshProUGUI titleText;

    [Header("Slot")]
    [SerializeField]
    private GameObject slotViewPrefab;
    [SerializeField]
    private Vector2 slotSize;
    // Slot ������ �Ÿ�
    [SerializeField]
    private float spacing;

    [Header("Line")]
    [SerializeField]
    private GameObject linkLinePrefab;
    [SerializeField]
    private float lineWidth;

    // ������ SlotView��
    private List<SkillTreeSlotView> slotViews = new();
    // ������ Line��
    private List<RectTransform> linkLines = new();

    // �� SlotView���� �ڽ��� �Ʒ��� �����ִ� Line���� �����ϴ� ����
    private Dictionary<SkillTreeSlotView, List<RectTransform>> linesBySlot = new();

    public Entity RequesterEntity { get; private set; }
    public SkillTree ViewingSkillTree { get; private set; }

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        SkillTooltip.Instance.Hide();
    }

    public void Show(Entity entity, SkillTree skillTree)
    {
        gameObject.SetActive(true);

        if (RequesterEntity == entity && ViewingSkillTree == skillTree)
            return;

        RequesterEntity = entity;
        ViewingSkillTree = skillTree;

        titleText.text = skillTree.DisplayName;

        var nodes = skillTree.GetSlotNodes();

        ChangeViewWidth(nodes);
        CreateSlotViews(nodes.Length);
        PlaceSlotViews(entity, nodes);
        LinkSlotViews(entity, nodes);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void ChangeViewWidth(SkillTreeSlotNode[] nodes)
    {
        // SkillTreeView�� ���̸� Node ���� �°� ����
        var maxOverIndex = nodes.Max(x => x.Index) + 1;
        var width = ((slotSize.x + spacing) * maxOverIndex) + spacing;
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new(width, rectTransform.sizeDelta.y);
    }

    private void CreateSlotViews(int nodeCount)
    {
        // slotViews���� �����Ѹ�ŭ SlotView�� ������
        int needSlotCount = nodeCount - slotViews.Count;
        for (int i = 0; i < needSlotCount; i++)
        {
            var slotView = Instantiate(slotViewPrefab, root).GetComponent<SkillTreeSlotView>();

            var rectTransform = slotView.GetComponent<RectTransform>();
            rectTransform.sizeDelta = slotSize;

            slotViews.Add(slotView);
        }
    }

    private RectTransform CreateLinkLine()
    {
        var linkLine = Instantiate(linkLinePrefab, root).GetComponent<RectTransform>();
        linkLine.transform.SetAsFirstSibling();

        linkLines.Add(linkLine);

        return linkLine;
    }

    private void PlaceSlotViews(Entity entity, SkillTreeSlotNode[] nodes)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            var slotView = slotViews[i];
            slotView.gameObject.SetActive(true);
            slotView.SetViewTarget(entity, node);

            // Slot�� ��ġ�ؾ��� ��ǥ�� ����
            var x = (node.Index * slotSize.x);
            var y = (node.Tier * -slotSize.y);
            // �ٸ� Node�� ������ �α����� �ʿ��� spacing ���� ����
            var xSpacing = spacing * (node.Index + 1);
            var ySpacing = spacing * (node.Tier + 1);
            var position = new Vector3(x + xSpacing, y - ySpacing);

            slotView.transform.localPosition = position;
        }

        // ���� �ʴ� SlotView���� ��� ����
        for (int i = nodes.Length; i < slotViews.Count; i++)
            slotViews[i].gameObject.SetActive(false);
    }

    private void LinkSlotViews(Entity entity, SkillTreeSlotNode[] nodes)
    {
        int nextLineIndex = 0;
        var halfSlotSize = slotSize * 0.5f;

        foreach ((var slotView, var lines) in linesBySlot)
            slotView.onSkillAcquired -= OnSkillAcquired;

        linesBySlot.Clear();

        for (int i = 1; i < nodes.Length; i++)
        {
            var node = nodes[i];
            var slotView = slotViews[i];

            foreach (var precedingNode in node.GetPrecedingSlotNodes())
            {
                var linkTargetSlot = slotViews.Find(x => x.SlotNode == precedingNode);
                // Cach�� Line�� ���Ҵٸ� ��������, ���ٸ� ���� ���� ������
                var line = nextLineIndex < linkLines.Count ? linkLines[nextLineIndex] : CreateLinkLine();
                nextLineIndex++;

                // ���� ���� Skill�� Entity�� SkillSystem�� ��ϵǾ� �ִٸ� Line�� ������� ǥ������
                if (entity.SkillSystem.Contains(precedingNode.Skill))
                    line.GetComponent<Image>().color = Color.yellow;
                else
                {
                    if (!linesBySlot.ContainsKey(linkTargetSlot))
                        linesBySlot[linkTargetSlot] = new List<RectTransform>();

                    // Line�� ���� ���� Slot Viwe�� ���ӽ�Ŵ
                    linesBySlot[linkTargetSlot].Add(line);

                    linkTargetSlot.onSkillAcquired += OnSkillAcquired;
                }

                // Line�� ���� ���� SlotView�� ���� SlotView�� ���� ��ġ�� �̵� ��Ŵ.
                var linePosition = (linkTargetSlot.transform.localPosition + slotView.transform.localPosition) * 0.5f;
                linePosition.x += halfSlotSize.x;
                linePosition.y -= halfSlotSize.y;

                line.localPosition = linePosition;

                // Line�� ���� ���� SlotView���� ���� SlotView�� �ٶ󺸴� ������ ȸ����Ŵ
                var direction = linkTargetSlot.transform.localPosition - slotView.transform.localPosition;
                float lineHeight = direction.y;

                line.transform.up = direction;
                line.sizeDelta = new(lineWidth, lineHeight);
            }
        }

        // ���� �ʴ� Line���� ��� ����
        for (int i = nextLineIndex; i < linkLines.Count; i++)
            linkLines[i].gameObject.SetActive(false);
    }

    private void OnSkillAcquired(SkillTreeSlotView slotView, Skill skill)
    {
        foreach (var line in linesBySlot[slotView])
            line.GetComponent<Image>().color = Color.yellow;
    }
}
