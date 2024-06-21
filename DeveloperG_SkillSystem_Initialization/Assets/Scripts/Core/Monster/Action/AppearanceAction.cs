using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class AppearanceAction : ICloneable
{
    public virtual void Start(object data) { }
    public virtual void Start(MonoBehaviour data, Transform transform) { }
    public virtual void Release(object data) { }
    public virtual void Release(MonoBehaviour data, Transform transform) { }

    public abstract object Clone();
}
