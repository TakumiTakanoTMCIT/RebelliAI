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

            /// <summary>
            /// どのキーも押されていない場合はfalse
            /// </summary>
            if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                return false;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
                return true;
            else
                return false;
        }

        public bool IsJumpKeyDown()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                return true;
            }
            else
                return false;
        }

        public bool IsDashKeyDown()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                return true;
            }
            else
                return false;
        }

        public bool IsDashKey()
        {
            if (Input.GetKey(KeyCode.LeftShift)) return true;
            else return false;
        }

        public bool IsShootKeyDown()
        {
            if (Input.GetKeyDown(KeyCode.J)) return true;
            else return false;
        }

        public bool IsShootKeyUp()
        {
            if (Input.GetKeyUp(KeyCode.J)) return true;
            else return false;
        }
    }
}
