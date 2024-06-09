using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SkillTreeSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    #region Events
    public delegate void SkillAcquiredHandler(SkillTreeSlotView slotView, Skill skill);
    #endregion

    [SerializeField]
    private Image iconImage;
    [SerializeField]
    private TextMeshProUGUI levelText;
    [SerializeField]
    private GameObject normalBorder;
    [SerializeField]
    private GameObject acquiredBorder;
    [SerializeField]
    private GameObject blind;

    private Entity requester;
    private Skill requesterOwnedSkill;

    public Entity Requester => requester;
    // Requester�� SkillSystem���� ã�ƿ� SlotSkill => Entity�� ������ Skill
    public Skill RequesterOwnedSkill => requesterOwnedSkill;
    public Skill SlotSkill => SlotNode.Skill;
    public SkillTreeSlotNode SlotNode { get; private set; }

    public event SkillAcquiredHandler onSkillAcquired;

    private void OnDestroy()
    {
        if (!requester)
            return;

        requester.SkillSystem.onSkillRegistered -= OnSkillRegistered;
    }

    private void Update()
    {
        if (requesterOwnedSkill)
            return;

        bool isAcquirable = SlotNode.IsSkillAcquirable(requester);
        blind.SetActive(!isAcquirable);
    }

    public void SetViewTarget(Entity requester, SkillTreeSlotNode slotNode)
    {
        if (requester)
            requester.SkillSystem.onSkillRegistered -= OnSkillRegistered;

        this.requester = requester;
        SlotNode = slotNode;

        var skill = slotNode.Skill;

        requesterOwnedSkill = requester.SkillSystem.Find(skill);
        if (!requesterOwnedSkill)
            requester.SkillSystem.onSkillRegistered += OnSkillRegistered;

        iconImage.sprite = skill.Icon;

        UpdateAcquisitionUI();
        UpdateLevelText();
    }

    private void UpdateAcquisitionUI()
    {
        normalBorder.SetActive(!requesterOwnedSkill);
        acquiredBorder.SetActive(requesterOwnedSkill);
        blind.SetActive(!requesterOwnedSkill && !SlotNode.IsSkillAcquirable(requester));
    }

    private void UpdateLevelText()
    {
        int level = requesterOwnedSkill ? requesterOwnedSkill.Level : 0;
        levelText.text = $"{level} / {SlotNode.Skill.MaxLevel}";
        levelText.color = (requesterOwnedSkill && requesterOwnedSkill.IsMaxLevel) ? Color.yellow : Color.white;
    }

    private void ShowTooltip()
    {
        SkillTooltip.Instance.Show(this);
    }

    private void HideTooltip()
    {
        SkillTooltip.Instance.Hide();
    }

    private void OnSkillRegistered(SkillSystem skillSystem, Skill skill)
    {
        if (skill.ID != SlotNode.Skill.ID)
            return;

        requesterOwnedSkill = skill;

        UpdateAcquisitionUI();
        UpdateLevelText();

        skillSystem.onSkillRegistered -= OnSkillRegistered;

        onSkillAcquired?.Invoke(this, requesterOwnedSkill);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        => ShowTooltip();

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        => HideTooltip();

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        // Entity�� skill�� �����߰�, Level Up ������ �޼� �ߴٸ� Level Up�� ��Ŵ
        if (requesterOwnedSkill && requesterOwnedSkill.IsCanLevelUp)
        {
            requesterOwnedSkill.LevelUp();
            UpdateLevelText();
            ShowTooltip();
        }
        // Entity�� Skill�� �������� �ʾҰ�, Skill�� ������ �� �ִ� ���¶�� ������
        else if (!requesterOwnedSkill && SlotNode.IsSkillAcquirable(requester))
        {
            SlotNode.AcquireSkill(requester);
            ShowTooltip();
        }
    }
}
