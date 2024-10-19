using System.Collections;
using System.Collections.Generic;
using PlayerAction;
using UnityEngine;

namespace KeyHandler
{
    public class InputHandler : MonoBehaviour
    {
        bool isAbleToInputKey = true;

        private void Awake()
        {
            ActionHandler.onPlayerDeath += DontAcceptInputCtrl;
            ActionHandler.onPlayerDamage += DontAcceptInputCtrl;
            ActionHandler.onPlayerDamageRecoverd += AcceptInputCtrl;
            AcceptInputCtrl();
        }

        void DontAcceptInputCtrl()
        {
            isAbleToInputKey = false;
        }

        void AcceptInputCtrl()
        {
            isAbleToInputKey = true;
        }

        public bool IsMoveLeftKey()
        {
            if (!isAbleToInputKey) return false;

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
            if (!isAbleToInputKey) return false;

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
            if (!isAbleToInputKey) return false;

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
            if (!isAbleToInputKey) return false;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                return true;
            }
            else
                return false;
        }

        public bool IsDashKeyDown()
        {
            if (!isAbleToInputKey) return false;

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                return true;
            }
            else
                return false;
        }

        public bool IsDashKey()
        {
            if (!isAbleToInputKey) return false;

            if (Input.GetKey(KeyCode.LeftShift)) return true;
            else return false;
        }

        public bool IsShootKeyDown()
        {
            if (!isAbleToInputKey) return false;

            if (Input.GetKeyDown(KeyCode.J)) return true;
            else return false;
        }

        public bool IsShootKeyUp()
        {
            if (!isAbleToInputKey) return false;

            if (Input.GetKeyUp(KeyCode.J)) return true;
            else return false;
        }
    }
}
