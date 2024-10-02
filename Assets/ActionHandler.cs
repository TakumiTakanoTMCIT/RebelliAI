using System.Collections;
using System.Collections.Generic;
using ActionStatusChk;
using PlayerInfo;
using UnityEditor;
using UnityEngine;

namespace PlayerAction
{
    public class ActionHandler : MonoBehaviour
    {
        Rigidbody2D rb;
        ActionStatusChecker actionStatusChecker;
        PlayerStatus status;

        void Awake()
        {
            rb = this.GetComponent<Rigidbody2D>();
            actionStatusChecker = this.GetComponent<ActionStatusChecker>();
            status = this.GetComponent<PlayerStatus>();
        }

        public void Stop()
        {
            if (rb == null)
            {
                Debug.LogError("Rigidbody2D が取得できていません。");
                EditorApplication.isPaused = true;
            }

            //Debug.Log("Stop");
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        public bool Walk(bool direction, float speed)
        {
            /// <summary>
            /// directionで左右を判定し、
            /// 壁にぶつかっていない場合のみ移動
            /// </summary>

            if (!actionStatusChecker.IsWall(direction))
            {
                if (direction)
                {
                    rb.velocity = new Vector2(speed, rb.velocity.y);
                }
                else
                {
                    rb.velocity = new Vector2(-speed, rb.velocity.y);
                }
                return true;
            }
            else
            {
                Stop();
                return false;
            }
        }

        public void Jump(float jumpForce)
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        }

        public void Dash(bool direction)
        {
            if(direction)
                rb.velocity = new Vector2(status.DashSpeed,rb.velocity.y);
            else
                rb.velocity = new Vector2(-status.DashSpeed,rb.velocity.y);
        }
    }
}
