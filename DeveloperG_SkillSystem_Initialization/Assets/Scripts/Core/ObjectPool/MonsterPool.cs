using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditorInternal;
using UnityEngine;

public class MonsterPool : MonoBehaviour
{
    private CapsuleCollider _collider;
    private Rigidbody rb;
    private Animator _animator;
    private Entity _entity;

    public Monster _monster;
    

    public void Initialize()
    {
        if(TryGetComponent(out CapsuleCollider capsuleCollider))
        {
            if (_collider == null) _collider = capsuleCollider;
        }
        else
        {
            this.gameObject.AddComponent<CapsuleCollider>();
            _collider = GetComponent<CapsuleCollider>();
        }

        if (TryGetComponent(out Rigidbody rigidbody))
        {
            if (rb == null) rb = rigidbody;
        }
        else
        {
            this.gameObject.AddComponent<Rigidbody>();
            rb = GetComponent<Rigidbody>();
        }

        if (TryGetComponent(out Animator animator))
        {
            if (_animator == null) _animator = animator;
        }
        else
        {
            this.gameObject.AddComponent<Animator>();
            _animator = GetComponent<Animator>();
        }

        if (TryGetComponent(out Entity entity))
        {
            if (_entity == null) _entity = entity;
        }
        else
        {
            this.gameObject.AddComponent<Entity>();
            _entity = GetComponent<Entity>();
        }
    }

    public void Setup(Monster monster)
    {
        if (_monster != null)
            _monster = null;

        _monster = monster;

        _collider.center = monster.Center;
        _collider.radius = monster.Radius;
        _collider.height = monster.Height;

        _animator.runtimeAnimatorController = monster.AnimatorOverrideController;

        // �׽�Ʈ�� �ڵ�
        rb.isKinematic = true;

        monster.StartCustomActions(this,this.transform);
    }

    [ContextMenu("�׽�Ʈ")]
    public void Dead(int delay = 2)
    {
        StageSystem.Instance.stage.CurrentKillCount++;
        _monster?.StartDGActions(_entity.Target,this.transform.position);
        Invoke("DeadDelay", delay);
    }

    private void DeadDelay()
    {
        _entity.onAttack -= _monster.AttackClip;
        _entity.onDead -= _monster.DeadClip;

        _monster = null;
        PoolManager.Instance.Recycle(this.gameObject);
    }
}
