using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CustomAnimationBehaviour : StateMachineBehaviour
{
    private EntityAI entityAI;
    public float normalizedTime = 1;

    private bool hasTriggered = false;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (entityAI != null)
            return;

        entityAI = animator.GetComponent<EntityAI>();
    }

    // �� �����Ӹ��� �ִϸ��̼� ���°� ������Ʈ�� �� ȣ��˴ϴ�.
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float currentTime = stateInfo.normalizedTime % 1;

        // �ִϸ��̼��� ���� �ð��� n%�� �����ϰ� �Լ��� ���� Ʈ���ŵ��� �ʾҴٸ� �Լ��� ȣ���մϴ�.
        if ((1.0f - currentTime) < 0.1f && !hasTriggered)
        {
            TriggerCustomFunction();
            hasTriggered = true;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasTriggered = false;
    }

    void TriggerCustomFunction()
    {
        if (!entityAI.Owner.IsDead && entityAI.isSelectTargetAlive)
        {
            entityAI.Owner.Target.TakeDamage(entityAI.Owner, null, 1);
            entityAI.Owner.IsAttack = false;
        }
    }
}
