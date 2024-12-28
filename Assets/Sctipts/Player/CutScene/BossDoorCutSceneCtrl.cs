using UnityEngine;
using UniRx;
using KeyHandler;
using Cysharp.Threading.Tasks;
using System;
using Door;
using PlayerAction;
using DG.Tweening;
using Zenject;

namespace Door
{
    public class BossDoorCutSceneCtrl : MonoBehaviour
    {
        [SerializeField] InputHandler inputHandler;
        [SerializeField] GamePlayerManager gamePlayerManager;
        [SerializeField] BossCutSceneHandler bossCutSceneHandler;
        [SerializeField] PlayerAnimStateHandler playerAnimStateHandler;
        [Inject]
        ActionHandler actionHandler;

        public IObserver<DoorAnimHandler> OnStartBossDoorCutScene => onStartBossDoorCutScene;
        private Subject<DoorAnimHandler> onStartBossDoorCutScene = new Subject<DoorAnimHandler>();
        public Subject<Unit> onFinishMoveCamera = new Subject<Unit>();

        Door.PlayerCtrl playerCtrl;

        private void Awake()
        {
            onStartBossDoorCutScene.Subscribe(doorAnimHandler =>
            {
                BossDoorAutoScroll(doorAnimHandler).Forget();
            })
            .AddTo(this);
        }

        //BossDoorBodyから呼び出される
        public void GetPlayerCtrlInfo(PlayerCtrl playerCtrl)
        {
            this.playerCtrl = playerCtrl;
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
            playerCtrl.GoToNextRoom().Forget();
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

            bossDoorBody.OnDoorFlowComplete();

            //時間を再開する
            gamePlayerManager.EnableTime.OnNext(Unit.Default);
            //プレイヤーの速度を停止させる
            actionHandler.StopX();
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

    public class PlayerCtrl
    {
        private GameObject playerObj;
        private Vector2 playerTeleportationPos;
        private bool isDebugMode;

        public PlayerCtrl(GameObject playerObj, Vector2 playerTeleportationPos, bool isDebugMode)
        {
            this.playerObj = playerObj;
            this.playerTeleportationPos = playerTeleportationPos;
            this.isDebugMode = isDebugMode;
        }

        public async UniTask GoToNextRoom()
        {
            if (isDebugMode) Debug.Log("プレイヤーが次の部屋に移動します");

            try
            {
                await playerObj.transform.DOLocalMoveX(playerTeleportationPos.x, GamePlayerManager.CameraChangingSceneSpeed)
                .SetUpdate(true)
                .SetEase(Ease.Linear);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
