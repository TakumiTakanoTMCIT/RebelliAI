using System;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KeyHandler
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField]
        private bool isDebugMode = false;

        [SerializeField]
        bool isAbleToInputKey = true,
            isTalkKeyMode = false,
            isEnteredBossRoom = false;

        [SerializeField]
        private bool isWalkKey = false,
            isJumpKey = false,
            isDashKey = false,
            isAttackKey = false;

        //エディタに表示しない
        [HideInInspector] public bool IsAttackKey { get { return isAttackKey; } }

        [SerializeField]
        private bool walkDirection = true;

        private bool
            isjumpkeyDown = false,
            isjumingKeyNow = false,
            isdashKeydown = false,
            isAttackKeydown = false,
            isattackinKeyNow = false,
            isdashingKeyNow = false;

        public static event Action onAcceptInputCtrl;

        public Subject<Unit> EnableInput = new Subject<Unit>();
        public Subject<Unit> DisableInput = new Subject<Unit>();

        private void Awake()
        {
            DisableInput.Subscribe(_ => DontAcceptInputCtrl()).AddTo(this);
            EnableInput.Subscribe(_ => AcceptInputCtrl()).AddTo(this);
        }

        public void OnEnteredDoor()
        {
            isEnteredBossRoom = true;
        }

        //イベントハンドラ
        void TalkKeyMode()
        {
            isTalkKeyMode = true;
            if (isAbleToInputKey)
                DontAcceptInputCtrl();
        }

        void DontAcceptInputCtrl()
        {
            isAbleToInputKey = false;
        }

        void AcceptInputCtrl()
        {
            if (isEnteredBossRoom)
                return;
            isAbleToInputKey = true;
            onAcceptInputCtrl?.Invoke();
        }

        public static event Action onPauseKeyDown,
            onTalkKeyDown;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                onPauseKeyDown?.Invoke();
            }

            if (isDebugMode || isTalkKeyMode)
                IsTalkKey();

            isjumpkeyDown = isJumpKey;
            isJumpKey = false;

            isdashKeydown = isDashKey;
            isDashKey = false;

            isAttackKeydown = isAttackKey;
            isAttackKey = false;
        }

        void IsTalkKey()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("TalkKey");
                onTalkKeyDown?.Invoke();
            }
        }

        public bool IsMoveLeftKey()
        {
            if (!isAbleToInputKey)
                return false;

            if (!walkDirection)
            {
                //Debug.Log("Left key is pressed.");
                return true;
            }
            else
                return false;
        }

        public bool IsMoveRightKey()
        {
            if (!isAbleToInputKey)
                return false;

            if (walkDirection)
            {
                //Debug.Log("Right key is pressed.");
                return true;
            }
            else
                return false;
        }

        public bool IsMoveKey()
        {
            if (!isAbleToInputKey)
                return false;

            /// <summary>
            /// 同時押しは無効
            /// </summary>
            /*if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
                return false;*/

            /// <summary>
            /// どのキーも押されていない場合はfalse
            /// </summary>
            if (!isWalkKey)
                return false;
            else
                return true;
        }

        public bool IsJumpKeyDown()
        {
            if (!isAbleToInputKey)
                return false;

            return isjumpkeyDown;
        }

        public bool IsDashKeyDown()
        {
            if (!isAbleToInputKey)
                return false;

            return isdashKeydown;
        }

        public bool IsDashKey()
        {
            if (!isAbleToInputKey)
                return false;

            return isdashingKeyNow;
        }

        public bool IsShootKeyDown()
        {
            if (!isAbleToInputKey)
                return false;

            return isAttackKeydown;
        }

        public bool IsShootKey()
        {
            return isattackinKeyNow;
        }

        public bool IsShootKeyUp()
        {
            if (!isAbleToInputKey)
                return false;

            if (!isattackinKeyNow)
                return true;
            else
                return false;
        }

        /// <summary>
        /// InputSystemでUnityEventを使っています
        /// </summary>

        public void OnJump(InputAction.CallbackContext context)
        {
            if(context.performed)
            {
                isJumpKey = true;
            }
        }

        public void OnWalk(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                isWalkKey = true;

                if (context.ReadValue<float>() > 0)
                {
                    walkDirection = true;
                }
                else
                {
                    walkDirection = false;
                }
            }

            if (context.canceled)
            {
                isWalkKey = false;
            }
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                isDashKey = true;
                isdashingKeyNow = true;
            }

            if (context.canceled)
            {
                isdashingKeyNow = false;
            }
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                isAttackKey = true;
                isattackinKeyNow = true;
            }

            if(context.canceled)
            {
                isattackinKeyNow = false;
            }
        }
    }
}
