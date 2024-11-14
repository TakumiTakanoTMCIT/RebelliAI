using System;
using UnityEngine;

namespace IntroBossExperimenter
{
    public class IntroBossExperimentAnimCtrl : MonoBehaviour
    {
        public static event Action onEndAttackAnim;
        public static event Action<bool> onChangeDirection;

        IAnimState currentState;
        internal IAnimState idleState, walkState, attackState, deathState;

        internal Animator animator;
        SpriteRenderer spriteRenderer;

        private void Awake()
        {
            animator = gameObject.MyGetComponent_NullChker<Animator>();
            spriteRenderer = gameObject.MyGetComponent_NullChker<SpriteRenderer>();
        }

        private void Update()
        {
            if (animator == null) Debug.LogWarning($"animatior: {animator}");
        }

        private void OnEnable()
        {
            IntroBossExperiment.onWakeUp += WakeUp;
        }

        private void OnDisable()
        {
            IntroBossExperiment.onWakeUp -= WakeUp;
        }

        //イベントハンドラー
        void WakeUp()
        {
            Debug.Log("AnimCtrlのステートが初期化される");
            idleState = new IdleAnimState(animator, this);
            walkState = new WalkAnimState(animator, this);
            attackState = new AttackAnimState(animator, this);
            deathState = new DeathAnimState(animator, this);

            Debug.Log("AnimStateにおいてcurrentStateが設定され、OnEnterが呼ばれる");
            currentState = idleState;
            currentState.OnEnter();
        }

        //アニメーションイベント
        private void OnEndAttackAnim()
        {
            onEndAttackAnim?.Invoke();
        }

        public void ChangeAnimState(IAnimState nextState)
        {
            if (currentState == null)
            {
                Debug.LogError("currentStateがnullです。");
                //UnityEditor.EditorApplication.isPaused = true;
                return;
            }

            if (currentState == nextState) return;



            currentState.OnExit();
            currentState = nextState;
            nextState.OnEnter();
        }

        internal void GetAnimtor()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
                if (animator == null)
                {
                    Debug.LogError("Animatorがnullです。");
                    //UnityEditor.EditorApplication.isPaused = true;
                }
            }
        }

        public void SetWalkDirection(bool direction)
        {
            spriteRenderer.flipX = direction;
            onChangeDirection?.Invoke(direction);
        }

        private void OnDestroy()
        {
            if (currentState != null)
            {
                currentState.OnExit();
                currentState = null;
            }

            onEndAttackAnim = null;
            animator = null;
            spriteRenderer = null;

            idleState = null;
            walkState = null;
            attackState = null;
        }
    }

    public interface IAnimState
    {
        void OnEnter();
        void OnExit();
    }

    public class IdleAnimState : IAnimState
    {
        IntroBossExperimentAnimCtrl animStateCtrl;

        public IdleAnimState(Animator animCtrl, IntroBossExperimentAnimCtrl animStateCtrl)
        {
            this.animStateCtrl = animStateCtrl;
        }

        public void OnEnter()
        {
            Debug.Log($"Idleに遷移しました！animatorは:{animStateCtrl.animator}です");
            animStateCtrl.GetAnimtor();
            animStateCtrl.animator.SetBool("isIdle", true);
        }

        public void OnExit()
        {
            Debug.Log($"IdleからExitします！animatorは:{animStateCtrl.animator}です");
            animStateCtrl.GetAnimtor();
            animStateCtrl.animator.SetBool("isIdle", false);
        }
    }

    public class WalkAnimState : IAnimState
    {
        IntroBossExperimentAnimCtrl animstateCtrl;

        public WalkAnimState(Animator animCtrl, IntroBossExperimentAnimCtrl animstateCtrl)
        {
            this.animstateCtrl = animstateCtrl;
        }

        public void OnEnter()
        {
            animstateCtrl.animator.SetBool("isWalk", true);
        }

        public void OnExit()
        {
            animstateCtrl.animator.SetBool("isWalk", false);
        }
    }

    public class AttackAnimState : IAnimState
    {
        IntroBossExperimentAnimCtrl animstateCtrl;

        public AttackAnimState(Animator animCtrl, IntroBossExperimentAnimCtrl animstateCtrl)
        {
            this.animstateCtrl = animstateCtrl;
        }

        public void OnEnter()
        {
            animstateCtrl.animator.SetBool("isAttack", true);
        }

        public void OnExit()
        {
            animstateCtrl.animator.SetBool("isAttack", false);
        }
    }

    public class DeathAnimState : IAnimState
    {
        IntroBossExperimentAnimCtrl animstateCtrl;

        public DeathAnimState(Animator animCtrl, IntroBossExperimentAnimCtrl animstateCtrl)
        {
            this.animstateCtrl = animstateCtrl;
        }

        public void OnEnter()
        {
            animstateCtrl.animator.speed = 0;
        }

        public void OnExit() { }
    }
}
