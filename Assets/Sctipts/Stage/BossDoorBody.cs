using System;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using KeyHandler;
using UniRx;


public class BossDoorBody : MonoBehaviour
{
    DoorAnimHandler doorAnimHandler;
    [SerializeField] internal GameObject playerObj, stageObj, cameraObj;
    [SerializeField] internal Vector2 playerTeleportationPos, cameraPos;
    InputHandler inputHandler;
    [SerializeField] internal bool isBossDoor = false, isDebugMode = false;
    [SerializeField] float canEnterDistance = 0.8f;
    public static event Action onDoorTouched;

    public static event Action<float, bool, Vector3> onCameraStartMove;

    [SerializeField] bool isTouchDoor = false;
    [SerializeField] public bool isEnterDoor = false;

    BossDoorCutSceneCtrl bossDoorCutSceneCtrl;
    internal CameraGoBossStageController cameraGoBossStageController;
    public bool IsTouchDoor => isTouchDoor;
    public bool IsEnterDoor => isEnterDoor;

    BoxCollider2D boxCollider2D;

    private void Awake()
    {
        bossDoorCutSceneCtrl = GameObject.Find("CutSceneManagers").MyGetComponent_NullChker<BossDoorCutSceneCtrl>();
        cameraGoBossStageController = GameObject.Find("GameMgr").MyGetComponent_NullChker<CameraGoBossStageController>();
        doorAnimHandler = gameObject.MyGetComponent_NullChker<DoorAnimHandler>();
        inputHandler = playerObj.MyGetComponent_NullChker<InputHandler>();
        boxCollider2D = gameObject.MyGetComponent_NullChker<BoxCollider2D>();
        boxCollider2D.isTrigger = true;

        gameObject.tag = "Untagged";

        isEnterDoor = false;
        isTouchDoor = false;
    }

    private void OnEnable()
    {
        DoorAnimHandler.onDoorOpened += OnDoorOpened;
        DoorAnimHandler.onDoorClosed += OnEnteredDoor;
    }

    private void OnDisable()
    {
        DoorAnimHandler.onDoorOpened -= OnDoorOpened;
        DoorAnimHandler.onDoorClosed -= OnEnteredDoor;
    }

    //イベントハンドラー
    void OnDoorOpened()
    {
        //TODO:このif文は必要か検討
        if (!isTouchDoor || isEnterDoor) return;

        if (isDebugMode)
        {
            Debug.LogAssertion($"DoorBodyからみた移動先の位置: {cameraPos.x}");
            Debug.LogWarning($"俺は : {gameObject.name}");
        }

        onCameraStartMove?.Invoke(cameraPos.x, isBossDoor, cameraObj.transform.position);
        if (isEnterDoor) return;
        isEnterDoor = true;
        GoToNextRoom().Forget();
    }

    //イベントハンドラー
    void OnEnteredDoor()
    {
        if (!isEnterDoor) return;

        boxCollider2D.isTrigger = false;
        gameObject.tag = "Ground";
        transform.SetParent(stageObj.transform);
    }

    public void BeEnterdDoor()
    {
        boxCollider2D.isTrigger = false;
        gameObject.tag = "Ground";
        transform.SetParent(stageObj.transform);
    }

    //デバッグです
    private void Update()
    {
        if (isTouchDoor) return;
        if (isDebugMode) Debug.Log($"プレイヤーとの距離: {Vector2.Distance(playerObj.transform.position, transform.position)}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTouchDoor) return;

        if (other.gameObject.name == playerObj.name)
        {
            if (isEnterDoor) return;
            if (Vector2.Distance(playerObj.transform.position, transform.position) > canEnterDistance) return;
            bossDoorCutSceneCtrl.onStartBossDoorCutScene.OnNext(doorAnimHandler);
            isEnterDoor = true;
            return;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isTouchDoor) return;

        if (other.gameObject.name == playerObj.name)
        {
            if (isEnterDoor) return;
            if (Vector2.Distance(playerObj.transform.position, transform.position) > canEnterDistance) return;
            bossDoorCutSceneCtrl.onStartBossDoorCutScene.OnNext(doorAnimHandler);
            isEnterDoor = true;
            return;
        }
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
