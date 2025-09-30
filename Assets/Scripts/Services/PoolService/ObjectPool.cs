using System.Collections.Generic;
using UnityEngine;

namespace Services.PoolService
{
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Queue<T> poolQueue = new();

        public ObjectPool(T prefab, Transform parent = null)
        {
            this.prefab = prefab;
            this.parent = parent;
        }

        public T Get()
        {
            if (poolQueue.Count > 0)
            {
                var obj = poolQueue.Dequeue();
                obj.gameObject.SetActive(true);
                return obj;
            }
            else
            {
                var obj = Object.Instantiate(prefab, parent);
                return obj;
            }
        }

        public void ReturnToPool(T obj)
        {
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(parent, false);
            poolQueue.Enqueue(obj);
        }

        public void Clear()
        {
            while (poolQueue.Count > 0)
            {
                var obj = poolQueue.Dequeue();
                Object.Destroy(obj.gameObject);
            }
        }

        public void PreWarm(int count, Transform parent = null)
        {
            var targetParent = parent ?? this.parent;
            for (var i = 0; i < count; i++)
            {
                var obj = Object.Instantiate(prefab, targetParent, worldPositionStays: false);
                obj.gameObject.SetActive(false);
                poolQueue.Enqueue(obj);
            }
        }
    }
}