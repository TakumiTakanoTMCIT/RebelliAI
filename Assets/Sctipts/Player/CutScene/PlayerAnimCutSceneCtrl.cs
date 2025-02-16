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
            animStateHandler.ChangeAnimState(animStateHandler.warpState);
        })
        .AddTo(this);
    }
}
