using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class StageSystem : MonoSingleton<StageSystem>
{
    public Stage stage;   
    public Entity Owner { get; private set; }

    private bool isBoss = false;

    private bool mapCreated = false;

    public bool IsMapCreated => mapCreated;
    public bool IsRegen => SpawnedCount() < stage.CurrentStageData.regenCount && !IsBoss;
    public int RegenCount => stage.CurrentStageData.regenCount - SpawnedCount();
    public bool IsBoss => isBoss;

    private void Start()
    {
        FloorHUD.Instance.Show(stage);
        stage.Setup();
        Setup();
    }


    private void Update()
    {
        Spawning();
    }

    public void Setup()
    {
        Release();
        CreateMap();
        CretaeNpc();
        PlayBGM();
    }

    private void Release()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        mapCreated = false;
        isBoss = false;
    }

    private void PlayBGM()
    {
        AudioManager.Instance.PlayBGM(stage.CurrentStageData.audioClip);
    }

    private void CreateMap()
    {
        Instantiate(stage.CurrentStageData.mapPrefab, this.transform).SetActive(true);
        mapCreated = true;
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

        // 생성 위치는 일단 만든 지점으로 ㅇㅇ;
        // 다시 생성하는 부분은 몬스터에서 regen 체크로 ㅇㅇ;
        for(int i = 0; i < stage.CurrentStageData.regenCount; i++)
        {
            int random = Random.Range(0, 4);
            int randomMonster = Random.Range(0, stage.CurrentStageData.monsters.Length);
            PoolManager.Instance.Spwan(stage.CurrentStageData.monsters[randomMonster], spwanerPoint[random].transform);
        }
    }

    private void CretaeNpc(int count)
    {
        GameObject[] spwanerPoint = new GameObject[4];

        for (int i = 1; i <= 4; i++)
        {
            if (GameObject.Find($"Spwaner_{i}") == true)
            {
                spwanerPoint[i - 1] = GameObject.Find($"Spwaner_{i}");
            }
        }

        // 생성 위치는 일단 만든 지점으로 ㅇㅇ;
        // 다시 생성하는 부분은 몬스터에서 regen 체크로 ㅇㅇ;
        for (int i = 0; i < count; i++)
        {
            int random = Random.Range(0, 4);
            int randomMonster = Random.Range(0, stage.CurrentStageData.monsters.Length);
            PoolManager.Instance.Spwan(stage.CurrentStageData.monsters[randomMonster], spwanerPoint[random].transform);
        }
    }

    private void Spawning()
    {
        if(IsMapCreated && IsRegen)
        {
            CretaeNpc(RegenCount);
        }
    }

    private int SpawnedCount()
    {
        GameObject[] gameObjects = new GameObject[stage.CurrentStageData.monsters.Length];
        int count = 0;
        foreach (var pair in stage.CurrentStageData.monsters)
        {
            gameObjects[count] = pair.Prefab;
            count++;
        }

        count = ObjectPool.CountSpawned(gameObjects);
        gameObjects = null;
        return count;
    }

    public void CreateBossNpc()
    {
        Debug.Log("보스 소환!");
        isBoss = true;
    }

    public void NextFloor()
    {
        LevelUp();
    }

    [ContextMenu("레벨 업")]
    public void LevelUp()
    {
        stage.Floor++;
    }

    public int testValue;
    [ContextMenu("목표 처치 수")]
    public void KillCountUp()
    {
        stage.CurrentKillCount = testValue;
    }
}
