using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeyHandler
{
    public class InputHandler : MonoBehaviour
    {
        public bool IsMoveLeftKey()
        {
            if (Input.GetKey(KeyCode.A))
            {
                //Debug.Log("Left key is pressed.");
                return true;
            }
            else
                return false;
        }

        public bool IsMoveRightKey()
        {
            if (Input.GetKey(KeyCode.D))
            {
                //Debug.Log("Right key is pressed.");
                return true;
            }
            else
                return false;
        }

        public bool IsMoveKey()
        {
            /// <summary>
            /// 同時押しは無効
            /// </summary>
            if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
                return false;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {
                //Debug.Log("Left or Right key is pressed.");
                return true;
            }
            else
                return false;
        }

        public bool IsJumpKeyDown()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                return true;
            }
            else
                return false;
        }
    }
}
