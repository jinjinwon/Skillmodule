using Google.Apis.Sheets.v4.Data;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private Entity entity;
    private void Start()
    {
        entity = GetComponent<Entity>();
        entity.SkillSystem.onSkillTargetSelectionCompleted += ReserveSkill;

        MouseController.Instance.onLeftClicked += SelectTarget;
    }

    private void OnEnable()
        => MouseController.Instance.onRightClicked += MoveToPosition;

    private void OnDisable()
        => MouseController.Instance.onRightClicked -= MoveToPosition;

    private void OnDestroy()
        => MouseController.Instance.onLeftClicked -= SelectTarget;

    #region 테스트용 코드
    [ContextMenu("ㅎㅎ")]
    public void Tees()
    {
        entity.UserClickedRevive = true;

        Invoke("Deea", 3f);
    }

    public void Deea()
    {
        entity.Stats.HPStat.DefaultValue = entity.Stats.HPStat.MaxValue;
        entity.UserClickedRevive = false;

        entity.EntityAI.Setup(entity);
    }
    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            var skillTreeView = SkillTreeView.Instance;
            if (!skillTreeView.gameObject.activeSelf)
                skillTreeView.Show(entity, entity.SkillSystem.DefaultSkillTree);
            else
                skillTreeView.Hide();
        }

        //Vector3 direction = Vector3.forward * Joystick.Instance.Horizontal + Vector3.right * -Joystick.Instance.Vertical;

        //if (direction != Vector3.zero)
        //{
        //    entity.Movement.Destination = entity.Movement.Destination + direction;
        //    entity.SkillSystem.CancelReservedSkill();
        //}
    }

    private void SelectTarget(Vector2 mousePosition)
    {
        var ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity))
        {
            var entity = hitInfo.transform.GetComponent<Entity>();
            if (entity)
                EntityHUD.Instance.Show(entity);
        }
    }

    private void MoveToPosition(Vector2 mousePosition)
    {
        var ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            entity.Movement.Destination = hitInfo.point;
            entity.SkillSystem.CancelReservedSkill();
        }
    }

    private void ReserveSkill(SkillSystem skillSystem, Skill skill, TargetSearcher targetSearcher, TargetSelectionResult result)
    {
        if (result.resultMessage != SearchResultMessage.OutOfRange ||
            !skill.IsInState<SearchingTargetState>())
            return;
        
        entity.SkillSystem.ReserveSkill(skill);

        var selectionResult = skill.TargetSelectionResult;

        if (selectionResult.selectedTarget)
            entity.Movement.TraceTarget = selectionResult.selectedTarget.transform;
        else
            entity.Movement.Destination = selectionResult.selectedPosition;
    }
}
