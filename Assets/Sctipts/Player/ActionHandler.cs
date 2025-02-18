using System;
using ActionStatusChk;
using UnityEngine;
using Zenject;
using UniRx;

namespace PlayerAction
{
    public interface IActionHandlerSubject
    {
        IObservable<bool> OnWalk { get; }
        IObservable<bool> OnDash { get; }
    }

    public class ActionHandler : IDisposable , IActionHandlerSubject
    {
        //Inject
        Rigidbody2D rb;
        ActionStatusChecker actionStatusChecker;
        private readonly IPlayerDirection playerDirection;
        private readonly PlayerDashKeepManager dashKeepManager;
        private readonly PlayerStats playerStatus;

        private Subject<bool> walkSubject = new Subject<bool>();
        public IObservable<bool> OnWalk => walkSubject;

        private Subject<bool> dashSubject = new Subject<bool>();
        public IObservable<bool> OnDash => dashSubject;

        public ActionHandler(Rigidbody2D rb, ActionStatusChecker actionStatusChecker, PlayerDashKeepManager dashKeepManager, LifeManager lifeManager, IPlayerDirection playerDirection, PlayerStats playerStatus, EventStreamer eventStreamer)
        {
            this.rb = rb;
            this.actionStatusChecker = actionStatusChecker;
            this.dashKeepManager = dashKeepManager;
            this.playerDirection = playerDirection;
            this.playerStatus = playerStatus;

            lifeManager.OnPlayerDead.Subscribe(_ =>
            {
                OnPlayerDeath();
            })
            .AddTo(lifeManager);

            eventStreamer.finishBossDoorCutScene.Subscribe(_ =>
            {
                StopX();
                StopY();
            })
            .AddTo(lifeManager);
        }

        //PlayerStateMgrが破棄されたときに呼ばれる
        public void Dispose()
        {
        }

        private void OnPlayerDeath()
        {
            StopX();
            StopY();
            DisableGravity();
        }

        //TODO : Gravityの処理は、GravityHandlerに移動したほうが良いと思う！できたらやろう！！
        public void EnableGravity()
        {
            if (rb == null) return;
            rb.gravityScale = playerStatus.DefaultGravity;
        }
        public void DisableGravity()
        {
            if (rb == null) return;
            rb.gravityScale = 0;
        }
        //--------------------------------------------------------------------------------

        public void StopX()
        {
            //いい対処法ではないので、後で確実に修正
            if (rb == null) return;
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        public void StopY()
        {
            //いい対処法ではないので、後で確実に修正
            if (rb == null) return;
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }

        public bool Walk(bool direction)
        {
            /// <summary>
            /// directionで左右を判定し、
            /// 壁にぶつかっていない場合のみ移動
            /// </summary>

            if (actionStatusChecker.IsWall(direction))
            {
                StopX();
                return false;
            }

            if (direction)
            {
                if (dashKeepManager.IsKeepDashSpeed)
                    rb.velocity = new Vector2(playerStatus.DashSpeed, rb.velocity.y);
                else
                    rb.velocity = new Vector2(playerStatus.MoveSpeed, rb.velocity.y);

                walkSubject?.OnNext(true);
            }
            else
            {
                if (dashKeepManager.IsKeepDashSpeed)
                    rb.velocity = new Vector2(-playerStatus.DashSpeed, rb.velocity.y);
                else
                    rb.velocity = new Vector2(-playerStatus.MoveSpeed, rb.velocity.y);

                walkSubject?.OnNext(false);
            }
            return true;
        }

        public void Jump()
        {
            rb.AddForce(new Vector2(0, playerStatus.JumpForce), ForceMode2D.Impulse);
        }

        public void WallFall()
        {
            rb.velocity = new Vector2(0, -playerStatus.WallFallSpeed);
        }

        public void Dash(bool direction)
        {
            if (direction)
                rb.velocity = new Vector2(playerStatus.DashSpeed, rb.velocity.y);
            else
                rb.velocity = new Vector2(-playerStatus.DashSpeed, rb.velocity.y);

            dashSubject?.OnNext(direction);
        }

        public void Damage()
        {
            //プレイヤーの向いている向きの逆側に吹っ飛ばす
            if (!playerDirection.Direction.Value)
            {
                rb.AddForce(new Vector2(playerStatus.damageForce.x, playerStatus.damageForce.y), ForceMode2D.Impulse);
            }
            else
            {
                rb.AddForce(new Vector2(-playerStatus.damageForce.x, playerStatus.damageForce.y), ForceMode2D.Impulse);
            }
        }
    }
}
