using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ����
//[System.Serializable]
public class SquareIndicatorViewAction : IndicatorViewAction
{
    [SerializeField]
    private GameObject indicatorPrefab;
    // ������ indicator�� size ��
    // ���� 0�̸� targetSearcher�� range�� ��� �����
    [SerializeField]
    private float indicatorSizeOverride;
    // Indicator�� ���� ä��� fillAmount Property�� ����� ���ΰ�?
    [SerializeField]
    private bool isUseIndicatorFillAmount;
    // Indicator�� requesterObject�� �ڽ� Object�� ���� ���ΰ�?
    [SerializeField]
    private bool isAttachIndicatorToRequester;

    // ShowIndicator �Լ��� ������ Indicator
    private Indicator spawnedRangeIndicator;

    public override void ShowIndicator(TargetSearcher targetSearcher, GameObject requesterObject,
        object range, float angle, float fillAmount)
    {
        Debug.Assert(range is float, "SquareIndicatorViewAction::ShowIndicator - range�� null �Ǵ� float���� ���˴ϴ�.");

        // �̹� Indicator�� �����ְ� �ִٸ� ���� Hide ó���� ����
        HideIndicator();

        // isUseIndicatorFillAmount Option�� true�� �ƴϸ� fillAmount ������ 0�� ��
        fillAmount = isUseIndicatorFillAmount ? fillAmount : 0f;
        // isAttachIndicatorToRequester Option�� true��� requesterObject�� transform�� ������
        var attachTarget = isAttachIndicatorToRequester ? requesterObject.transform : null;
        // indicatorSizeOverride�� 0�̶�� ���ڷ� ���� targetSearcher�� range��,
        // �ƴ϶�� indicatorSizeOverride�� Indicator�� size�� ��
        float size = Mathf.Approximately(indicatorSizeOverride, 0f) ? (float)range : indicatorSizeOverride;

        // Indicator�� �����ϰ�, Setup �Լ��� ������ ���� ������ Setting����
        spawnedRangeIndicator = GameObject.Instantiate(indicatorPrefab).GetComponent<Indicator>();
        spawnedRangeIndicator.TypeChanger(IndicatorType.Square);
        spawnedRangeIndicator.Setup(size, fillAmount, attachTarget);
    }

    public override void HideIndicator()
    {
        if (!spawnedRangeIndicator)
            return;

        GameObject.Destroy(spawnedRangeIndicator.gameObject);
    }

    public override void SetFillAmount(float fillAmount)
    {
        if (!isUseIndicatorFillAmount || spawnedRangeIndicator == null)
            return;

        spawnedRangeIndicator.FillAmount = fillAmount;
    }

    public override object Clone()
    {
        return new SquareIndicatorViewAction()
        {
            indicatorPrefab = indicatorPrefab,
            indicatorSizeOverride = indicatorSizeOverride,
            isUseIndicatorFillAmount = isUseIndicatorFillAmount,
            isAttachIndicatorToRequester = isAttachIndicatorToRequester
        };
    }
}
