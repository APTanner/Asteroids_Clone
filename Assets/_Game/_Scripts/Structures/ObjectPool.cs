using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly Queue<T> m_queue = new Queue<T>();
    private readonly T prefab;
    private readonly Transform m_parentTransform = null;

    public ObjectPool(T prefab, int size, Transform parentTransform = null)
    {
        this.prefab = prefab;
        this.m_parentTransform = parentTransform;
        for (int i = 0; i < size; i++)
        {
            CreateObject();
        }
    }

    public bool TryGet(out T obj)
    {
        obj = null;
        if (m_queue.Count == 0)
        {
            return false;
        }

        obj = m_queue.Dequeue();
        return obj;
    }

    public void Release(T obj)
    {
        m_queue.Enqueue(obj);
    }

    private void CreateObject()
    {
        T newObj;
        if (m_parentTransform != null) 
        {
            newObj = Object.Instantiate(prefab, m_parentTransform);
        }
        else
        {
            newObj = Object.Instantiate(prefab);
        }

        newObj.gameObject.SetActive(false);
        m_queue.Enqueue(newObj);
    }

    public void Reset()
    {
        m_queue.Clear();
        T[] objects = Resources.FindObjectsOfTypeAll<T>();
        for (int i = 0; i < objects.Length; ++i)
        {
            Release(objects[i]);
            objects[i].gameObject.SetActive(false);
        }
    }
}
