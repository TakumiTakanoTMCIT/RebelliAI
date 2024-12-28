using ActionStatusChk;
using UnityEngine;
using Zenject;

namespace PlayerAction
{
    public class ActionHandler
    {
        Rigidbody2D rb;
        ActionStatusChecker actionStatusChecker;

        [Inject]
        PlayerStats playerStatus;
        PlayerDashKeepManager dashKeepManager;

        public ActionHandler(Rigidbody2D rb, ActionStatusChecker actionStatusChecker, PlayerDashKeepManager dashKeepManager)
        {
            this.rb = rb;
            this.actionStatusChecker = actionStatusChecker;
            this.dashKeepManager = dashKeepManager;
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

        public void Jump(float jumpForce)
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
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
