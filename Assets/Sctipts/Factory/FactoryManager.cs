using UnityEngine;
using UnityEngine.Pool;
using Warp;
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
        //Inject
        WarpEffectBody.Factory _factory;
        PoolHandler poolHandler;

        public WarpPool(GameObject prefab, int maxCapacity, WarpEffectBody.Factory factory, PoolHandler poolHandler) : base(prefab, maxCapacity)
        {
            poolHandler.SetReleaseObjCallBack((obj) => ReturnObject(obj));
            this.poolHandler = poolHandler;
            _factory = factory;
            InitPool();
        }

        protected override GameObject CreateObj()
        {
            var obj = _factory.Create();
            return obj.gameObject;
        }
    }

    public class DanboruPool : BasePool
    {
        private readonly EnemyBodyFactory enmeyBodyFactory;

        [Inject]
        public DanboruPool(GameObject prefab, int maxCapacity, EnemyBodyFactory enemyFactory) : base(prefab, maxCapacity)
        {
            enmeyBodyFactory = enemyFactory;
            InitPool();
        }

        protected override GameObject CreateObj()
        {
            var obj = enmeyBodyFactory.Create();
            obj.SetReleaseObjCallBack((thisobj) => ReturnObject(thisobj.gameObject));

            return obj.gameObject;
        }
    }
}
