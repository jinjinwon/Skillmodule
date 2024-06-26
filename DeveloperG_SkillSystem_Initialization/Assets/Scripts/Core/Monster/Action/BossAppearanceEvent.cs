using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BossAppearanceEvent : AppearanceAction
{
    private CinemachineVirtualCamera bossCamera;
    private Transform bossTransform;

    [UnderlineTitle("���� �Ÿ�")]
    public float distance;
    [UnderlineTitle("���� ���� �ð�")]
    public float duration;

    private Transform baseTransform;
    public override void Start(object data, MonoBehaviour context, Transform transform)
    {
        bossTransform = transform;

        if(bossCamera == null)
            bossCamera = Camera.main.GetComponent<Cinemachine.CinemachineVirtualCamera>();

        baseTransform = bossCamera.Follow;
        context.StartCoroutine(ZoomInOnBoss());
    }

    private IEnumerator ZoomInOnBoss()
    {
        bossCamera.enabled = true;
        bossCamera.LookAt = bossTransform;
        bossCamera.Follow = bossTransform;

        CinemachineComponentBase component = bossCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        CinemachineFramingTransposer transposer = component as CinemachineFramingTransposer;

        float initialDistance = transposer.m_CameraDistance;
        float targetDistance = initialDistance / distance; // ���� �Ÿ�

        float elapsedTime = 0f;

        // ���� �ִϸ��̼�
        while (elapsedTime < duration)
        {
            transposer.m_CameraDistance = Mathf.Lerp(initialDistance, targetDistance, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transposer.m_CameraDistance = targetDistance;

        // ��� ��� �� ���� �Ÿ��� ����
        yield return new WaitForSeconds(1); // �� ��� �ð��� ���ϴ� ��� ������ �� �ֽ��ϴ�.
        transposer.m_CameraDistance = initialDistance; // ���� ī�޶� �Ÿ��� ����

        bossCamera.LookAt = baseTransform;
        bossCamera.Follow = baseTransform;
    }

    public override object Clone() => new BossAppearanceEvent();
}
