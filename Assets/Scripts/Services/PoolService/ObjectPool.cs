using System.Collections.Generic;
using Element;
using UnityEngine;

namespace Services.PoolService
{
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly List<T> pool = new();
        private readonly int expandBy;
        private int nextIndex = 0;

        public ObjectPool(T prefab, Transform parent = null, int initialSize = 0, int expandBy = 5)
        {
            if (prefab == null)
            {
                return;
            }

            this.prefab = prefab;
            this.parent = parent;
            this.expandBy = expandBy;

            PreWarm(initialSize);
        }

        public T Get()
        {
            if (nextIndex > 0)
            {
                var obj = pool[nextIndex - 1];
                nextIndex--;
                if (obj != null)
                {
                    obj.gameObject.SetActive(true);
                    return obj;
                }
            }
            else
            {
                PreWarm(expandBy);
                return Get();
            }

            var newObj = Object.Instantiate(prefab, parent);

            pool.Add(newObj);
            return newObj;
        }

        public void ReturnToPool(T obj)
        {
            if (obj == null)
            {
                return;
            }

            if (!pool.Contains(obj))
            {
                Object.Destroy(obj.gameObject);
                return;
            }

            ResetObject(obj);
            pool[nextIndex] = obj;
            nextIndex++;
        }

        public void PreWarm(int count, Transform overrideParent = null)
        {
            if (count <= 0) return;

            var targetParent = overrideParent ?? parent;
            for (var i = 0; i < count; i++)
            {
                var obj = Object.Instantiate(prefab, targetParent, worldPositionStays: false);

                ResetObject(obj);
                pool.Add(obj);
                nextIndex++;
            }
        }

        public void Clear()
        {
            for (var i = 0; i < nextIndex; i++)
            {
                if (pool[i] != null && !pool[i].gameObject.activeSelf)
                {
                    Object.Destroy(pool[i].gameObject);
                }
            }

            pool.Clear();
            nextIndex = 0;
        }

        public void Shrink(int targetSize)
        {
            if (targetSize < 0 || targetSize >= nextIndex) return;

            for (var i = targetSize; i < nextIndex; i++)
            {
                if (pool[i] != null && !pool[i].gameObject.activeSelf)
                {
                    Object.Destroy(pool[i].gameObject);
                }
            }

            pool.RemoveRange(targetSize, nextIndex - targetSize);
            nextIndex = targetSize;
        }

        private void ResetObject(T obj)
        {
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.localRotation = Quaternion.identity;

            if (obj is ElementView elementView)
            {
                elementView.SetAlpha(1f);
                elementView.SetSprite(null);
            }

            var rectTransform = obj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        public int AvailableCount => nextIndex;
        public int TotalCount => pool.Count;
    }
}