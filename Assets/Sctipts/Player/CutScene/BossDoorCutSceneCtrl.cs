using UnityEngine;
using UniRx;
using KeyHandler;
using Cysharp.Threading.Tasks;
using System;
using UnityEditor;
using PlayerAction;

public class BossDoorCutSceneCtrl : MonoBehaviour
{
    [SerializeField] InputHandler inputHandler;
    [SerializeField] GamePlayerManager gamePlayerManager;
    [SerializeField] BossCutSceneHandler bossCutSceneHandler;
    [SerializeField] ActionHandler actionHandler;
    [SerializeField] PlayerAnimStateHandler playerAnimStateHandler;

    public Subject<DoorAnimHandler> onStartBossDoorCutScene = new Subject<DoorAnimHandler>();
    public Subject<Unit> onFinishMoveCamera = new Subject<Unit>();

    private void Awake()
    {
        onStartBossDoorCutScene.Subscribe(doorAnimHandler =>
        {
            BossDoorAutoScroll(doorAnimHandler).Forget();
        })
        .AddTo(this);
    }

    private async UniTask BossDoorAutoScroll(DoorAnimHandler doorAnimHandler)
    {
        Debug.Log("ボス部屋のドアに触れました");
        BossDoorBody bossDoorBody = doorAnimHandler.GetComponent<BossDoorBody>();

        //時間を止める
        gamePlayerManager.PauseTime.OnNext(Unit.Default);
        //操作を受け付けない
        inputHandler.DisableInput.OnNext(Unit.Default);
        //アニメーションを変更不可能にする
        playerAnimStateHandler.OnEnterDoor.OnNext(Unit.Default);

        //ドアの開けるアニメーションを再生
        Debug.Log("ドアの開けるアニメーションを再生");
        doorAnimHandler.OnOpenDoorStart();

        //ドアが開くまで待つ
        Debug.Log("ドアが開くまで待つ");
        try
        {
            await doorAnimHandler.onDoorOpenedSubject
                .First()
                .ToUniTask();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            //EditorApplication.isPaused = true;
            return;
        }

        Debug.Log("ドアが開きました");

        if (bossDoorBody.isBossDoor)
        {
            //音楽を止める
            BGMCtrl.onStopBGM.OnNext(Unit.Default);
        }
        //プレイヤーを移動させる
        bossDoorBody.GoToNextRoom().Forget();
        Debug.Log("カメラを移動させる");
        //カメラを移動させる
        await bossDoorBody.cameraGoBossStageController.OpenBoseDoor(bossDoorBody.cameraPos.x, bossDoorBody.isBossDoor, bossDoorBody.cameraObj.transform.position);

        Debug.Log("カメラ移動完了!!!!!");
        //ドアの閉めるアニメーションを再生
        doorAnimHandler.OnClosingDoorStart();

        //ドアが閉まるまで待つ
        try
        {
            await doorAnimHandler.onDoorClosedSubject
                .First()
                .ToUniTask();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            //EditorApplication.isPaused = true;
            return;
        }

        bossDoorBody.BeEnterdDoor();
        bossDoorBody.isEnterDoor = true;

        //時間を再開する
        gamePlayerManager.EnableTime.OnNext(Unit.Default);
        //プレイヤーの速度を停止させる
        actionHandler.Stop();
        actionHandler.StopY();
        //アニメーションを変更可能にする
        playerAnimStateHandler.OnExitDoor.OnNext(Unit.Default);

        //ボス部屋のドアでないなら
        if (!bossDoorBody.isBossDoor)
        {
            //操作を受け付ける
            Debug.Log("操作を受け付ける");
            inputHandler.EnableInput.OnNext(Unit.Default);
        }
        else
        {
            //ボス部屋のカットシーンを再生
            bossCutSceneHandler.OnStartCutScene().Forget();
        }
    }
}
