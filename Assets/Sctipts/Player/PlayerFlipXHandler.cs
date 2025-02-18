using UnityEngine;
using Zenject;
using UniRx;
using PlayerAction;
using PlayerState;

namespace PlayerFlip
{
    /// <summary>
    /// このクラスは、プレイヤーの向きを変更するロジックを提供するクラスです。
    ///　インターフェースを介して使用してください
    /// </summary>
    public interface ILogic
    {
        void FlipX(bool direction);
        void Reverse();
    }

    /// <summary>
    /// このインターフェースを介して、プレイヤーの向きを取得してください。
    /// </summary>
    public interface IDirection
    {
        IReadOnlyReactiveProperty<bool> Direction { get; }
    }

    /// <summary>
    /// このクラスは、プレイヤーの向きを変更するロジックを提供するクラスです。
    /// </summary>
    public class Logic : IDirection, ILogic
    {
        private ReactiveProperty<bool> _direction = new ReactiveProperty<bool>(true);

        //インターフェースを介して使用してください
        public IReadOnlyReactiveProperty<bool> Direction => _direction;

        //[Inject] Examplerr examplerr;

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
    /// ZenjectでAsSingleとして動かします。
    /// </summary>
    public class Handler
    {
        public Handler(ILogic playerFlipXLogic, IActionHandlerSubject actionHandler, IWallFallSubject wallFallSubject, DisposableMgr _disposableMgr)
        {
            actionHandler.OnWalk.Subscribe(direction =>
            {
                playerFlipXLogic.FlipX(direction);
            })
            .AddTo(_disposableMgr.disposables);

            wallFallSubject.OnEnteredWallFall.Subscribe(_ =>
            {
                playerFlipXLogic.Reverse();
            })
            .AddTo(_disposableMgr.disposables);

            wallFallSubject.OnExitWallFall.Subscribe(_ =>
            {
                playerFlipXLogic.Reverse();
            })
            .AddTo(_disposableMgr.disposables);

            actionHandler.OnDash.Subscribe(direction =>
            {
                playerFlipXLogic.FlipX(direction);
            })
            .AddTo(_disposableMgr.disposables);
        }
    }
}
