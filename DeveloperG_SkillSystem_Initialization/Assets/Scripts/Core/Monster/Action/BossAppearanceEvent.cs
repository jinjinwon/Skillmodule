using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BossAppearanceEvent : AppearanceAction
{
    private CinemachineVirtualCamera bossCamera;
    private Transform bossTransform;

    [UnderlineTitle("¡‹¿Œ ∞≈∏Æ")]
    public float distance;
    [UnderlineTitle("¡‹¿Œ ¡ˆº” Ω√∞£")]
    public float duration;


    public override void Start(MonoBehaviour context, Transform transform)
    {
        bossTransform = transform;

        if(bossCamera != null)
            bossCamera = Camera.main.GetComponent<Cinemachine.CinemachineVirtualCamera>();

        context.StartCoroutine(ZoomInOnBoss());
    }

    private IEnumerator ZoomInOnBoss()
    {
        bossCamera.enabled = true;
        bossCamera.LookAt = bossTransform;

        CinemachineComponentBase component = bossCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        CinemachineFramingTransposer transposer = component as CinemachineFramingTransposer;

        float initialDistance = transposer.m_CameraDistance;
        float targetDistance = initialDistance / distance; // ¡‹¿Œ ∞≈∏Æ

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transposer.m_CameraDistance = Mathf.Lerp(initialDistance, targetDistance, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transposer.m_CameraDistance = targetDistance;
    }

    public override object Clone() => new BossAppearanceEvent();
}
