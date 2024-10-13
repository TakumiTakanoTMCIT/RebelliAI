using UnityEngine;
using UnityEngine.Pool;
using UnityEditor;
using System.Collections;
using PlayerInfo;

public class AfterGrowFactory : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] float maxTime = 0.3f, minTime = 0.1f;
    [SerializeField] int defaultCapacity = 10;

    private GameObject effect;
    bool isInstantiable = true;
    private ObjectPool<GameObject> pool;

    PlayerDashTimeCtrl playerDashTimeCtrl;
    PlayerDashKeepManager playerDashKeepManager;
    PlayerStatus playerStatus;

    Coroutine coroutine;
    private IEnumerator CountTime()
    {
        isInstantiable = false;
        yield return new WaitForSeconds(Random.Range(minTime, maxTime));
        isInstantiable = true;
    }

    private void Awake()
    {
        effect = Resources.Load<GameObject>("DashAfterGrow");
        if (effect == null)
        {
            Debug.LogWarning("Effect is null");
            EditorApplication.isPaused = true;
        }

        playerDashTimeCtrl = player.MyGetComponent_NullChker<PlayerDashTimeCtrl>();
        playerDashKeepManager = player.MyGetComponent_NullChker<PlayerDashKeepManager>();
        playerStatus = player.MyGetComponent_NullChker<PlayerStatus>();

        InitPool();
    }

    private void Update()
    {
        if (!isInstantiable) return;

        if (playerDashTimeCtrl.IsDashNow || playerDashKeepManager.IsKeepDashSpeed)
        {
            pool.Get();
        }
    }

    //--Pool Methods--↓↓

    //これは、プールを初期化するためのメソッドです。（Initに書くとごちゃつくのでここに書きました）
    private void InitPool()
    {
        pool = new ObjectPool<GameObject>
        (
            createFunc: CreateEffect,
            actionOnGet: GetEffect,
            actionOnRelease: ReleaseEffect,
            actionOnDestroy: DestroyEffect,
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: 99
        );
    }

    private GameObject CreateEffect()
    {
        var instance = Instantiate(effect, player.transform.position, Quaternion.identity);
        instance.gameObject.MyGetComponent_NullChker<AfterGrowMain>().Init(pool, player.transform, playerStatus);
        instance.SetActive(false);
        return instance;
    }

    private void GetEffect(GameObject effect)
    {
        effect.SetActive(true);
        effect.MyGetComponent_NullChker<AfterGrowMain>().StartAnim_Movement();
        coroutine = StartCoroutine(CountTime());
    }

    private void ReleaseEffect(GameObject effect)
    {
        effect.SetActive(false);
    }

    private void DestroyEffect(GameObject effect)
    {
        Destroy(effect);
    }

    //--Pool Methods--↑↑
}
