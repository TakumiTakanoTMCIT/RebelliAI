using System;
using Item.HP;
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
    public interface IEnemy
    {
        public void MyAwake(Vector3 position, Transform parent, ExplosionSpawner explosionSpawner);
        Action<GameObject> releaseObject { get; set; }
        void DieAndReleaseObj();
    }

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
        SpriteRenderer spRenderer;
        Item.HP.HPItemSpawner hPItemSpawner;

        //コールバック
        //Inject
        public Action<GameObject> releaseObject;

        [Inject]
        public void Construct(PoolReleaser poolReleaser, Item.HP.HPItemSpawner itemSpawner)
        {
            hPItemSpawner = itemSpawner;
            releaseObject = obj => poolReleaser.ReleaseObj(obj);
        }

        private void Awake()
        {
            IsAlivingNow = false;
            animStateMgr = gameObject.MyGetComponent_NullChker<DanboruAnimStateMgr>();

            conflictPlayerFinder = gameObject.MyGetComponent_NullChker<ConflictPlayerFinder>();
            conflictPlayerFinder.Init(conflictDamage);

            spRenderer = GetComponent<SpriteRenderer>();
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
            DieAndReleaseObj();
        }

        public void TakeDamage(int damage)
        {
            //Debug.Log("TakeDamage");
            if (!gameObject.activeSelf)
                return;
            hp -= damage;

            if (hp <= 0)
            {
                DieAndReleaseObj();
                explosionSpawner.MakeExplosion(transform.position);

                //とりあえずHPアイテムを出す
                hPItemSpawner.DropHPItem(transform.position);
            }
        }

        public void DieAndReleaseObj()
        {
            if (!IsAlivingNow) return;

            hp = 0;
            IsAlivingNow = false;
            //Debug.Log("Dead");
            //TODO: これでいいんじゃないの？↓この書き方でも問題ない気がする。おそらくZenjectの関係でこうなっています
            //spawnPoser.ResetInstance();
            releaseObject?.Invoke(gameObject);
        }

        public bool IsVisible()
        {
            return spRenderer.isVisible;
        }
    }

    public class EnemyBodyFactory : PlaceholderFactory<EnemyBody> { }

    //Inject
    public class PoolReleaser
    {
        Action<GameObject> releaseObj;

        public void SetReleaseObjCallBack(Action<GameObject> releaseObj)
        {
            this.releaseObj = releaseObj;
        }

        public void ReleaseObj(GameObject obj)
        {
            //Debug.Log("ReleaseObj");
            if (releaseObj != null)
                releaseObj.Invoke(obj);
            else
            {
                Debug.LogError("releaseObj is null");
            }
        }
    }
}
