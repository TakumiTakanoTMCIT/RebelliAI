using System;
using System.Collections;
using ActionStatusChk;
using Cysharp.Threading.Tasks;
using PlayerInfo;
using PlayerState;
using UnityEngine;
using UnityEngine.Pool;

public class AllShellManager : MonoBehaviour
{
    [SerializeField] private Transform mameParentTransform;
    [SerializeField] private int defaultCapacity = 3;
    [SerializeField] private float shootMameInterval = 0.5f;
    [SerializeField] private ActionStatusChecker actionStatusChecker;

    private ObjectPool<GameObject> pool;
    PlayerStatus status;
    PlayerStateMgr playerStateMgr;
    GameObject player;
    GameObject shellPrefab;

    bool isMameShootable;

    public static event Action onShootChargedShell, onShotNow;

    private void Awake()
    {
        Debug.Log($"this.gameObject.name: {this.gameObject.name}");
        isMameShootable = true;
        player = transform.parent.gameObject;
        status = player.MyGetComponent_NullChker<PlayerStatus>();
        playerStateMgr = player.MyGetComponent_NullChker<PlayerStateMgr>();

        shellPrefab = Resources.Load<GameObject>("Shell");
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
        GameObject shell = Instantiate(shellPrefab);
        shell.GetComponent<ShellMainBodyCrtl>().Init(pool);
        shell.transform.SetParent(mameParentTransform);

        return shell;
    }

    private void GetShell(GameObject shell)
    {
        shell.SetActive(true);
        shell.MyGetComponent_NullChker<ShellMainBodyCrtl>().GetShellAndSetDirection(actionStatusChecker.Direction, playerStateMgr.WhatCurrentState(playerStateMgr.dashState));
        shell.transform.position = player.transform.position;
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
        var chargedShell = Instantiate(shell, player.transform.position, Quaternion.identity);
        chargedShell.SetActive(true);
    }

    private async void mameInterval()
    {
        isMameShootable = false;
        await UniTask.Delay(TimeSpan.FromSeconds(shootMameInterval));
        isMameShootable = true;
    }
}
