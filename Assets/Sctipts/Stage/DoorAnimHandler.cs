using System;
using UnityEngine;
using UniRx;
public class DoorAnimHandler : MonoBehaviour
{
    public static event Action onDoorOpened, onDoorClosed;

    public Subject<Unit> onDoorOpenedSubject = new Subject<Unit>();
    public Subject<Unit> onDoorClosedSubject = new Subject<Unit>();

    Animator animator;
    BossDoorBody doorBody;

    private void Awake()
    {
        doorBody = gameObject.MyGetComponent_NullChker<BossDoorBody>();
        animator = gameObject.MyGetComponent_NullChker<Animator>();
    }

    /*private void OnEnable()
    {
        BossDoorBody.onDoorTouched += OnOpenDoorStart;
        CameraGoBossStageController.onCameraMovingFinish += OnClosingDoorStart;
    }

    private void OnDisable()
    {
        BossDoorBody.onDoorTouched -= OnOpenDoorStart;
        CameraGoBossStageController.onCameraMovingFinish -= OnClosingDoorStart;
    }*/

    //イベントハンドラー
    public void OnOpenDoorStart()
    {
        /*if (doorBody.IsEnterDoor) return;
        if (!doorBody.IsTouchDoor) return;*/
        animator.SetBool("isOpen", true);
        DoorSoundCtrl.onPlayDoorOpenSE.OnNext(Unit.Default);
    }

    //イベントハンドラー
    public void OnClosingDoorStart()
    {
        //if (!doorBody.IsTouchDoor) return;
        animator.SetBool("isOpen", false);
        DoorSoundCtrl.onPlayDoorCloseSE.OnNext(Unit.Default);
    }

    //アニメーションイベント
    void OnDoorOpeningAnimFinish()
    {
        onDoorOpenedSubject.OnNext(Unit.Default);
        //TODO:このif文は必要か検討
        if (doorBody.IsEnterDoor) return;
        //onDoorOpened?.Invoke();
    }

    //アニメーションイベント
    void OnDoorClosingAnimFinish()
    {
        onDoorClosedSubject.OnNext(Unit.Default);
        animator.SetTrigger("onClose");
        Debug.LogAssertion("onDoorClosedイベント発火");
        //onDoorClosed?.Invoke();
    }
}
