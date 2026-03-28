using UnityEngine;
using System;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private uint initPoolSize;
    [SerializeField] private PooledObject objectToPool;

    private Stack<PooledObject> stack;

    private void Awake()
    {
        SetupPool();
    }

    private void SetupPool()
    {
        Debug.Log("SetupPool");
        stack = new Stack<PooledObject>();
        PooledObject instance = null;
	
        for (int i = 0; i < initPoolSize; i++)
        {
            instance = Instantiate(objectToPool);
            instance.Pool = this;
            instance.gameObject.SetActive(false);
            stack.Push(instance);
        }
    }

    public PooledObject GetPooledObject()
    {
        //PooledObject‚Ìî•ñ‚ð•Ô‚µ‚Â‚ÂSetTrue
        if (stack.Count == 0)
        {
            PooledObject newInstance = Instantiate(objectToPool);
            newInstance.Pool = this;
            return newInstance;
        }
        
        PooledObject nextInstance = stack.Pop();
        nextInstance.gameObject.SetActive(true);
        return nextInstance;
    }
    
    public void ReturnToPool(PooledObject pooledObject)
    {
        //PooledObject‚ðfalse
        stack.Push(pooledObject);
        pooledObject.gameObject.SetActive(false);
    }
}
