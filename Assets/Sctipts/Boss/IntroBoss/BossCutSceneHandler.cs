using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using IntroBossExperimenter;
using HPBar;
using System.Threading;
using UnityEditor;
using KeyHandler;
using UniRx;

public interface IBossCutSceneHandler
{
    void OnEnterBossRoom();
    UniTask OnStartCutScene();
    UniTask OnCompletedAppearance();
}

public class BossCutSceneHandler : MonoBehaviour, IBossCutSceneHandler
{
    private Subject<Unit> onCompletedBossCutScene = new Subject<Unit>();
    public IObservable<Unit> OnCompletedBossCutScene => onCompletedBossCutScene;

    public static event Action onApperanceCutSceneStart, onStartBossTalk, onInitHPBar, onStartBattle, onStartExplodeCutScene, onExplode;

    bool isEndTalkPhase = false, isFinishInitializeHPBar = false, isDeadBoss = false;

    BossMgr bossMgr;
    [SerializeField] private TalkSystem bossTalkSystem, endTalkSystem;
    [SerializeField] private GameObject hpBarPanelObj;
    [SerializeField] private GroundChk groundChkPlayer;
    [SerializeField] private Vector2 bossAppearPos;
    [SerializeField] float waitTime = 1.0f, startTalkWaitTime = 2.0f, battleStartWaitTime = 0.5f;
    [SerializeField] private InputHandler inputHandler;
    [SerializeField] private ExplosionPanelCtrl explosionPanelCtrl;

    CancellationTokenSource cts;

    private void Awake()
    {
        bossMgr = GetComponent<BossMgr>();
    }

    private void OnEnable()
    {
        CameraGoBossStageController.onEnterBossRoom += OnEnterBossRoom;
        BackGroundTalkPanelAnimCtrl.onHideBackGround += OnEndTalk;
        IntroBossHPBarHandler.onFinishInitializeHPBar += OnFinishInitializeHPBar;
        IntroBossHPBarHandler.onDead += OnDead;
        HPBarHandler.onPlayerDeath += OnPlayerDead;
    }

    private void OnDisable()
    {
        CameraGoBossStageController.onEnterBossRoom -= OnEnterBossRoom;
        BackGroundTalkPanelAnimCtrl.onHideBackGround -= OnEndTalk;
        IntroBossHPBarHandler.onFinishInitializeHPBar -= OnFinishInitializeHPBar;
        IntroBossHPBarHandler.onDead -= OnDead;
        HPBarHandler.onPlayerDeath -= OnPlayerDead;
    }

    //イベントハンドラー
    void OnPlayerDead()
    {
        if (cts != null)
        {
            cts?.Cancel();
            cts.Dispose();
            cts = null;
        }
    }

    //イベントハンドラー
    void OnEndTalk()
    {
        isEndTalkPhase = true;
    }

    //イベントハンドラー
    void OnDead()
    {
        isDeadBoss = true;
    }

    //イベントハンドラ
    //HPバーの初期化が終わったら呼び出されます
    void OnFinishInitializeHPBar()
    {
        isFinishInitializeHPBar = true;
    }

