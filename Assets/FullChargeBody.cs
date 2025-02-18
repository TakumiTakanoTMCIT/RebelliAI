using UnityEngine;
using ActionStatusChk;
using PlayerState;
using Zenject;
using UniRx;
using PlayerShot;
using PlayerFlip;

namespace PlayerShot
{
    public interface IChargeShot
    {
        void TakeDamage();
        void Refrect();
    }

    public abstract class ShellBase : MonoBehaviour, IChargeShot
    {
        [SerializeField]
        protected float speed;

        [Inject(Id = "Muzzle")]
        protected GameObject muzzleObj;

        protected Rigidbody2D rb;
        protected PlayerStateMgr playerStateMgr;
        protected IAnimatable animatorCtrl;

        [Inject]
        protected ActionStatusChecker actionStatusChecker;

        public abstract void DestroyShell();
        public abstract void End_BiginingAnim();
        protected abstract void OnBecameInvisible();
        public abstract void Refrect();

        protected void Awake()
        {
            playerStateMgr = GameObject.Find("Player").MyGetComponent_NullChker<PlayerStateMgr>();
            rb = gameObject.MyGetComponent_NullChker<Rigidbody2D>();

            CustomAwake();
        }

        //Awakeを継承先で少し内容を変えたい場合は、CustomAwakeを使う
        protected virtual void CustomAwake() { }

        //インターフェースの実装
        public abstract void TakeDamage();
    }

    public interface IAnimatable
    {
        void Construct(Animator animator);
        void StartAnim();
        void MoveAnim();
        void TakeDamage();
        void RefrectShell();
    }

    public abstract class ShellAnimCtrlBase : MonoBehaviour, IAnimatable
    {
        protected Animator animator;

        private void Awake()
        {
            animator = gameObject.MyGetComponent_NullChker<Animator>();
        }

        public abstract void Construct(Animator animator);
        public abstract void StartAnim();
        public abstract void MoveAnim();
        public abstract void TakeDamage();
        public abstract void RefrectShell();
    }

}

namespace FullCharge
{
    public class FullChargeBody : ShellBase
    {
        [SerializeField] private int myLevel = 1;

        HitBoxCtrl hitBoxCtrl;
        SetInitialPositionLogic initPositioner;
        StateCtrl stateCtrl;
        VisualCtrl visualCtrl;
        MoveLogic moveCtrl;
        IDirection playerDirection;

        [Inject]
        public void Construct([Inject(Id = "FullCharge")] IAnimatable animCtrl, HitBoxCtrl hitBoxCtrl, SetInitialPositionLogic initPositioner, StateCtrl stateCtrl, VisualCtrl visualCtrl, MoveLogic.Factory factory, IDirection playerDirection)
        {
            animatorCtrl = animCtrl;
            this.hitBoxCtrl = hitBoxCtrl;
            this.initPositioner = initPositioner;
            this.stateCtrl = stateCtrl;
            this.visualCtrl = visualCtrl;
            this.playerDirection = playerDirection;
            moveCtrl = factory.Create();
        }

        protected override void CustomAwake()
        {
            animatorCtrl.Construct(gameObject.MyGetComponent_NullChker<Animator>());
            gameObject.MyGetComponent_NullChker<ChargedShellDamageAbleFinder>().Construct(animatorCtrl);
            moveCtrl.GetBodyStats(rb, speed);

            hitBoxCtrl.GetHitBox(gameObject.MyGetComponent_NullChker<BoxCollider2D>());
            initPositioner.Init(muzzleObj.transform, transform);
            visualCtrl.GetPlayerStats(gameObject.MyGetComponent_NullChker<SpriteRenderer>(), playerDirection);
        }

        private void Start()
        {
            animatorCtrl.StartAnim();
            hitBoxCtrl.HitBoxStats(false);
            stateCtrl.SetStartedMove(false);

            //TODO : このコードはstateCtrlからサブスクライブするように変更しよう！それがUniRxの本領だと思う！
            //スタートアニメーション中はプレイヤーの銃口に追従する
            Observable.EveryUpdate()
                .Where(_ => !stateCtrl.IsStartedMove)
                .Subscribe(_ =>
                {
                    initPositioner.SetShellPositionToMuzzle();
                    visualCtrl.SetFlip();
                }).AddTo(this);
        }

        /// <summary>
        /// AnimationEventから呼び出される。アニメーションが終わったらシェルを動かす
        /// </summary>
        public override void End_BiginingAnim()
        {
            animatorCtrl.MoveAnim();
            hitBoxCtrl.HitBoxStats(true);
            stateCtrl.SetStartedMove(true);
            visualCtrl.SetFlip();
            gameObject.MyGetComponent_NullChker<ChargedShellDamageAbleFinder>().IsExtraDamage(playerStateMgr.WhatCurrentState(playerStateMgr.dashState));
            //発射音を再生
            SoundEffectCtrl.OnPlayShotSE.OnNext(myLevel);

            moveCtrl.MoveShell();
        }

        protected override void OnBecameInvisible()
        {
            if (!stateCtrl.IsStartedMove) return;
            DestroyShell();
        }

        public override void DestroyShell()
        {
            Destroy(this.gameObject);
        }

        //アニメーションイベント
        void FinishRefrectAnim()
        {
            DestroyShell();
        }

        //インターフェースの実装
        public override void TakeDamage()
        {
            moveCtrl.Stop();
        }

        public override void Refrect()
        {
            moveCtrl.Stop();
        }
    }

    public class MoveLogic
    {
        public class Factory : PlaceholderFactory<MoveLogic> { }

        private Rigidbody2D rb;
        private float speed;

        //Inject
        private IDirection playerDirection;

        //Inject
        public MoveLogic(IDirection playerDirection)
        {
            this.playerDirection = playerDirection;
        }

        public void GetBodyStats(Rigidbody2D rb, float speed)
        {
            this.rb = rb;
            this.speed = speed;
        }

        public void MoveShell()
        {
            //プレイヤーの向きに合わせて速度を決める
            if (playerDirection.Direction.Value)
            {
                rb.velocity = new Vector2(speed, 0);
            }
            else
            {
                rb.velocity = new Vector2(-speed, 0);
            }
        }

        public void Stop()
        {
            rb.velocity = Vector2.zero;
        }
    }

    public class HitBoxCtrl
    {
        private BoxCollider2D boxCollider2D;

        public void GetHitBox(BoxCollider2D boxCollider2D)
        {
            this.boxCollider2D = boxCollider2D;
        }

        public void HitBoxStats(bool stats)
        {
            boxCollider2D.enabled = stats;
        }
    }

    public class SetInitialPositionLogic
    {
        private Transform muzzleTrans;
        private Transform shellTrans;

        public void Init(Transform muzzleTrans, Transform shellTrans)
        {
            this.muzzleTrans = muzzleTrans;
            this.shellTrans = shellTrans;
        }

        public void SetShellPositionToMuzzle()
        {
            shellTrans.position = muzzleTrans.position;
        }
    }

    public class StateCtrl
    {
        public bool IsStartedMove { get; private set; }

        public void SetStartedMove(bool isStartedMove)
        {
            IsStartedMove = isStartedMove;
        }
    }

    public class VisualCtrl
    {
        private SpriteRenderer spriteRenderer;
        private IDirection playerDirection;

        public void GetPlayerStats(SpriteRenderer spriteRenderer, IDirection playerDirection)
        {
            this.spriteRenderer = spriteRenderer;
            this.playerDirection = playerDirection;
        }

        public void SetFlip()
        {
            spriteRenderer.flipX = !playerDirection.Direction.Value;
        }
    }
}
