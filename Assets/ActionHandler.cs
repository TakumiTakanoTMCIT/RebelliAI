using System;
using System.Collections;
using System.Collections.Generic;
using ActionStatusChk;
using Cysharp.Threading.Tasks;
using PlayerInfo;
using Door;
using UnityEngine;

namespace PlayerAction
{
    public class ActionHandler : MonoBehaviour
    {
        Rigidbody2D rb;
        ActionStatusChecker actionStatusChecker;
        PlayerStatus status;
        PlayerDashKeepManager dashKeepManager;

        public void Init(Rigidbody2D rb, ActionStatusChecker actionStatusChecker, PlayerStatus status, PlayerDashKeepManager dashKeepManager)
        {
            LateInit(rb, actionStatusChecker, status, dashKeepManager);
        }

        //待ってから初期化
        async void LateInit(Rigidbody2D rb, ActionStatusChecker actionStatusChecker, PlayerStatus status, PlayerDashKeepManager dashKeepManager)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.02f));

            this.rb = rb;
            this.actionStatusChecker = actionStatusChecker;
            this.status = status;
            this.dashKeepManager = dashKeepManager;
        }

        public void Stop()
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
                Stop();
                return false;
            }

            if (direction)
            {
                if (dashKeepManager.IsKeepDashSpeed)
                    rb.velocity = new Vector2(status.DashSpeed, rb.velocity.y);
                else
                    rb.velocity = new Vector2(status.MoveSpeed, rb.velocity.y);
            }
            else
            {
                if (dashKeepManager.IsKeepDashSpeed)
                    rb.velocity = new Vector2(-status.DashSpeed, rb.velocity.y);
                else
                    rb.velocity = new Vector2(-status.MoveSpeed, rb.velocity.y);
            }
            return true;
        }

        public void Jump(float jumpForce)
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        }

        public void WallFall()
        {
            rb.velocity = new Vector2(0, -status.WallFallSpeed);
        }

        public void Dash(bool direction)
        {
            if (direction)
                rb.velocity = new Vector2(status.DashSpeed, rb.velocity.y);
            else
                rb.velocity = new Vector2(-status.DashSpeed, rb.velocity.y);
        }

        public void Damage()
        {
            if (actionStatusChecker.Direction)
            {
                rb.AddForce(new Vector2(-status.damageForce.x, status.damageForce.y), ForceMode2D.Impulse);
            }
            else
            {
                rb.AddForce(new Vector2(status.damageForce.x, status.damageForce.y), ForceMode2D.Impulse);
            }
        }
    }
}
