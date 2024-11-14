using UnityEngine;
using Cinemachine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System;
using UniRx;
using Cysharp.Threading.Tasks.Triggers;
using JetBrains.Annotations;

public class CameraGoBossStageController : MonoBehaviour
{
    [SerializeField] internal Vector2 bossSaveRoomCameraPos;
    [SerializeField] BossDoorCutSceneCtrl bossDoorCutSceneCtrl;
    [SerializeField] bool isDebugMode = false;
    CameraSwitcher cameraSwitcher;

    public static event Action onEnterBossRoom;

    private void Awake()
    {
        cameraSwitcher = GameObject.Find("GameMgr").gameObject.MyGetComponent_NullChker<CameraSwitcher>();
    }

    private async void OnEnable()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
    }

    public void SetCameraInBossRoom()
    {
        //カメラをボス部屋の位置に
        cameraSwitcher.SwitchCamera(CameraSwitcher.BOSSSAVEROOM);
        cameraSwitcher.virtualCameras[CameraSwitcher.BOSSSAVEROOM].transform.position = new Vector3(bossSaveRoomCameraPos.x, bossSaveRoomCameraPos.y, cameraSwitcher.virtualCameras[CameraSwitcher.BOSSSAVEROOM].transform.position.z);
    }

    //イベントハンドラー
    public async UniTask OpenBoseDoor(float targetPos, bool isBossDoor, Vector3 resetCameraPos)
    {
        Debug.Log("OpenBoseDoorイベント発火");
        cameraSwitcher.SwitchCamera(CameraSwitcher.BOSSSAVEROOM);
        Debug.Log($"CameraSwitch: {CameraSwitcher.BOSSSAVEROOM}");

        cameraSwitcher.virtualCameras[CameraSwitcher.BOSSSAVEROOM].transform.position = resetCameraPos;
        if (isDebugMode)
        {
            Debug.Log($"ボス部屋のカメラの位置: {cameraSwitcher.virtualCameras[CameraSwitcher.BOSSSAVEROOM].transform.position}");
            Debug.LogAssertion($"移動先の位置: {targetPos}");
        }

        Debug.Log("カメラ移動開始");
        await cameraSwitcher.virtualCameras[CameraSwitcher.BOSSSAVEROOM].transform.DOMoveX(targetPos, GamePlayerManager.CameraChangingSceneSpeed)
            .SetUpdate(true)
            .SetEase(Ease.Linear);

        //onCameraMovingFinish?.Invoke();
        bossDoorCutSceneCtrl.onFinishMoveCamera.OnNext(Unit.Default);
        if (isDebugMode) Debug.Log("カメラ移動終了");

        if (isBossDoor)
        {
            cameraSwitcher.SwitchCamera(CameraSwitcher.BOSSROOM);
            Debug.LogAssertion("onEnterBossRoomイベント発火");
            //onEnterBossRoom?.Invoke();
        }
    }
}
