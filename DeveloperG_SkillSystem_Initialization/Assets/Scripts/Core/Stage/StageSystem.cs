using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSystem : MonoSingleton<StageSystem>
{
    public Stage stage;   
    public Entity Owner { get; private set; }

    private void Start()
    {
        FloorHUD.Instance.Show(stage);
        stage.Setup();
        Setup();
    }

    public void Setup()
    {
        Release();
        CreateMap();
        CretaeNpc();
    }

    private void Release()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    private void CreateMap()
    {
        Instantiate(stage.CurrentStageData.mapPrefab, this.transform).SetActive(true);
    }

    private void CretaeNpc()
    {
        GameObject[] spwanerPoint = new GameObject[4];

        for(int i = 1; i <= 4; i++)
        {
            if(GameObject.Find($"Spwaner_{i}") == true)
            {
                spwanerPoint[i - 1] = GameObject.Find($"Spwaner_{i}");
            }
        }

        // ���� ��ġ�� �ϴ� ���� �������� ����;
        // �ٽ� �����ϴ� �κ��� ���Ϳ��� regen üũ�� ����;
        for(int i = 0; i < stage.CurrentStageData.regenCount; i++)
        {
            int random = Random.Range(0, 4);
            int randomMonster = Random.Range(0, 2);
            //Instantiate(stage.CurrentStageData.monsters[randomMonster], spwanerPoint[random].transform).SetActive(true);
        }
    }

    public void CreateBossNpc()
    {
        Debug.Log("���� ��ȯ!");
    }

    public void NextFloor()
    {
        LevelUp();
    }

    [ContextMenu("���� ��")]
    public void LevelUp()
    {
        stage.Floor++;
    }

    public int testValue;
    [ContextMenu("��ǥ óġ ��")]
    public void KillCountUp()
    {
        stage.CurrentKillCount = testValue;
    }
}
