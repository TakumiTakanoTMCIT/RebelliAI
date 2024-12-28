using UniRx;
using PlayerInfo;
using UnityEngine;
using ActionStatusChk;
using PlayerState;

namespace PlayerShot
{
    public interface IChargeShot
    {
        void TakeDamage();
        void Refrect();
    }

    public interface IDestroyable
    {
        void DestroyShell();
    }

    public abstract class ShellBase : MonoBehaviour, IDestroyable, IChargeShot
    {
        [SerializeField]
        protected float speed;

        protected bool isStartedMove;

        protected Rigidbody2D rb;
        protected SpriteRenderer spriteRenderer;
        protected PlayerStatus playerStatus;
        protected PlayerStateMgr playerStateMgr;
        protected IAnimatable animatorCtrl;
        protected Transform playerPos;
        protected ActionStatusChecker actionStatusChecker;

        public abstract void DestroyShell();
        public abstract void End_BiginingAnim();
        protected abstract void OnBecameInvisible();
        public abstract void StopMove();
        public abstract void Refrect();
        protected abstract void CustomMoveShell();

        protected void Awake()
        {
            actionStatusChecker = GameObject.Find("Player").MyGetComponent_NullChker<ActionStatusChecker>();
            playerStatus = GameObject.Find("Player").MyGetComponent_NullChker<PlayerStatus>();
            playerStateMgr = GameObject.Find("Player").MyGetComponent_NullChker<PlayerStateMgr>();
            rb = gameObject.MyGetComponent_NullChker<Rigidbody2D>();
            spriteRenderer = gameObject.MyGetComponent_NullChker<SpriteRenderer>();
            animatorCtrl = GetComponent<IAnimatable>();
            playerPos = playerStatus.transform;

            isStartedMove = false;

            CustomAwake();
        }

        protected void MoveShell()
        {
            //ダッシュ中はダメージが増加する
            gameObject.MyGetComponent_NullChker<ChargedShellDamageAbleFinder>().IsExtraDamage(playerStateMgr.WhatCurrentState(playerStateMgr.dashState));

            //プレイヤーの向きに合わせて速度を決める
            if (actionStatusChecker.Direction)
            {
                rb.velocity = new Vector2(speed, 0);
            }
            else
            {
                rb.velocity = new Vector2(-speed, 0);
            }

            //プレイヤーの向きに合わせて画像を反転させる
            spriteRenderer.flipX = !actionStatusChecker.Direction;

            //移動開始のフラグを立てる
            isStartedMove = true;

            CustomMoveShell();
        }

        //Awakeを継承先で少し内容を変えたい場合は、CustomAwakeを使う
        protected virtual void CustomAwake() { }

        //インターフェースの実装
        public abstract void TakeDamage();
    }

    public interface IAnimatable
    {
        void StartAnim();
        void MoveAnim();
        void TakeDamage();
        void RefrectShell();
    }

    public abstract class ShellAnimCtrlBase : MonoBehaviour , IAnimatable
    {
        private void Awake()
        {
            animator = gameObject.MyGetComponent_NullChker<Animator>();
        }

        protected Animator animator;

        public abstract void StartAnim();
        public abstract void MoveAnim();
        public abstract void TakeDamage();
        public abstract void RefrectShell();
    }

    //--ここから具象クラスの実装をしていきます--

    public class FullChargeBody : ShellBase
    {
        [SerializeField] private int myLevel = 1;

        BoxCollider2D boxCollider2D;

        protected override void CustomAwake()
        {
            boxCollider2D = gameObject.MyGetComponent_NullChker<BoxCollider2D>();
        }

        private void Start()
        {
            animatorCtrl.StartAnim();
            boxCollider2D.enabled = false;
            isStartedMove = false;
        }

        private void Update()
        {
            //スタートアニメーション中はプレイヤーの銃口に追従する
            if (isStartedMove) return;
            transform.position = playerStatus.transform.position;
        }

        /// <summary>
        /// AnimationEventから呼び出される。アニメーションが終わったらシェルを動かす
        /// </summary>
        public override void End_BiginingAnim()
        {
            animatorCtrl.MoveAnim();
            boxCollider2D.enabled = true;
            MoveShell();
        }

        protected override void CustomMoveShell()
        {
            //発射音を再生
            SoundEffectCtrl.OnPlayShotSE.OnNext(myLevel);
        }

        protected override void OnBecameInvisible()
        {
            if (!isStartedMove) return;
            DestroyShell();
        }

        public override void DestroyShell()
        {
            Destroy(this.gameObject);
        }

        public override void StopMove()
        {
            rb.velocity = Vector2.zero;
        }

        //アニメーションイベント
        void FinishRefrectAnim()
        {
            DestroyShell();
        }

        //インターフェースの実装
        public override void TakeDamage()
        {
            StopMove();
        }

        public override void Refrect()
        {
            StopMove();
        }
    }

    public class FullChargeBodyAnim : ShellAnimCtrlBase
    {
        FullChargeBody bodyCtrl;

        public FullChargeBodyAnim(Animator animator, FullChargeBody bodyCtrl)
        {
            this.animator = animator;
            this.bodyCtrl = bodyCtrl;
        }

        public override void StartAnim()
        {
            animator.SetTrigger("onReset");
        }

        public override void MoveAnim()
        {
            animator.SetTrigger("onFinishBigin");
        }

        public override void TakeDamage()
        {
            bodyCtrl.StopMove();
            animator.SetTrigger("isHit");
        }

        public override void RefrectShell()
        {
            bodyCtrl.StopMove();
            animator.SetTrigger("isRefrect");
        }
    }
}
