using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 보류.. 이펙트 관련되어 생각이 정리되면 작업

public class PoolManager : MonoSingleton<PoolManager>
{
    [Header("Monster")]
    [SerializeField]
    private int monsterMaxSize;
    [SerializeField]
    private GameObject monsterPrefab;

    [Header("Player"),Space(1)]
    [SerializeField]
    private int playerMaxSize;
    [SerializeField]
    private GameObject playerPrefab;

    private void Start()
    {
        ObjectPool.CreatePool(monsterPrefab, monsterMaxSize);
        //ObjectPool.CreatePool(playerPrefab, playerMaxSize);
    }

    [SerializeField]
    private Monster nnster;

    [ContextMenu("테스트")]
    public void Test()
    {
        Spwan(nnster);
    }

    public void Spwan(Monster monster, Transform parent = null, Vector3 vector = new(),Quaternion quaternion = new())
    {
        if (parent == null)
            parent = this.transform;

        GameObject go = ObjectPool.Spawn(monster.Prefab, parent, vector, quaternion);

        if (go.TryGetComponent(out MonsterPool monsterPool))
        {
            monsterPool.Initialize();
            monsterPool.Setup(monster);
        }
        else
        {
            MonsterPool tempMonsterPool = go.AddComponent<MonsterPool>();
            
            if (tempMonsterPool == null)
            {
                go.Recycle();
                return;
            }

            tempMonsterPool.Initialize();
            tempMonsterPool.Setup(monster);
        }
    }

    public void Recycle(GameObject go)
    {
        go.Recycle();
    }

    public void RecycleAll()
    {
        ObjectPool.RecycleAll();
    }

    public void DestroyPooled(GameObject go)
    {
        go.DestroyPooled();
    }

    public void DestroyAll(GameObject go)
    {
        ObjectPool.DestroyAll(go);
    }
}
