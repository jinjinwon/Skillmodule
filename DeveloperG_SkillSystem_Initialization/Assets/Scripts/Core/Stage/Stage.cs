using Google.Apis.Sheets.v4.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public class Stage : IdentifiedObject
{
    #region EventsHandler
    public delegate void NextFloorGoalGaugeHandler(Stage stage);                                                       // ���� �� ��ǥ �������� ���� �� ��� ����Ǵ� Event
    public delegate void FloorGoalGaugeChangeHandler(Stage stage, int currentKillCount, int maxKillCount);             // ���� �� ��ǥ �������� ������ �ִ� ��� ����Ǵ� Event
    public delegate void FloorChangeHandler(StageData stageData);                                                      // ���� ������ ���� ��� ����Ǵ� Event
    public delegate void BossSummonHandler(Stage stage);                                                               // ���� ��ȯ�� ������ �� ����Ǵ� Event
    #endregion

    #region Events
    public event NextFloorGoalGaugeHandler onNextFloorGoalGauge;
    public event FloorGoalGaugeChangeHandler onFloorGoalGaugeChange;
    public event FloorChangeHandler onFloorChange;
    public event BossSummonHandler onBossSummon;
    #endregion

    [SerializeField]
    private int maxFloor;

    private int floor;

    [SerializeField]
    private bool isAllowFloorExceedDatas;

    [SerializeField]
    private int defaultFloor = 1;

    [SerializeField]
    private StageData[] stageDatas;
    private StageData currentData;
    private bool isBoss;

    private int currentKillCount;
    private readonly Dictionary<NextFloorType, NextFloorAction[]> customActionsByType = new();

    public int MaxFloor => maxFloor;

    public StageData[] StageDatas => stageDatas;
    public StageData CurrentStageData => currentData;

    // �� ������
    public int Floor
    {
        get => floor;
        set
        {
            Debug.Assert(value >= 1 && value <= MaxFloor,
                $"Stage.Floor = {value} - value�� 1�� MaxFloor({MaxFloor}) ���� ���̿����մϴ�.");

            // �׽�Ʈ�� ���ǹ�
            if (IsMaxFloor)
            {
                Debug.Log("�ִ� ���Դϴ�.");
                return;
            }

            if (floor == value)
                return;

            int prevFloor = floor;
            floor = value;

            // ���ο� Level�� ���� ����� Level Data�� ã�ƿ�
            var newData = stageDatas.Last(x => x.floor <= floor);
            ChaneFloor(newData);
            onFloorChange?.Invoke(newData);
        }
    }

    // ���� ������ ��쿡 ��ǥ Ƚ���� �� ä�� ���
    public bool isRoundClear => CurrentStageData.nextFloorKill <= CurrentKillCount && IsBoss == true;

    // ���� �� ������ ��Ҵ���
    public int CurrentKillCount
    {
        get => currentKillCount;
        set
        {
            if (currentKillCount == value)
                return;

            int prevCount = currentKillCount;
            currentKillCount = value;

            // ���� ������ ������ �� ���������� ��ǥ ���� ���մϴ�.
            var newData = stageDatas.Last(x => x.floor <= floor);
            if (newData.nextFloorKill <= currentKillCount)
            {
                currentKillCount = newData.nextFloorKill;
                
                if(IsBoss)
                {
                    onBossSummon?.Invoke(this);
                }
                else
                {
                    onNextFloorGoalGauge?.Invoke(this);
                }
            }
            onFloorGoalGaugeChange?.Invoke(this, currentKillCount, newData.nextFloorKill);
        }
    }

    // ���� ���� ���� ?
    public bool IsBoss => currentKillCount >= currentData.nextFloorKill && currentData.isBossRound;

    // ���� ���� ������ ������ ?
    public bool IsMaxFloor => floor == maxFloor;

    public void Setup()
    {
        floor = 1;
        currentKillCount = 0;
        var newData = stageDatas.Last(x => x.floor <= floor);
        currentData = newData;

        onFloorChange?.Invoke(CurrentStageData);
        onFloorGoalGaugeChange?.Invoke(this, currentKillCount, CurrentStageData.nextFloorKill);

        UpdateCustomActions();
    }

    private void ChaneFloor(StageData stageData)
    {
        RunCustomActions(NextFloorType.FadeIn_Out);
        currentData = stageData;
        CurrentKillCount = 0;
        UpdateCustomActions();

        StageSystem.Instance.Setup();
    }

    public void RunCustomActions(NextFloorType type)
    {
        foreach (var customAction in customActionsByType[type])
            customAction.Run(this);
    }

    public void ReleaseCustomActions(NextFloorType type)
    {
        foreach (var customAction in customActionsByType[type])
            customAction.Release(this);
    }

    private void UpdateCustomActions()
    {
        customActionsByType[NextFloorType.FadeIn_Out] = currentData.customActionsFade;
    }
}
