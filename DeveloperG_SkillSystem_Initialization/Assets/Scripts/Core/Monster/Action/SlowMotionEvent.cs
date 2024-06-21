using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlowMotionEvent : AppearanceAction
{
    private CinemachineVirtualCamera bossCamera;
    private Transform bossTransform;

    [UnderlineTitle("���� �Ÿ�")]
    public float distance;
    [UnderlineTitle("���� ���� �ð�")]
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
        float targetDistance = initialDistance / distance; // ���� �Ÿ�

        float elapsedTime = 0f;

        Time.timeScale = 0.5f; // ���ο� ��� ȿ��
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
