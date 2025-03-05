using HPBar;
using UnityEngine;
using Zenject;
using UnityEngine.Pool;
using System;
using UniRx;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.InputSystem.LowLevel;
using ObjectPoolFactory;

namespace Item
{
    namespace HP
    {
        public class ObjPoolLogic
        {
            private ObjectPool<GameObject> pool;
            private HPItem.Factory factory;

            //Inject
            private readonly EventMediator eventMediator;

            public ObjPoolLogic(HPItem.Factory factory, [Inject(Id = "HPItemParent")] Transform parent, EventMediator eventMediator)
            {
                this.eventMediator = eventMediator;

                int maxCapacity = 10;

                this.factory = factory;

                pool = new ObjectPool<GameObject>(
                    createFunc: () => CreateObj(),
                    actionOnGet: (obj) => obj.SetActive(true),
                    actionOnRelease: (obj) => obj.SetActive(false),
                    actionOnDestroy: (obj) => GameObject.Destroy(obj),
                    true,
                    maxSize: maxCapacity,
                    defaultCapacity: maxCapacity
                );

                //先にオブジェクトを生成しておく
                //処理の負荷の軽減を図ります
                for (int i = 0; i < maxCapacity; i++)
                {
                    var obj = CreateObj();
                    obj.transform.parent = parent;
                    pool.Release(obj);
                }
            }

            private GameObject CreateObj()
            {
                var obj = factory.Create();
                return obj.gameObject;
            }

            public GameObject Instantiate(Vector2 pos)
            {
                var obj = pool.Get();
                obj.transform.position = pos;
                eventMediator.OnSpawnObserver.OnNext(Unit.Default);

                return obj;
            }

            public void Release(GameObject obj)
            {
                pool.Release(obj);
            }
        }

        public class HPItemSpawner
        {
            private readonly ObjPoolLogic objPoolLogic;
            private readonly ActLogic actLogic;

            public HPItemSpawner(ObjPoolLogic objPoolLogic, ActLogic actLogic, EventMediator eventMediator, DisposableMgr disposableMgr)
            {
                this.objPoolLogic = objPoolLogic;
                this.actLogic = actLogic;

                eventMediator.OnDispose
                    .Subscribe(obj => objPoolLogic.Release(obj))
                    .AddTo(disposableMgr.disposables);
            }

            public GameObject DropHPItem(Vector2 pos)
            {
                var obj = objPoolLogic.Instantiate(pos);
                actLogic.Jump(obj.GetComponent<Rigidbody2D>(), 5);
                return obj;
            }
        }

        [RequireComponent(typeof(SpriteRenderer))]
        public class HPItem : MonoBehaviour
        {
            public class Factory : PlaceholderFactory<HPItem> { }

            [SerializeField]
            private int healAmount = 1;

            private SpriteRenderer spriteRenderer;

            //Inject
            IHealth playerHealth;
            EventMediator eventMediator;
            AutoDisappearLogic autoDisappearLogic;
            AnimHandler animHandler;

            [Inject]
            public void Construct(IHealth health, EventMediator eventMediator, AutoDisappearLogic autoDisappearLogic, AnimHandler animHandler, RbLogic rbLogic)
            {
                playerHealth = health;
                this.eventMediator = eventMediator;
                this.autoDisappearLogic = autoDisappearLogic;
                this.animHandler = animHandler;

                animHandler.Init(GetComponent<Animator>(), GetComponent<Rigidbody2D>());
            }

            private void Awake()
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            private void OnEnable()
            {
                autoDisappearLogic.AutoDisappear(gameObject, spriteRenderer, 2f, 2f).Forget();
            }

            private void OnTriggerEnter2D(Collider2D other)
            {
                if (other.gameObject.CompareTag("Player"))
                {
                    //プレイヤーのHPを回復させる
                    playerHealth.Heal(healAmount);

                    //プールに自分を戻します
                    eventMediator.OnDisposeObserver.OnNext(gameObject);

                    // 点滅をキャンセルします
                    // この処理は重要です！やらないとバグるぞ！！
                    autoDisappearLogic.CancellDirection();
                    animHandler.EndAnim();
                }
            }
        }

        public class AnimHandler
        {
            //Inject
            private readonly AnimLogic animLogic;

            private Animator animator;
            private Rigidbody2D rb;

            bool isSpawn = false;

