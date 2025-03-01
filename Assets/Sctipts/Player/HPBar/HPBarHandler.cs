using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UniRx;
using Zenject;
using System.Threading;
using UnityEngine.UI;

namespace HPBar
{
    namespace Base
    {
        public class BaseHandler
        {
            public readonly RandomSpriteSetter randomSpriteSetter;
            private readonly VisualLogic visualLogic;
            private readonly HPBarInfo hPBarInfo;
            private readonly PlayerState.EventMediator stateEventMediator;

            private CancellationTokenSource cts;

            public BaseHandler(RandomSpriteSetter randomSpriteSetter, VisualLogic visualLogic, HPBarInfo hPBarInfo, HPBar.EventMediator hpbarEventMediator, DisposableMgr disposableManager, PlayerState.EventMediator stateEventMediator)
            {
                this.randomSpriteSetter = randomSpriteSetter;
                this.visualLogic = visualLogic;
                this.hPBarInfo = hPBarInfo;
                this.stateEventMediator = stateEventMediator;

                hpbarEventMediator.OnPlayerDamage.Subscribe(_ =>
                {
                    Direction().Forget();
                })
                .AddTo(disposableManager.disposables);
            }

            /// <summary>
            /// このメソッドが呼ばれたら、ランダムにBaseの見た目を変更し続けます
            /// </summary>
            private async UniTask Direction()
            {
                cts = new CancellationTokenSource();

                UniTask.Void(async () =>
                {
                    try
                    {
                        await stateEventMediator.OnPlayerDamageRecover
                            .First();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"エラー発生しました : {e.Message}");
                        return;
                    }
                    finally
                    {
                        cts.Cancel();
                        cts.Dispose();
                    }
                });

