using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlowMotionEvent : AppearanceAction
{
    private CinemachineVirtualCamera bossCamera;
    private Transform bossTransform;

    [UnderlineTitle("줌인 거리")]
    public float distance;
    [UnderlineTitle("줌인 지속 시간")]
    public float duration;

    public override void Start(MonoBehaviour context, Transform transform)
    {
        bossTransform = transform;

        if (bossCamera != null)
            bossCamera = Camera.main.GetComponent<Cinemachine.CinemachineVirtualCamera>();

        context.StartCoroutine(SlowMotionZoomIn());
    }

    private IEnumerator SlowMotionZoomIn()
    {
        bossCamera.enabled = true;
        bossCamera.LookAt = bossTransform;

        CinemachineComponentBase component = bossCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        CinemachineFramingTransposer transposer = component as CinemachineFramingTransposer;

        float initialDistance = transposer.m_CameraDistance;
        float targetDistance = initialDistance / distance; // 줌인 거리

        float elapsedTime = 0f;

        Time.timeScale = 0.5f; // 슬로우 모션 효과
        while (elapsedTime < duration)
        {
            transposer.m_CameraDistance = Mathf.Lerp(initialDistance, targetDistance, elapsedTime / duration);
            elapsedTime += Time.deltaTime / Time.timeScale;
            yield return null;
        }
        Time.timeScale = 1.0f;

        transposer.m_CameraDistance = targetDistance;
    }

    public override object Clone() => new SlowMotionEvent();
}
