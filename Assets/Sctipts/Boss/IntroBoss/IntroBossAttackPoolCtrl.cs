using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

namespace IntroBossExperimenter
{
    public class IntroBossAttackPoolCtrl : MonoBehaviour
    {
        [SerializeField] private GameObject attackPrefab;
        [SerializeField] internal bool isDebugMode = false;
        [SerializeField] internal List<GameObject> attackList;
        [SerializeField] internal int maxCount = 15;

        AttackObjectPoolHandler poolHandler;

        private void Awake()
        {
            attackList = new List<GameObject>();
            poolHandler = new AttackObjectPoolHandler(attackPrefab, this);
        }

        public GameObject GetAttack()
        {
            return poolHandler.GetMyAttack();
        }
    }

    public class AttackObjectPoolHandler
    {
        IntroBossAttackPoolCtrl poolCtrl;
        GameObject attackPrefab;
        internal ObjectPool<GameObject> pool;

        public AttackObjectPoolHandler(GameObject attackPrefab, IntroBossAttackPoolCtrl poolCtrl)
        {
            this.poolCtrl = poolCtrl;
            this.attackPrefab = attackPrefab;

            Init();
        }

        public GameObject GetMyAttack()
        {
            if (poolCtrl.isDebugMode) Debug.Log("新しく攻撃オブジェクトを生成します");
            var attack = pool.Get();
            return attack;
        }

        public void ReleaseMine(GameObject attack)
        {
            pool.Release(attack);
        }

        //--オブジェクトプールの内部の処理--

        private void Init()
        {
            pool = new ObjectPool<GameObject>
            (
                createFunc: CreateAttack,
                actionOnGet: GetAttack,
                actionOnRelease: ReleaseAttack,
                actionOnDestroy: DestroyAttack,
                collectionCheck: true,
                defaultCapacity: poolCtrl.maxCount,
                maxSize: poolCtrl.maxCount
            );

            for (int COUNT = 0; COUNT < poolCtrl.maxCount; COUNT++)
            {
                if (poolCtrl.isDebugMode) Debug.LogWarning("初期化処理を行います");
                GameObject attack = CreateAttack();
                pool.Release(attack);
            }
        }

        private GameObject CreateAttack()
        {
            GameObject attack = GameObject.Instantiate(attackPrefab);
            attack.gameObject.MyGetComponent_NullChker<IntroBossAttackBody>().Init(this, GameObject.Find("IntroBossAttacks").transform);
            poolCtrl.attackList.Add(attack);
            return attack;
        }

        private void GetAttack(GameObject attack)
        {
            attack.SetActive(true);
        }

        private void ReleaseAttack(GameObject attack)
        {
            attack.SetActive(false);
        }

        private void DestroyAttack(GameObject attack)
        {
            poolCtrl.attackList.Remove(attack);
            GameObject.Destroy(attack);
        }
    }
}
