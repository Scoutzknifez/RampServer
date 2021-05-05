using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler pooler;

    public GameObject pooledObject;
    public int pooledAmount;
    public bool willGrow;

    List<GameObject> pooledObjects;

    private void Awake()
    {
        if (pooler == null)
        {
            pooler = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        pooledObjects = new List<GameObject>();
        for (int i = 0; i < pooledAmount; i++)
        {
            pooledObjects.Add(CreateNew());
        }
    }

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if(!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }

        if (willGrow)
        {
            GameObject newObj = CreateNew();
            pooledObjects.Add(newObj);
            return newObj;
        }

        return null;
    }

    public GameObject CreateNew()
    {
        GameObject obj = Instantiate(pooledObject, transform);
        obj.SetActive(false);
        return obj;
    }
}
