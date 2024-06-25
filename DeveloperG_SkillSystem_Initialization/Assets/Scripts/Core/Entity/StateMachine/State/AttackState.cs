using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State<Entity>
{
    private EntityMovement movement;
    protected override void Setup()
    {
        movement = Entity.GetComponent<EntityMovement>();
    }

    public override void Enter()
    {
        Debug.Log($"{movement.Owner.name} Attack In");

        if (movement)
            movement.enabled = false;
    }

    public override void Exit()
    {
        Debug.Log($"{movement.Owner.name} Attack Out");

        if (movement)
            movement.enabled = true;
    }
}
