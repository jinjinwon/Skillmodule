using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StageData
{
    public int floor;

    [UnderlineTitle("�� ������")]
    public GameObject mapPrefab;

    [UnderlineTitle("���� ����")] // ������ Monster Ŭ������ �־�� �� �ϴ� ���������� ���� �۾��ϹǷ� ���� ������Ʈ�� ����;
    [SerializeReference]
    public Monster[] monsters;

    [UnderlineTitle("�Ϲ� ���� ���� ����")]
    public int regenCount;

    [UnderlineTitle("���� ���� ����")]
    public bool isBossRound;

    [UnderlineTitle("���� ���� ���� �� �Ϲ� ���� ���� ����")]
    public bool bossMonsterGen;

    [UnderlineTitle("���� ������ �ʿ��� ų Ƚ��")]
    public int nextFloorKill;

    [UnderlineTitle("BGM")]
    public AudioClip audioClip;

    [UnderlineTitle("���� ������ �Ѿ �� ���� �� �׼�")]
    [SerializeReference, SubclassSelector]
    public NextFloorAction[] customActionsFade;

}
