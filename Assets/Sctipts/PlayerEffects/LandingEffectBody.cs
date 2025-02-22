using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace LandingEffect
{
    public class LandingEffectHandler
    {
        //Inject
        PoolHandler poolHandler;
        PositionLogic positionLogic;
        Transform playerTransform;

        public LandingEffectHandler(PoolHandler poolHandler, EventMediator eventMediator, DisposableMgr disposableMgr, GroundChk groundChk, PositionLogic positionLogic, [Inject(Id = "PlayerTrans")] Transform playerTransform)
        {
            this.poolHandler = poolHandler;
            this.positionLogic = positionLogic;
            this.playerTransform = playerTransform;

            //着地エフェクトをプールに戻します
            eventMediator.OnEndAnim
                .Subscribe(body => poolHandler.Release(body))
                .AddTo(disposableMgr.disposables);

            //地面についたらエフェクトを生成します
            groundChk.OnGround
                .Subscribe(_ => SpawnEffect().Forget())
                .AddTo(disposableMgr.disposables);
        }

        async private UniTask SpawnEffect()
        {
            positionLogic.SetSpawnPos(playerTransform.position);

            //3回左右に生成
            for (int i = 0; i < 3; i++)
            {
                var rightEffect = poolHandler.Get();
                rightEffect.OnSpawn(true);

                var leftEffect = poolHandler.Get();
                leftEffect.OnSpawn(false);

                await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
            }
        }
    }

    public class LandingEffectBody : MonoBehaviour
    {
        public class Factory : PlaceholderFactory<LandingEffectBody> { }

        [SerializeField] Vector2 offset = new Vector2(0.75f, 0f);
        [SerializeField] float moveSpeed = 5f;

        //Inject
        PositionLogic posMgr;
        EventMediator eventMediator;
        AnimCtrl animCtrl;
        FlipLogic flipLogic;
        MoveLogic moveLogic;

        [Inject]
        public void Construct(PositionLogic posMgr, EventMediator mediator, AnimCtrl animCtrl, FlipLogic flipLogic, MoveLogic moveLogic)
        {
            this.posMgr = posMgr;
            this.eventMediator = mediator;
            this.animCtrl = animCtrl;
            this.flipLogic = flipLogic;
            this.moveLogic = moveLogic;
        }

        private void Awake()
        {
            animCtrl.Init(GetComponent<Animator>());
            flipLogic.Init(GetComponent<SpriteRenderer>());
            posMgr.Init(offset);
            moveLogic.Init(GetComponent<Rigidbody2D>(), moveSpeed);
        }

        //位置を正す
        //アニメーション再生
        public void OnSpawn(bool direction)
        {
            posMgr.SetPosition(transform, direction);
            flipLogic.SetDirection(direction);
            animCtrl.PlayAnim();
            moveLogic.Move(direction);
        }

        //Unityがアニメーション終了時に呼び出す
        public void EndAnim()
        {
            //アニメーション終了時にプールに戻す
            eventMediator.OnEndAnimObserver.OnNext(this);
        }
    }

    public class PositionLogic
    {
        Vector2 playerTrans;

        Vector2 offset;

        public void Init(Vector2 offset)
        {
            this.offset = offset;
        }

        public void SetSpawnPos(Vector3 targetPos)
        {
            playerTrans = targetPos;
        }

        public void SetPosition(Transform transform, bool direction)
        {
            if (!direction)
                transform.position = new Vector3(playerTrans.x + offset.x, playerTrans.y + offset.y, 0);
            else
                transform.position = new Vector3(playerTrans.x - offset.x, playerTrans.y + offset.y, 0);
        }
    }

    public class AnimCtrl
    {
        Animator anim;

        public void Init(Animator animator)
        {
            anim = animator;
        }

        public void PlayAnim()
        {
            anim.SetTrigger("onStart");
        }
    }

    public class FlipLogic
    {
        SpriteRenderer spRenderer;

        public void Init(SpriteRenderer spRenderer)
        {
            this.spRenderer = spRenderer;
        }

        public void SetDirection(bool direction)
        {
            spRenderer.flipX = !direction; //右向きの場合はflipXをfalseにする。なぜなら右向きがオフだからです。
        }
    }

    public class MoveLogic
    {
        Rigidbody2D rb;
        float moveSpeed;

        public void Init(Rigidbody2D rb, float speed)
        {
            this.rb = rb;
            moveSpeed = speed;
        }

        public void Move(bool direction)
        {
            if(direction)
            {
                rb.AddForce(Vector2.left * moveSpeed, ForceMode2D.Impulse);
            }
            else
            {
                rb.AddForce(Vector2.right * moveSpeed, ForceMode2D.Impulse);
            }
        }
    }

    public class PoolHandler
    {
        LandingEffectBody.Factory factory;
        UnityEngine.Pool.ObjectPool<LandingEffectBody> pool;

        public PoolHandler(LandingEffectBody.Factory factory)
        {
            this.factory = factory;
            int maxCapacity = 6;

            pool = new UnityEngine.Pool.ObjectPool<LandingEffectBody>(
                createFunc: CreateObj,
                actionOnGet: GetObj,
                actionOnRelease: ReleaseObj,
                actionOnDestroy: DestroyObj,
                defaultCapacity: maxCapacity,
                maxSize: maxCapacity
            );

            for (int i = 0; i < maxCapacity; i++)
            {
                var obj = CreateObj();
                pool.Release(obj);
            }
        }

        private LandingEffectBody CreateObj()
        {
            var obj = factory.Create();
            return obj;
        }

        private void GetObj(LandingEffectBody obj)
        {
            obj.gameObject.SetActive(true);
        }

        private void ReleaseObj(LandingEffectBody obj)
        {
            obj.gameObject.SetActive(false);
        }

        private void DestroyObj(LandingEffectBody obj)
        {
            GameObject.Destroy(obj.gameObject);
        }

        public LandingEffectBody Get()
        {
            return pool.Get();
        }

        public void Release(LandingEffectBody obj)
        {
            pool.Release(obj);
        }
    }

    public class EventMediator
    {
        private Subject<LandingEffectBody> onEndAnimSubject = new Subject<LandingEffectBody>();
        public IObservable<LandingEffectBody> OnEndAnim => onEndAnimSubject;
        public IObserver<LandingEffectBody> OnEndAnimObserver => onEndAnimSubject;
    }
}
