using Google.Apis.Sheets.v4.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityAI : MonoBehaviour
{
    private Dictionary<Entity,float> sortedTargets = new Dictionary<Entity,float>();
    // ������ ����
    [SerializeField]
    private float detectionRadius;
    public Entity Owner { get; private set; }
    public Dictionary<Entity, float> SortedTargets { get => sortedTargets;}

    private Collider[] hitColliders;

    // Ÿ�� ���ǿ� �´���
    public bool isTarget(Entity entity) => Owner.ControlType != entity.ControlType && entity.IsDead == false && sortedTargets.ContainsKey(entity) == false;
    // ���õ� Ÿ���� �׾��ִ��� ��� �ִ���
    public bool isSelectTargetAlive => Owner.Target != null && Owner.Target.IsDead == false;
    // ���� ��������?
    public bool isMonster => Owner.ControlType == EntityControlType.AI;

    public void Setup(Entity entity)
    {
        Owner = entity;
        detectionRadius = 200f;
        StartCoroutine("UpdateTarget");
    }

    private IEnumerator UpdateTarget()
    {
        // ���� �ױ�������
        while(!Owner.IsDead)
        {
            // ĳ���ؼ� ����ϴ°ɷ� �����ؾ��� ����..
            // �ϴ� �׽�Ʈ��
            yield return new WaitForSeconds(1f);
            TargetSerching();
            if (isSelectTargetAlive) StartTracking();
        }
    }

    private void TargetSerching()
    {
        sortedTargets.Clear();
        // Ÿ�� �˻�
        hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent(out Entity entity))
            {
                if (entity != null && isTarget(entity))
                {
                    float distance = Vector3.Distance(transform.position, hitCollider.transform.position);

                    sortedTargets.Add(entity, distance);
                }
            }
        }

        // �Ÿ��� �������� ����
        sortedTargets.OrderBy(pair => pair.Value);

        if(sortedTargets.Count <= 0 && Owner.Target != null)
        {
            Owner.Target = null;
            Owner.Movement.TraceTarget = null;
            Owner.IsAttack = false;
        }

        // ���� ����� Ÿ�� ����
        if (isSelectTargetAlive == false)
            Targeting();
    }    

    private bool Targeting()
    {
        if(sortedTargets.Count > 0 && sortedTargets.FirstOrDefault().Key.IsDead == false)
        {
            // ó�� Ÿ���� �����ų� Ÿ���� �ٲ� ��� �� ���� �����ȿ� Ÿ���� �����ϸ� �� ������ �Ⱥ��� ���װ� ���� -> ���� �ڵ� : Owner.Movement.LookAt(Owner.Target.transform.position);
            Owner.Target = sortedTargets.FirstOrDefault().Key;
            Owner.Movement.LookAt(Owner.Target.transform.position);
            return true;
        }
        else
        {
            Owner.Target = null;
            Owner.Movement.TraceTarget = null;

            if (Owner.IsAttack)
                Owner.IsAttack = false;

            return false;
        }
    }

    // ���� ���� (���� ������ Movement���� �˾Ƽ� ���� ������)
    private void StartTracking()
    {
        Owner.Movement.TraceTarget = Owner.Target.gameObject.transform;
    }
}
