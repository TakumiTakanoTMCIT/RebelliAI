using System;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

namespace ObjectPoolFactory
{
    public abstract class BasePool
    {
        protected GameObject prefab;
        private int maxCapacity;
        private bool isInit = false;

        private ObjectPool<GameObject> pool;

        public BasePool(GameObject prefab, int maxCapacity)
        {
            this.maxCapacity = maxCapacity;
            this.prefab = prefab;

            pool = new ObjectPool<GameObject>
            (
                createFunc: CreateObj,
                actionOnGet: GetObj,
                actionOnRelease: ReleaseObj,
                actionOnDestroy: DestroyObj,
                collectionCheck: false,
                defaultCapacity: maxCapacity,
                maxSize: maxCapacity
            );
        }

        protected void InitPool()
        {
            if (isInit)
            {
                Debug.Log("Pool is already initialized");
                return;
            }

            for (int count = 0; count < maxCapacity; count++)
            {
                var obj = CreateObj();
                pool.Release(obj);
            }

            isInit = true;
        }

        protected virtual GameObject CreateObj()
        {
            return GameObject.Instantiate(prefab);
        }

        private void GetObj(GameObject getObj)
        {
            getObj.SetActive(true);
        }

        private void ReleaseObj(GameObject releaseObj)
        {
            releaseObj.SetActive(false);
        }

        private void DestroyObj(GameObject destroyObj)
        {
            GameObject.Destroy(destroyObj);
        }

        public GameObject GetObject()
        {
            return pool.Get();
        }

        public void ReturnObject(GameObject obj)
        {
            pool.Release(obj);
        }
    }

    public class WarpPool : BasePool
    {
        public WarpPool(GameObject prefab, int maxCapacity) : base(prefab, maxCapacity)
        {
            InitPool();
        }
    }

    public class DanboruPool : BasePool
    {
        private readonly Lazy<EnemyBody.Factory> enmeyBodyFactory;

        [Inject]
        public DanboruPool(GameObject prefab, int maxCapacity, Lazy<EnemyBody.Factory> enemyFactory) : base(prefab, maxCapacity)
        {
            enmeyBodyFactory = enemyFactory;
            InitPool();
        }

        protected override GameObject CreateObj()
        {
            return enmeyBodyFactory.Value.Create().gameObject;
        }
    }
}
