using PlayerInfo;
using UnityEngine;
using UnityEngine.Pool;

public class ShellManager : MonoBehaviour
{
    [SerializeField] PlayerStatus status;

    [SerializeField] private GameObject shellPrefab;
    [SerializeField] private GameObject player;
    [SerializeField] private int defaultCapacity = 5;

    [SerializeField] private float mameSpeed = 15.0f;

    private ObjectPool<GameObject> pool;

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
    }

    private GameObject CreateShell()
    {
        //Debug.Log("CreateShell");

        GameObject shell = Instantiate(shellPrefab);
        shell.GetComponent<ShellMainBodyCrtl>().Init(this, pool, mameSpeed);

        return shell;
    }

    private void GetShell(GameObject shell)
    {
        //Debug.Log("Active!!");

        shell.SetActive(true);
        var shellMainBodyCtrl = shell.GetComponent<ShellMainBodyCrtl>();

        bool direction = status.playerdirection;
        shellMainBodyCtrl.SetDirection(direction);

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

        pool.Get();
    }

    public void ShootChargedShell(GameObject shell)
    {
        var instanceShell = Instantiate(shell, player.transform.position, Quaternion.identity);
    }
}
