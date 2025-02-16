using UnityEngine;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;
using System;
using HPBar;
using ActionStatusChk;
using UniRx;
using Zenject;

public class AfterGrowFactory : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] float maxTime = 0.3f, minTime = 0.1f;
    [SerializeField] int defaultCapacity = 10;
    [SerializeField] private Transform parentTransform;
    [SerializeField] private ActionStatusChecker actionStatusChecker;

    //Inject
    LifeManager lifeManager;
    AfterGrowMain.Factory afterGrowMainFactory;

    private GameObject effect;
    bool isInstantiable = true, isPlayerDamage_Death = false;
    private ObjectPool<GameObject> pool;

    PlayerDashTimeCtrl playerDashTimeCtrl;
    PlayerDashKeepManager playerDashKeepManager;

    async private void CountInterval()
    {
        isInstantiable = false;
        await UniTask.Delay(TimeSpan.FromSeconds(UnityEngine.Random.Range(minTime, maxTime)));
        isInstantiable = true;
    }

    [Inject]
    public void Construct(LifeManager lifeManager , AfterGrowMain.Factory factory)
    {
        this.lifeManager = lifeManager;
        this.afterGrowMainFactory = factory;
    }

    private void Awake()
    {
        effect = Resources.Load<GameObject>("DashAfterGrow");
        if (effect == null)
        {
            Debug.LogWarning("Effect is null");
            //EditorApplication.isPaused = true;
        }

        playerDashTimeCtrl = player.MyGetComponent_NullChker<PlayerDashTimeCtrl>();
        playerDashKeepManager = player.MyGetComponent_NullChker<PlayerDashKeepManager>();

        isPlayerDamage_Death = false;

        InitPool();

        lifeManager.OnPlayerDead.Subscribe(_ =>
        {
            OnPlayerDeath_Damage();
        })
        .AddTo(this);
    }

    private void Update()
    {
        if (!isInstantiable) return;
        if (isPlayerDamage_Death) return;

        if (playerDashTimeCtrl.IsDashNow || playerDashKeepManager.IsKeepDashSpeed)
        {
            pool.Get();
        }
    }

    void OnPlayerDeath_Damage() => isPlayerDamage_Death = true;

    void OnPlayerRecoverDamage() => isPlayerDamage_Death = false;

    private void OnEnable()
    {
        HPBarHandler.onPlayerDamage += OnPlayerDeath_Damage;

        PlayerState.DamageState.onPlayerDamageRecover += OnPlayerRecoverDamage;
    }

    private void OnDisable()
    {
        HPBarHandler.onPlayerDamage -= OnPlayerDeath_Damage;

        PlayerState.DamageState.onPlayerDamageRecover -= OnPlayerRecoverDamage;
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

        for (int count = 0; count < defaultCapacity; count++)
        {
            var instance = CreateEffect();
            instance.transform.SetParent(parentTransform);
            instance.SetActive(false);
            pool.Release(instance);
        }
    }

    private GameObject CreateEffect()
    {
        var instance = afterGrowMainFactory.Create();
        instance.Init(pool, player.transform);
        instance.gameObject.SetActive(false);
        return instance.gameObject;
    }

    private void GetEffect(GameObject effect)
    {
        effect.SetActive(true);
        effect.MyGetComponent_NullChker<AfterGrowMain>().StartAnim_Movement();
        CountInterval();
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
