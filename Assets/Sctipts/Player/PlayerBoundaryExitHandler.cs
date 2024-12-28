using UnityEngine;
using UniRx;
using HPBar;

public class PlayerBoundaryExitHandler : MonoBehaviour
{
    [SerializeField] private HPBarHandler hPBarHandler;

    private void Start()
    {
        if (hPBarHandler == null)
        {
            Debug.LogWarning("HPBarHandler is not assigned!");
        }
    }

    //プレイヤーが画面外に出たら
    private void OnBecameInvisible()
    {
        hPBarHandler.onPlayerInVoid.OnNext(Unit.Default);
    }
}
