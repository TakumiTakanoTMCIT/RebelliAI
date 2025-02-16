using UnityEngine;
using Zenject;
using UniRx;
using PlayerAction;
using PlayerState;

/// <summary>
/// このクラスは、プレイヤーの向きを変更するロジックを提供するクラスです。
///　インターフェースを介して使用してください
/// </summary>
public interface IPlayerFlipXLogic
{
    void FlipX(bool direction);
    void Reverse();
}

/// <summary>
/// このインターフェースを介して、プレイヤーの向きを取得してください。
/// </summary>
public interface IPlayerDirection
{
    IReadOnlyReactiveProperty<bool> Direction { get; }
}

/// <summary>
/// このクラスは、プレイヤーの向きを変更するロジックを提供するクラスです。
/// </summary>
public class PlayerDirectionLogic : IPlayerDirection, IPlayerFlipXLogic
{
    private ReactiveProperty<bool> _direction = new ReactiveProperty<bool>(false);

    //インターフェースを介して使用してください
    public IReadOnlyReactiveProperty<bool> Direction => _direction;

    public void FlipX(bool direction)
    {
        _direction.Value = direction;
    }

    public void Reverse()
    {
        _direction.Value = !_direction.Value;
    }
}

/// <summary>
/// logicのハンドラーを提供するクラスです。
/// </summary>
public class PlayerFlipXHandler : MonoBehaviour
{
    //Inject
    IPlayerFlipXLogic playerFlipXLogic;
    IActionHandlerSubject actionHandler;
    IWallFallSubject wallFallSubject;
    IDamageStateSubject damageStateSubject;

    [Inject]
    public void Construct(IPlayerFlipXLogic playerFlipXLogic, IActionHandlerSubject actionHandler, IWallFallSubject wallFallSubject, IDamageStateSubject damageStateSubject)
    {
        this.playerFlipXLogic = playerFlipXLogic;
        this.actionHandler = actionHandler;
        this.wallFallSubject = wallFallSubject;
        this.damageStateSubject = damageStateSubject;
    }

    private void Awake()
    {
        actionHandler.OnWalk.Subscribe(direction =>
        {
            playerFlipXLogic.FlipX(direction);
        })
        .AddTo(this);

        wallFallSubject.OnEnteredWallFall.Subscribe(_ =>
        {
            playerFlipXLogic.Reverse();
        })
        .AddTo(this);

        wallFallSubject.OnExitWallFall.Subscribe(_ =>
        {
            playerFlipXLogic.Reverse();
        })
        .AddTo(this);

        actionHandler.OnDash.Subscribe(direction =>
        {
            playerFlipXLogic.FlipX(direction);
        })
        .AddTo(this);
    }
}
