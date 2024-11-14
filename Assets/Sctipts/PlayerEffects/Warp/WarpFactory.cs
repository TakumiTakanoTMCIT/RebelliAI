using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Pool;
using UniRx;

namespace ObjectPoolFactory
{
    public class WarpFactory : ObjPoolBase
    {
        [SerializeField] Transform playerTransform;
        [SerializeField] int MakeAmount = 20;
        [SerializeField] internal float showingTime = 1f, createIntervel = 0.1f, initialYPossition = 7f, randomXRange = 1f;

        private Subject<Unit> onCompletedWarpEffect = new Subject<Unit>();
        public IObservable<Unit> OnCompletedWarpEffect => onCompletedWarpEffect;

        private void Awake()
        {
            Init();
        }

        //ワープエフェクトを演出処理

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            if (playerTransform != null) Gizmos.DrawSphere(new Vector3(playerTransform.position.x, initialYPossition + playerTransform.position.y, 0), 0.8f);

            Gizmos.color = Color.red;
            if (playerTransform != null) Gizmos.DrawSphere(playerTransform.position, 0.8f);
        }

        public async UniTask StartWarpDirection(bool isOnStage)
        {
            //MakeAmount回エフェクトを上から下に生成（OnStageのとき）
            //OffStageのときにはプレイヤーのワープアニメーションが終わったらエフェクトを下から上に生成していく。

            for (int count = 0; count < MakeAmount; count++)
            {
                //インターバルだけ待機
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(createIntervel));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    //UnityEditor.EditorApplication.isPaused = true;
                    return;
                }

                /*pool.Get().gameObject.MyGetComponent_NullChker<WarpEffectBody>().Init(this, playerTransform.position.x, transform.position.y + initialYPossition - (initialYPossition / MakeAmount * count));*/
                pool.Get().gameObject.MyGetComponent_NullChker<WarpEffectBody>().Init(
                    this,
                    playerTransform.position.x + UnityEngine.Random.Range(-randomXRange, randomXRange),
                    UnityEngine.Random.Range(playerTransform.position.y, initialYPossition + playerTransform.position.y));
            }
            //すべて生成し終わったらプレイヤーのワープアニメーションが終わるまで待つ
            onCompletedWarpEffect.OnNext(Unit.Default);
        }

        //--外部から実行されるメソッド--

        public void MakeWarpEffect()
        {
            pool.Get();
        }

        public void ReleaseWarpEffect(GameObject releaseObj)
        {
            pool.Release(releaseObj);
        }

        //--抽象メソッド--

        protected override GameObject CreateObj()
        {
            var instance = Instantiate(prefab);
            instance.transform.SetParent(parentObj.transform);
            return instance;
        }

        protected override void GetObj(GameObject getObj)
        {
            getObj.SetActive(true);
        }

        protected override void ReleaseObj(GameObject releaseObj)
        {
            releaseObj.SetActive(false);
        }

        protected override void DestroyObj(GameObject destroyObj)
        {
            Destroy(destroyObj);
        }
    }

    public abstract class ObjPoolBase : MonoBehaviour
    {
        protected abstract GameObject CreateObj();
        protected abstract void GetObj(GameObject getObj);
        protected abstract void ReleaseObj(GameObject releaseObj);
        protected abstract void DestroyObj(GameObject destroyObj);

        [SerializeField] protected GameObject prefab, parentObj;
        [SerializeField] private int DefaultCapacity = 5;

        protected ObjectPool<GameObject> pool;
        protected void Init()
        {
            pool = new ObjectPool<GameObject>
            (
                createFunc: CreateObj,
                actionOnGet: GetObj,
                actionOnRelease: ReleaseObj,
                actionOnDestroy: DestroyObj,
                collectionCheck: true,
                defaultCapacity: DefaultCapacity,
                maxSize: DefaultCapacity
            );

            for (int count = 0; count < DefaultCapacity; count++)
            {
                pool.Release(CreateObj());
            }
        }
    }

}
