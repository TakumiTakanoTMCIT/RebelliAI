using System;
using ActionStatusChk;
using HPBar;
using UnityEngine;
using Zenject;
using UniRx;

namespace PlayerAction
{
    public class ActionHandler : IDisposable
    {
        Rigidbody2D rb;
        ActionStatusChecker actionStatusChecker;

        [Inject]
        PlayerStats playerStatus;
        PlayerDashKeepManager dashKeepManager;
        LifeManager lifeManager;

        public ActionHandler(Rigidbody2D rb, ActionStatusChecker actionStatusChecker, PlayerDashKeepManager dashKeepManager, LifeManager lifeManager)
        {
            this.rb = rb;
            this.actionStatusChecker = actionStatusChecker;
            this.dashKeepManager = dashKeepManager;
            this.lifeManager = lifeManager;

            lifeManager.OnPlayerDead.Subscribe(_ =>
            {
                OnPlayerDeath();
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
            }
            else
            {
                if (dashKeepManager.IsKeepDashSpeed)
                    rb.velocity = new Vector2(-playerStatus.DashSpeed, rb.velocity.y);
                else
                    rb.velocity = new Vector2(-playerStatus.MoveSpeed, rb.velocity.y);
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
        }

        public void Damage()
        {
            if (actionStatusChecker.Direction)
            {
                rb.AddForce(new Vector2(-playerStatus.damageForce.x, playerStatus.damageForce.y), ForceMode2D.Impulse);
            }
            else
            {
                rb.AddForce(new Vector2(playerStatus.damageForce.x, playerStatus.damageForce.y), ForceMode2D.Impulse);
            }
        }
    }
}
