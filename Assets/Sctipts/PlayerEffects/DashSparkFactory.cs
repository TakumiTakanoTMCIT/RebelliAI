using UnityEngine;
using UnityEngine.Pool;
using ActionStatusChk;
using Zenject;

public class DashSparkFactory : MonoBehaviour
{
    //Inject
    private IPlayerDirection playerDirection;

    GameObject dashSparkPrefab;
    [SerializeField] private GameObject player;
    [SerializeField] private int DefaultCapacity = 5;

    private ObjectPool<GameObject> pool;

    [Inject]
    public void Construct(IPlayerDirection playerDirection)
    {
        this.playerDirection = playerDirection;
    }

    private void Awake()
    {
        pool = new ObjectPool<GameObject>
        (
            createFunc: CreateEffect,
            actionOnGet: GetEffect,
            actionOnRelease: ReleaseEffect,
            actionOnDestroy: DestroyEffect,
            collectionCheck: true,
            defaultCapacity: DefaultCapacity,
            maxSize: DefaultCapacity
        );

        dashSparkPrefab = Resources.Load<GameObject>("DashSpark");

        if (player == null)
        {
            Debug.LogWarning("Player が見つかりませんでした。追加してください");
            //EditorApplication.isPaused = true;
        }
    }

    void Start()
    {
        for (int count = 0; count < DefaultCapacity; count++)
        {
            var instance = CreateEffect();
            instance.SetActive(false);
            pool.Release(instance);
        }
    }

    private GameObject CreateEffect()
    {
        var instance = Instantiate(dashSparkPrefab);
        instance.transform.SetParent(transform);
        //instance.SetActive(false);
        return instance;
    }

    private void GetEffect(GameObject effect)
    {
        effect.SetActive(true);
        effect.gameObject.GetComponent<DashSparkBody>().Init(pool, player.transform, playerDirection.Direction.Value);
    }

    private void ReleaseEffect(GameObject effect)
    {
        effect.SetActive(false);
    }

    private void DestroyEffect(GameObject effect)
    {
        Destroy(effect);
    }

    public GameObject MakeEffect()
    {
        return pool.Get();
    }
}