    //イベントハンドラー
    //インターフェース実装
    public async void OnEnterBossRoom()
    {
        //TODO:WARNING表示をあとで実装する

        //プレイヤーが地面につくまで待機する
        try
        {
            await UniTask.WaitUntil(() => groundChkPlayer.IsGround);
        }
        catch (Exception e)
        {
            Debug.LogError($"awaitでエラーが起きました。{e}");
            return;
        }

        //少し待機する
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(waitTime));
        }
        catch (Exception e)
        {
            Debug.LogError($"awaitでエラーが起きました。{e}");
            return;
        }

        OnStartCutScene().Forget();
    }

    //インターフェース実装
    public async UniTask OnCompletedAppearance()
    {
        //地面に着地したら登場完了とする

        //地面に着地するまで待つ
        try
        {
            await UniTask.WaitUntil(() => groundChkPlayer.IsGround);
        }
        catch (Exception e)
        {
            Debug.LogError($"awaitでエラーが起きました。{e}");
            return;
        }
    }

    public async UniTask OnStartCutScene()
    {
        //登場演出
        bossMgr.WakeUp();
        bossMgr.BossObj.gameObject.transform.position = bossAppearPos;
        //アニメーションをIdleで固定する
        onApperanceCutSceneStart?.Invoke();

        //登場が完了するまで待つ（地面に着地するまで）
        try
        {
            await OnCompletedAppearance();
        }
        catch (Exception e)
        {
            Debug.LogError($"awaitでエラーが起きました。{e}");
            return;
        }

        //登場したあとに少し待機する
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(startTalkWaitTime));
        }
        catch (Exception e)
        {
            Debug.LogError($"awaitでエラーが起きました。{e}");
            return;
        }

        //会話
        isEndTalkPhase = false;
        onStartBossTalk?.Invoke();
        bossTalkSystem.StartTalk();

        //会話が終了するまで待つ
        try
        {
            await UniTask.WaitUntil(() => isEndTalkPhase);
        }
        catch (Exception e)
        {
            Debug.LogError($"awaitでエラーが起きました。{e}");
            return;
        }

        //HPバー表示
        hpBarPanelObj.SetActive(true);
        onInitHPBar?.Invoke();

        //HPバーの初期化が終わるまで待つ
        try
        {
            await UniTask.WaitUntil(() => isFinishInitializeHPBar);
        }
        catch (Exception e)
        {
            Debug.LogError($"awaitでエラーが起きました。{e}");
            return;
        }

        //少しの沈黙のあとスタート
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(battleStartWaitTime));
        }
        catch (Exception e)
        {
            Debug.LogError($"awaitでエラーが起きました。{e}");
            return;
        }

        //戦闘開始
        Debug.Log("戦闘開始");
        inputHandler.EnableInput.OnNext(Unit.Default);
        onStartBattle?.Invoke();
        //BGMを再生
        BGMCtrl.onPlayBGM.OnNext(0);

        //ボスかプレイヤーが死ぬまで待機
        cts = new CancellationTokenSource();
        try
        {
            await UniTask.WaitUntil(() => isDeadBoss, cancellationToken: cts.Token);
        }
        catch (OperationCanceledException)
        {
            //戦闘中にプレイヤーが死んだらキャンセルします。イベントハンドラで登録しています
            Debug.Log("プレイヤーが死んだのキャンセルされました");
            return;
        }
        catch (Exception e)
        {
            Debug.LogError($"awaitでエラーが起きました。{e}");
            return;
        }
        cts.Dispose();
        cts = null;

        //ボスが死んだら爆発演出
        hpBarPanelObj.SetActive(false);
        onStartExplodeCutScene?.Invoke();
        //きーを無効にする
        inputHandler.DisableInput.OnNext(Unit.Default);
        //BGMを止める
        BGMCtrl.onStopBGM.OnNext(Unit.Default);

        //時間を止める
        try
        {
            //ignoreTimeScaleをtrueにすることで、Time.timeScaleが0のときでも待機できる
            await UniTask.Delay(TimeSpan.FromSeconds(1.0f), ignoreTimeScale: true);
        }
        catch (Exception e)
        {
            Debug.LogError($"awaitでエラーが起きました。{e}");
            return;
        }

        Debug.Log("爆発演出");
        //爆発演出が終わるまで待つ
        onExplode?.Invoke();
        try
        {
            await explosionPanelCtrl.OnCompletedExplosionDirection
                .First()
                .ToUniTask();
        }
        catch (OperationCanceledException)
        {
            //演出のスキップなどデバッグモードのときにいつかついかします
        }
        catch (Exception e)
        {
            Debug.Log($"awaitでエラーが起きました。{e}");
            //EditorApplication.isPaused = true;
            return;
        }

        //終わりの会話を表示する
        isEndTalkPhase = false;
        endTalkSystem.StartTalk();
        onStartBossTalk?.Invoke();

        //会話が終了するまで待つ
        try
        {
            await UniTask.WaitUntil(() => isEndTalkPhase);
        }
        catch (Exception e)
        {
            Debug.LogError($"awaitでエラーが起きました。{e}");
            return;
        }

        onCompletedBossCutScene.OnNext(Unit.Default);

        //ワープでThanks for playingを表示する(別のシーンに遷移する)
    }
}
