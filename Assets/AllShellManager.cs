using System.Collections;
using PlayerInfo;
using UnityEngine;
using UnityEngine.Pool;

public class AllShellManager : MonoBehaviour
{
    [SerializeField] private Transform mameParentTransform;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private float shootMameInterval = 0.5f;

    private ObjectPool<GameObject> pool;
    PlayerStatus status;
    GameObject player;
    GameObject shellPrefab;

    bool isMameShootable;

    private void Awake()
    {
        isMameShootable = true;
        player = transform.parent.gameObject;
        status = player.MyGetComponent_NullChker<PlayerStatus>();

        shellPrefab = Resources.Load<GameObject>("Shell");
        if (shellPrefab == null)
            Debug.LogWarning("shellPrefabが設定されていません");
    }

    private void Start()
    {
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
            defaultCapacity: 3,
            maxSize: defaultCapacity
        );

        /// <summary>
        /// デフォルトの容量分だけ先に生成して処理を軽くするぞ！
        /// </summary>
        int count;
        count = 0;
        while (count < defaultCapacity)
        {
            pool.Get();
            count++;
        }
    }

    private GameObject CreateShell()
    {
        GameObject shell = Instantiate(shellPrefab);
        shell.GetComponent<ShellMainBodyCrtl>().Init(pool);
        shell.transform.parent = mameParentTransform;

        return shell;
    }

    private void GetShell(GameObject shell)
    {
        shell.SetActive(true);
        var shellMainBodyCtrl = shell.MyGetComponent_NullChker<ShellMainBodyCrtl>();

        bool direction = status.playerdirection;
        shellMainBodyCtrl.GetShellAndSetDirection(direction, status.IsDashNow());

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

    public void ShootMame()
    {
        /// <summary>
        /// 最大値になっていたら銃を打てません
        /// </summary>
        if (pool.CountActive >= defaultCapacity) return;

        /// <summary>
        /// インターバルを設ける
        /// </summary>
        if(!isMameShootable) return;
        pool.Get();
        StartCoroutine(mameIntervalCoroutine());
    }

    public void ShootChargedShell(GameObject shell)
    {
        Instantiate(shell, player.transform.position, Quaternion.identity);
    }

    IEnumerator mameIntervalCoroutine()
    {
        isMameShootable = false;
        yield return new WaitForSeconds(shootMameInterval);
        isMameShootable = true;
    }
}