            public AnimHandler(AnimLogic animLogic, DisposableMgr disposableMgr, EventMediator eventMediator, RbLogic rbLogic)
            {
                isSpawn = false;

                this.animLogic = animLogic;

                //上昇中だけど上昇スピードが遅くなったらSpawnのアニメーションを再生します
                Observable.EveryUpdate()
                    .Where(_ => !isSpawn)
                    .Where(_ => rbLogic.IsSlowlyJumping(rb, 2f))
                    .Subscribe(_ =>
                    {
                        animLogic.SetTrigger(animator, "onSpawn");
                        isSpawn = true;
                    })
                    .AddTo(disposableMgr.disposables);
            }

            public void Init(Animator animator, Rigidbody2D rb)
            {
                this.animator = animator;
                this.rb = rb;

                animLogic.SetTrigger(animator, "onSpawn");
            }

            public void EndAnim()
            {
                animLogic.SetTrigger(animator, "onEnding");
            }
        }

        public class RbLogic
        {
            private Rigidbody2D rb;

            public void Init(Rigidbody2D rb)
            {
                this.rb = rb;
            }

            public bool IsSlowlyJumping(Rigidbody2D rb, float threshold)
            {
                return rb.velocity.y < threshold && rb.velocity.y > 0;
            }
        }

        public class AutoDisappearLogic
        {
            //Inject
            private readonly EventMediator eventMediator;
            private readonly BlinkingHandler blinkingHandler;

            private CancellationTokenSource cts;

            public AutoDisappearLogic(EventMediator eventMediator, BlinkingHandler blinkingHandler)
            {
                this.eventMediator = eventMediator;
                this.blinkingHandler = blinkingHandler;
            }

            public void CancellDirection()
            {
                cts?.Cancel();
                cts = null;
            }

            public async UniTask AutoDisappear(GameObject obj, SpriteRenderer spriteRenderer, float initialDisplayTime, float blinkingTime)
            {
                //　初期化
                blinkingHandler.SetSpriteRenderer(spriteRenderer);
                blinkingHandler.Blink(true);

                cts = new CancellationTokenSource();

                // initialDisplayTime秒だけまつ
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(initialDisplayTime), cancellationToken: cts.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e} : が発生しました！！！！！");
                    return;
                }

                // 点滅開始！
                blinkingHandler.StartBlinking(0.05f, cts.Token).Forget();

                // blinkingTime秒だけ点滅させる
                // つまりblinkingTime秒だけ待機します。
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(blinkingTime), cancellationToken: cts.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e} : が発生しました！！！！！");
                    return;
                }
                finally
                {
                    cts?.Cancel();
                    cts = null;
                }

                if (!obj.activeSelf) return;
                eventMediator.OnDisposeObserver.OnNext(obj);
            }
        }

        public class BlinkingHandler
        {
            bool toggle = false;
            SpriteRenderer sr;

            VisualLogic visualLogic;
            public BlinkingHandler(VisualLogic visualLogic)
            {
                this.visualLogic = visualLogic;
            }

            public void SetSpriteRenderer(SpriteRenderer sr)
            {
                this.sr = sr;
            }

            public async UniTask StartBlinking(float blinkingIntervalTime, CancellationToken ct)
            {
                while (true)
                {
                    Blink();

                    try
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(blinkingIntervalTime), cancellationToken: ct);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"{e} : が発生しました！！！！！");
                        return;
                    }
                    finally
                    {
                        visualLogic.Visible(sr);
                    }
                }
            }

            public void Blink()
            {
                if (toggle)
                    visualLogic.Visible(sr);
                else
                    visualLogic.Invisible(sr);

                toggle = !toggle;
            }

            //初期化などに使って
            public void Blink(bool blink)
            {
                if (blink)
                    visualLogic.Visible(sr);
                else
                    visualLogic.Invisible(sr);
            }
        }

        public class VisualLogic
        {
            public void Visible(SpriteRenderer sr)
            {
                sr.color = Color.white;
            }

            public void Invisible(SpriteRenderer sr)
            {
                sr.color = Color.clear;
            }
        }

        public class ActLogic
        {
            public void Jump(Rigidbody2D targetRb, float force)
            {
                targetRb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
            }
        }

        public class AnimLogic
        {
            public void SetTrigger(Animator animator, string triggerName)
            {
                animator.SetTrigger(triggerName);
            }
        }

        public class EventMediator
        {
            private Subject<GameObject> onDispose = new Subject<GameObject>();
            public IObservable<GameObject> OnDispose => onDispose;
            public IObserver<GameObject> OnDisposeObserver => onDispose;

            private Subject<Unit> onSpawn = new Subject<Unit>();
            public IObservable<Unit> OnSpawn => onSpawn;
            public IObserver<Unit> OnSpawnObserver => onSpawn;
        }
    }
}
