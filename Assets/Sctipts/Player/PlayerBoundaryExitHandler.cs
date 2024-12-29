using UnityEngine;
using UniRx;
using HPBar;

public class PlayerBoundaryExitHandler : MonoBehaviour
{
    private HPBarHandler hPBarHandler;

    public void OtherComponentGetter(HPBarHandler hPBarHandler)
    {
        this.hPBarHandler = hPBarHandler;
    }

    //プレイヤーが画面外に出たら
    private void OnBecameInvisible()
    {
        hPBarHandler.onPlayerInVoid.OnNext(Unit.Default);
    }
}
