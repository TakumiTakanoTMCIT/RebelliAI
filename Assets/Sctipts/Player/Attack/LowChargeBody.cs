using UnityEngine;
using PlayerShot;
using Zenject;
using ActionStatusChk;
using UniRx;

namespace LowChargeShot
{
    public class LowChargeBody : ShellBase
    {
        [SerializeField] private int myLevel = 1;

        //Inject
        MoveCtrl moveCtrl;
        VisualCtrl visualCtrl;
        InitPositioner initPositioner;
        HitBoxCtrl hitBoxCtrl;
        StateCtrl stateCtrl;

        [Inject]
        public void Construct(MoveCtrl moveCtrl, VisualCtrl visualCtrl, InitPositioner initPositioner, HitBoxCtrl hitBoxCtrl, StateCtrl stateCtrl)
        {
            this.moveCtrl = moveCtrl;
            this.visualCtrl = visualCtrl;
            this.initPositioner = initPositioner;
            this.hitBoxCtrl = hitBoxCtrl;
            this.stateCtrl = stateCtrl;
        }

        protected override void CustomAwake()
        {
            moveCtrl.GetShellStats(rb, speed);
            visualCtrl.GetPlayerStats(spriteRenderer, actionStatusChecker);
            initPositioner.GetShellStats(muzzleObj, transform);
            hitBoxCtrl.GetBoxCollider2D(gameObject.MyGetComponent_NullChker<BoxCollider2D>());
        }

        protected override void CustomStart()
        {
            hitBoxCtrl.SetActive(false);
            stateCtrl.Stop();

            //移動を始めていなかったらプレイヤーの位置に合わせる
            Observable.EveryUpdate()
                .Where(_ => !stateCtrl.IsStarted)
                .Subscribe(_ =>
                {
                    initPositioner.SetMuzzlePositoin();
                })
                .AddTo(this);
        }

        public override void End_BiginingAnim()
        {
            hitBoxCtrl.SetActive(true);
            stateCtrl.Start();

            animatorCtrl.MoveAnim();
            moveCtrl.Move();
            visualCtrl.SetFlip();
            SoundEffectCtrl.OnPlayShotSE.OnNext(myLevel);
        }

        public override void DestroyShell()
        {
            if (!gameObject.activeSelf) return;

            Debug.LogError("LowChargeBody DestroyShell");
            Destroy(gameObject);
        }

        protected override void OnBecameInvisible()
        {
            DestroyShell();
        }

        public override void StopMove()
        {
            //rb.velocity = Vector2.zero;
        }

        protected override void CustomMoveShell()
        {
            //SoundEffectCtrl.OnPlayShotSE.OnNext(myLevel);
        }

        //インターフェース
        public override void TakeDamage()
        {
            moveCtrl.Stop();
        }

        public override void Refrect()
        {
            moveCtrl.Stop();
        }
    }

    public class MoveCtrl
    {
        //Inject
        private float speed;
        private Rigidbody2D rb;
        private ActionStatusChecker actionStatusChecker;

        [Inject]
        public MoveCtrl(ActionStatusChecker actionStatusChecker)
        {
            this.actionStatusChecker = actionStatusChecker;
        }

        public void GetShellStats(Rigidbody2D rb, float speed)
        {
            this.speed = speed;
            this.rb = rb;
        }

        public void CallAble()
        {
            Debug.LogError("MoveCtrl CallAble");
        }

        public void Move()
        {
            Debug.Log($"Move , actionStatusChk {actionStatusChecker}, rb {rb}");

            if (actionStatusChecker.Direction)
                rb.velocity = new Vector2(speed, 0);
            else
                rb.velocity = new Vector2(-speed, 0);
        }

        public void Stop()
        {
            rb.velocity = Vector2.zero;
        }
    }

    public class VisualCtrl
    {
        private SpriteRenderer spriteRenderer;
        private ActionStatusChecker actionStatusChecker;

        public void GetPlayerStats(SpriteRenderer spriteRenderer, ActionStatusChecker actionStatusChecker)
        {
            this.spriteRenderer = spriteRenderer;
            this.actionStatusChecker = actionStatusChecker;
        }

        public void SetFlip()
        {
            spriteRenderer.flipX = !actionStatusChecker.Direction;
        }
    }

    public class InitPositioner
    {
        private GameObject muzzleObj;
        private Transform shellTrans;

        public void GetShellStats(GameObject muzzleObj, Transform transform)
        {
            this.muzzleObj = muzzleObj;
            this.shellTrans = transform;
        }

        public void SetMuzzlePositoin()
        {
            shellTrans.position = muzzleObj.transform.position;
        }
    }

    public class HitBoxCtrl
    {
        private BoxCollider2D boxCollider2D;

        public void GetBoxCollider2D(BoxCollider2D boxCollider2D)
        {
            this.boxCollider2D = boxCollider2D;
        }

        public void SetActive(bool isActive)
        {
            boxCollider2D.enabled = isActive;
        }
    }

    public class StateCtrl
    {
        public bool IsStarted{get; private set;}

        public void Start()
        {
            IsStarted = true;
        }

        public void Stop()
        {
            IsStarted = false;
        }
    }
}
