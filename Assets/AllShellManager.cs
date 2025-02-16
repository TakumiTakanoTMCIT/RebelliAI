using System;
using ActionStatusChk;
using Cysharp.Threading.Tasks;
using PlayerState;
using UnityEngine;
using UniRx;
using UnityEngine.Pool;
using Zenject;

public class AllShellManager : MonoBehaviour
{
    [SerializeField] private Transform mameParentTransform, muzzleTransform;
    [SerializeField] private int defaultCapacity = 3;
    [SerializeField] private float shootMameInterval = 0.5f;
    [SerializeField]GameObject shellPrefab;

    [Inject]
    DiContainer container;
    //Inject
    IPlayerDirection playerDirection;

    private ObjectPool<GameObject> pool;
    PlayerStateMgr playerStateMgr;
    GameObject player;

    bool isMameShootable;

    public static event Action onShootChargedShell, onShotNow;

    [Inject]
    public void Construct(IPlayerDirection playerDirection)
    {
        this.playerDirection = playerDirection;
    }

    private void Awake()
    {
        Debug.Log(gameObject.name + "が生成されました");

        isMameShootable = true;
        player = transform.parent.gameObject;
        playerStateMgr = player.MyGetComponent_NullChker<PlayerStateMgr>();

        if (shellPrefab == null)
            Debug.LogWarning("shellPrefabが設定されていません");

        /// <summary>
        /// オブジェクトプールをインスタンス化
        /// </summary>
        pool = new ObjectPool<GameObject>
        (
            createFunc: CreateShell,
            actionOnGet: GetShell,
            actionOnRelease: ReleaseShell,
            actionOnDestroy: DestroyShell,
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: defaultCapacity
        );

        /// <summary>
        /// デフォルトの容量分だけ先に生成して処理を軽くするぞ！
        /// </summary>
        int count = 0;
        while (count < defaultCapacity)
        {
            pool.Release(CreateShell());
            count++;
        }
    }

    private GameObject CreateShell()
    {
        GameObject shell = container.InstantiatePrefab(shellPrefab);
        shell.GetComponent<ShellMainBodyCrtl>().Init(pool);
        shell.transform.SetParent(mameParentTransform);

        return shell;
    }

    private void GetShell(GameObject shell)
    {
        shell.SetActive(true);
        shell.MyGetComponent_NullChker<ShellMainBodyCrtl>().GetShellAndSetDirection(playerDirection.Direction.Value, playerStateMgr.WhatCurrentState(playerStateMgr.dashState));
        shell.transform.position = muzzleTransform.position;
    }

    private void ReleaseShell(GameObject shell)
    {
        shell.SetActive(false);
    }

    private void DestroyShell(GameObject shell)
    {
        Destroy(shell);
    }

    public void ShootMame(bool isCharged)
    {
        //チャージされた豆ならチャージショットとして扱います
        if (isCharged) onShootChargedShell?.Invoke();

        /// <summary>
        /// 最大値になっていたら銃を打てません
        /// </summary>
        if (pool.CountActive >= defaultCapacity) return;

        /// <summary>
        /// インターバルを設ける
        /// </summary>
        if (!isMameShootable) return;
        pool.Get();
        onShotNow?.Invoke();
        mameInterval();
    }

    public void ShootChargedShell(GameObject shell)
    {
        onShotNow?.Invoke();
        onShootChargedShell?.Invoke();
        var chargedShell = container.InstantiatePrefab(shell);
        chargedShell.SetActive(true);
    }

    private async void mameInterval()
    {
        isMameShootable = false;
        await UniTask.Delay(TimeSpan.FromSeconds(shootMameInterval));
        isMameShootable = true;
    }
}
