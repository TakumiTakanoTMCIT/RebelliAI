using UnityEngine;
using UniRx;

public class PlayerAnimCutSceneCtrl : MonoBehaviour
{
    [SerializeField] PlayerAnimStateHandler animStateHandler;
    public Subject<Unit> PlayerWarpInDirection = new Subject<Unit>();

    private void Awake()
    {
        PlayerWarpInDirection.Subscribe(_ =>
        {
            Debug.Log("PlayerWarpInDirectionが呼ばれました");
            animStateHandler.ChangeAnimState(animStateHandler.warpState);
        })
        .AddTo(this);
    }
}
