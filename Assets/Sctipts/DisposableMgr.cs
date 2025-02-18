using UnityEngine;
using UniRx;

/// <summary>
/// このクラスは、ピュアクラスが破棄されたとときに、本来ならばDisposeを明示的に実行できるように書かないといけないけど、このクラスのインスタンスを渡してあげれば、自動でDisposeを実行してくれるようにするためのクラスです。
/// </summary>
public class DisposableMgr : MonoBehaviour
{
    public readonly CompositeDisposable disposables = new CompositeDisposable();

    private void OnDestroy()
    {
        disposables.Dispose();
    }
}
