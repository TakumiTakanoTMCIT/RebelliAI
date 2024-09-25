using System.Collections;
using System.Collections.Generic;
using ActionStatusChk;
using UnityEditor;
using UnityEngine;

namespace PlayerAction
{
    public class ActionHandler : MonoBehaviour
    {
        Rigidbody2D rb;
        ActionStatusChecker actionStatusChecker;

        void Awake()
        {
            rb = this.GetComponent<Rigidbody2D>();
            actionStatusChecker = this.GetComponent<ActionStatusChecker>();
        }

        public void Stop()
        {
            if (rb == null)
            {
                Debug.LogError("Rigidbody2D が取得できていません。");
                EditorApplication.isPaused = true;
            }
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
    }
}
