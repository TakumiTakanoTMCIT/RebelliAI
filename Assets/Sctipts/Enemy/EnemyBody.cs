using System;
using ObjectPoolFactory;
using UnityEngine;
using Warp;
using Zenject;

public interface IConflictableAndAttackableToPlayer
{
    ConflictPlayerFinder conflictPlayerFinder { get; set; }
}

public interface IDamageableFromShot
{
    bool IsAlivingNow { get; set; }
    void TakeDamage(int damage);
}

public interface IPrefabEnemyBody
{
    void OnBecameInvisible();
    void MyAwake(Vector3 position, Transform parent, ExplosionSpawner explosionSpawner);
}

namespace Enemy
{
    [RequireComponent(typeof(ConflictPlayerFinder))]
    public class EnemyBody : MonoBehaviour, IDamageableFromShot, IPrefabEnemyBody, IConflictableAndAttackableToPlayer
    {
        //インターフェース実装--------------------
        public ConflictPlayerFinder conflictPlayerFinder { get; set; }

        [SerializeField] int initialHp = 3;
        [SerializeField] private int hp = 3;
        [SerializeField] private int conflictDamage = 1;
        public bool IsAlivingNow { get; set; }

        EnemySpawnPoser spawnPoser;
        ExplosionSpawner explosionSpawner;
        DanboruAnimStateMgr animStateMgr;

        //コールバック
        //Inject
        Action<GameObject> releaseObject;

        [Inject]
        public void Construct(PoolReleaser poolReleaser)
        {
            releaseObject = obj => poolReleaser.ReleaseObj(obj);
        }

        private void Awake()
        {
            IsAlivingNow = false;
            animStateMgr = gameObject.MyGetComponent_NullChker<DanboruAnimStateMgr>();

            conflictPlayerFinder = gameObject.MyGetComponent_NullChker<ConflictPlayerFinder>();
            conflictPlayerFinder.Init(conflictDamage);
        }

        //インターフェース実装------------------------------
        public void MyAwake(Vector3 position, Transform parent, ExplosionSpawner explosionSpawner)
        {
            IsAlivingNow = true;
            hp = initialHp;
            transform.position = position;
            transform.parent = parent;
            spawnPoser = parent.gameObject.MyGetComponent_NullChker<EnemySpawnPoser>();
            this.explosionSpawner = explosionSpawner;

            animStateMgr.MyAwake();
        }

        //Unityから呼び出されます
        public void OnBecameInvisible()
        {
            if (!IsAlivingNow) return;
            IsAlivingNow = false;
            releaseObject?.Invoke(gameObject);
        }

        public void TakeDamage(int damage)
        {
            Debug.Log("TakeDamage");
            if (!gameObject.activeSelf)
                return;
            hp -= damage;

            if (hp <= 0)
            {
                IsAlivingNow = false;
                hp = 0;
                //Debug.Log($"{releaseObject} : releaseObject");
                releaseObject?.Invoke(gameObject);
                spawnPoser.ResetInstance();
                explosionSpawner.MakeExplosion(transform.position);
                Debug.Log("Dead");
            }
        }
    }

    public class EnemyBodyFactory : PlaceholderFactory<EnemyBody> { }

    //Inject
    public class PoolReleaser
    {
        Action<GameObject> releaseObj;

        public PoolReleaser()
        {
            Debug.Log("PoolReleaser");
        }

        public void SetReleaseObjCallBack(Action<GameObject> releaseObj)
        {
            //Debug.Log($"releaseObj : {releaseObj}");
            this.releaseObj = releaseObj;
        }

        public void ReleaseObj(GameObject obj)
        {
            //Debug.Log("ReleaseObj");
            if(releaseObj != null)
                releaseObj.Invoke(obj);
            else
            {
                Debug.LogError("releaseObj is null");
            }
        }
    }
}
