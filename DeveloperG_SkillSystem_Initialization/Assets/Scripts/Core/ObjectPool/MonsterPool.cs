using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class MonsterPool : MonoBehaviour
{
    private CapsuleCollider _collider;
    private Rigidbody rb;
    private Animator _animator;

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

        // 테스트용 코드
        rb.isKinematic = true;

        monster.StartCustomActions(this,this.transform);
    }

    [ContextMenu("테스트")]
    public void Dead()
    {
        _monster = null;
        PoolManager.Instance.Recycle(this.gameObject);
    }
}