                while (!cts.IsCancellationRequested)
                {
                    randomSpriteSetter.Direction();

                    try
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(hPBarInfo.RandomWaitTime), cancellationToken: cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"エラー発生しました : {e.Message}");
                        return;
                    }
                    finally
                    {
                        SetDefaultSprite();
                    }
                }
            }

            private void SetDefaultSprite()
            {
                visualLogic.SetSprite(hPBarInfo.defaultSprite);
            }
        }

        public class RandomSpriteSetter
        {
            public readonly RandomLogic randomLogic;
            public readonly VisualLogic visualLogic;

            public RandomSpriteSetter(VisualLogic visualLogic, RandomLogic randomLogic)
            {
                this.visualLogic = visualLogic;
                this.randomLogic = randomLogic;
            }

            /// <summary>
            /// このメソッドが呼ばれたら、ランダムにBaseの見た目を変更します
            /// </summary>
            public void Direction()
            {
                //ランダムにSpriteを取得
                var sprite = randomLogic.GetRandomSprite();
                //Spriteをセット
                visualLogic.SetSprite(sprite);
            }
        }

        public class VisualLogic
        {
            private readonly Image baseImage;

            public VisualLogic([Inject(Id = "Base")] Image baseiamge)
            {
                this.baseImage = baseiamge;
            }

            public void SetSprite(Sprite sprite)
            {
                baseImage.sprite = sprite;
            }
        }

        public class RandomLogic
        {
            HPBarInfo hpBarInfo;

            public RandomLogic(HPBarInfo hpBarInfo)
            {
                this.hpBarInfo = hpBarInfo;
            }

            public Sprite GetRandomSprite()
            {
                int random = UnityEngine.Random.Range(0, hpBarInfo.sprites.Count);
                return hpBarInfo.sprites[random];
            }
        }
    }

    namespace Top
    {
        public class Handler
        {
            private readonly VisualLogic visualLogic;
            private readonly RandomLogic randomLogic;
            private readonly PlayerState.EventMediator stateEventMediator;
            private readonly HPBarInfo hPBarInfo;

            private CancellationTokenSource cts;

            public Handler(HPBar.EventMediator hpbarEventMediator, DisposableMgr disposableMgr, PlayerState.EventMediator stateEventMediator, HPBarInfo hPBarInfo, VisualLogic visualLogic, RandomLogic randomLogic)
            {
                this.visualLogic = visualLogic;
                this.randomLogic = randomLogic;
                this.stateEventMediator = stateEventMediator;
                this.hPBarInfo = hPBarInfo;

                hpbarEventMediator.OnPlayerDamage.Subscribe(_ =>
                {
                    Direction().Forget();
                })
                .AddTo(disposableMgr.disposables);
            }

            //ダメージを食らったら演出を行います
            //終了条件：ダメージから回復したら
            private async UniTask Direction()
            {
                UniTask.Void(async () =>
                {
                    cts = new CancellationTokenSource();

                    try
                    {
                        await stateEventMediator.OnPlayerDamageRecover
                            .First();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"エラー発生しました : {e.Message}");
                        return;
                    }
                    finally
                    {
                        cts.Cancel();
                        cts.Dispose();
                    }
                });

                while (true)
                {
                    ChangeSprite();

                    try
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(hPBarInfo.RandomWaitTime), cancellationToken: cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"エラー発生しました : {e.Message}");
                        return;
                    }
                    finally
                    {
                        visualLogic.SetSprite(hPBarInfo.topDefaultSprite);
                    }
                }
            }

            private void ChangeSprite()
            {
                var sprite = randomLogic.GetRandomSprite();
                visualLogic.SetSprite(sprite);
            }
        }

        public class RandomLogic
        {
            private readonly HPBarInfo hPBarInfo;

            public RandomLogic(HPBarInfo hPBarInfo)
            {
                this.hPBarInfo = hPBarInfo;
            }

            public Sprite GetRandomSprite()
            {
                int random = UnityEngine.Random.Range(0, hPBarInfo.topSprites.Count);
                return hPBarInfo.topSprites[random];
            }
        }

        public class VisualLogic
        {
            private readonly Image topImage;

            public VisualLogic([Inject(Id = "Top")] Image topImage)
            {
                this.topImage = topImage;
            }

            public void SetSprite(Sprite sprite)
            {
                topImage.sprite = sprite;
            }
        }
    }

    namespace Mid
    {
        /// <summary>
        /// このクラスは、現在の中間バーを複数のグループに分ける責務を持ちます
        /// </summary>
        public class DivideLogic
        {
            //グループに分けるポイント(位置)
            private List<int> devidePoint;// = new List<int> { 3, 5, 2 };

            private List<List<GameObject>> grounpList = new List<List<GameObject>>();

            //inject
            private List<GameObject> midBarList;
            private GroupSpriteSetter groupSpriteSetter;
            private RandomDividePoint randomDividePoint;
            private PlayerState.EventMediator playerStatsEventMediator;
            private HPBarInfo hPBarInfo;
            private GrouopRandMoveSetter grouopRandMoveSetter;
            private RandomInVisibleLogic randomInVisibleLogic;

            private CancellationTokenSource cts;

            public DivideLogic(Mids mids, GroupSpriteSetter groupSpriteSetter, RandomDividePoint randomDividePoint, HPBar.EventMediator hpbarEventMediator, DisposableMgr disposableMgr, PlayerState.EventMediator playerStatsEventMediator, HPBarInfo hPBarInfo, GrouopRandMoveSetter grouopRandMoveSetter, RandomInVisibleLogic randomInVisibleLogic)
            {
                this.randomDividePoint = randomDividePoint;
                this.groupSpriteSetter = groupSpriteSetter;
                this.midBarList = mids.midBarList;
                this.playerStatsEventMediator = playerStatsEventMediator;
                this.hPBarInfo = hPBarInfo;
                this.grouopRandMoveSetter = grouopRandMoveSetter;
                this.randomInVisibleLogic = randomInVisibleLogic;

                hpbarEventMediator.OnPlayerDamage.Subscribe(_ =>
                {
                    Direction().Forget();
                })
                .AddTo(disposableMgr.disposables);

                /*Observable.Timer(TimeSpan.FromSeconds(0.01f)).Subscribe(_ =>
                {
                    Direction();
                });*/
            }

            private async UniTask Direction()
            {
                UniTask.Void(async () =>
                {
                    cts = new CancellationTokenSource();

                    try
                    {
                        await playerStatsEventMediator.OnPlayerDamageRecover
                            .First();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"エラー発生しました : {e.Message}");
                        return;
                    }
                    finally
                    {
                        cts.Cancel();
                        cts.Dispose();


                        //元に戻す
                        //この処理は重要です！
                        for (int i = 0; i < midBarList.Count; i++)
                        {
                            groupSpriteSetter.SetDefaultSprite(midBarList[i].GetComponent<Image>());
                        }

                        grouopRandMoveSetter.SetDefaultPosition(midBarList);
                        randomInVisibleLogic.RecoverAll(midBarList);
                    }
                });

                while (!cts.IsCancellationRequested)
                {
                    devidePoint = randomDividePoint.GetRandomDividePoint();

                    int count = 0;

                    //まずグループに分けます

                    //devedePointの数分だけグループを作成する
                    for (int i = 0; i < devidePoint.Count; i++)
                    {
                        //グループを作る
                        var group = new List<GameObject>();

                        //そのグループに、devidePointの位置までの中間バーを追加する
                        for (int j = 0; j < devidePoint[i]; j++)
                        {
                            group.Add(midBarList[count]);
                            randomInVisibleLogic.RandomInVisible(midBarList[count].GetComponent<Image>());
                            count++;
                        }
                        grounpList.Add(group);

                        //Debug.Log($"{i}番目のグループの要素は、{grounpList[i].Count}個です");
                        SetSprite(i);
                        grouopRandMoveSetter.MoveGroup(grounpList[i]);
                    }

                    try
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(hPBarInfo.UnitRandomWaitTime), cancellationToken: cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"エラー発生しました : {e.Message}");
                        return;
                    }
                }
            }

            //指定のグループに対して、ランダムなSpriteをセットします
            private void SetSprite(int groupNum)
            {
                groupSpriteSetter.SaveGroupRandomSprite();
                for (int i = 0; i < grounpList[groupNum].Count; i++)
                {
                    groupSpriteSetter.Direction(grounpList[groupNum][i].GetComponent<Image>());
                }
            }
        }

        public class GroupSpriteSetter
        {
            private readonly RandomLogic randomLogic;
            private readonly VisualLogic visualLogic;
            private readonly HPBarInfo hPBarInfo;

            private Sprite groupSprite;

            public GroupSpriteSetter(RandomLogic randomLogic, VisualLogic visualLogic, HPBarInfo hPBarInfo)
            {
                this.randomLogic = randomLogic;
                this.visualLogic = visualLogic;
                this.hPBarInfo = hPBarInfo;
            }

            public void SaveGroupRandomSprite()
            {
                groupSprite = randomLogic.GetRandomSprite();
            }

            public void Direction(Image target)
            {
                visualLogic.SetSprite(target, groupSprite);
            }

            public void SetDefaultSprite(Image target)
            {
                visualLogic.SetSprite(target, hPBarInfo.midDefaultSprite);
            }
        }

        public class VisualLogic
        {
            public void SetSprite(Image target, Sprite sprite)
            {
                target.sprite = sprite;
            }
        }

        public class RandomLogic
        {
            private HPBarInfo hPBarInfo;

            public RandomLogic(HPBarInfo hPBarInfo)
            {
                this.hPBarInfo = hPBarInfo;
            }

            public Sprite GetRandomSprite()
            {
                int random = UnityEngine.Random.Range(0, hPBarInfo.midSprites.Count);
                return hPBarInfo.midSprites[random];
            }
        }

        public class RandomDividePoint
        {
            private readonly HPBarInfo hPBarInfo;
            private readonly Mids mids;

            public RandomDividePoint(HPBarInfo hPBarInfo, Mids mids)
            {
                this.hPBarInfo = hPBarInfo;
                this.mids = mids;
            }

            public List<int> GetRandomDividePoint()
            {
                int random = UnityEngine.Random.Range(0, hPBarInfo.devidePoints.Count);
                var devidePoint = hPBarInfo.devidePoints[random].devidePoint;

                int total = 0;
                for (int i = 0; i < devidePoint.Count; i++)
                {
                    total += devidePoint[i];
                }

                if (total > mids.midBarList.Count)
                {
                    Debug.LogError("グループに分けるポイントの合計が中間バーの数を超えています");
                    return new List<int> { };
                }

                return hPBarInfo.devidePoints[random].devidePoint;
            }
        }

        public class GrouopRandMoveSetter
        {
            private readonly RandomMovePosition randomMovePosition;
            private readonly MoveLogic moveLogic;

            public GrouopRandMoveSetter(RandomMovePosition randomMovePosition, MoveLogic moveLogic)
            {
                this.randomMovePosition = randomMovePosition;
                this.moveLogic = moveLogic;
            }

            public void MoveGroup(List<GameObject> group)
            {
                Debug.Log("MoveGroup");
                for (int i = 0; i < group.Count; i++)
                {
                    //50%の確率で移動する
                    var random = UnityEngine.Random.Range(0, 2);
                    if (random == 0)
                    {
                        return;
                    }

                    var rectTrans = group[i].GetComponent<RectTransform>();
                    moveLogic.Move(rectTrans, randomMovePosition.GetRandomMovePosition(rectTrans));
                }
            }

            public void SetDefaultPosition(List<GameObject> group)
            {
                for (int i = 0; i < group.Count; i++)
                {
                    var rectTrans = group[i].GetComponent<RectTransform>();

                    var pos = new Vector3(0, rectTrans.anchoredPosition.y);

                    rectTrans.anchoredPosition = pos;
                }
            }
        }

        public class MoveLogic
        {
            public void Move(RectTransform target, Vector3 pos)
            {
                target.anchoredPosition = pos;
            }
        }

        public class RandomMovePosition
        {
            private readonly HPBarInfo hPBarInfo;

            public RandomMovePosition(HPBarInfo hPBarInfo)
            {
                this.hPBarInfo = hPBarInfo;
            }

            public Vector2 GetRandomMovePosition(RectTransform caller)
            {
                float x = caller.anchoredPosition.x + UnityEngine.Random.Range(-hPBarInfo.unitMoveThereshold, hPBarInfo.unitMoveThereshold);

                //25％の確率で移動量が倍になる
                if (UnityEngine.Random.Range(0, 4) == 0)
                {
                    x *= 5;
                }

                return new Vector2(x, caller.anchoredPosition.y);
            }
        }

        public class RandomInVisibleLogic
        {
            public void RandomInVisible(Image img)
            {
                //10%の確率で非表示にする
                if (UnityEngine.Random.Range(0, 10) == 0)
                {
                    img.enabled = false;
                }
            }

            public void RecoverAll(List<GameObject> midBarList)
            {
                for (int i = 0; i < midBarList.Count; i++)
                {
                    midBarList[i].GetComponent<Image>().enabled = true;
                }
            }
        }
    }

    public abstract class HPBarBase : MonoBehaviour
    {
        [SerializeField] protected GameObject midPrefab, BaseObj, hpUnitPrefab;
        [SerializeField] protected RectTransform topTransform;
        [SerializeField] protected float barHeight, addMidBarIntervalTime, intervalTime;
        [SerializeField] protected bool isDebugMode = false;
        [SerializeField] protected int currentLife, initMaxLife;
        [SerializeField] protected List<GameObject> hpUnitList;


        private int cacheMaxLife = 0;
        protected abstract void OnDead();
        protected abstract UniTask AddLife(int addAmount);
        protected abstract UniTask Heal(int healAmount);
        public abstract void Damage(int damageAmount);

        protected virtual void DamageLife(int damageAmount)
        {
            for (int count = 0; count < damageAmount; count++)
            {
                Destroy(hpUnitList[hpUnitList.Count - 1]);
                hpUnitList.RemoveAt(hpUnitList.Count - 1);

                currentLife--;
                if (currentLife <= 0)
                {
                    currentLife = 0;

                    OnDead();
                    return;
                }
            }
        }

        protected async Task MakeMidBar()
        {
            //中間のバーを作成
            var instance = Instantiate(midPrefab);
            instance.transform.SetParent(BaseObj.transform);
            instance.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, cacheMaxLife * barHeight);

            //蓋の位置を上げる
            Vector2 pos = topTransform.anchoredPosition;
            pos.y += barHeight;
            topTransform.anchoredPosition = pos;

            //キャッシュの値を増やす
            cacheMaxLife++;

            try
            {
                //インターバルだけ待つ
                if (isDebugMode) await UniTask.Yield();
                else await UniTask.Delay(TimeSpan.FromSeconds(addMidBarIntervalTime));
            }
            catch (Exception e)
            {
                Debug.LogError($"エラー発生しました、{e.Message}");
                //UnityEditor.EditorApplication.isPaused = true;
                return;
            }
        }

        protected async Task HealLife()
        {
            var instance = Instantiate(hpUnitPrefab);
            instance.transform.SetParent(BaseObj.transform);
            var rectTrasnform = instance.MyGetComponent_NullChker<RectTransform>();
            rectTrasnform.anchoredPosition = new Vector2(0, currentLife * barHeight);

            hpUnitList.Add(instance);
            currentLife++;
            SoundEffectCtrl.onPlayHealHPSound.OnNext(Unit.Default);

            try
            {
                if (isDebugMode) await UniTask.Yield();
                else await UniTask.Delay(TimeSpan.FromSeconds(intervalTime));
            }
            catch (Exception e)
            {
                Debug.LogError($"エラー発生しました : {e.Message}");
                //UnityEditor.EditorApplication.isPaused = true;
                return;
            }
        }
    }

    public interface IHealth
    {
        void Damage(int damageAmount);
        void Heal(int healAmount);
    }

    public class HPBarHandler : MonoBehaviour, IHealth
    {
        //Inject
        LifeManager lifeManager;
        EventMediator eventMediator;

        //プレイヤーのHPが0になったときに使用するSubject
        private Subject<Unit> onHPZero = new Subject<Unit>();
        public IObservable<Unit> OnHPZero => onHPZero;

        //Injectするために必要な変数
        PlayerHPVisual playerHPVisual;
        PlayerHP playerHP;
        PlayerStats playerStats = new PlayerStats();

        [Inject]
        public void Construct(PlayerHP playerHP, PlayerHPVisual playerHPVisual, LifeManager lifeManager, HPBar.EventMediator eventMediator)
        {
            this.playerHP = playerHP;
            this.playerHPVisual = playerHPVisual;
            this.lifeManager = lifeManager;
            this.eventMediator = eventMediator;
        }

        private void Awake()
        {
            lifeManager?.OnPlayerDead.Subscribe(_ =>
            {
                playerHP.Damage(playerHP.MaxLife);
                playerHPVisual.ChangeCurrentLife(-playerHP.MaxLife);
            })
            .AddTo(this);
        }

        private void Start()
        {
            playerHPVisual.InitVisual();

            //初期最大HP分だけ初期化
            playerHPVisual.ChangeMaxLife(playerHP.MaxLife);
            playerHPVisual.ChangeCurrentLife(playerHP.CurrentLife);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                playerHPVisual.ChangeCurrentLife(1);
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                playerHPVisual.ChangeCurrentLife(-3);
            }
        }

        public void Heal(int amount)
        {
            playerHP.Heal(amount);
            playerHPVisual.ChangeCurrentLife(amount);
        }

        public void Damage(int amount)
        {
            playerHP.Damage(amount);
            playerHPVisual.ChangeCurrentLife(-amount);

            if (playerHP.CurrentLife == 0)
            {
                playerStats.Dead();
            }

            if (playerStats.IsDead)
            {
                onHPZero?.OnNext(Unit.Default);
            }
            else
            {
                eventMediator.OnPlayerDamageObserver.OnNext(Unit.Default);
            }
        }
    }

    public class Mids
    {
        public List<GameObject> midBarList = new List<GameObject>();
    }

    /// <summary>
    /// 見た目の処理を行うクラスです
    /// </summary>
    public class PlayerHPVisual
    {
        //Injectするために必要な変数
        GameObject Base;
        GameObject midPrefab;
        GameObject TopObj;
        GameObject hpUnit;
        PlayerHP playerHP;
        int barHeight;

        List<GameObject> hpUnitList = new List<GameObject>();
        List<GameObject> midBarList;

        RectTransform topTransform;

        int cachedMaxLifeYPos = 0;

        [Inject]
        public PlayerHPVisual(PlayerHP playerHP, [Inject(Id = "Base")] GameObject Base, [Inject(Id = "Mid")] GameObject midPrefab, [Inject(Id = "Top")] GameObject TopObj, [Inject(Id = "HPUnit")] GameObject hpUnit, [Inject(Id = "BarHeight")] int barHeight, Mids mids)
        {
            this.playerHP = playerHP;
            this.barHeight = barHeight;

            this.Base = Base;
            this.midPrefab = midPrefab;
            this.TopObj = TopObj;
            this.hpUnit = hpUnit;
            this.midBarList = mids.midBarList;
        }

        public void InitVisual()
        {
            //TopObjを初期化
            topTransform = TopObj.GetComponent<RectTransform>();
            Vector2 pos = topTransform.anchoredPosition;
            pos.y = 0f;
            topTransform.anchoredPosition = pos;
        }

        //amountの値分だけ最大体力を変更する
        public void ChangeMaxLife(int amount)
        {
            if (amount > 0)
            {
                for (int i = 0; i < amount; i++)
                {
                    //中間のバーを作成
                    var instance = GameObject.Instantiate(midPrefab);
                    var instanceTrans = instance.GetComponent<RectTransform>();
                    midBarList.Add(instance);

                    //中間のバーの位置を変更
                    instance.transform.SetParent(Base.transform);
                    instanceTrans.anchoredPosition = new Vector3(
                        0,
                        cachedMaxLifeYPos + (barHeight * i));
                    //なぜcachedMaxLifeYPosを使うのか？
                    //中間バーを作成するたびに基準点をcachedMaxLifeYPosに加算しているためです。

                    //TopObjを中間バーの子オブジェクトにして、座標を0にすると自動的に中間バーの上に配置される
                    //アンカーを正しく設定しているためです。
                    //子オブジェクトにしたら0の座標が中間バーの上となります
                    //毎回forで実行する必要が無い気がしますが、"最後のforのみ実行"ということができないので毎回実行するようにしています。
                    TopObj.transform.SetParent(instance.transform);
                    topTransform.anchoredPosition = Vector2.zero;
                }

                cachedMaxLifeYPos += barHeight * amount;

                return;
            }
            else if (amount < 0)
            {
                if (midBarList.Count == 0)
                {
                    return;
                }

                amount = -amount;

                //TopObjの親子関係をBaseに変更
                //これをしないと、上から中間バーを消してしまうとTopObjが消えてしまいます
                TopObj.transform.SetParent(Base.transform);

                for (int i = 0; i < amount; i++)
                {
                    if (midBarList.Count == 0)
                    {
                        //TopObjの位置を変更
                        topTransform.anchoredPosition = Vector2.zero;

                        //親子関係もBaseに変更
                        //明示的に戻しておきます
                        TopObj.transform.SetParent(Base.transform);
                        return;
                    }

                    //中間のバーを削除
                    GameObject.Destroy(midBarList[midBarList.Count - 1]);
                    midBarList.RemoveAt(midBarList.Count - 1);

                    //最上位の中間バーの位置を正す
                    cachedMaxLifeYPos -= barHeight;

                    //forが最後のループのときだけTopObjの位置を変更
                    //TopObjの親子関係を一番上の中間バーに変更
                    if (i == amount - 1)
                    {
                        //TopObjの位置を変更
                        Vector2 pos = topTransform.anchoredPosition;
                        pos.y = midBarList[midBarList.Count - 1].GetComponent<RectTransform>().anchoredPosition.y;
                        topTransform.anchoredPosition = pos;

                        //TopObjの親子関係を一番上の中間バーに変更
                        //座標も正しておく
                        TopObj.transform.SetParent(midBarList[midBarList.Count - 1].transform);
                        topTransform.anchoredPosition = Vector2.zero;
                    }
                }
                return;
            }
            else
            {
                Debug.LogError("0を入力することはできません");
                return;
            }
        }

        //amountの値分だけ現在の体力を変更する
        public void ChangeCurrentLife(int amount)
        {
            int cachedCurrentLifeYPos = hpUnitList.Count * barHeight;

            if (amount > 0)
            {
                for (int i = 0; i < amount; i++)
                {
                    if (hpUnitList.Count == playerHP.MaxLife)
                    {
                        return;
                    }

                    //HPのユニットを作成
                    var instance = GameObject.Instantiate(hpUnit);
                    var instanceTrans = instance.GetComponent<RectTransform>();
                    hpUnitList.Add(instance);

                    instance.transform.SetParent(Base.transform);
                    instanceTrans.anchoredPosition = new Vector2(
                        0,
                        cachedCurrentLifeYPos + (i * barHeight));
                }
            }
            else if (amount < 0)
            {
                for (int i = 0; i < -amount; i++)
                {
                    if (hpUnitList.Count == 0)
                    {
                        return;
                    }

                    //HPのユニットを削除
                    GameObject.Destroy(hpUnitList[hpUnitList.Count - 1]);
                    hpUnitList.RemoveAt(hpUnitList.Count - 1);

                    if (hpUnitList.Count == 0)
                    {
                        return;
                    }
                }
            }
            else
            {
                Debug.LogError("0を入力することはできません");
                return;
            }
        }
    }

    public interface IPlayerHP
    {
        void Heal(int amount);
        void Damage(int amount);
        void UpdateMaxLife(int amount);
    }

    /// <summary>
    /// プレイヤーのHPを数値上で管理するクラスです
    /// </summary>
    public class PlayerHP : IPlayerHP
    {
        public int MaxLife => maxLife;
        public int CurrentLife => currentLife;
        private int maxLife, currentLife;

        public PlayerHP()
        {
            maxLife = 10;
            currentLife = maxLife;
        }

        public void Heal(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogError("プラスの値を入力しないと体力が増えません");
                return;
            }

            currentLife += amount;
        }

        public void Damage(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogError("プラスの値を入力しないとダメージを受けません");
                return;
            }

            currentLife -= amount;
            if (currentLife <= 0)
            {
                currentLife = 0;
            }
        }

        public void UpdateMaxLife(int amount)
        {
            if (amount == 0)
            {
                Debug.LogError("0を入力することはできません");
                return;
            }

            maxLife += amount;
            if (maxLife < 0)
            {
                maxLife = 0;
            }
        }
    }

    /// <summary>
    /// プレイヤーのステータスを管理するクラスです
    /// </summary>
    public class PlayerStats
    {
        public bool IsDead => isDead;
        private bool isDead = false;

        public void Dead()
        {
            isDead = true;
        }
    }

    public class EventMediator
    {
        private Subject<Unit> onPlayerDamage = new Subject<Unit>();
        public IObservable<Unit> OnPlayerDamage => onPlayerDamage;
        public IObserver<Unit> OnPlayerDamageObserver => onPlayerDamage;
    }
}
