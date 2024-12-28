using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Pool;
using PlayerInfo;
using ActionStatusChk;
public class DashSparkFactory : MonoBehaviour
{
    GameObject dashSparkPrefab;
    PlayerStatus status;
    [SerializeField] private GameObject player;
    [SerializeField] private int DefaultCapacity = 5;
    [SerializeField] private ActionStatusChecker actionStatusChecker;

    private ObjectPool<GameObject> pool;

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
        status = player.MyGetComponent_NullChker<PlayerStatus>();

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
        effect.gameObject.GetComponent<DashSparkBody>().Init(pool, player.transform, actionStatusChecker.Direction);
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
