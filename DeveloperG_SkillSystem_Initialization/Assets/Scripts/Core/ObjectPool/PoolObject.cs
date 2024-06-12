using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObject : MonoBehaviour
{
    public PoolType type;

    public virtual PoolObject Apply(int initialSize = 1)
    {
        return PoolManager.Instance.GetObject(type, this, initialSize);
    }

    public virtual void Release()
    {
        PoolManager.Instance.ReleaseObject(type, this);
    }
}
