using System.Collections.Generic;
using UnityEngine;

public class ShadowPool
{
    private GameObject shadowPrefab;
    private Queue<GameObject> pool = new Queue<GameObject>();

    public ShadowPool(GameObject prefab)
    {
        shadowPrefab = prefab;
    }

    public GameObject GetShadow()
    {
        if (pool.Count > 0)
        {
            GameObject shadow = pool.Dequeue();
            shadow.SetActive(true);
            return shadow;
        }
        return Object.Instantiate(shadowPrefab);
    }

    public void ReturnShadow(GameObject shadow)
    {
        shadow.SetActive(false);
        pool.Enqueue(shadow);
    }
}