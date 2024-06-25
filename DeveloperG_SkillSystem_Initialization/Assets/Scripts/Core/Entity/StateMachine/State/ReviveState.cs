using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReviveState : State<Entity>
{
    private PlayerController playerController;
    private EntityMovement movement;
    protected override void Setup()
    {
        playerController = Entity.GetComponent<PlayerController>();
        movement = Entity.GetComponent<EntityMovement>();
    }

    public override void Enter()
    {
        Debug.Log("Revive");

        if (playerController)
            playerController.enabled = false;

        if (movement)
            movement.enabled = false;
    }

    // UI ��ư�� �̺�Ʈ�� �־��ָ� ��
    public void Revive()
    {
        // HP ���� �ٲ��ָ� Entity.isDead�� ���� false�� �ٲ�
        movement.Owner.Stats.HPStat.DefaultValue = movement.Owner.Stats.HPStat.MaxValue;
        movement.Owner.UserClickedRevive = false;
    }

    public override void Exit()
    {
        if (playerController)
            playerController.enabled = true;

        if (movement)
        {
            movement.enabled = true;
            movement.Owner.UserClickedRevive = false;
        }
    }
}
