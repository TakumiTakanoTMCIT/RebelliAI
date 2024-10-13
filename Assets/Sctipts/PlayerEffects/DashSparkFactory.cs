using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Pool;
using PlayerInfo;
public class DashSparkFactory : MonoBehaviour
{
    GameObject dashSparkPrefab;
    PlayerStatus status;
    [SerializeField] private GameObject player;
    [SerializeField] private int DefaultCapacity = 3;

    private ObjectPool<GameObject> pool;

    void Start()
    {
        pool = new ObjectPool<GameObject>
        (
            createFunc: CreateEffect,
            actionOnGet: GetEffect,
            actionOnRelease: ReleaseEffect,
            actionOnDestroy: DestroyEffect,
            collectionCheck: true,
            defaultCapacity: DefaultCapacity,
            maxSize: 3
        );

        dashSparkPrefab = Resources.Load<GameObject>("DashSpark");
        status = player.MyGetComponent_NullChker<PlayerStatus>();

        if (player == null)
        {
            Debug.LogWarning("Player が見つかりませんでした。追加してください");
            EditorApplication.isPaused = true;
        }
    }

    private GameObject CreateEffect()
    {
        var instance = Instantiate(dashSparkPrefab, transform.position, Quaternion.identity);
        return instance;
    }

    private void GetEffect(GameObject effect)
    {
        Debug.Log("GetEffect");
        effect.SetActive(true);
        effect.gameObject.GetComponent<DashSparkBody>().Init(pool, player.transform, status.playerdirection);
    }

    private void ReleaseEffect(GameObject effect)
    {
        effect.SetActive(false);
    }

    private void DestroyEffect(GameObject effect)
    {
        Destroy(effect);
    }

    public void MakeEffect()
    {
        pool.Get();
    }
}
