using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraShakeEvent : AppearanceAction
{
    public override void Start(object data)
        => Camera.main.GetComponent<Cinemachine.CinemachineImpulseSource>().GenerateImpulse();

    public override object Clone() => new CameraShakeEvent();
}