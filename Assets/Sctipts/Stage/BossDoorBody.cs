using UniRx;
using UnityEngine;
using Zenject;
using System;

namespace Door
{
    public class BossDoorBody : MonoBehaviour
    {
        DoorAnimHandler doorAnimHandler;
        [SerializeField] internal GameObject playerObj, stageObj, cameraObj;
        [SerializeField] internal Vector2 playerTeleportationPos, cameraPos;

        [SerializeField] internal bool isBossDoor = false;
        [SerializeField] static bool isDebugMode = false;
        const float canEnterDistance = 0.65f;

        BossDoorCutSceneCtrl bossDoorCutSceneCtrl;
        internal CameraGoBossStageController cameraGoBossStageController;

        PlayerCtrl playerCtrl;

        BoxCollider2D boxCollider2D;
        StateHandler stateHandler;
        DoorIDAssignerHandler idLogic;

        //Inject
        ColliderLogic colliderLogic;
        TagLogic tagLogic;
        ParentSetter parentSetter;
        DoorManager doorManager;
        EventStreamer eventStreamer;

        private Subject<Unit> onEnteredDoor = new Subject<Unit>();
        private Subject<Unit> onExitedDoor = new Subject<Unit>();

        public Subject<Unit> onFinishBossDoorCutScene = new Subject<Unit>();

        [Inject]
        public void Construct(DoorManager doorManager, ColliderLogic colliderLogic, TagLogic tagLogic, ParentSetter parentSetter, EventStreamer eventStreamer)
        {
            this.doorManager = doorManager;
            this.colliderLogic = colliderLogic;
            this.tagLogic = tagLogic;
            this.parentSetter = parentSetter;
            this.eventStreamer = eventStreamer;
        }

        private void Awake()
        {
            bossDoorCutSceneCtrl = GameObject.Find("CutSceneManagers").MyGetComponent_NullChker<BossDoorCutSceneCtrl>();
            cameraGoBossStageController = GameObject.Find("GameMgr").MyGetComponent_NullChker<CameraGoBossStageController>();
            doorAnimHandler = gameObject.MyGetComponent_NullChker<DoorAnimHandler>();
            boxCollider2D = gameObject.MyGetComponent_NullChker<BoxCollider2D>();
            stateHandler = gameObject.MyGetComponent_NullChker<StateHandler>();
            idLogic = gameObject.MyGetComponent_NullChker<DoorIDAssignerHandler>();

            //nullチェック
            if (doorManager == null) Debug.LogError("DoorManagerが設定されていません");
            if (stateHandler == null) Debug.LogError("StateHandlerが設定されていません");
            if (idLogic == null) Debug.LogError("DoorIDAssignerHandlerが設定されていません");

            colliderLogic.Init(boxCollider2D);
            tagLogic.Init(gameObject);
            parentSetter.Init(gameObject);

            /// <summary>
            /// ドアに入った時の処理
            /// </summary>
            onEnteredDoor.Subscribe(_ =>
            {
                colliderLogic.DisableCollider();
                tagLogic.SetTag("Ground");
                parentSetter.SetParent(stageObj);
            })
            .AddTo(this);

            /// <summary>
            /// ドアの外にいるときの処理
            /// </summary>
            onExitedDoor.Subscribe(_ =>
            {
                colliderLogic.EnableCollider();
                tagLogic.SetTag("Door");
                parentSetter.SetParent(null);
            })
            .AddTo(this);

            onFinishBossDoorCutScene.Subscribe(_ =>
            {
                onEnteredDoor.OnNext(Unit.Default);
            })
            .AddTo(this);
        }

        private void Start()
        {
            if (stateHandler.IsEntered)
            {
                onEnteredDoor.OnNext(Unit.Default);
            }
            else
            {
                onExitedDoor.OnNext(Unit.Default);
            }
        }

        //デバッグです
        private void Update()
        {
            if (isDebugMode) Debug.Log($"プレイヤーとの距離: {Vector2.Distance(playerObj.transform.position, transform.position)}");
        }

        //プレイヤーがドアに触れた場合の処理です
        private void OnTriggerEnter2D(Collider2D other) => OnTriggerAction(other);
        private void OnTriggerStay2D(Collider2D other) => OnTriggerAction(other);
        private void OnTriggerAction(Collider2D other)
        {
            //このドアがすでに開いている場合は処理を行わない
            if (stateHandler.IsEntered) return;

            //プレイヤーがドアに触れた場合
            if (other.gameObject.name == playerObj.name)
            {
                //プレイヤーとの距離が一定以上の場合は処理を行わない。つまり近くないと行わない
                //演出のためです
                if (Vector2.Distance(playerObj.transform.position, transform.position) > canEnterDistance) return;

                //ドアが開いたことを記録
                //もしボス部屋のドアなら記録しない
                if (!isBossDoor)
                {
                    doorManager.RegisterDoor(idLogic.DoorID);
                }
                else
                {
                    //ボス部屋のセーブポイントを設定
                    eventStreamer.saveBossRoomSavePoint.OnNext(Unit.Default);
                }

                //TODO : Zenjectでインスタンスの生成を行ったほうがいいかもしれない
                //カットシーンコントローラーにプレイヤーの情報を渡す
                playerCtrl = new PlayerCtrl(playerObj, playerTeleportationPos, isDebugMode);
                bossDoorCutSceneCtrl.GetPlayerCtrlInfo(playerCtrl);
                bossDoorCutSceneCtrl.GetDoorAnimHandler(doorAnimHandler);

                //カットシーンを再生
                eventStreamer.startBossDoorCutScene.OnNext(Unit.Default);
            }
        }
    }

    public class ColliderLogic
    {
        BoxCollider2D boxCollider2D;

        public void Init(BoxCollider2D boxCollider)
        {
            boxCollider2D = boxCollider;
        }

        public void EnableCollider()
        {
            boxCollider2D.isTrigger = true;
        }

        public void DisableCollider()
        {
            boxCollider2D.isTrigger = false;
        }
    }

    public class TagLogic
    {
        GameObject gameObject;

        public void Init(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }

        public void SetTag(string tag)
        {
            gameObject.tag = tag;
        }
    }

    public class ParentSetter
    {
        GameObject gameObject;

        public void Init(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }

        public void SetParent(GameObject parent)
        {
            if (parent == null)
            {
                gameObject.transform.SetParent(null);
                return;
            }

            gameObject.transform.SetParent(parent.transform);
        }
    }
}
