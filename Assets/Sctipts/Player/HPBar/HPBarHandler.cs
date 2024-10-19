using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using PlayerState;

public class HPBarHandler : MonoBehaviour
{
    [SerializeField] int InitialMaxLife = 1, InitialCurrentLife = 1;
    [SerializeField] int healAndDamageAmount = 5;
    [SerializeField] float intervalTime = 0.05f;
    [SerializeField] GameObject Base;
    [SerializeField] GameObject midPrefab;
    [SerializeField] GameObject Top;
    [SerializeField] GameObject hpUnit;
    [SerializeField] float barHeight = 6;
    [SerializeField] PlayerStateMgr stateMgr;
    [SerializeField] bool isDebugMode = false;

    RectTransform topTransform;

    [SerializeField] List<GameObject> hpUnitList;

    [SerializeField] private int playerMaxLife = 0, currentLife = 0;

    /// <summary>
    /// コンポーネントのインスタンスの取得のみを行う
    /// </summary>
    private void Awake()
    {
        topTransform = Top.MyGetComponent_NullChker<RectTransform>();
        hpUnitList = new List<GameObject>();
    }

    private void Start()
    {
        topTransform.anchoredPosition = Vector2.zero;
        AddLife(InitialMaxLife);
        Heal(InitialCurrentLife);
    }

    //デバッグ用です
    private void Update()
    {
        if(!isDebugMode) return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            Damage(healAndDamageAmount);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            Heal(healAndDamageAmount);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            AddLife(2);
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            Heal(playerMaxLife);
        }
    }

    public async void Heal(int healAmount)
    {
        if (healAmount <= 0)
            return;

        for (int i = 0; i < healAmount; i++)
        {
            if (currentLife >= playerMaxLife)
            {
                return;
            }

            await HealLife();
        }
    }

    public async void Damage(int damageAmount)
    {
        Debug.Log($"ダメージの値は{damageAmount}です");

        if (damageAmount <= 0)
        {
            Debug.Log("プラスの値を入力しないとダメージを受けません");
            return;
        }

        if (currentLife <= 0)
        {
            Debug.Log("HPが0以下になるとダメージを受けません。つまり死んでいます");
            return;
        }

        await DamageLife(damageAmount);
    }

    public async void AddLife(int addAmount)
    {
        if (addAmount <= 0)
            return;

        for (int i = 0; i < addAmount; i++)
        {
            await MakeMidBar();
        }
    }

    /// <summary>
    /// これより下は、内部処理を行う関数です。決して外部から呼び出さないでください。
    /// 呼び出すときには、上記の関数を使用してください。
    /// </summary>

    async Task DamageLife(int damageAmount)
    {
        Debug.Log("DamageLife関数が呼ばれました");
        for (int count = 0; count < damageAmount; count++)
        {
            currentLife--;

            Destroy(hpUnitList[hpUnitList.Count - 1]);
            hpUnitList.RemoveAt(hpUnitList.Count - 1);

            if (currentLife <= 0)
            {
                Debug.Log("死んだ！！！");
                stateMgr.OnDeath();
                return;
            }

            stateMgr.OnDamage();
            await UniTask.Delay(TimeSpan.FromSeconds(intervalTime));
        }
    }

    async Task MakeMidBar()
    {
        var instance = (GameObject)Instantiate(midPrefab);
        instance.transform.SetParent(Base.transform);
        instance.transform.position = new Vector3(Base.transform.position.x, Base.transform.position.y + (playerMaxLife * barHeight));

        Vector2 pos = topTransform.anchoredPosition;
        pos.y += barHeight;
        topTransform.anchoredPosition = pos;

        playerMaxLife++;

        await UniTask.Delay(TimeSpan.FromSeconds(intervalTime));
    }

    async Task HealLife()
    {
        var instance = (GameObject)Instantiate(hpUnit);
        instance.transform.SetParent(Base.transform);
        var rectTrasnform = instance.MyGetComponent_NullChker<RectTransform>();
        rectTrasnform.anchoredPosition = new Vector2(0, currentLife * barHeight);

        hpUnitList.Add(instance);
        currentLife++;

        await UniTask.Delay(TimeSpan.FromSeconds(intervalTime));
    }
}
