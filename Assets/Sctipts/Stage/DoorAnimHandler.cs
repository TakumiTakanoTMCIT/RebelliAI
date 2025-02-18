using UnityEngine;
using UniRx;
using Zenject;

public class DoorAnimHandler : MonoBehaviour
{
    public Subject<Unit> onDoorOpenedSubject = new Subject<Unit>();
    public Subject<Unit> onDoorClosedSubject = new Subject<Unit>();

    Animator animator;

    EventStreamer eventStreamer;

    [Inject]
    public void Construct(EventStreamer eventStreamer)
    {
        this.eventStreamer = eventStreamer;
    }

    private void Awake()
    {
        animator = gameObject.MyGetComponent_NullChker<Animator>();
    }

    //イベントハンドラー
    public void OnOpenDoorStart()
    {
        animator.SetBool("isOpen", true);
        DoorSoundCtrl.onPlayDoorOpenSE.OnNext(Unit.Default);
    }

    //イベントハンドラー
    public void OnClosingDoorStart()
    {
        animator.SetBool("isOpen", false);
        DoorSoundCtrl.onPlayDoorCloseSE.OnNext(Unit.Default);
    }

    //アニメーションイベント
    void OnDoorOpeningAnimFinish()
    {
        onDoorOpenedSubject.OnNext(Unit.Default);
    }

    //アニメーションイベント
    void OnDoorClosingAnimFinish()
    {
        onDoorClosedSubject.OnNext(Unit.Default);
        animator.SetTrigger("onClose");
        Debug.LogAssertion("onDoorClosedイベント発火");
    }
}
