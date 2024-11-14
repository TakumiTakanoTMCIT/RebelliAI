using UnityEngine;
using UnityEditor;
using Cysharp.Threading.Tasks;
using System;

public class BossMgr : MonoBehaviour
{
    [SerializeField] private bool isDebugMode = false;
    [SerializeField] private GameObject bossObj;
    public GameObject BossObj { get => bossObj; }
    IBoss boss;
    MonoBehaviour bossMonobehaviour;
    bool isInitBoss;

    private void Awake()
    {
        if (isDebugMode) Debug.Log("BossMgrがAwakeされました");

        if (bossObj == null)
        {
            Debug.LogError("BossMgrです。ボスのオブジェクトをセットしてください。");
            //UnityEditor.EditorApplication.isPaused = true;
            return;
        }
        boss = bossObj.GetComponent<IBoss>();
        if (boss == null)
        {
            Debug.LogError("ボスのスクリプトがIBossを継承していません。");
            //UnityEditor.EditorApplication.isPaused = true;
            return;
        }
        bossMonobehaviour = boss as MonoBehaviour;
        if (bossMonobehaviour == null)
        {
            Debug.LogError("ボスのスクリプトがMonoBehaviourを継承していません。");
            //UnityEditor.EditorApplication.isPaused = true;
            return;
        }

        bossMonobehaviour.gameObject.SetActive(true);
        isInitBoss = false;
    }

    private void Start()
    {
        if (isDebugMode) Debug.Log("BossMgrがStartされました");

        if (isDebugMode)
        {
            WakeUp();
            return;
        }
        else
        {
            bossMonobehaviour.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        DeathPanelCtrl.onFinishDeathPanel += DestroyMe;
        ExplosionPanelCtrl.onFinishExplosionPanel += Sleep;
    }

    private void OnDisable()
    {
        DeathPanelCtrl.onFinishDeathPanel -= DestroyMe;
        ExplosionPanelCtrl.onFinishExplosionPanel -= Sleep;
    }

    //イベントハンドラー
    void DestroyMe()
    {
        Destroy(gameObject);
    }

    internal void WakeUp()
    {
        bossMonobehaviour.gameObject.SetActive(true);
        if (isDebugMode) Debug.LogAssertion("ボスが起きました");

        boss.WakeUp();
        isInitBoss = true;
    }

    void Sleep()
    {
        if (isDebugMode) Debug.LogAssertion("ボスが寝ました");
        bossMonobehaviour.gameObject.SetActive(false);
    }
}
