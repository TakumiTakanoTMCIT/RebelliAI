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

        internal bool isShootKey = false;

        public static event Action onAcceptInputCtrl;

        public Subject<Unit> EnableInput = new Subject<Unit>();
        public Subject<Unit> DisableInput = new Subject<Unit>();

        private void Awake()
        {
            DisableInput.Subscribe(_ => DontAcceptInputCtrl()).AddTo(this);
            EnableInput.Subscribe(_ => AcceptInputCtrl()).AddTo(this);
        }

        /*private void Start()
        {
            AcceptInputCtrl();
        }*/

        /*private void OnEnable()
        {
            GamePlayerManager.onPlayerOffStage += DontAcceptInputCtrl;
            GamePlayerManager.onPlayerOnStage += AcceptInputCtrl;

            HPBarHandler.onPlayerDeath += DontAcceptInputCtrl;

            HPBarHandler.onPlayerDamage += DontAcceptInputCtrl;
            PlayerState.DamageState.onPlayerDamageRecover += AcceptInputCtrl;

            BossDoorBody.onDoorTouched += DontAcceptInputCtrl;
            DoorAnimHandler.onDoorClosed += AcceptInputCtrl;

            BossCutSceneHandler.onStartBossTalk += TalkKeyMode;

            BossCutSceneHandler.onStartBattle += BattleStart;

            IntroBossHPBarHandler.onDead += DontAcceptInputCtrl;
        }

        //イベントの登録解除
        private void OnDisable()
        {
            GamePlayerManager.onPlayerOffStage -= DontAcceptInputCtrl;
            GamePlayerManager.onPlayerOnStage -= AcceptInputCtrl;

            HPBarHandler.onPlayerDeath -= DontAcceptInputCtrl;

            HPBarHandler.onPlayerDamage -= DontAcceptInputCtrl;
            PlayerState.DamageState.onPlayerDamageRecover -= AcceptInputCtrl;

            BossDoorBody.onDoorTouched -= DontAcceptInputCtrl;
            DoorAnimHandler.onDoorClosed -= AcceptInputCtrl;

            BossCutSceneHandler.onStartBossTalk -= TalkKeyMode;

            BossCutSceneHandler.onStartBattle -= BattleStart;

            IntroBossHPBarHandler.onDead -= DontAcceptInputCtrl;
        }*/

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

        void BattleStart()
        {
            isAbleToInputKey = true;
            isEnteredBossRoom = false;
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
            if (!isAbleToInputKey)
                return false;

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
            if (!isAbleToInputKey)
                return false;

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
            if (!isAbleToInputKey)
                return false;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                return true;
            }
            else
                return false;
        }

        public bool IsDashKeyDown()
        {
            if (!isAbleToInputKey)
                return false;

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                return true;
            }
            else
                return false;
        }

        public bool IsDashKey()
        {
            if (!isAbleToInputKey)
                return false;

            if (Input.GetKey(KeyCode.LeftShift))
                return true;
            else
                return false;
        }

        public bool IsShootKeyDown()
        {
            if (!isAbleToInputKey)
                return false;

            if (Input.GetKeyDown(KeyCode.J))
                return true;
            else
                return false;
        }

        public bool IsShootKey()
        {
            //if (!isAbleToInputKey) return false;

            if (Input.GetKey(KeyCode.J))
                return true;
            else
                return false;
        }

        public bool IsShootKeyUp()
        {
            if (!isAbleToInputKey)
                return false;

            if (Input.GetKeyUp(KeyCode.J))
                return true;
            else
                return false;
        }

        /// <summary>
        /// InputSystemでUnityEventを使っています
        /// </summary>

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                Debug.Log("Experiment");
            }
        }

        public void OnWalk(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                Debug.Log("Walk");
            }

            if (context.performed)
            {
                Debug.Log("Walking : " + context.ReadValue<float>());
            }

            if (context.canceled)
            {
                Debug.Log("Stop Walk");
            }
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                Debug.Log("Dash");
            }
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                Debug.Log("Attack");
            }
        }
    }
}
