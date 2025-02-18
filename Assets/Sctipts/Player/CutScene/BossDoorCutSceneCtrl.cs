using UnityEngine;
using UniRx;
using KeyHandler;
using Cysharp.Threading.Tasks;
using System;
using Door;
using PlayerAction;
using DG.Tweening;
using Zenject;
using IntroBossExperimenter;

namespace Door
{
    public class BossDoorCutSceneCtrl : MonoBehaviour
    {
        [SerializeField] InputHandler inputHandler;
        [SerializeField] GamePlayerManager gamePlayerManager;
        [SerializeField] BossCutSceneHandler bossCutSceneHandler;
        [SerializeField] PlayerAnimStateHandler playerAnimStateHandler;

        //Inject
        ActionHandler actionHandler;
        EventStreamer eventStreamer;

        public Subject<Unit> onFinishMoveCamera = new Subject<Unit>();

        Door.PlayerCtrl playerCtrl;
        DoorAnimHandler doorAnimHandler;

        [Inject]
        public void Construct(EventStreamer eventStreamer, ActionHandler actionHandler)
        {
            this.actionHandler = actionHandler;
            this.eventStreamer = eventStreamer;
        }

        private void Awake()
        {
            eventStreamer.startBossDoorCutScene.Subscribe(_ =>
            {
                BossDoorAutoScroll(this.doorAnimHandler).Forget();
            })
            .AddTo(this);
        }

        //BossDoorBodyから呼び出される
        public void GetPlayerCtrlInfo(PlayerCtrl playerCtrl)
        {
            this.playerCtrl = playerCtrl;
        }

        public void GetDoorAnimHandler(DoorAnimHandler doorAnimHandler)
        {
            this.doorAnimHandler = doorAnimHandler;
        }

        private async UniTask BossDoorAutoScroll(DoorAnimHandler doorAnimHandler)
        {
            Debug.Log("ボス部屋のドアに触れました");
            BossDoorBody bossDoorBody = doorAnimHandler.GetComponent<BossDoorBody>();

            //時間を止める
            //操作を受け付けない
            //アニメーションを変更不可能にする
            //ドアの開けるアニメーションを再生

            //なぜイベントで行わないのかというと、”どのドア”のアニメーションを再生するかを明確にしたいためです。
            doorAnimHandler.OnOpenDoorStart();

            //ドアが開くまで待つ
            Debug.Log("ドアが開くまで待つ");
            try
            {
                //　イベントを受け取るまで待機します
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

            eventStreamer.finishBossDoorCutScene.OnNext(Unit.Default);
            //時間を再開する
            //プレイヤーの速度を停止させる
            //アニメーションを変更可能にする

            //なんでこれだけ直接イベントを使っているのかというと、”この””bossDoorBodyのイベントを実行することを明確にしたいからです。
            bossDoorBody.onFinishBossDoorCutScene.OnNext(Unit.Default);

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
