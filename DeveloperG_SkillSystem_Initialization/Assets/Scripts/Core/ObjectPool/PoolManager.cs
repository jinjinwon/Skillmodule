using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 보류.. 이펙트 관련되어 생각이 정리되면 작업

public class PoolManager : MonoSingleton<PoolManager>
{
    [SerializeField]
    private int maxSize = 10;

    public int MaxSize => maxSize;

    private Dictionary<PoolType, Queue<PoolObject>> poolDictionary = new Dictionary<PoolType, Queue<PoolObject>>();


    public void Start()
    {
        // Setup 함수
        Setup();
    }

    private void Setup()
    {
        // 미리 추가해야 하는 이펙트들은 여기에 추가 시킵니다.
    }

    public void CreatePool(PoolType type, PoolObject prefab, int initialSize)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            poolDictionary[type] = new Queue<PoolObject>();

            for (int i = 0; i < initialSize; i++)
            {
                PoolObject newObject = Instantiate(prefab);
                newObject.gameObject.SetActive(false);
                poolDictionary[type].Enqueue(newObject);
            }
        }
    }

    public PoolObject GetObject(PoolType type, PoolObject prefab, int initialSize = 1)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            CreatePool(type, prefab, initialSize);
        }

        if (poolDictionary.ContainsKey(type) && poolDictionary[type].Count > 0)
        {
            PoolObject objectToReuse = poolDictionary[type].Dequeue();
            objectToReuse.gameObject.SetActive(true);
            objectToReuse.Apply();
            return objectToReuse;
        }
        else
        {
            Debug.LogWarning("No objects available in pool of type " + type);
            return null;
        }
    }

    public void ReleaseObject(PoolType type, PoolObject obj)
    {
        if (poolDictionary.ContainsKey(type))
        {
            if (poolDictionary[type].Count < maxSize)
            {
                obj.Release();
                obj.gameObject.SetActive(false);
                poolDictionary[type].Enqueue(obj);
            }
            else
            {
                Destroy(obj.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("No pool found for type " + type);
            Destroy(obj.gameObject);
        }
    }
}
