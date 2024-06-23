using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPool : MonoBehaviour
{
    private CapsuleCollider _collider;
    private Rigidbody rb;

    public Monster _monster;

    public void Initialize()
    {
        if(TryGetComponent(out CapsuleCollider capsuleCollider))
        {
            if (_collider == null) _collider = GetComponent<CapsuleCollider>();
        }
        else
        {
            this.gameObject.AddComponent<CapsuleCollider>();
            _collider = GetComponent<CapsuleCollider>();
        }

        if (TryGetComponent(out Rigidbody rigidbody))
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
        }
        else
        {
            this.gameObject.AddComponent<Rigidbody>();
            rb = GetComponent<Rigidbody>();
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

        // �׽�Ʈ�� �ڵ�
        rb.isKinematic = true;
    }

    [ContextMenu("�׽�Ʈ")]
    public void Dead()
    {
        _monster = null;
        PoolManager.Instance.Recycle(this.gameObject);
    }
}
