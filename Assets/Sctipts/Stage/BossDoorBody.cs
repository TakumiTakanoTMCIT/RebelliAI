using UnityEngine;

namespace Door
{
    public class BossDoorBody : MonoBehaviour
    {
        [SerializeField] private DoorManager doorManager;
        [SerializeField] public string doorID;
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

        private bool isOpened = false;

        private void Awake()
        {
            bossDoorCutSceneCtrl = GameObject.Find("CutSceneManagers").MyGetComponent_NullChker<BossDoorCutSceneCtrl>();
            cameraGoBossStageController = GameObject.Find("GameMgr").MyGetComponent_NullChker<CameraGoBossStageController>();
            doorAnimHandler = gameObject.MyGetComponent_NullChker<DoorAnimHandler>();
            boxCollider2D = gameObject.MyGetComponent_NullChker<BoxCollider2D>();

            //nullチェック
            if (doorManager == null) { Debug.LogError("DoorManagerが設定されていません"); }

            //自分のドアが開いていたかどうかを確認
            isOpened = doorManager.GetDoorState(doorID);

            if (isOpened)
            {
                boxCollider2D.isTrigger = false;
                gameObject.tag = "Ground";
                transform.SetParent(stageObj.transform);
            }
            else
            {
                boxCollider2D.isTrigger = true;
                gameObject.tag = "Door";
                transform.SetParent(null);
            }
        }

        public void OnDoorFlowComplete()
        {
            //ドアが開いたことを記録
            //もしボス部屋のドアなら記録しない
            if (!isBossDoor) doorManager.RegisterDoor(doorID);

            boxCollider2D.isTrigger = false;
            gameObject.tag = "Ground";
            transform.SetParent(stageObj.transform);
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
            if (isOpened) return;

            //プレイヤーがドアに触れた場合
            if (other.gameObject.name == playerObj.name)
            {
                //プレイヤーとの距離が一定以上の場合は処理を行わない。つまり近くないと行わない
                //演出のためです
                if (Vector2.Distance(playerObj.transform.position, transform.position) > canEnterDistance) return;

                //カットシーンコントローラーにプレイヤーの情報を渡す
                playerCtrl = new PlayerCtrl(playerObj, playerTeleportationPos, isDebugMode);
                bossDoorCutSceneCtrl.GetPlayerCtrlInfo(playerCtrl);

                //カットシーンを再生
                bossDoorCutSceneCtrl.OnStartBossDoorCutScene.OnNext(doorAnimHandler);

                boxCollider2D.isTrigger = false;
                gameObject.tag = "Ground";
                transform.SetParent(stageObj.transform);
            }
        }
    }
}
