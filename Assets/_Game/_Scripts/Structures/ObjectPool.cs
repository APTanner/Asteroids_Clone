using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly Queue<T> m_queue = new();
    private readonly HashSet<T> m_objects = new();
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
        if (!m_objects.Contains(obj))
        {
            Debug.LogError($"Tried to add an object of type '{typeof(T)}' to an object pool it is not a part of.");
        }
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
        m_objects.Add(newObj);
    }

    public void Reset()
    {
        m_queue.Clear();
        foreach (T obj in m_objects)
        {
            Release(obj);
            obj.gameObject.SetActive(false);
        }
    }
}
