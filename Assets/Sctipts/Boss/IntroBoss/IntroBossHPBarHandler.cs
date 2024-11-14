using UnityEngine;
using HPBar;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System;

public class IntroBossHPBarHandler : HPBarBase
{
    [SerializeField] private float healCutSceneWaitTime = 1.0f;
    public static event Action onDead, onFinishInitializeHPBar;

    private void Awake()
    {
        hpUnitList = new List<GameObject>();
        InitMaxLife(initMaxLife);
    }

    private void OnEnable()
    {
        IntroBossHealthCtrl.onDamage += Damage;
        BossCutSceneHandler.onInitHPBar += Init;
    }

    private void OnDisable()
    {
        IntroBossHealthCtrl.onDamage -= Damage;
        BossCutSceneHandler.onInitHPBar -= Init;
    }

    private async void Init()
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(healCutSceneWaitTime));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            //UnityEditor.EditorApplication.isPlaying = false;
            return;
        }

        await Heal(initMaxLife);
        onFinishInitializeHPBar?.Invoke();
    }

    private void Update()
    {
        if (!isDebugMode) return;

        if (Input.GetKeyDown(KeyCode.N))
        {
            Damage(10);
        }
    }

    protected override void OnDead()
    {
        onDead?.Invoke();
    }

    void InitMaxLife(int maxLife)
    {
        for (int COUNT = 0; COUNT < maxLife; COUNT++)
        {
            var instance = Instantiate(midPrefab);
            instance.transform.SetParent(BaseObj.transform);
            instance.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, COUNT * barHeight);
            hpUnitList.Add(instance);
        }

        topTransform.anchoredPosition = new Vector2(0, maxLife * barHeight);
    }

    protected override async UniTask AddLife(int lifeUpCount)
    {
        for (int count = 0; count < lifeUpCount; count++)
        {
            try
            {
                Debug.Log("AddLife");
                await MakeMidBar();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                //UnityEditor.EditorApplication.isPlaying = false;
                return;
            }
        }
    }

    protected override async UniTask Heal(int healAmount)
    {
        for (int count = 0; count < healAmount; count++)
        {
            try
            {
                await HealLife();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                //UnityEditor.EditorApplication.isPlaying = false;
                return;
            }
        }
    }

    public override void Damage(int damageAmount)
    {
        DamageLife(damageAmount);
    }
}
