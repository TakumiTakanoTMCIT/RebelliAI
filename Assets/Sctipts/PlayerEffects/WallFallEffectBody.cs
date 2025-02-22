using System;
using PlayerFlip;
using PlayerState;
using UniRx;
using UnityEngine;
using Zenject;

namespace WallFallEffect
{
    public class EffectFactory : PlaceholderFactory<WallFallEffectBody> { }

    /// <summary>
    /// このクラスは、表示された時に、ランダムな色のアニメーションを再生するクラスです。
    /// また、アニメーションが終了した時に、アニメーションを停止する処理を行います。
    /// </summary>
    public class WallFallEffectBody : MonoBehaviour
    {
        //Inject
        RandomColorLogic randomColor;
        AnimLogic animLogic;
        EventMediator eventMediator;
        Transform playerTransform;
        IDirection direction;

        Vector2 displacement = new Vector2(0.405f, -0.2f);
        AnimLogic.BoolName myColor;

        [Inject]
        public void Construct(RandomColorLogic randomColor, AnimLogic animLogic, EventMediator eventMediator, PlayerStateMgr playerStateMgr, IDirection direction)
        {
            this.randomColor = randomColor;
            this.animLogic = animLogic;
            this.eventMediator = eventMediator;
            playerTransform = playerStateMgr.transform;
            this.direction = direction;
        }

        private void Awake()
        {
            animLogic.Init(GetComponent<Animator>());
        }

        private void OnEnable()
        {
            myColor = randomColor.GetRandomColor();

            animLogic.EnableBool(myColor);

            if (direction.Direction.Value)//右向きの時
            {
                transform.position = playerTransform.position - (Vector3)displacement;
            }
            else
            {

                transform.position = playerTransform.position + (Vector3)displacement;
            }
        }

        //Animation Eventです。
        public void LastFrame()
        {
            animLogic.DisableBool();
            eventMediator.disableObjObserver.OnNext(gameObject);
        }
    }

    public class AnimLogic
    {
        Animator animator;

        public enum BoolName
        {
            isBlue,
            isDarkBlue,
            isPink
        }

        public void Init(Animator animator)
        {
            this.animator = animator;
        }

        public void EnableBool(BoolName animName)
        {
            animator.SetBool(animName.ToString(), true);
        }

        public void DisableBool()
        {
            animator.SetBool(BoolName.isBlue.ToString(), false);
            animator.SetBool(BoolName.isDarkBlue.ToString(), false);
            animator.SetBool(BoolName.isPink.ToString(), false);
        }
    }

    public class RandomColorLogic
    {
        public AnimLogic.BoolName GetRandomColor()
        {
            int randomColor = UnityEngine.Random.Range(0, 3);
            switch (randomColor)
            {
                case 0:
                    return AnimLogic.BoolName.isBlue;
                case 1:
                    return AnimLogic.BoolName.isDarkBlue;
                case 2:
                    return AnimLogic.BoolName.isPink;
                default:
                    Debug.LogError("Blue : Error!!!!");
                    return AnimLogic.BoolName.isBlue;
            }
        }
    }

    /// <summary>
    /// WallFallEffectのオブジェクトプールを提供するクラスです。
    /// </summary>
    public class WallFallEffectPoolLogic
    {
        //Inject
        private EffectFactory factory;

        public UnityEngine.Pool.ObjectPool<GameObject> pool;

        [Inject]
        public WallFallEffectPoolLogic(EffectFactory factory)
        {
            this.factory = factory;

            //TODO : マジックナンバーを消す
            int PoolSize = 50;

            pool = new UnityEngine.Pool.ObjectPool<GameObject>(
                createFunc: CreateObj,
                actionOnGet: EnableObj,
                actionOnRelease: DisableObj,
                actionOnDestroy: DestroyObj,
                defaultCapacity: PoolSize,
                maxSize: PoolSize
            );

            for (int i = 0; i < PoolSize; i++)
            {
                var obj = CreateObj();
                pool.Release(obj);
            }
        }

        private GameObject CreateObj()
        {
            Debug.Log("CreateObj");
            return factory.Create().gameObject;
        }

        private void EnableObj(GameObject obj)
        {
            obj.SetActive(true);
        }

        private void DisableObj(GameObject obj)
        {
            obj.SetActive(false);
        }

        private void DestroyObj(GameObject obj)
        {
            GameObject.Destroy(obj);
        }
    }

    /// <summary>
    /// WallFallEffectをいつ生成するか決めるクラスです
    /// </summary>
    public class WallFallEffectHandler
    {
        //Inject
        WallFallEffectPoolLogic poolLogic;

        [Inject]
        public WallFallEffectHandler(WallFallEffectPoolLogic poolLogic, DisposableMgr disposableMgr, EventMediator eventMediator, PlayerStateMgr playerStateMgr)
        {
            this.poolLogic = poolLogic;

            TimeSpan timeSpan = TimeSpan.FromSeconds(0.08f);
            Observable.Interval(timeSpan)
                .Where(_ => playerStateMgr.WhatCurrentState(playerStateMgr.wallFallState))//WallFallStateならば〜
                .Subscribe(_ => CreateWallFallEffect())
                .AddTo(disposableMgr.disposables);

            eventMediator.OnDisableObj
                .Subscribe(obj => Release(obj))
                .AddTo(disposableMgr.disposables);
        }

        /// <summary>
        /// 数秒おきにWallFallEffectを生成します。
        /// </summary>
        public void CreateWallFallEffect()
        {
            var wallFallEffect = poolLogic.pool.Get();
        }

        //TODO : インターフェースで渡してあげる。保守性が高まる
        private void Release(GameObject obj)
        {
            poolLogic.pool.Release(obj);
        }
    }

    public class EventMediator
    {
        private Subject<GameObject> disableObj = new Subject<GameObject>();
        public IObservable<GameObject> OnDisableObj => disableObj;
        public IObserver<GameObject> disableObjObserver => disableObj;
    }
}
