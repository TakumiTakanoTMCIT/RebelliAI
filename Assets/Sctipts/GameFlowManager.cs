using UnityEngine;
using UniRx;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using KeyHandler;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    [SerializeField] bool isDebugMode = false;
    [SerializeField] InputHandler inputHandler;
    [SerializeField] PlayerAnimCutSceneCtrl playerAnimCutSceneCtrl;
    [SerializeField] PlayerAnimStateHandler playerAnimStateHandler;
    [SerializeField] GamePlayerManager gamePlayerManager;
    [SerializeField] GameStartTerminalCtrl gameStartTerminalCtrl;
    [SerializeField] BossCutSceneHandler bossCutSceneHandler;
    [SerializeField] SavePointHandler savePointHandler;
    [SerializeField] CameraGoBossStageController cameraGoBossStageController;
    [SerializeField] WarpDirection warpDirection;
    [SerializeField] Parallax parallax;

    public static Subject<Unit> onCompletedShowStandbyTerminal = new Subject<Unit>();
    public static Subject<Unit> onCompletedPlayerWarpIn = new Subject<Unit>();
    public static Subject<Unit> StartBattleAction = new Subject<Unit>();
    private Subject<Unit> onCompletedEscapeAnim = new Subject<Unit>();
    public IObserver<Unit> OnCompletedEscapeAnim => onCompletedEscapeAnim;

    CancellationTokenSource cts;

    private void Start()
    {
        StartDirection().Forget();
    }

    private async UniTask StartDirection()
    {
        //デバッグモードは即座にアクションスタートさせる機能がある
        //ですが、初期化は行う（重要、ですが初期化はAwakeで行っているのでおそらく大丈夫）
        if (isDebugMode)
        {
            //キーの受付を開始
            //アクションスタート
            inputHandler.EnableInput.OnNext(Unit.Default);
            StartBattleAction.OnNext(Unit.Default);
            Blaster.DisplayCtrl.OnDisplayBlaster.OnNext(Unit.Default);
            gamePlayerManager.isInGameArea = true;
            return;
        }
        //プレイヤーの初期化
        gamePlayerManager.isInGameArea = false;
        //プレイヤーのポジションをセーブポジションに変更
        bool isBossSaveRoom = savePointHandler.SetPlayerOnStagePositionSavePoint();
        //セーブポイントがボス部屋ならカメラをボス部屋の位置にする
        if (isBossSaveRoom)
        {
            cameraGoBossStageController.SetCameraInBossRoom();
        }

        //プレイヤーをワープ登場演出まで非表示にして、操作不可能にする。
        gamePlayerManager.DeactivePlayer.OnNext(Unit.Default);
        inputHandler.DisableInput.OnNext(Unit.Default);
        if (isDebugMode) Debug.LogAssertion("プレイヤーをワープ登場演出まで非表示にして、操作不可能にする。");

        //スタンバイターミナルを表示
        gameStartTerminalCtrl.onStartStandbyTerminal.OnNext(Unit.Default);

        //背景を正しい位置にする
        parallax.Init();

        //表示完了するまで待機
        try
        {
            //キャンセルトークンの生成
            cts = new CancellationTokenSource();

            await onCompletedShowStandbyTerminal
            .First()
            .ToUniTask(cancellationToken: cts.Token);
        }
        catch (OperationCanceledException)
        {
            Debug.LogError("awaitがキャンセルされました。");
            return;
        }
        catch (Exception e)
        {
            Debug.LogError($"awaitでエラーが起きました。{e}");
            return;
        }

        //Debug.LogAssertion("スタンバイターミナルを表示完了");
        //BGMを再生
        BGMCtrl.onPlayBGM.OnNext(0);

        //ワープエフェクトを表示し、終わるまで待機
        warpDirection.StartWarpDirection().Forget();
        try
        {
            await warpDirection.OnCompletedWarpEffect
                .First()
                .ToUniTask();
        }
        catch (Exception e)
        {
            Debug.Log($"awaitでエラーが起きました。{e}");
            //EditorApplication.isPaused = true;
            return;
        }

        //プレイヤーがワープで登場
        gamePlayerManager.ActivePlayer.OnNext(Unit.Default);
        await UniTask.Yield(PlayerLoopTiming.Update);
        playerAnimCutSceneCtrl.PlayerWarpInDirection.OnNext(Unit.Default);
        //Debug.LogAssertion("プレイヤーがワープで登場");
        //ブラスターも表示
        Blaster.DisplayCtrl.OnDisplayBlaster.OnNext(Unit.Default);

        //登場完了するまで待機
        try
        {
            await onCompletedPlayerWarpIn
                .First()
                .ToUniTask(cancellationToken: cts.Token);
        }
        catch (OperationCanceledException)
        {
            Debug.LogError("awaitがキャンセルされました。");
            return;
        }
        catch (Exception e)
        {
            Debug.LogError($"awaitでエラーが起きました。{e}");
            return;
        }

        //プレイヤーをオンステージ
        gamePlayerManager.isInGameArea = true;
        //アクションスタート
        StartBattleAction.OnNext(Unit.Default);
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
        //キーの受付を開始
        inputHandler.EnableInput.OnNext(Unit.Default);
        //Debug.LogAssertion("キーの受付を開始、アクションスタート");
        //プレイヤーをonStageにする

        //ボスの処理はBossCutSceneHandlerでやる

        //ボス戦が終わるまで待機
        try
        {
            await bossCutSceneHandler.OnCompletedBossCutScene
                .First()
                .ToUniTask();
        }
        catch (Exception e)
        {
            Debug.Log($"awaitでエラーが起きました。{e}");
            //EditorApplication.isPaused = true;
            return;
        }

        //ワープエフェクトを表示し、終わるまで待機
        try
        {
            await warpDirection.StartWarpDirection();
        }
        catch (Exception e)
        {
            Debug.Log($"awaitでエラーが起きました。{e}");
            //EditorApplication.isPaused = true;
            return;
        }
        playerAnimStateHandler.ChangeAnimState(playerAnimStateHandler.warpEscapeState);

        //エスケープアニメーションが終わるまで待機
        try
        {
            onCompletedEscapeAnim.Subscribe(_ =>
            {
                gamePlayerManager.DeactivePlayer.OnNext(Unit.Default);
            });
        }
        catch (Exception e)
        {
            Debug.Log($"awaitでエラーが起きました。{e}");
            //EditorApplication.isPaused = true;
            return;
        }

        await UniTask.Delay(TimeSpan.FromSeconds(3.0f));

        //パネルが表示されるまで待機
        Debug.Log("パネルが表示されるまで待機");
        ClearPanelCtrl.showPanel.OnNext(Unit.Default);
        await ClearPanelCtrl.onCompletedShowPanel
            .First()
            .ToUniTask();

        //クリアシーンに遷移
        SceneManager.LoadScene("StageSelect");
    }
}
