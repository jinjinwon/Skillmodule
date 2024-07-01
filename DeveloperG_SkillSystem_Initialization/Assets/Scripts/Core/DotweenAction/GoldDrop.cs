using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using JetBrains.Annotations;
using Unity.VisualScripting;

[System.Serializable]
public class GoldDrop : DGAction
{
    public GameObject goldPrefab; // ��� ������
    private Transform goldUI;      // UI ���� ��� �̹��� ��ġ

    // ���Ͱ� �׾��� �� ȣ��Ǵ� �Լ�
    public override void Start(Entity entiy, Vector3 monsterPosition)
    {
        goldUI = GameObject.Find("Money").transform;
        // ��� �������� ���� ��ġ�� ����
        GameObject gold = ObjectPool.Spawn(goldPrefab, GameObject.Find("Canvas").transform, monsterPosition, Quaternion.identity);

        // ȭ�� ��ǥ�� ��ȯ
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(monsterPosition);

        // Canvas ������ �̵�
        gold.transform.position = screenPoint;

        // ��� UI ��ġ�� �̵� �ִϸ��̼� // ����� �ؔf�µ� ������ �� ������..
        gold.transform.DOMove(goldUI.position, 1.0f).OnComplete(() =>
        {
            // ��� ���� �� ó��
            ObjectPool.Recycle(gold);
            // ��� ȹ�� ó��
            UpdateGoldUI(entiy);
        });
    }

    private void UpdateGoldUI(Entity entity)
    {
        // ��� UI ������Ʈ ���� �ۼ�
        if(entity.ControlType == EntityControlType.Player)
        {
            // ���� ������ ����..
            entity.GetGold(10);
        }
    }

    public override object Clone() => new GoldDrop();

}
